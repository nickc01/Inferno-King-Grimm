using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Utilities;
using Random = UnityEngine.Random;

public class GrimmBatMovement : MonoBehaviour 
{
	public BatState State { get; private set; }

	new SpriteRenderer renderer;
	Animator animator;
	new Rigidbody2D rigidbody;

	float turnCooldown;

	public enum BatState
	{
		Dormant,
		SentOut,
		BringingIn
	}

	void Start()
	{
		animator = GetComponent<Animator>();
		renderer = GetComponent<SpriteRenderer>();
		rigidbody = GetComponent<Rigidbody2D>();

		float num = UnityEngine.Random.Range(0.7f, 0.9f);

		transform.localScale = transform.localScale.With(num,num);

		transform.position = transform.position.With(z: 0f);

		renderer.enabled = false;
	}

	void Update()
	{
		turnCooldown -= Time.deltaTime;
	}


	public void SendOut()
	{
		if (State != BatState.Dormant)
		{
			return;
		}
		StartCoroutine("SendOutRoutine");
		StopCoroutine("BringInRoutine");
	}

	public void BringIn()
	{
		StartCoroutine("BringInRoutine");
		StopCoroutine("SendOutRoutine");
	}

	CardinalDirection RandomDirection()
	{
		switch (Random.Range(0,4))
		{
			case 0:
				return CardinalDirection.Up;
			case 1:
				return CardinalDirection.Down;
			case 2:
				return CardinalDirection.Left;
			case 3:
				return CardinalDirection.Right;
			default:
				return CardinalDirection.Up;
		}
	}

	protected IEnumerator SendOutRoutine()
	{
		State = BatState.SentOut;

		var grimmPosition = GrimmBatController.CurrentGrimm.transform.position;

		transform.position = grimmPosition.With(z: 0f);

		renderer.enabled = true;

		animator.PlayAnimation("Bat Fly");

		CardinalDirection? direction = null;//RandomDirection();

		while (true)
		{
			if (direction == null)
			{
				direction = RandomDirection();
			}
			Vector2 velocityMin = Vector2.zero;
			Vector2 velocityMax = Vector2.zero;
			int mainAxis = 1;
			int otherAxis = 0;
			float axisMultiplier = 1f;

			switch (direction)
			{
				case CardinalDirection.Up:
					velocityMin = new Vector2(1f,2f);
					velocityMax = new Vector2(4f,3f);
					mainAxis = 1;
					otherAxis = 0;
					axisMultiplier = 0.3f;
					break;
				case CardinalDirection.Down:
					velocityMin = new Vector2(1f, -3f);
					velocityMax = new Vector2(4f, -2f);
					mainAxis = 1;
					otherAxis = 0;
					axisMultiplier = 0.3f;
					break;
				case CardinalDirection.Left:
					velocityMin = new Vector2(-5f, 0.5f);
					velocityMax = new Vector2(-3f, 2f);
					mainAxis = 0;
					otherAxis = 1;
					axisMultiplier = 0.5f;
					break;
				case CardinalDirection.Right:
					velocityMin = new Vector2(3f, 0.5f);
					velocityMax = new Vector2(5f, 2f);
					mainAxis = 0;
					otherAxis = 1;
					axisMultiplier = 0.5f;
					break;
			}
			Vector2 accel = new Vector2(UnityEngine.Random.Range(velocityMin.x, velocityMax.x), UnityEngine.Random.Range(velocityMin.y, velocityMax.y));
			if (UnityEngine.Random.Range(0, 1) == 0)
			{
				accel[otherAxis] = -accel[otherAxis];
			}
			Vector2 velocity = rigidbody.velocity;
			velocity[mainAxis] = velocity[mainAxis] * axisMultiplier;
			rigidbody.velocity = velocity;
			accel *= 0.5f;
			Vector2 maxSpeed = accel * 10f;
			maxSpeed.x = Mathf.Abs(maxSpeed.x);
			maxSpeed.y = Mathf.Abs(maxSpeed.y);

			for (float timer = 0.2f; timer > 0f; timer -= Time.deltaTime)
			{
				FaceDirection((rigidbody.velocity.x <= 0f) ? -1 : 1, false);
				Accelerate(accel, new Vector2(15f, 10f));
				yield return null;
			}

			direction = null;
			float timer2 = UnityEngine.Random.Range(0.5f, 1.5f);
			while (direction == null && timer2 > 0f)
			{
				FaceDirection((rigidbody.velocity.x <= 0f) ? -1 : 1, false);
				Accelerate(accel, maxSpeed);
				Vector2 position = transform.position;
				if (position.x < 73f)
				{
					direction = CardinalDirection.Right;
					break;
				}
				if (position.x > 99f)
				{
					direction = CardinalDirection.Left;
					break;
				}
				if (position.y < 8f)
				{
					direction = CardinalDirection.Up;
					break;
				}
				if (position.y > 15f)
				{
					direction = CardinalDirection.Right;
					break;
				}
				yield return null;
				timer2 -= Time.deltaTime;
			}
		}

	}

	private void FaceDirection(int sign, bool snap)
	{
		float num = Mathf.Abs(transform.localScale.x) * sign;
		if (!Mathf.Approximately(transform.localScale.x, num) && (snap || this.turnCooldown <= 0f))
		{
			if (!snap)
			{
				animator.PlayAnimation("Bat TurnToFly");
				turnCooldown = 0.5f;
			}
			renderer.flipX = num < 0f;
			//transform.localScale = transform.localScale.With(x: num);
		}
	}

	private void Accelerate(Vector2 fixedAcceleration, Vector2 speedLimit)
	{
		Vector2 a = fixedAcceleration / Time.fixedDeltaTime;
		Vector2 vector = rigidbody.velocity;
		vector += a * Time.deltaTime;
		vector.x = Mathf.Clamp(vector.x, -speedLimit.x, speedLimit.x);
		vector.y = Mathf.Clamp(vector.y, -speedLimit.y, speedLimit.y);
		rigidbody.velocity = vector;
	}

	protected IEnumerator BringInRoutine()
	{
		//this.state = FakeBat.States.In;
		State = BatState.BringingIn;
		int sign = (GrimmBatController.CurrentGrimm.transform.position.x - rigidbody.velocity.x <= 0f) ? -1 : 1;
		FaceDirection(sign, true);
		rigidbody.velocity = Vector2.zero;
		for (; ; )
		{
			Vector2 source = this.transform.position;
			Vector2 destination = GrimmBatController.CurrentGrimm.transform.position;
			Vector2 next = Vector2.MoveTowards(source, destination, 25f * Time.deltaTime);
			transform.position = transform.position.With(next.x,next.y);
			//this.transform.SetPosition2D(next);
			if (Vector2.Distance(next, destination) < Mathf.Epsilon)
			{
				break;
			}
			yield return null;
		}
		yield return animator.PlayAnimationTillDone("Bat End");
		/*this.spriteAnimator.Play("Bat End");
		while (this.spriteAnimator.ClipTimeSeconds < this.spriteAnimator.CurrentClip.Duration - Mathf.Epsilon)
		{
			yield return null;
		}*/
		//this.meshRenderer.enabled = false;
		renderer.enabled = false;
		//this.transform.SetPositionY(-50f);
		transform.position = transform.position.With(y: -50f);
		//this.state = FakeBat.States.Dormant;
		State = BatState.Dormant;
		yield break;
	}

}
