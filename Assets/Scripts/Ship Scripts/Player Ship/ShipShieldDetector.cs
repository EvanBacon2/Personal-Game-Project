using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipShieldDetector : MonoBehaviour {	

	public ShieldOverhead overhead;
	public bool called = false;
	public int test = 0;

	void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag("Enemy Bullet"))
		{
			overhead.hitByBullet();
			Destroy(other);
			test++;
		}
	}
}
