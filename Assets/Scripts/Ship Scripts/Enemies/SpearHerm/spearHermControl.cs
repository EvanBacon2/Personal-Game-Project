using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spearHermControl : MonoBehaviour
{
	//for moving position
	public float MoveSpeed = 5f;
	public float Radius = 20f;
	public float arcL;
	private Rigidbody2D rb2d;
	public float orbitSpeed;
	private List<GameObject> spawner;
	private Vector2 playerPushForce;//force required to keep distance from player
	private Vector2 enPushForce;//force required to keep distance from other enemies

	//for rotating
	public Transform Target;
	public float rotSpeed;
	private Quaternion lookRotation;
	private Vector2 pointD;
	private float turnAngle;

	//for charging
	public bool doCharge;
	public float chargeForce;
	public bool contact = true;
	public float chargePause;
	private float chargePauseTimeStamp;

	//vars for dodging
	//size.y == 28.65, size.x == 15
	public Rigidbody2D shipRB;
	public float angleDiff;
	private float yExt;
	private Bounds spriteBound;
	public float minGap;
	public float minAng;
	public float rad;
	public float destX;
	public float destY;
	public float angz;
	public float dist;

	//vars for health
	public float health;

	//vars for getting hit
	private SpriteRenderer spriteRend;
	public Sprite oK;
	public Sprite notoK;
	private bool stillHit;

	void Start ()
	{
		rb2d = GetComponent<Rigidbody2D>();
		spawner = EnemySpawner.getEnList();
		spriteRend = GetComponent<SpriteRenderer>();
		rb2d.freezeRotation = true;
		yExt = spriteRend.bounds.extents.y;
		spriteBound = spriteRend.bounds;
		angz = transform.localEulerAngles.z + 90;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		//if a playerBullet is colliding with spearHerm
		if (other.CompareTag("PlayerBullet") || other.CompareTag("Walls"))
		{
			if (contact == true)
				chargePauseTimeStamp = Time.time + chargePause;
			health -= 100;
			//tells spearHearm to charge at the player
			//doCharge = true;
			//won't get set to true until spearHerm actually hits player
			contact = false;
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		//if spearHerm has hit player
		if (other.gameObject.CompareTag("Player") && doCharge)
		{
			//spearHerm is taken out of charge mode and resumes normal motion
			contact = true;
			doCharge = false;
			//player is disabled momentarily, rendering it unable to move or input any actions in general
			shipControl.isControllable = false;
			shipControl.rebootTime = Time.time + 2;
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		stillHit = true;
		//checks to see if any bullet are still colliding with lasHerm
		if (other.gameObject.CompareTag("PlayerBullet"))
		{
			if (spriteRend.sprite != notoK)			
				spriteRend.sprite = notoK;
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		//once all bullets have stopped colliding with spearHerm, sprite goes back to ok
		if (!stillHit)
		{
			spriteRend.sprite = oK;
		}
	}

	void Update ()
	{
		//checks to see if spearHerm is dead
		if (health <= 0)
		{
			Destroy(gameObject);
		}
		//set new rotation
		if (!doCharge)
		{
			pointD = Target.position - transform.position;
			turnAngle = Mathf.Atan2(pointD.y, pointD.x) * Mathf.Rad2Deg;
			lookRotation = Quaternion.AngleAxis(turnAngle + 90, transform.forward);
			transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotSpeed * Time.deltaTime);
		}
	}

	private void FixedUpdate()
	{
		angz = transform.localEulerAngles.z + 90;
		if (!doCharge)
		{
			//moves spearHerm towards or away from player, while trying to maintain a certain distance from player
			if (Vector2.Distance(transform.position, Target.position) < 60)// if lasHerm is to close to the player
				playerPushForce = new Vector2(transform.position.x - Target.position.x, transform.position.y - Target.position.y).normalized * orbitSpeed * 1.5f;
			else//if spearHerm is to far away
				playerPushForce = new Vector2(transform.position.x - Target.position.x, transform.position.y - Target.position.y).normalized * orbitSpeed * -1.5f;

			//spearHerm checks the positions of all currently spawned enemies to see if any are too close to him
			for (int i = 0; i < spawner.Count; i++)
			{
				Transform entrans;//enemy transform
				try
				{
					entrans = spawner[i].transform;
				}
				catch (NullReferenceException)//occures when enemy died on this frame and was destroyed before spearHerm's code ran
				{
					continue;
				}
				float dist = Vector2.Distance(transform.position, entrans.position);
				if (dist < 40 && dist != 0)// if spearHerm is to close to another enemy
				{
					enPushForce = new Vector2(transform.position.x - entrans.position.x, transform.position.y - entrans.position.y).normalized * orbitSpeed * 1.5f;
				}
			}

			//adds the vector that pushes spearHerm away from player and the vector that pushes it away from other enemies to get a vector which will push spearHerm away from both.
			Vector2 totalForce = enPushForce + playerPushForce;
			rb2d.AddForce(totalForce);
			playerPushForce = new Vector2(0, 0);
			enPushForce = new Vector2(0, 0);

		}
		else if (contact == false)
		{
			if (Time.time < chargePauseTimeStamp)
			{ 
				rb2d.velocity = new Vector2(0, 0);
				pointD = Target.position - transform.position;
				turnAngle = Mathf.Atan2(pointD.y, pointD.x) * Mathf.Rad2Deg;
				lookRotation = Quaternion.AngleAxis(turnAngle - 90, transform.forward);
				transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotSpeed * Time.deltaTime);
			}

			if (Time.time >= chargePauseTimeStamp)
				rb2d.AddForce(new Vector2(chargeForce * pointD.x, chargeForce * pointD.y), ForceMode2D.Impulse);
		}
	}
		
	private void LateUpdate()
	{
		stillHit = false;//if any bullets are still colliding with spearHerm, this will get set to true in TriggerStay on the next frame
	}
}