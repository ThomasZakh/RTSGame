﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusType
{
	SwarmResist,
	CriticalBurnImmune,
	SpawnSwarmSpeedNerf,
	SuperlaserMark,
	ArmorMelt,
	SelfDestructSpeedBuff,
	IonSuppressed,
	IonStunned,
	ModeSpeedNerf
}

[System.Serializable]
public class Status
{
	public GameObject from;
	public StatusType statusType;
	[System.NonSerialized]
	private float timeLeft = 1;

	public Status(GameObject g, StatusType statType)
	{
		from = g;
		statusType = statType;
		timeLeft = 1;
	}

	public bool UpdateTimeLeft(float delta)
	{
		timeLeft -= delta * (1 / StatusUtils.GetDuration(statusType));
		if (timeLeft < 0)
			return false;
		else
			return true;
	}

	public void RefreshTimeLeft()
	{
		timeLeft = 1;
	}

	public float GetTimeLeft()
	{
		return timeLeft;
	}

	public void SetTimeLeft(float time)
	{
		timeLeft = time;
	}

	public void AddTimeLeft(float time)
	{
		timeLeft += time;
	}
}

public static class StatusUtils
{
	public static float GetDuration(StatusType statType)
	{
		switch (statType)
		{
			case StatusType.ArmorMelt:
				return 5;
			case StatusType.IonSuppressed:
				return 5;
			case StatusType.IonStunned:
				return 10;
			default:
				return 1;
		}
	}

	public static bool ShouldCountDownDuration(StatusType statType)
	{
		switch (statType)
		{
			case StatusType.SpawnSwarmSpeedNerf:
				return false;
			case StatusType.SuperlaserMark:
				return false;
			case StatusType.SelfDestructSpeedBuff:
				return false;
			case StatusType.ModeSpeedNerf:
				return false;
			default:
				return true;
		}
	}

	public static bool ShouldStackDuration(StatusType statType)
	{
		switch (statType)
		{
			case StatusType.SuperlaserMark:
				return true;
			default:
				return false;
		}
	}

	public static bool ShouldSuspendAbilities(StatusType statType)
	{
		switch (statType)
		{
			case StatusType.IonSuppressed:
				return true;
			case StatusType.IonStunned:
				return true;
			default:
				return false;
		}
	}

	public static bool ShouldStun(StatusType statType)
	{
		switch (statType)
		{
			case StatusType.IonStunned:
				return true;
			default:
				return false;
		}
	}

	public static string GetDisplayName(StatusType statType)
	{
		switch (statType)
		{
			case StatusType.SwarmResist:
				return "Fighter Support";
			case StatusType.CriticalBurnImmune:
				return "Burn Immune";
			case StatusType.SpawnSwarmSpeedNerf:
				return "Hangars Open";
			case StatusType.SuperlaserMark:
				return "Hellrazor Mark";
			case StatusType.ArmorMelt:
				return "Disintegration";
			case StatusType.IonSuppressed:
				return "Suppressed";
			case StatusType.IonStunned:
				return "Disabled";
			default:
				return "default";
		}
	}

	public static string GetDisplayDesc(StatusType statType)
	{
		switch (statType)
		{
			case StatusType.SwarmResist:
				return "Allied fighters will absorb some of all incoming damage and guard this unit from enemy fighters.";
			case StatusType.CriticalBurnImmune:
				return "Even if this unit's health is below the burn threshold, it will not take burn damage over time.";
			case StatusType.SpawnSwarmSpeedNerf:
				return "Once all fighters have been deployed, hangars will close and engines will return to full power.";
			case StatusType.SuperlaserMark:
				return "Marked for reactor radiation collection by an enemy Hellrazor cannon.";
			case StatusType.ArmorMelt:
				return "Armor is weakened by corrosive chemicals.";
			case StatusType.IonSuppressed:
				return "Unit's special ability systems are offline.";
			case StatusType.IonStunned:
				return "Unable to move, fire turrets, or use special abilities.";
			default:
				return "default";
		}
	}

	public static bool ShouldDisplay(StatusType statType)
	{
		switch (statType)
		{
			case StatusType.SwarmResist:
				return true;
			case StatusType.SpawnSwarmSpeedNerf:
				return true;
			case StatusType.ArmorMelt:
				return true;
			case StatusType.IonSuppressed:
				return true;
			case StatusType.IonStunned:
				return true;
			default:
				return false;
		}
	}

	/// <summary>
	/// [0] is bkg color, [1] is icon color
	/// </summary>
	/// <param name="statType"></param>
	/// <returns></returns>
	public static Color[] GetDisplayColors(StatusType statType)
	{
		switch (statType)
		{
			case StatusType.SwarmResist:
				return new Color[] { new Color32(0x90, 0x11, 0x11, 0xFF), new Color32(0xFF, 0x70, 0x88, 0xFF) };
			case StatusType.SpawnSwarmSpeedNerf:
				return new Color[] { new Color32(0x90, 0x11, 0x11, 0xFF), new Color32(0xFF, 0x70, 0x88, 0xFF) };
			case StatusType.ArmorMelt:
				return new Color[] { new Color32(0x61, 0x61, 0x61, 0xFF), new Color32(0xFF, 0xFF, 0xFF, 0xFF) };
			case StatusType.IonSuppressed:
				return new Color[] { new Color32(0x6D, 0xE5, 0xE5, 0xFF), new Color32(0xFF, 0xFF, 0xFF, 0xFF) };
			case StatusType.IonStunned:
				return new Color[] { new Color32(0x6D, 0xE5, 0xE5, 0xFF), new Color32(0xFF, 0xFF, 0xFF, 0xFF) };
			default:
				return new Color[0];
		}
	}

	public static Sprite GetDisplayIcon(StatusType type)
	{
		Sprite sprite = Resources.Load<Sprite>("IconStatus_" + type);
		if (sprite)
			return sprite;
		else
			return Resources.Load<Sprite>("IconEmpty");
	}
}