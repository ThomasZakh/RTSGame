﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Particle = UnityEngine.ParticleSystem.Particle;

public class Ability : MonoBehaviour {
	protected AbilityType abilityType; // Used to determine how to interact with this Ability
	protected int team; // Doesn't need to be public

	// Common primtive data
	protected int abilityIndex = -1;
	protected float cooldownDelta = 1;
	protected float cooldownTimer = 0; // Tracks when ability can be used again next
	protected bool offCooldown = true;
	protected int stacks = 0; // Counter used for tracking ability level, uses left, etc.

	// Common display data
	protected AbilityDisplayInfo displayInfo;

	// Common object data
	//protected AbilityTarget target;
	protected GameRules gameRules;
	protected Unit parentUnit;

	// Use this for initialization
	protected void Start ()
	{
		parentUnit = GetComponent<Unit>();
		team = parentUnit.team;
		abilityIndex = parentUnit.abilities.IndexOf(this);

		gameRules = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Manager_Game>().GameRules;

		displayInfo = new AbilityDisplayInfo();
	}

	protected void Update()
	{
		if (!offCooldown)
		{
			if (!gameRules.useTestValues)
				cooldownTimer -= cooldownDelta * Time.deltaTime;
			else
				cooldownTimer -= cooldownDelta * (1f / gameRules.TESTtimeMult) * Time.deltaTime;

			if (cooldownTimer <= 0)
				offCooldown = true;
			Display();
		}
	}

	void Display()
	{
		if (displayInfo.displayFill)
			return;

		displayInfo.cooldown = cooldownTimer;
		UpdateDisplay(abilityIndex, false);
	}
	
	public virtual void UseAbility(AbilityTarget targ)
	{
		//target = targ;
		StartCooldown();
	}

	// For abilities that want customized ability cast logic
	protected void StartCooldown()
	{
		cooldownTimer = 1;
		Display();
		offCooldown = false;
	}

	// For abilities that want customized ability cast logic
	protected void ResetCooldown()
	{
		cooldownTimer = 0;
		Display();
		offCooldown = true;
	}

	public virtual void End()
	{
	}

	protected virtual void UpdateAbilityBar()
	{

	}

	protected virtual void UpdateDisplay(int index, bool updateStacks)
	{
		parentUnit.UpdateAbilityDisplay(index, updateStacks);
	}

	protected virtual void InitCooldown()
	{
		cooldownDelta = AbilityUtils.GetDeltaDurations(abilityType).x;
	}

	/*public AbilityTarget GetTarget()
	{
		return target;
	}*/

	public AbilityType GetAbilityType()
	{
		return abilityType;
	}

	public AbilityDisplayInfo GetDisplayInfo()
	{
		return displayInfo;
	}
}

public class AbilityDisplayInfo
{
	public bool displayInactive = false;
	public bool displayStacks = false;
	public float cooldown = 0;
	public bool displayFill = false;
	public int stacks = 0;
	public float fill = 0;
}

public enum AbilityType
{
	Default,
	ArmorDrain,
	ArmorRegen,
	SpawnSwarm,
	MoveSwarm,
	ShieldProject,
	HealField,
	Chain,
	Superlaser
}

public static class AbilityUtils
{
	// All default to 1 second
	/// <summary>
	/// X = Cooldown, Y = Active Duration, Z = Reset Duration
	/// </summary>
	public static Vector3 GetDeltaDurations(AbilityType type)
	{
		switch (type)
		{
			case AbilityType.ArmorDrain:
				return GetDeltaOf(new Vector3(2, 10, 20));
			case AbilityType.SpawnSwarm:
				return GetDeltaOf(new Vector3(10, 0, 0));
			case AbilityType.MoveSwarm:
				return GetDeltaOf(new Vector3(0.5f, 0, 0));
			case AbilityType.ShieldProject:
				return GetDeltaOf(new Vector3(10, 0, 0));
			case AbilityType.HealField:
				return GetDeltaOf(new Vector3(2, 0, 0));
			case AbilityType.Chain:
				return GetDeltaOf(new Vector3(20, 0, 0));
			case AbilityType.Superlaser:
				return GetDeltaOf(new Vector3(40, 0, 0));
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
			case AbilityType.SpawnSwarm:
				return 1;
			case AbilityType.MoveSwarm:
				return 1;
			case AbilityType.ShieldProject:
				return 1;
			case AbilityType.Chain:
				return 1;
			case AbilityType.Superlaser:
				return 1;
			default:
				return 0;
		}
	}

	// Display name of ability
	public static string GetDisplayName(AbilityType type)
	{
		switch (type)
		{
			case AbilityType.ArmorDrain:
				return "Armor Well";
			case AbilityType.SpawnSwarm:
				return "Deploy Fighters";
			case AbilityType.MoveSwarm:
				return "Order Fighters";
			case AbilityType.ShieldProject:
				return "Project Shield";
			case AbilityType.HealField:
				return "Metasteel Pool";
			case AbilityType.Chain:
				return "Gravity Chain";
			case AbilityType.Superlaser:
				return "Hellrazor";
			default:
				return "default";
		}
	}

	// Display icon of ability
	public static Sprite GetDisplayIcon(AbilityType type)
	{
		Sprite sprite = Resources.Load<Sprite>("IconAbility_" + type);
		if (sprite)
			return sprite;
		else
			return Resources.Load<Sprite>("IconEmpty");
	}

	// Secondary display icon of ability
	public static Sprite GetDisplayIconSecondary(AbilityType type, int stacks)
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
}