using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CameraController : MonoBehaviour
{
	public GameObject player;
	private Vector3 offset;
	private Vector3 mouseOffset;

	// Use this for initialization
	void Start()
	{
		transform.position = player.transform.position;
		offset = new Vector3(0, 0, -10);
	}
	
	// Update is called once per frame
	void LateUpdate()
	{
		mouseOffset = new Vector2(100f * ((Input.mousePosition.x - Screen.width / 2) / (Screen.width)), 100f * ((Input.mousePosition.y - Screen.height / 2) / (Screen.height)));
		transform.position = player.transform.position + offset + mouseOffset;
	}
	
}

