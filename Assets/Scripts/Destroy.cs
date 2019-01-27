using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
	public float expiryTime = 0;
	private float killTime;

	private void Start()
	{
		killTime = Time.time + expiryTime;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Player"))
		    Destroy(gameObject);
	}


	void Update ()
	{
		if (Time.time >= killTime)
			shipControl.playerBulls.Remove(gameObject);
		Destroy(gameObject, expiryTime);
	}
}
