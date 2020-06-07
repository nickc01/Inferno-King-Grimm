using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

public class GrimmBall : MonoBehaviour 
{
	[SerializeField]
	AnimationCurve Curve;
	[SerializeField]
	float TweenY;
	[SerializeField]
	float Force;
	[SerializeField]
	float XVelocity;
	[SerializeField]
	float time = 0.7f;
	[SerializeField]
	float maxLifeTime = 10f;

	new Rigidbody2D rigidbody;
	ParticleSystem Smoke;
	ParticleSystem Particles;
	new Collider2D collider;

	Coroutine MainCoroutine;
	Coroutine ShrinkCoroutine;

	void Start () 
	{
		Smoke = transform.Find("Smoke").GetComponent<ParticleSystem>();
		Particles = transform.Find("Particles").GetComponent<ParticleSystem>();
		rigidbody = GetComponent<Rigidbody2D>();
		collider = GetComponent<Collider2D>();
		rigidbody.velocity = new Vector2(XVelocity,0);
		MainCoroutine = StartCoroutine(MainAction());
	}

	IEnumerator MainAction()
	{
		float startY = transform.position.y;
		float clock = 0f;
		do
		{
			yield return null;
			clock += Time.deltaTime;
			//transform.position = transform.position.With(y: Mathf.Lerp(startY,startY + TweenY,Curve.Evaluate(clock / time)));
			transform.SetYPosition(Mathf.Lerp(startY, startY + TweenY, Curve.Evaluate(clock / time)));

			rigidbody.AddForce(new Vector2(Force,0f));

		} while (clock < time);
		do
		{
			yield return null;
			clock += Time.deltaTime;
			rigidbody.AddForce(new Vector2(Force, 0f));
		} while (clock < maxLifeTime);
		ShrinkAndStop();
	}

	void ShrinkAndStop()
	{
		if (MainCoroutine != null)
		{
			StopCoroutine(MainCoroutine);
			MainCoroutine = null;
		}
		if (ShrinkCoroutine == null)
		{
			ShrinkCoroutine = StartCoroutine(ShrinkRoutine());
		}
	}

	IEnumerator ShrinkRoutine()
	{
		Debugger.Log("Stopping");
		Smoke.Stop();
		Particles.Stop();
		collider.enabled = false;
		Vector3 oldScale = transform.localScale;

		float clock = 0f;
		float end = 0.5f;
		do
		{
			yield return null;
			clock += Time.deltaTime;
			transform.localScale = Vector3.Lerp(oldScale,Vector3.zero,clock / time);
			rigidbody.velocity *= 0.85f;
		} while (clock < end);

		Destroy(gameObject);
	}

	public static GrimmBall Spawn(Vector3 position, float tweenY, float force, float xVelocity)
	{
		var instance = Instantiate(ReignitedKingGrimm.Prefabs.GrimmBall, position, Quaternion.identity);
		instance.TweenY = tweenY;
		instance.Force = force;
		instance.XVelocity = xVelocity;
		return instance;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") && ShrinkCoroutine == null)
		{
			ShrinkAndStop();
		}
	}
}
