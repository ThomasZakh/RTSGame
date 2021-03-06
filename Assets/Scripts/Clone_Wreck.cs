﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clone_Wreck : MonoBehaviour
{
	[SerializeField]
	private GameObject startEffect;
	[SerializeField]
	private GameObject shatterEffect;

	private float mass;

	[SerializeField]
	private float spinSpeed = 15;
	private Vector3 angularVelocity;

	private float curFallSpeed = 0;
	private Vector3 velocity; // TODO: Add final velocity of source unit to initial velocity of wreck

	private Coroutine destroyCoroutine;

	private GameRules gameRules;

	void Awake()
	{
		gameRules = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Manager_Game>().GameRules;
	}

	void Start ()
	{
		Instantiate(startEffect, transform.position, Quaternion.identity);
		

		destroyCoroutine = StartCoroutine(DestroyCoroutine());

		angularVelocity = new Vector3(RandomValue(), RandomValue(), RandomValue()) * spinSpeed;
	}

	float RandomValue()
	{
		return Random.value * 2 - 1;
		//return 1;
	}

	public void SetMass(float health, float armor)
	{
		mass = health * gameRules.WRCK_massHealthMult + armor * gameRules.WRCK_massArmorMult;
	}

	public void SetHVelocity(Vector3 vel)
	{
		velocity = new Vector3(vel.x, 0, vel.z) * gameRules.WRCK_initialVelMult;
	}

	void Update()
	{
		transform.Rotate(angularVelocity * (curFallSpeed / gameRules.WRCK_fallSpeedMax) * Time.deltaTime);

		curFallSpeed = Mathf.Clamp(curFallSpeed + gameRules.WRCK_fallSpeedAccel * Time.deltaTime, -gameRules.WRCK_fallSpeedMax, gameRules.WRCK_fallSpeedMax);
		velocity.y = -curFallSpeed;
		transform.Translate(velocity * Time.deltaTime, Space.World);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.transform.position.y >= transform.position.y) // They have to be below us
			return;

		Unit unit = other.GetComponentInParent<Unit>();
		if (unit)
		{
			unit.Damage(mass, 0, DamageType.Wreck);
			if (unit.GetHP().x > 0) // If we don't kill the unit on impact, shatter against it
				Die(true);
			else // If we do kill the unit, briefly slow down and reset lifetime
			{
				curFallSpeed -= curFallSpeed * gameRules.WRCK_collisionSpeedPenalty;
				StopCoroutine(destroyCoroutine);
				destroyCoroutine = StartCoroutine(DestroyCoroutine());
			}
		}
	}

	IEnumerator DestroyCoroutine()
	{
		yield return new WaitForSeconds(gameRules.WRCK_lifetime);

		Die(false);
	}

	void Die(bool shatter)
	{
		if (shatter)
			Instantiate(shatterEffect, transform.position, Quaternion.identity);

		Effect_Container[] containers = GetComponents<Effect_Container>();
		foreach (Effect_Container container in containers)
			container.End(velocity);

		if (shatter)
			Destroy(gameObject, 0.05f);
		else
			Destroy(gameObject);
	}
}
