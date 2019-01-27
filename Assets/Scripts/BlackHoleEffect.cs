using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleEffect : MonoBehaviour
{
	public GameObject BlackHole;
	private Rigidbody2D rb2d;
	public float pull;
	public float killZone;

	private void Start()
	{
		rb2d = GetComponent<Rigidbody2D>();
		BlackHole = GameObject.Find("Black Hole");
	}


	void FixedUpdate ()
	{
		if (BlackHoleDistX() <= killZone && BlackHoleDistY() <= killZone)
			Destroy(gameObject);
		else if (BlackHoleDist() <= 50 && BlackHoleDistX() > .5 && BlackHoleDistY() > .5)
			rb2d.AddForce(new Vector2(pull * (BlackHole.transform.position.x - gameObject.transform.position.x), pull * (BlackHole.transform.position.y - gameObject.transform.position.y)));
	}

	private double BlackHoleDist()
	{
		return Math.Sqrt(Math.Pow(BlackHoleDistX(), 2) + Math.Pow(BlackHoleDistY(), 2));
	}

	private double BlackHoleDistX()
	{
		return Math.Abs(gameObject.transform.position.x - BlackHole.transform.position.x);
	}

	private double BlackHoleDistY()
	{
		return Math.Abs(gameObject.transform.position.y - BlackHole.transform.position.y);
	}
}
