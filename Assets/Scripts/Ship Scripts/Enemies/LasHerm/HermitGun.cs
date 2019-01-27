using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HermitGun : MonoBehaviour
{
	//vars for rotating Gun
	public float rotSpeed;
	public Transform ship;
	private Quaternion lookRotation;
	private Vector2 pointD;
	private float turnAngle;

	//vars for moving Gun
	public Transform parentHerm;

	//vars for shooting bullets
	public GameObject bullet;
	public float boltSpeed;
	public float shootRate;
	private float shootRateTimeStamp = 0;

	void Update ()
	{
		Vector3 ang = parentHerm.eulerAngles;
		pointD = ship.position - transform.position;
		turnAngle = Mathf.Atan2(pointD.y, pointD.x) * Mathf.Rad2Deg;
		lookRotation = Quaternion.AngleAxis(turnAngle + 90, transform.forward);
		//set rotation
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotSpeed * Time.deltaTime);
		//set position
		transform.position = new Vector2(parentHerm.position.x + (1.3507f * Mathf.Cos((ang.z + 90f) * Mathf.Deg2Rad)), parentHerm.position.y + (1.3507f * Mathf.Sin((ang.z + 90f) * Mathf.Deg2Rad)));
	}

	private void FixedUpdate()
	{
		Vector3 ang = transform.eulerAngles;

		if (Time.time > shootRateTimeStamp)
		{

			GameObject hermLaser = (GameObject)Instantiate(bullet, new Vector2(transform.position.x + (10.8f * Mathf.Cos((ang.z - 90) * Mathf.Deg2Rad)), transform.position.y + (10.8f * Mathf.Sin((ang.z - 90) * Mathf.Deg2Rad))), transform.rotation);
			hermLaser.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sin(-ang.z * Mathf.Deg2Rad), Mathf.Cos(ang.z * Mathf.Deg2Rad)) * -boltSpeed;

			shootRateTimeStamp = shootRate + Time.time;
		}
	}
}
