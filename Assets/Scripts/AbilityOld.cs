﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityType
{
	Default,
	ArmorDrain,
	ArmorRegen,
	SpawnSwarm,
	MoveSwarm,
	ShieldProject,
	HealField,
	Chain
}

[System.Serializable]
public class AbilityOld
{
	[SerializeField]
	private AbilityType type;
	[System.NonSerialized]
	[HideInInspector]
	public GameRules gameRules;

	[System.NonSerialized]
	[HideInInspector]
	public bool isActive;

	[HideInInspector]
	public Effect_Point pointEffect; // Ability VFX and SFX at a particular position
	[HideInInspector]
	public Unit user;
	[HideInInspector]
	public AbilityTarget target;
	[HideInInspector]
	public List<Unit> unitList; // If ability involves finding multiple units, they are stored in this field
	[System.NonSerialized]
	[HideInInspector]
	public bool displayAsUnusable = false;

	[System.NonSerialized]
	[HideInInspector]
	public float curEnergy = 1; // How long toggle can continue to operate
	[System.NonSerialized]
	[HideInInspector]
	public float curCooldown = 0; // Cooldown for reactivating / deactivating ability
	[System.NonSerialized]
	[HideInInspector]
	public int stacks = 0;
	[System.NonSerialized]
	[HideInInspector]
	public float pool = 0; // Arbitrary float pool which sits alongside energy pool

	public void Init(Unit user, GameRules gameRules) // Handled by ability component Start method
	{
		// Set user and game rules to use later
		this.user = user;
		this.gameRules = gameRules;

		// Spawn ability effect prefab
		AbilityUtils.InitAbility(this);
	}

	public void Activate(AbilityTarget target) // 
	{
		this.target = target;
		AbilityUtils.StartAbility(this);
	}

	public void End() // Handled by End in ability component
	{
		AbilityUtils.EndAbility(this);
	}

	public void AbilityTick()
	{
		AbilityUtils.TickAbility(this);
	}

	public AbilityType GetAbilityType()
	{
		return type;
	}
}

public static class AbilityUtils
{
	public static void InitAbility(AbilityOld ability)
	{
		switch (ability.GetAbilityType())
		{
			case AbilityType.ArmorDrain:
				{
					//GameObject go = Object.Instantiate(Resources.Load(ability.GetAbilityType().ToString() + "Effect") as GameObject, ability.user.transform.position, Quaternion.identity);
					//ability.pointEffect = go.GetComponent<Effect_Point>();
				}
				break;
			case AbilityType.SpawnSwarm:
				{
					ability.stacks = ability.gameRules.ABLYswarmMaxUses;
					//ability.unitList.Add(ability.user);
				}
				break;
			case AbilityType.MoveSwarm:
				{
					ability.stacks = -1;
					ability.displayAsUnusable = true;
				}
				break;
			case AbilityType.ShieldProject:
				{
					ability.pool = ability.gameRules.ABLYshieldProjectMaxPool;
				}
				break;
			case AbilityType.HealField:
				{
					Ability_HealField healManager = ability.user.GetComponent<Ability_HealField>();
					healManager.SetAbility(ability);
				}
				break;
			default:
				break;
		}

		if (ability.pointEffect)
			ability.pointEffect.SetEffectActive(ability.isActive);
	}

