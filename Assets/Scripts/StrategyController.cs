﻿using UnityEngine;
using System.Collections;

public class StrategyController : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private float rotSpeed = 100;
	private Quaternion initRot;
	[SerializeField]
	private float speed = 10;
	//[SerializeField]
	//private float zoomSpeed = 1;
	[SerializeField]
	private int screenBorderSize = 50;

	[Header("Objects")]
	[SerializeField]
	private Camera cam; // Guess
	[SerializeField]
	private GameObject camRoot; // Cam parent

	// Use this for initialization
	void Start ()
	{
		initRot = cam.transform.rotation;
	}
	
	// Update is called once per frame
	void Update ()
	{
		float alt = Input.GetAxis("Altitude");
		camRoot.transform.Translate(new Vector3(0, alt * speed * Time.deltaTime, 0), Space.Self);

		cam.transform.position = camRoot.transform.position;

		Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
		if (true/*screenRect.Contains(Input.mousePosition)*/)
		{
			Vector3 velocityVector = Vector3.zero;

			if (Input.mousePosition.x < screenBorderSize)
				velocityVector.x = -speed * Time.deltaTime;
			else if (Input.mousePosition.x > Screen.width - screenBorderSize)
				velocityVector.x = speed * Time.deltaTime;

			if (Input.mousePosition.y < screenBorderSize)
				velocityVector.z = -speed * Time.deltaTime;
			else if (Input.mousePosition.y > Screen.height - screenBorderSize)
				velocityVector.z = speed * Time.deltaTime;

			camRoot.transform.Translate(velocityVector, Space.Self);
		}

		/*// TODO: Zooming
		float zoom = Input.GetAxis("Zoom");
		cam.transform.Translate(new Vector3(0, 0, zoom * zoomSpeed), Space.Self);
		*/

		float rotAxis = Input.GetAxis("Rotate");
		cam.transform.Rotate(new Vector3(0, rotAxis * rotSpeed * Time.deltaTime, 0), Space.World);
		Transform camPam = cam.transform.parent;
		cam.transform.SetParent(null);
		camRoot.transform.Rotate(new Vector3(0, rotAxis * rotSpeed * Time.deltaTime, 0), Space.World);

		if (Input.GetButtonDown("ResetRotate"))
		{
			cam.transform.rotation = initRot;
			camRoot.transform.rotation = Quaternion.identity;
		}
	}
}
