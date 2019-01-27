using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
	public Vector3 mousePos;

	void Start ()
	{
		mousePos = Camera.main.WorldToScreenPoint(Input.mousePosition);//Input.mousePosition;
		mousePos.z = -10;
		transform.position = new Vector2(mousePos.x, mousePos.y);
	}
	
	
	void Update ()
	{
		mousePos = Camera.main.WorldToScreenPoint(Input.mousePosition);// Input.mousePosition;
		transform.position = new Vector2(mousePos.x, mousePos.y);
	}
}