	public static void StartAbility(AbilityOld ability)
	{
		// Can't be used yet
		if (ability.curCooldown > 0)
			return;
			
		ability.curCooldown = 1;

		if (GetActivationStyle(ability.GetAbilityType()) == 1)
		{
			ability.isActive = true;
			if (ability.pointEffect)
				ability.pointEffect.SetEffectActive(true);
		}
		else if (GetActivationStyle(ability.GetAbilityType()) == 2)
		{
			ability.isActive = !ability.isActive;
			if (ability.pointEffect)
				ability.pointEffect.SetEffectActive(ability.isActive);
		}
			

		

		switch (ability.GetAbilityType()) // Ability is off cooldown
		{
			case AbilityType.ArmorDrain:
				{
					Ability_ArmorDrain armorDrain = ability.user.GetComponent<Ability_ArmorDrain>();
					armorDrain.UseAbility(null); // No target needed
				}
				break;
			case AbilityType.SpawnSwarm:
				{
					if (ability.stacks > 0) // If we have swarms in reserve, spawn one and tell all swarms to move
					{
						if (ability.stacks == ability.gameRules.ABLYswarmMaxUses)
						{
							/*
							foreach (AbilityOld a in ability.user.abilities)
							{
								if (a.GetAbilityType() == AbilityType.MoveSwarm)
								{
									a.stacks = 0;
									a.displayAsUnusable = false;
								}
							}
							*/
						}

						Ability_Swarming swarmManager = ability.user.GetComponent<Ability_Swarming>();

						swarmManager.UseAbility(ability.target);

						ability.stacks--;
						//swarmManager.SpawnSwarm();
					}

					if (ability.stacks == 0)
						ability.displayAsUnusable = true;
				}
				break;
			case AbilityType.MoveSwarm:
				{
					if (ability.stacks == 0) // Marked as being good to go
					{
						Ability_Swarming swarming = ability.user.GetComponent<Ability_Swarming>();
						swarming.UseAbility(ability.target);
					}
				}
				break;
			case AbilityType.Default:
				Object.Instantiate(Resources.Load(ability.GetAbilityType().ToString() + "Effect") as GameObject, ability.user.transform.position, Quaternion.identity);
				ability.user.DamageSimple(ability.user.GetHP().y * 0.8f, 0);
				break;
			case AbilityType.ShieldProject:
				{
					Ability_ShieldProject shieldProject = ability.user.GetComponent<Ability_ShieldProject>();
					shieldProject.UseAbility(ability.target);
				}
				break;
			case AbilityType.HealField:
				{
					Ability_HealField healField = ability.user.GetComponent<Ability_HealField>();
					healField.UseAbility(null);
				}
				break;
			case AbilityType.Chain:
				{
					Ability_Chain chain = ability.user.GetComponent<Ability_Chain>();
					chain.UseAbility(ability.target);
				}
				break;
			default:
				break;
		}
	}

	public static void TickAbility(AbilityOld ability)
	{
		ability.curCooldown = ability.curCooldown - Time.deltaTime * GetDeltaDurations(ability.GetAbilityType()).x; // Restore cooldown

		if (ability.isActive && ability.curEnergy < 0) // Out of energy
		{
			ability.isActive = false;
			if (ability.pointEffect)
				ability.pointEffect.SetEffectActive(ability.isActive);

			return;
		}

		if (!ability.isActive) // If ability is not being used. Passives are always off
		{
			
			if (GetActivationStyle(ability.GetAbilityType()) == 2) // If it's a toggle ability
				ability.curEnergy = Mathf.Min(ability.curEnergy + Time.deltaTime * GetDeltaDurations(ability.GetAbilityType()).z, 1); // Restore energy according to its reset duration


			switch (ability.GetAbilityType())
			{
				case AbilityType.ArmorDrain:
					{
					}
					break;
				case AbilityType.ArmorRegen: // Regenerate armor over time based on missing armor
					{
						int regenIndex = Mathf.Max(Mathf.CeilToInt(5 * (1 - (ability.user.GetHP().z / ability.user.GetHP().w))) - 1, 0);
						ability.user.DamageSimple(0, -ability.gameRules.ABLYarmorRegenHPS[regenIndex] * Time.deltaTime);
					}
					break;
				
				default:
					break;
			}
			return; // Leave, ability isn't being used
		}

		if (GetActivationStyle(ability.GetAbilityType()) == 2) // If it's a toggle ability
		{
			ability.curEnergy -= Time.deltaTime * GetDeltaDurations(ability.GetAbilityType()).y; // Consume energy
		}

		// Active ability tick
		switch (ability.GetAbilityType())
		{
			case AbilityType.ShieldProject:
				{
					/*
					// TODO: Fix pool zeroing bug!
					if (ability.unitList.Count > 0)
					{
						if (ability.unitList[0] == ability.user) // Regenerate pool rapidly while shield is inacive
						{
							ability.pool = Mathf.Clamp(ability.pool + ability.gameRules.ABLYshieldProjectInactiveGPS * Time.deltaTime, 0, ability.gameRules.ABLYshieldProjectMaxPool);
						}
						else if (!ability.unitList[0]) // If the shield is active but our charge accidentally dies, self-cast
						{
							ability.target.unit = ability.user; // Avoid manually setting target if possible, but necessary here
							StartAbility(ability);
						}
					}
					*/
				}
				break;
			case AbilityType.SpawnSwarm:
				{
					if (ability.stacks < ability.gameRules.ABLYswarmMaxUses) // If we've already used the ability (so we should have a target already set)
					{
						Ability_Swarming swarming = ability.user.GetComponent<Ability_Swarming>();
						if (!swarming.GetTarget().unit) // Self-cast if the target dies
						{
							swarming.UseAbility(new AbilityTarget(ability.user));
						}
					}
				}
				break;
			default:
				break;
		}
	}

