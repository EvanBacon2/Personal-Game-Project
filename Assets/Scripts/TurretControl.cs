using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretControl : MonoBehaviour
{
	public float shootRate;
	private float shootRateTimeStamp = 0;
	public GameObject enemyBullet;
	public Transform Turret;
	public float enBulletSpeedX;
	public float enBulletSpeedY;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if (Time.time > shootRateTimeStamp)
		{
			GameObject enBullet = (GameObject)Instantiate(enemyBullet, Turret.position, Turret.rotation);
			enBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(enBulletSpeedX * Time.deltaTime, enBulletSpeedY * Time.deltaTime);
			shootRateTimeStamp = shootRate + Time.time;
		}
	}
}
