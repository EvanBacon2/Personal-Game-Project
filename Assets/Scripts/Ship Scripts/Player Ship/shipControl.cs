using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class shipControl : MonoBehaviour
{
	//vars for basic movement
	public float speed;//affects ship's acceleration
	public float maxSpeed;
	private Rigidbody2D rb2d;
	public static bool isControllable = true;//can player move the ship
	public static float rebootTime;//time it takes for ship to regain control after being hit by spikeHerm

	//vars for shooting bullets
	public Transform ship;//ships position
	public GameObject bullet;
	public float boltSpeed;//velocity of ship's bullets
	public float shootRate;//fire rate of ship
	private float shootRateTimeStamp = 0;//set whenever ship shoots, used to check if enough time has passed for ship to shoot again
	public static bool doShoot;//whether or not ship will shoot on this frame
	public static List<GameObject> playerBulls = new List<GameObject>();//stores all active bullets fired from ship

	//vars for rotating ship
	public float rotSpeed;
	private float turnAngle;
	private Quaternion lookRotation;
	public Vector3 mouse_pos;//position of mouse cursor

	//vars for boost
	private bool boostStart;//whether or not ship will begin boosting on this frame
	private Vector2 boostPlace = new Vector2(1000000, 1000000);//holds position of ship just before boost starts
	private Vector2 boostPlaceSpeed;
	public boostOverhead boostOverhead;

	void Start()
	{
		//create necessary objects
		rb2d = GetComponent<Rigidbody2D>();
		//so ship only rotates when player moves mouse
		rb2d.freezeRotation = true;
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		//rammed by spikeHerm
		if (other.gameObject.CompareTag("SpearHerm") && other.gameObject.GetComponent<spearHermControl>().contact == false)
		{
			//isControllable = false;
			//rebootTime = Time.time + 2;
		}
	}

	private void Update()
	{
		if (isControllable == true)
		{
			//updates the rotaion of ship based on the position of the mouse cursor
			mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			turnAngle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
			lookRotation = Quaternion.AngleAxis(turnAngle - 90, Vector3.forward);
			transform.rotation = lookRotation;
			//

			if (boostDist() == false || Input.GetKey(KeyCode.LeftShift))
				boostPlace = new Vector2(100000, 100000);
		}

		//once rebootTime time has passed after being struck by spikeHerm, make ship contrallable again
		if (Time.time >= rebootTime)
			isControllable = true;

		//ship will begin boosting on this frame
		if (Input.GetKeyDown(KeyCode.Space) && boostOverhead.getBoost() >= 0) 
		{
			boostStart = true;
			boostOverhead.useBoost();
		}
	
	}

	void FixedUpdate()
	{
		doShoot = false;

		if (isControllable == true)
		{
			//get movement inputs from keyboard, and current angle of ship
			float moveHorizontal = Input.GetAxis("Horizontal");
			float moveVertical = Input.GetAxis("Vertical");
			Vector3 ang = transform.eulerAngles;

			//direction vector to push the ship in
			Vector2 movement = new Vector2(moveHorizontal, moveVertical);
			float mod = Mathf.Abs(Vector2.Angle(rb2d.velocity, movement)) / 180f * 13000;

			//if ship is not boosing, push it in the direction of movement
			if (boostStart == false && speed != maxSpeed)
			{
				rb2d.AddForce(movement * (speed + mod) * Time.deltaTime);
			}

			//ship is shooting this frame
			if (Input.GetMouseButton(0))
			{
				Vector2 camPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				//if enough has time has passed since last shot, then may shoot on this frame
				if (Time.time > shootRateTimeStamp)
				{
					doShoot = true;
					var proHolder = ProjectileHolder.playerBull;

					//create bullet that will launch from right side launcher
					GameObject boltR = (GameObject)Instantiate(bullet, new Vector2(ship.position.x + (12.1f * Mathf.Cos((ang.z + 49.9f) * Mathf.Deg2Rad)), ship.position.y + (12.1f * Mathf.Sin((ang.z + 49.9f) * Mathf.Deg2Rad))), transform.rotation);
					boltR.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sin(-ang.z * Mathf.Deg2Rad), Mathf.Cos(ang.z * Mathf.Deg2Rad)) * boltSpeed;
					playerBulls.Add(boltR);

					//create bullet that will launch from left side launcher
					GameObject boltL = (GameObject)Instantiate(bullet, new Vector2(ship.position.x + (12.1f * -Mathf.Cos((ang.z - 49.9f) * Mathf.Deg2Rad)), ship.position.y + (12.1f * -Mathf.Sin((ang.z - 49.9f) * Mathf.Deg2Rad))), transform.rotation);
					boltL.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sin(-ang.z * Mathf.Deg2Rad), Mathf.Cos(ang.z * Mathf.Deg2Rad)) * boltSpeed;
					playerBulls.Add(boltL);

					//once passed this time, ship may shoot again
					shootRateTimeStamp = shootRate + Time.time;
				}
			}

			//stop the ship
			if (Input.GetKey(KeyCode.LeftShift))
				rb2d.velocity = new Vector2(0, 0);

			//if ship is beginning boost on this frame
			if (boostStart == true)
			{
				//save velocity of ship right before boost starts
				boostPlaceSpeed = rb2d.velocity;
				//mark position of ship right before boost starts
				boostPlace = transform.position;
				//increase velocity
				rb2d.velocity = movement * 500;

				boostStart = false;
			}

			//if boost is over then put ship back into normal flying mode
			if (boostDist() == false && boostPlaceSpeed != new Vector2(0, 0))
			{
				rb2d.drag = .4f;
				//set velocity back to normal
				rb2d.velocity = movement.normalized * boostPlaceSpeed.magnitude;
				//set values back to default
				boostPlace = new Vector2(1000000, 1000000);
				boostPlaceSpeed = new Vector2(0, 0);
			}
		}
	}

	//checks if ship should keep boosting by seeing how much distance has been covered since it's boost started
	bool boostDist()
	{
		return Vector2.Distance(boostPlace, transform.position) < 70;
	}
}
