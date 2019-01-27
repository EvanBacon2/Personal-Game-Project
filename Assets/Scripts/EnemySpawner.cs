using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	//Prefab templates for enemies
	public GameObject laserHerm;
	public GameObject laserHermGun;
	public GameObject spearHerm;
	public GameObject spikeHerm;

	//vars for shiip
	public GameObject shiip;
	public Transform ship;
	public Sprite Ok;

	//different lists for holding currently spawned enemies
	private static List<GameObject> enemyList = new List<GameObject>();
	private static List<GameObject> lasHermList = new List<GameObject>();
	private static List<GameObject> lasGunList = new List<GameObject>();
	private static List<GameObject> spearHermList = new List<GameObject>();
	private static List<GameObject> spikeHermList = new List<GameObject>();

	//Booleans used to specify whether or not to specify certain enemies
	private bool spawnLas;
	private bool spawnSpear;
	private bool spawnSpike;

	public static Spike spikes;

	void Start() 
	{
		spikes = ScriptableObject.CreateInstance(typeof(Spike)) as Spike;
		//spawnLasHerm();
		//spawnLasHerm(-440,221);
		//spawnLasHerm(-388,358);
		//spawnLasHerm(-215,221);
	}

	void Update ()
	{
		//pressing g will spawn 1 Laser Hermit
		if (Input.GetKeyDown("g"))
			spawnLasHerm();
		//pressing h will spawn 1 Spear Hermit
		if (Input.GetKeyDown("h"))
			spawnSpearHerm();
		//pressing j will spawn 1 Spike Hermit
		if (Input.GetKeyDown("j"))
			spawnSpikeHerm();
		//prressing r will respawn the player, resetting their health
		if (Input.GetKeyDown("r"))
			respawnShip();

		for (int i = 0; i < enemyList.Count; i++)
		{
			//when an enemy is killed, its place in enemyList will become null, as such this for loop is called so as to clear enemyList of any null positions
			if (enemyList[i] == null)
			{
				//clears the null position from enemyList
				enemyList.RemoveAt(i);
			}
		}
	}

	private void spawnLasHerm() 
	{
		spawnLasHerm(0,0);
	}

	private void spawnLasHerm(float xPos, float  yPos)
	{
		//Create lasHerm
		GameObject Las = (GameObject)Instantiate(laserHerm, new Vector3(xPos, yPos), transform.rotation);
		//set lasHerm's Target value to the player's ship, used for following the player
		var LasScript = Las.GetComponent<lasHermControl>();
		LasScript.Target = ship;
		//add lasHerm to the total enemy list, as well as the lasHerm List
		enemyList.Add(Las);
		lasHermList.Add(Las);
		//lasHerm's name will be set according to how many lasHerms there currently are spawned in the game.  eg. if there are 2 lasHerms arleady spawned,
		//then the third lasHerm spawned will get the name "Las3"
		Las.name = "Las" + lasHermList.Count;
		//Create gun for lasHerm
		GameObject LasGun = (GameObject)Instantiate(laserHermGun, new Vector3(xPos, yPos), transform.rotation);
		//set lasHerm's lasGun value to the just created LasGun
		LasScript.lasGun = LasGun;
		//set lasGun's Target value to the player's ship, used for targeting
		var LasGunScript = LasGun.GetComponent<HermitGun>();
		LasGunScript.ship = ship;
		//allows lasGun to change it's position based on how it's parent lasHerm changes position
		LasGunScript.parentHerm = Las.transform;
		//adds lasGun to the lasGun list
		lasGunList.Add(LasGun);
		//giving lasGun it's name, see lasHerm for explanation of naming system
		LasGun.name = Las.name + "Gun";
		//done so another lasHerm won't be spawned until g is once pressed again
		spawnLas = false;
	}

	private void spawnSpearHerm() 
	{
		spawnSpearHerm(0,0);
	}

	private void spawnSpearHerm(float xPos, float yPos) 
	{
		//Create spearHerm
		GameObject Spear = (GameObject)Instantiate(spearHerm, new Vector3(xPos, yPos), transform.rotation);
		//set spearHerm's Target value to the player's ship, used for following the player
		var SpearScript = Spear.GetComponent<spearHermControl>();
		SpearScript.Target = ship;
		//add spearHerm to the total enemy list, as well as the spearHerm List
		enemyList.Add(Spear);
		spearHermList.Add(Spear);
		//giving spearHerm it's name, see lasHerm for explanation of naming system
		Spear.name = "Spear" + spearHermList.Count;
		//done so another spearHerm won't be spawned until h is once pressed again
		spawnSpear = false;
	}

	private void spawnSpikeHerm()
	{
		spawnSpikeHerm(0,0);
	}

	private void spawnSpikeHerm(float xPos, float yPos)
	{
		//Create spikeHerm
		GameObject Spike = (GameObject)Instantiate(spikeHerm, new Vector3(xPos, yPos), transform.rotation);
		//set spikeHerm's Target value to the player's ship, used for following the player
		var SpikeScript = Spike.GetComponent<spikeHermControl>();
		SpikeScript.Target = ship;
		//add spikeHerm to the total enemy list, as well as the spikeHerm List
		enemyList.Add(Spike);
		spikeHermList.Add(Spike);
		//giving spikeHerm it's name, see spikeHerm for explanation of naming system
		Spike.name = "Spike" + spikeHermList.Count;
		//done so another spikeHerm won't be spawned until j is once pressed again
		spawnSpike = false;
	}

	private void respawnShip()
	{
		respawnShip(0,0);
	}

	private void respawnShip(float xPos, float yPos)
	{
		//makes ship appear in the game again
		shiip.SetActive(true);
		//resets ship health
		var shiipScript = shiip.GetComponent<shipControl>();
		shiip.GetComponent<DamageControl>().health = 1000;
		//makes sure ship's sprite is set to OK, which is just the default sprite
		var shiipSprite = shiip.GetComponent<SpriteRenderer>();
		shiipSprite.sprite = Ok;
		//sets ship's position to the center of the game world
		Transform shiipPos = shiip.GetComponent<Transform>();
		shiipPos.position = new Vector2(xPos, yPos);
	}

	public static List<GameObject> getEnList()
	{
		return enemyList;
	}

	public static List<GameObject> getLasList()
	{
		return lasHermList;
	}
}
