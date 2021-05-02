using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;
using Random = UnityEngine.Random;

public class GrimmSpike : MonoBehaviour {

	[SerializeField]
	float groundHeight = 1.109754f;

	[SerializeField]
	float belowGroundHeight = -0.89f;

	[SerializeField]
	float leftEdge = 2.6f;
	[SerializeField]
	float rightEdge = 35.1f;

	bool inRange = false;
	new SpriteRenderer renderer;
	new PolygonCollider2D collider;
	Animator animator;
	bool spikesRising = false;

	void Start () 
	{
		renderer = GetComponent<SpriteRenderer>();
		collider = GetComponent<PolygonCollider2D>();
		animator = GetComponent<Animator>();

		if (Random.value > 0.5f)
		{
			renderer.flipX = true;
		}

		float rotationOffset = Random.Range(-5f, 5f);

		transform.eulerAngles = new Vector3(0f,0f,rotationOffset);

		renderer.enabled = false;
		collider.enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void PrepareForAttack()
	{
		inRange = transform.localPosition.x >= leftEdge && transform.localPosition.x <= rightEdge;
		renderer.enabled = inRange;
		if (inRange)
		{
			transform.localPosition = new Vector3(transform.localPosition.x, groundHeight, 0f);

		}
		else
		{
			transform.localPosition = new Vector3(transform.localPosition.x, belowGroundHeight, 0f);
		}

		PlayAnimation("Spike Ready");
	}

	IEnumerator RaiseSpikesRoutine()
	{
		spikesRising = true;
		yield return PlayAnimationTillDone("Spike Antic");
		spikesRising = false;
		collider.enabled = true;
		yield return PlayAnimationTillDone("Spike Up");
	}

	public void RaiseSpikes()
	{
		if (!inRange)
		{
			return;
		}
		StartCoroutine(RaiseSpikesRoutine());

	}

	IEnumerator LowerSpikesRoutine(bool resetLocalX, float originalLocalX)
	{
		collider.enabled = false;
		yield return PlayAnimationTillDone("Spike Down");
		renderer.enabled = false;
		if (resetLocalX)
		{
			transform.SetXLocalPosition(originalLocalX);
		}
	}

	IEnumerator SpikesCancelRoutine(bool resetLocalX, float originalLocalX)
	{
		collider.enabled = false;
		yield return PlayAnimationTillDone("Spike Cancel");
		renderer.enabled = false;
		if (resetLocalX)
		{
			transform.SetXLocalPosition(originalLocalX);
		}
	}

	public void LowerSpikes(bool resetLocalX, float originalLocalX)
	{
		if (!inRange)
		{
			return;
		}
		if (spikesRising)
		{
			StopAllCoroutines();
			StartCoroutine(SpikesCancelRoutine(resetLocalX, originalLocalX));
		}
		else
		{
			StartCoroutine(LowerSpikesRoutine(resetLocalX, originalLocalX));
		}
	}

	IEnumerator PlayAnimationTillDone(string animationStateName)
	{
		animator.Play(animationStateName);

		yield return null;

		var stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		yield return new WaitForSeconds(stateInfo.length / stateInfo.speed);
	}

	void PlayAnimation(string animationStateName)
	{
		animator.Play(animationStateName);
	}
}
