using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldOverhead : MonoBehaviour {
	
	public Transform shieldHolder;
	public GameObject shield1;
	public GameObject shield2;
	public GameObject shield3;
	public GameObject shield4;
	private GameObject[] shields;
	public static float shieldHealth;
	private int shieldCap = 3;
	public bool shieldActive;

	void Start () 
	{
		shields = new GameObject[] {shield1, shield2, shield3, shield4};
		shieldHealth = 300;
		shieldActive = true;
	}

	void Update () 
	{
		if (shieldActive)
		{
			float d = Input.GetAxis("Mouse ScrollWheel");
			if (d > 0 && shieldCap < 3) 
			{
				shieldCap++;
				shields[shieldCap].SetActive(true);
			} else if (d < 0 && shieldCap >= 1) 
			{
				shields[shieldCap].SetActive(false);
				shieldCap--;
			}
				
			if (shieldHealth <= 0) 
				deactivateShield();
		}

		shieldHolder.rotation = transform.rotation;
		shieldHolder.position = transform.position;
	}

	public void hitByBullet() {
		shieldHealth -= 100;
	}

	private void deactivateShield() 
	{
		for (int i = 0; i <= 3; i++) {
			shields[i].SetActive(false);
		}
		shieldActive = false;
	}
}
