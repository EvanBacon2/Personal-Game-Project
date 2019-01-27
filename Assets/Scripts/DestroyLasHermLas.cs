using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyLasHermLas : MonoBehaviour
{
	public float expiryTime;

	private void LateUpdate()
	{
		Destroy(gameObject, expiryTime);
	}
}