	public static void EndAbility(AbilityOld ability)
	{
		switch (ability.GetAbilityType())
		{
			case AbilityType.ArmorDrain:
				{
					Ability_ArmorDrain armorDrain = ability.user.GetComponent<Ability_ArmorDrain>();
					armorDrain.End();
				}
				break;
			case AbilityType.ShieldProject:
				{
					Ability_ShieldProject shieldProject = ability.user.GetComponent<Ability_ShieldProject>();
					shieldProject.End();
				}
				break;
			case AbilityType.HealField:
				{
					Ability_HealField healManager = ability.user.GetComponent<Ability_HealField>();
					healManager.End();
				}
				break;
			case AbilityType.SpawnSwarm:
				{
					Ability_Swarming swarmManager = ability.user.GetComponent<Ability_Swarming>();
					swarmManager.End();
				}
				break;
			case AbilityType.Chain:
				{
					Ability_Chain chainManager = ability.user.GetComponent<Ability_Chain>();
					chainManager.End();
				}
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// 0 for passive, 1 for active instant, 2 for active toggle
	/// </summary>
	public static int GetActivationStyle(AbilityType type) // 0 = passive, 1 = instant, 2 = toggle
	{
		switch (type)
		{
			case AbilityType.ArmorRegen:
				return 0;
			default:
				return 1; // Most abilities are instant actives
		}
	}

	// All default to 1 second
	/// <summary>
	/// X = Cooldown, Y = Active Duration, Z = Reset Duration
	/// </summary>
	public static Vector3 GetDeltaDurations(AbilityType type)
	{
		switch (type)
		{
			case AbilityType.ArmorDrain:
				return GetDeltaOf(new Vector3(2.0f, 15.0f, 30.0f));
			case AbilityType.SpawnSwarm:
				return GetDeltaOf(new Vector3(5.0f, 0, 0));
			case AbilityType.MoveSwarm:
				return GetDeltaOf(new Vector3(0.5f, 0, 0));
			default:
				return GetDeltaOf(new Vector3());
		}
	}

	// Converts durations to multipliers per second
	static Vector3 GetDeltaOf(Vector3 vector)
	{
		// Avoid division by zero
		if (vector.x == 0)
			vector.x = 1;
		if (vector.y == 0)
			vector.y = 1;
		if (vector.z == 0)
			vector.z = 1;

		return new Vector3(1.0f / vector.x, 1.0f / vector.y, 1.0f / vector.z);
	}

	// 0 = no, 1 = unit, 2 = position
	public static int GetTargetRequirement(AbilityType type)
	{
		switch (type)
		{
			case AbilityType.ArmorDrain:
				return 0;
			case AbilityType.SpawnSwarm:
				return 1;
			case AbilityType.MoveSwarm:
				return 1;
			case AbilityType.ShieldProject:
				return 1;
			case AbilityType.Chain:
				return 1;
			default:
				return 0;
		}
	}

	// Display name of ability
	public static string GetAbilityName(AbilityType type)
	{
		switch (type)
		{
			case AbilityType.ArmorDrain:
				return "Disintegrate";
			case AbilityType.SpawnSwarm:
				return "Deploy Swarm";
			default:
				return "default";
		}
	}

	// Display icon of ability
	public static Sprite GetAbilityIcon(AbilityType type)
	{
		Sprite sprite = Resources.Load<Sprite>("IconAbility_" + type);
		if (sprite)
			return sprite;
		else
			return Resources.Load<Sprite>("IconEmpty");
	}

	// Secondary display icon of ability
	public static Sprite GetSecondaryAbilityIcon(AbilityType type, int stacks)
	{
		bool loadSprite = true;

		switch (type)
		{
			case AbilityType.HealField:
				{
					if (stacks == 0) // Not borrowing, don't show secondary icon
						loadSprite = false;
				}
				break;
		}

		Sprite sprite = null;
		if (loadSprite)
			 sprite = Resources.Load<Sprite>("IconAbility_" + type + "_B");

		if (sprite)
			return sprite;
		else
			return Resources.Load<Sprite>("IconEmpty");
	}

	static Unit GetUnitFromCol(Collider col)
	{
		Entity ent = col.GetComponentInParent<Entity>();
		if (ent)
		{
			if (ent.GetType() == typeof(Unit) || ent.GetType().IsSubclassOf(typeof(Unit)))
				return (Unit)ent;
			else
				return null;
		}
		else
		{
			return null;
		}
	}
}
