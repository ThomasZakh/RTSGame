﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Particle = UnityEngine.ParticleSystem.Particle;
using MainModule = UnityEngine.ParticleSystem.MainModule;
using EmitParams = UnityEngine.ParticleSystem.EmitParams;

public class Manager_Hitscan : MonoBehaviour
{
	private LayerMask mask;
	[SerializeField]
	private ParticleSystem pS;
	//private MainModule main;

	private float width = 0.1f;
	private float directionMult = 0.01f;

	private int newProjectilesThisFrame;

	private Manager_VFX vfx;
	private GameRules gameRules;

	void Awake()
	{
		gameRules = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Manager_Game>().GameRules;
		mask = gameRules.entityLayerMask;

		width = pS.main.startSizeX.constant;

		vfx = GameObject.FindGameObjectWithTag("VFXManager").GetComponent<Manager_VFX>();
	}

	public void SpawnHitscan(Hitscan temp, Vector3 position, Vector3 direction, Unit from, Status onHit)
	{
		SpawnHitscan(temp, position, direction, from, onHit, null);
	}

	public void SpawnHitscan(Hitscan temp, Vector3 position, Vector3 direction, Unit from, Status onHit, ITargetable goal)
	{
		Hitscan scan = new Hitscan(temp);
		scan.startPosition = position;
		scan.direction = direction;
		scan.SetFrom(from);
		scan.SetStatus(onHit);
		//hitscans.Add(scan);

		bool noGoal = IsNull(goal);

		// Raycast or do damage immediately. Use actual distance / hit information to inform visuals
		Vector3 dif = noGoal ? Vector3.zero : (position - goal.GetPosition());
		float length = noGoal ? Raycast(scan) : dif.magnitude;
		if (!noGoal) // Has goal, do damage manually
		{
			goal.Damage(scan.GetDamage(), length, scan.GetDamageType());
			vfx.SpawnEffect(VFXType.Hit_Near, position + direction * length, direction, scan.GetFrom().GetTeam());
		}

		Vector3 size = new Vector3(width, length, 1);

		EmitParams param = new EmitParams()
		{
			position = position,
			velocity = direction * directionMult,
			startSize3D = size,
			//startColor = Random.value * Color.red + Random.value * Color.green + Random.value * Color.blue,
			startLifetime = scan.GetLifetime() // 2x just in case. Particles dying prematurely is the worst thing that could happen to this system
		};
		pS.Emit(param, 1);
	}

	// Called immediately after a hitscan is spawned
	float Raycast(Hitscan scan)
	{
		// Raycast according to start position, direction, and range
		RaycastHit hit;
		if (Physics.Raycast(scan.startPosition, scan.direction, out hit, scan.GetRange(), mask))
		{
			int projTeam = scan.GetFrom().team;
			Unit unit = null;
			bool hitSelf = false;
			if (hit.collider.transform.parent) // Is this a unit?
			{
				unit = hit.collider.transform.parent.GetComponent<Unit>();
				if (unit != scan.GetFrom()) // If we hit a unit and its not us, damage it
				{
					Status status = scan.GetStatus();
					if (status != null)
					{
						if (status.statusType == StatusType.SuperlaserMark)
							status.SetTimeLeft(scan.GetDamage()); // Store damage in timeLeft field of status

						unit.AddStatus(status);
					}

					float actualRange = (hit.point - scan.startPosition).magnitude;
					if (unit.team != scan.GetFrom().team) // If we hit an enemy, do full damage
					{
						unit.Damage(scan.GetDamage(), actualRange, scan.GetDamageType());
					}
					else // If we hit an ally, do reduced damage because it was an accidental hit
					{
						unit.Damage(scan.GetDamage() * gameRules.PRJfriendlyFireDamageMult, actualRange, scan.GetDamageType());
					}
				}
				else
				{
					// Ignore this collision
					hitSelf = true; // TODO: Adapt friendly fire code for raycasting here
				}
			}

			Vector3 endPosition = (hit.point - scan.direction * gameRules.PRJhitOffset);

			// Don't do anything if we are passing through the unit that fired us
			if (!hitSelf)
			{
				if (unit)
				{
					if (unit.GetShields().x > 0) // Shielded
						vfx.SpawnEffect(VFXType.Hit_Absorbed, endPosition, -scan.direction, projTeam);
					else // Normal hit
						vfx.SpawnEffect(VFXType.Hit_Normal, endPosition, -scan.direction, projTeam);
				}
				else // Terrain
					vfx.SpawnEffect(VFXType.Hit_Normal, endPosition, -scan.direction, projTeam);
				return (scan.startPosition - endPosition).magnitude; // Return actual length of hitscan
			}
		}//if Raycast
		return scan.GetRange();
	}

	bool IsNull(ITargetable t)
	{
		if ((MonoBehaviour)t == null)
			return true;
		else
			return false;
	}
}