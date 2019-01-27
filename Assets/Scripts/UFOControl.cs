using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UFOControl : MonoBehaviour
{
	//vars for basic movement
	public float speed;
	private Rigidbody2D rb2d;

	//vars for shooting bullets
	public Transform ship;
	public GameObject bullet;
	public float boltSpeed;
	public float shootRate;
	private float shootRateTimeStamp = 0;

	//vars for rotating ship
	public float rotSpeed;
	private float turnAngle;
	private Quaternion lookRotation;
	public Vector3 mouse_pos;
	
	//vars for boost
	private bool isBoost = false;
	private Vector2 boostStart = new Vector2(1000000, 1000000);
	private Vector2 boostStartSpeed;

	//vars for health
	public float health;

	void Start()
	{
		rb2d = GetComponent<Rigidbody2D>();
		spriteRend = GetComponent<SpriteRenderer>();
		rb2d.WakeUp();
		rb2d.freezeRotation = true;
	}

	//vars for getting hit
	private static SpriteRenderer spriteRend;
	public Sprite oK;
	public Sprite notoK;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Enemy Bullet") == true)
		{
			health -= 100;
		}

		if (other.gameObject.CompareTag("LasHermLas") == true)
		{
			health -= 300;
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Enemy Bullet") == true)
		{
			if (spriteRend.sprite != notoK)
				spriteRend.sprite = notoK;

			if (health <= 0)
			{
				gameObject.SetActive(false);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		spriteRend = GetComponent<SpriteRenderer>();
		spriteRend.sprite = oK;
	}

	private void Update()
	{
		mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		turnAngle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
		lookRotation = Quaternion.AngleAxis(turnAngle - 90, Vector3.forward);
		transform.rotation = lookRotation;// Quaternion.Slerp(transform.rotation, lookRotation, speed * Time.deltaTime);

		if (boostDist() == false || Input.GetKey(KeyCode.LeftShift))
			boostStart = new Vector2(100000, 100000);

	}

	void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");
		Vector3 ang = transform.eulerAngles;

		if (Input.GetKey(KeyCode.Space))
			isBoost = true;

		Vector2 movement = new Vector2(moveHorizontal, moveVertical);

		if (isBoost == false)
		{
			rb2d.AddForce(movement * speed * Time.deltaTime);
		}

		if (Input.GetMouseButton(0))
		{
			Vector2 camPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			if (Time.time > shootRateTimeStamp)
			{

				GameObject boltR = (GameObject)Instantiate(bullet, new Vector2(ship.position.x + (12.1f * Mathf.Cos((ang.z + 49.9f) * Mathf.Deg2Rad)), ship.position.y + (12.1f * Mathf.Sin((ang.z + 49.9f) * Mathf.Deg2Rad))), transform.rotation);
				boltR.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sin(-ang.z * Mathf.Deg2Rad), Mathf.Cos(ang.z * Mathf.Deg2Rad)) * boltSpeed;

				GameObject boltL = (GameObject)Instantiate(bullet, new Vector2(ship.position.x + (12.1f * -Mathf.Cos((ang.z - 49.9f) * Mathf.Deg2Rad)), ship.position.y + (12.1f * -Mathf.Sin((ang.z - 49.9f) * Mathf.Deg2Rad))), transform.rotation);
				boltL.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sin(-ang.z * Mathf.Deg2Rad), Mathf.Cos(ang.z * Mathf.Deg2Rad)) * boltSpeed;

				shootRateTimeStamp = shootRate + Time.time;
			}
		}

		if (Input.GetKey(KeyCode.LeftShift))
			rb2d.velocity = new Vector2(0, 0);

		if (isBoost == true)
		{
			rb2d.velocity = new Vector2(movement.x * 500, movement.y * 500);
			boostStart = transform.position;
			boostStartSpeed = rb2d.velocity;
			isBoost = false;
		}

		if (boostDist() == false && boostStartSpeed != new Vector2(0, 0))
		{
			rb2d.drag = .4f;
			rb2d.velocity = boostStartSpeed / 4;
			boostStart = new Vector2(1000000, 1000000);
			boostStartSpeed = new Vector2(0, 0);
		}


	}

	bool boostDist()
	{
		return Vector2.Distance(boostStart, transform.position) < 35;
	}

	


}
