﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util_TimedDestructor : MonoBehaviour
{
	[SerializeField]
	private float time = 1;

	void Start ()
	{
		Destroy(gameObject, time);
	}
}
