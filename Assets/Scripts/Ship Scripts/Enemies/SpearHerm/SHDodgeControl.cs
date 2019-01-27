using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHDodgeControl : MonoBehaviour {
	
	public Transform Target;
	private bool bLeft;
	private float rad;
	private Vector2[] rayOrigins = new Vector2[360];
	private SpriteRenderer spriteRend;
	public bool check1;
	private int currRay;
	public int startRay;

	// Use this for initialization
	void Start () 
	{
		for (int i = 0; i <= 359; i ++) 
		{
			rayOrigins[i].x = Mathf.Cos((i + 1) * Mathf.Deg2Rad);
			rayOrigins[i].y = Mathf.Sin((i + 1) * Mathf.Deg2Rad);
		}
		spriteRend = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate() 
	{
		if (shipControl.doShoot) 
		{
			float bulletRot = shipControl.playerBulls[shipControl.playerBulls.Count - 1].transform.localEulerAngles.z;
			float currRot = transform.localEulerAngles.z;
			rad = Vector2.Distance(Target.position, transform.position);
			if (do_Dodge(bulletRot, currRot))
			{
				rad = Vector2.Distance(Target.position, transform.position);
				int minGap = Mathf.CeilToInt(findMinGap() * 2);
				//int startRay;
				if (bLeft)
					startRay = Mathf.FloorToInt(currRot - minGap);
				else 
					startRay = Mathf.FloorToInt(currRot + minGap);

				startRay = checkAngle(startRay);

				int nullCounter = 0;
				//int currRay;
				if (bLeft)
					currRay = startRay - 1;
				else
					currRay = startRay + 1;	
				currRay = checkAngle(currRay);
				float destAngle = 400;
				while (destAngle == 400 && currRay != startRay) 
				{
					if (Physics2D.Raycast(rayOrigins[currRay - 1] * rad, rayOrigins[currRay - 1], GetComponent<Collider2D>().bounds.size.y) == false)
						nullCounter++;
					else
						nullCounter = 0;
					
					if (nullCounter == minGap)
						destAngle = currRay - (minGap / 2f);

					if (bLeft)
						currRay--;
					else
						currRay++;
					currRay = checkAngle(currRay);
				}

				if (destAngle != 400)
				{
					float destX = Target.position.x + (rad * Mathf.Cos(destAngle * Mathf.Deg2Rad));
					float destY = Target.position.y + (rad * Mathf.Sin(destAngle * Mathf.Deg2Rad));
					transform.rotation = Quaternion.AngleAxis(destAngle, Vector3.forward);
					transform.position = new Vector2(destX, destY);
					bLeft = false;
					destAngle = 400;
				}
			}
		}
	}

	private float findMinGap()// use to determine whether or not ship is looking at spearHerm
	{
		//the distance between ship and the very front of spearHerm
		float dist = rad - spriteRend.bounds.extents.y;
		float shipWidth = Target.GetComponent<SpriteRenderer>().bounds.extents.x;
		float tempR = Mathf.Sqrt((shipWidth * shipWidth) + dist * dist);
		float minAng = 90 - (Mathf.Acos(dist / tempR) * Mathf.Rad2Deg) + 5;
		return minAng;
	}

	private bool do_Dodge(float z1, float z2)
	{
		float angleDiff = Mathf.DeltaAngle(z1, z2);
		if (angleDiff >= 0 && angleDiff < findMinGap())
		{
			bLeft = true;
			return true;
		}
		else if (angleDiff < 0 && angleDiff > -findMinGap())
			return true;
		else
			return false;
	}

	private int checkAngle(int ang) 
	{
		if (ang > 360)
			return ang -= 360;
		if (ang <= 0)
			return ang += 360;
		return ang;
	}
}
