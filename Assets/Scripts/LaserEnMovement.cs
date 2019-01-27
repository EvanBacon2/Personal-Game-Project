using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEnMovement : MonoBehaviour
{
	//for moving position
	public float MoveSpeed = 5f;
	public float Radius = 20f;
	public float arcL;
	private float direction = 1;
	private Rigidbody2D rb2d;
	public float orbitSpeed;
	private List<GameObject> spawner;
	//force required to keep distance from player
	private Vector2 playerPushForce;
	//force required to keep distance from other enemies
	private Vector2 enPushForce;
	
	//for rotating
	public Transform Target;
	public float rotSpeed;
	private Quaternion lookRotation;
	private Vector2 pointD;
	private float turnAngle;

	//for shooting laser
	public float lasRate;
	private float lasRateTimeStamp;
	public float shootStop;
	public float alignStop;
	private Boolean shootStart;
	public Boolean doAlign = false;
	private Boolean startLaser;
	private Boolean newSet = true;
	public GameObject lasHermLas;

	//vars for health
	public float health;

	private Vector2 centre;
	private float angle = .001f;

	private void Start()
	{
		rb2d = GetComponent<Rigidbody2D>();
		centre = new Vector2(transform.position.x, transform.position.y - Radius);
		lasRateTimeStamp = Time.time + lasRate;
		spawner = EnemySpawner.getEnList();
		spriteRend = GetComponent<SpriteRenderer>();
		gunSprite = lasGun.GetComponent<SpriteRenderer>();
		rb2d.freezeRotation = true;
	}

	//vars for getting hit
	private SpriteRenderer spriteRend;
	private SpriteRenderer gunSprite;
	public Sprite oK;
	public Sprite notoK;
	private bool stillHit;
	public GameObject lasGun;

	private void OnTriggerStay2D(Collider2D other)
	{
		stillHit = true;
		if (other.gameObject.CompareTag("PlayerBullet") == true)
		{
			if (spriteRend.sprite != notoK)
			{
				gunSprite.sortingOrder = 0;
				gunSprite.sortingLayerName = "Default";
				spriteRend.sprite = notoK;
			}

			health -= 100;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		//checks to see if any bullet are still colliding with lasHerm
		if (stillHit == false)
		{
			gunSprite.sortingOrder = 0;
			gunSprite.sortingLayerName = "Las Turret";
			spriteRend.sprite = oK;
		}
	}

	private void Update()
	{
		//checks to see if lasherm is dead
		if (health <= 0)
		{
			Destroy(gameObject);
			GameObject gun = GameObject.Find(gameObject.name + "Gun");
			Destroy(gun);
		}
		//set new rotation
		if (doAlign == false)
		{
			pointD = Target.position - transform.position;
			turnAngle = Mathf.Atan2(pointD.y, pointD.x) * Mathf.Rad2Deg;
			lookRotation = Quaternion.AngleAxis(turnAngle + 90, transform.forward);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotSpeed * Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{

		if (doAlign == false)
		{
			/*
			if (angle > arcL)
				direction = -1;
			if (angle < -arcL)
				direction = 1;
			MoveSpeed = (.06f / ((Mathf.Pow(Mathf.Abs(angle), 2) + .5f)));
			angle += MoveSpeed * Time.deltaTime * direction;
			var offset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * Radius;
			transform.position = centre + offset;
			*/

			//moves lasHerm towards or away from player, while trying to maintain a certain distance from player
			if (Vector2.Distance(transform.position, Target.position) < 40)// if lasHerm is to close to the player
			{
				playerPushForce = new Vector2(transform.position.x - Target.position.x, transform.position.y - Target.position.y).normalized * orbitSpeed;
			}
			else if (Vector2.Distance(transform.position, Target.position) == 40)
			{ }
			else//if LasHerm is to far away
			{
				playerPushForce = new Vector2(-(transform.position.x - Target.position.x), -(transform.position.y - Target.position.y)).normalized * orbitSpeed;
			}

			for (int i = 0; i < spawner.Count; i++)
			{
				Transform entrans;
				try
				{
					entrans = spawner[i].transform;
				}
				catch (NullReferenceException)
				{
					break;
				}
				float dist = Vector2.Distance(transform.position, entrans.position);
				if (dist < 40 && dist != 0)// if lasHerm is to close to another enemy
				{
					enPushForce = new Vector2(transform.position.x - entrans.position.x, transform.position.y - entrans.position.y).normalized * orbitSpeed;
				}
			}

			Vector2 totalForce = enPushForce + playerPushForce;
			rb2d.AddForce(totalForce);
			playerPushForce = new Vector2(0, 0);
			enPushForce = new Vector2(0, 0);

		}
		else
			rb2d.velocity = new Vector2(0, 0);

		//once enough time has passed, lasHerm begins lasShoot phase
		if (newSet == true && Time.time > lasRateTimeStamp && shootStart == false)
		{
			doAlign = true;
			newSet = false;
			alignStop = Time.time + 1;
			shootStart = true;
		}

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
			GameObject Las = Instantiate(lasHermLas, new Vector2(transform.position.x + (153.4755f * Mathf.Cos((ang.z + 180) * Mathf.Deg2Rad)), transform.position.y + (153.4755f * Mathf.Sin((ang.z + 180) * Mathf.Deg2Rad))), transform.rotation);
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

	private void LateUpdate()
	{
		stillHit = false;//if any bullets are still colliding with lasHerm, this will get set to true in TriggerStay
	}

	public Boolean getAlign()
	{
		return doAlign;
	}

}
