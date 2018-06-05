﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildButton : MonoBehaviour
{
	[SerializeField]
	private int team;
	private int index;

	[SerializeField]
	private Text costText;
	private int costCur;
	[SerializeField]
	private Text countText;
	private int countCur;
	[SerializeField]
	private Button button;

	private bool[] interactable; // [0] false = not enough resources, [1] false = not enough counter

	//[SerializeField]
	//private Image borderFill;

	//private UIRules uiRules;
	private Manager_Game gameManager;

	private Commander commander;

	void Start()
	{
		//uiRules = GameObject.FindGameObjectWithTag("UIManager").GetComponent<Manager_UI>().UIRules;
		gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Manager_Game>();
		UpdateCommander();

		interactable = new bool[2];
	}

	public void SetIndex(int i)
	{
		index = i;
	}

	public void SetTeam(int t)
	{
		team = t;
		UpdateCommander();
	}

	void UpdateCommander()
	{
		commander = gameManager.Commanders[team];
	}

	public void Build()
	{
		commander.BuildButton(index);
	}

	public void CheckInteractable()
	{
		if (costCur > commander.GetResources())
			interactable[0] = false;
		else
			interactable[0] = true;

		if (countCur >= commander.GetBuildUnit(index).unitCap)
			interactable[1] = false;
		else
			interactable[1] = true;

		button.interactable = ButtonInteractable();
	}

	public void SetCost()
	{
		costCur = commander.GetBuildUnit(index).cost;
		costText.text = costCur.ToString();
		CheckInteractable();
	}

	public void SetCounter(int cur)
	{
		countCur = cur;
		countText.text = countCur + "/" + commander.GetBuildUnit(index).unitCap;
		CheckInteractable();
	}

	bool ButtonInteractable()
	{
		for (int i = 0; i < interactable.Length; i++)
		{
			if (interactable[i] == false)
				return false;
		}
		return true;
	}
}
