using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lasHermControl : MonoBehaviour
{
	//for moving position
	public float MoveSpeed = 5f;
	public float Radius = 20f;
	public float arcL;
	private float direction = 1;
	private Rigidbody2D rb2d;
	public float orbitSpeed;
	private List<GameObject> spawner;

	//for rotating
	public Transform Target;
	public Transform targetShield;
	public float rotSpeed;
	private Quaternion lookRotation;
	private Vector2 pointD;
	private float turnAngle;

	//for shooting laser
	public float lasRate;
	private float lasRateTimeStamp;
	public float shootStop;
	public float alignStop;
	private bool shootStart;
	public bool doAlign = false;
	private bool startLaser;
	private bool newSet = true;
	public GameObject lasHermLas;

	//vars for getting hit
	private SpriteRenderer spriteRend;
	private SpriteRenderer gunSprite;
	public GameObject lasGun;

	private Vector2 centre;

	//vars for moving
	private Vector2[] rayOrigins = new Vector2[72];
	public LayerMask sightMask;
	public LayerMask enemyMask;
	public LayerMask wallMask;
	private List<Vector2> targetPath = new List<Vector2>();
	private Vector2 currentPathNode;
	public int pathStep;
	public bool sightCheck;
	public bool nosightCheck;
	public int pathCalled;
	private bool drawPath;
	private PathMaker pathMakr;
	private DamageControl dmgControl;
	private MoveControl mvControl;
	public bool wallCalled;

	void Start()
	{
		rb2d = GetComponent<Rigidbody2D>();
		centre = new Vector2(transform.position.x, transform.position.y - Radius);
		lasRateTimeStamp = Time.time + lasRate;
		spawner = EnemySpawner.getEnList();
		rb2d.freezeRotation = true;
		pathMakr = new PathMaker();

		dmgControl = GetComponent<DamageControl>();
		dmgControl.addObjects.Add(lasGun);
		dmgControl.addTags.Add(lasGun.GetComponent<SpriteRenderer>().sortingLayerName);

		mvControl = GetComponent<MoveControl>();
		mvControl.targetTags.Add("Player");
		mvControl.targetTags.Add("Ship Shield");

		for (int i = 0; i <= 71; i ++) 
		{
			rayOrigins[i].x = Mathf.Cos((i + 1) * 5 * Mathf.Deg2Rad);
			rayOrigins[i].y = Mathf.Sin((i + 1) * 5 * Mathf.Deg2Rad);
		}
	}

	void Update()
	{
		//Debug.DrawRay(transform.position, rb2d.velocity, Color.red);
		//set new rotation
		if (doAlign == false)
			mvControl.setRotation(90f);
	}

	void FixedUpdate()
	{
		if (!doAlign)
		{
			//add method for movement and pathfinding
		}
		else
			rb2d.velocity = new Vector2(0, 0);
		/*
		//once enough time has passed, lasHerm begins lasShoot phase
		if (newSet == true && Time.time > lasRateTimeStamp && shootStart == false)
		{
			doAlign = true;
			newSet = false;
			alignStop = Time.time + 1;
			shootStart = true;
		}*/

		//first lasHerm rotates so laser is pointing at player
		if (Time.time < alignStop)
		{
			rotSpeed = 40;
			pointD = Target.position - transform.position;
			turnAngle = Mathf.Atan2(pointD.y, pointD.x) * Mathf.Rad2Deg;
			lookRotation = Quaternion.AngleAxis(turnAngle + 180, transform.forward);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotSpeed * Time.deltaTime);
		}
		//lasHerm fires laser, stops rotating
		else if (newSet == false && shootStart == true)
		{
			rotSpeed = 0;
			rb2d.isKinematic = true;
			Vector3 ang = transform.eulerAngles;
			GameObject Las = Instantiate(lasHermLas, new Vector2(transform.position.x + (194.401f * Mathf.Cos((ang.z + 179.721f) * Mathf.Deg2Rad)), 
				                                                 transform.position.y + (194.401f * Mathf.Sin((ang.z + 179.721f) * Mathf.Deg2Rad))), transform.rotation);
			Las.transform.parent = this.transform;
			shootStop = Time.time + .5f;
			startLaser = true;
			shootStart = false;
		}
		//lasHerm stops firing laser, ends lasShoot phase, goes back to normal
		if (Time.time > shootStop && startLaser == true)
		{
			lasRateTimeStamp = Time.time + lasRate;
			newSet = true;
			startLaser = false;
			doAlign = false;
			rotSpeed = 2;
			rb2d.isKinematic = false;
		}
	}

	public bool getAlign()
	{
		return doAlign;
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position, 25f);
	}
}
