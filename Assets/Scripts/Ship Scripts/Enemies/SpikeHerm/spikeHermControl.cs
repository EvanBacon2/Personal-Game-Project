using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spikeHermControl : MonoBehaviour
{
	//for moving position
	public float MoveSpeed = 5f;
	public float Radius = 20f;
	public float arcL;
	private float direction = 1;
	private Rigidbody2D rb2d;
	public float orbitSpeed;
	private List<GameObject> spawner;
	private Vector2 playerPushForce;//force required to keep distance from player						
	private Vector2 enPushForce;//force required to keep distance from other enemies

	//for rotating
	public Transform Target;//what spikeHerm looks at/follows, for now will always be the player
	public float rotSpeed;//speed at which spikeHerm rotates around the z axis
	private Quaternion lookRotation;
	private Vector2 pointD;
	private float turnAngle;

	//vars for shooting spikes
	private bool doSpike;//bool used to active spike launching mode
	private bool launchStarted;//used to say whether or not spikeHerm is currently launching spikes
	private bool launchFin;//used to mark when spikeHerm has launched all 43 spikes
	public float spikeSpeed;//speed of the spikes
	public float spikeLaunchDelay;//time between which each spike is launched
	public Sprite noSpikes;//spikeHerm sprite w/o any of the spikes
	private GameObject[] spikes;//list containing all the spikes
	private bool cr_running = false;

	MoveControl mvControl;

	void Start ()
	{
		rb2d = GetComponent<Rigidbody2D>();
		spawner = EnemySpawner.getEnList();
		rb2d.freezeRotation = true;
		spikes = new GameObject[43];

		mvControl = GetComponent<MoveControl>();
		mvControl.targetTags.Add("Player");
		mvControl.targetTags.Add("Ship Shield");
	}

	void Update()
	{
		//set new rotation
		if (doSpike == false)
			mvControl.setRotation(90);

		//as of right now spikeHerm will only shoot spikes after q is pressed, so that it can be manuelly controlled and tested
		if (Input.GetKey("q") == true)
			doSpike = true;
	}

	private void FixedUpdate()
	{
		if (doSpike == false)
		{
			/*
			//moves lasHerm towards or away from player, while trying to maintain a certain distance from player
			if (Vector2.Distance(transform.position, Target.position) < 40)// if lasHerm is to close to the player
				playerPushForce = new Vector2(transform.position.x - Target.position.x, transform.position.y - Target.position.y).normalized * orbitSpeed;
			else//if LasHerm is to far away
				playerPushForce = new Vector2(-(transform.position.x - Target.position.x), -(transform.position.y - Target.position.y)).normalized * orbitSpeed;

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
					enPushForce = new Vector2(transform.position.x - entrans.position.x, transform.position.y - entrans.position.y).normalized * orbitSpeed;
			}

			//adds the vector that pushes spikeHerm away from player and the vector that pushes it away from other enemies to get a vector which will push spikeHerm away from both.
			rb2d.AddForce(enPushForce + playerPushForce);
			playerPushForce = new Vector2(0, 0);
			enPushForce = new Vector2(0, 0);
			*/
		}
		else
		{
			//stops spikeHerm and instantiates all of the spikes first
			if (launchStarted == false)
			{

				//stops spikeHerm, freezes rotation, and sets the sprite to noSpikes
				rb2d.velocity = new Vector2 (0, 0);
				rb2d.freezeRotation = true;
				//spriteRend.sprite = noSpikes;

				//initiates all 43 spikes, sets there position and rotation
				for (int i = 0; i < 43; i++)
				{
					spikes[i] = EnemySpawner.spikes.createSpike(i, transform);
				}
				//allows spikeHerm to begin launching it's spikes
				launchStarted = true;
			} 
			else
			{
				if (!cr_running) 
				   StartCoroutine(LaunchSpikes());
			}

			//once spikeHerm has launched all of its spikes, doSpike is set to false, resulting in spikeHerm reverting to normal behavior
			if (launchFin == true)
			{
				doSpike = false;
				launchStarted = false;
				launchFin = false;
				cr_running = false;
				//spriteRend.sprite = oK;
			}
		}
	}

	//Launches all 43 spikes in timed intervals
	IEnumerator LaunchSpikes()
	{
		cr_running = true;
		for (int i = 0; i < 43; i++) 
		{
			//sets spikeHerms velocity to the direction it needs to launched off in, effectively launching it off from spikeHerm
			float launchAng = EnemySpawner.spikes.spikeVects[i] + transform.eulerAngles.z;
			spikes[i].GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Cos(launchAng * Mathf.Deg2Rad) * 100, Mathf.Sin(launchAng * Mathf.Deg2Rad) * 100);//spikeDirection;
			//once spikeHerm has launched all 43 spikes, launchFin is set to true so that doSpike will be set to false in Update() during the next frame
			if (i == 42)
				launchFin = true;
			//with this the for loop won't complete another iteration until spikeLaunchDelay has passed, thus allowing spikeHerm to launch spikes a timed intervals
			yield return new WaitForSeconds(spikeLaunchDelay);
		}
	}
}
