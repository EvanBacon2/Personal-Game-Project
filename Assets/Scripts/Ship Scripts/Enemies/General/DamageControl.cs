using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageControl : MonoBehaviour 
{
	private SpriteRenderer spriteRend;//sprite renderer, accessed so that the sprite can be changed
	public Sprite oK;//sprite rendered when not colliding with any damaging objects
	public Sprite notoK;//sprite rendered otherwise
	public int health;//total current health
	public bool doTakeHit;//if true then hits from damaging objects will be registered, if false they will be ignored
	public bool doChangeSprite;//if true then the sprite will be change when hit with damaging objects, false sprite will not change for such reasons
	private float notoKTimer;//tracks how long a ship should be rendereing its notoK sprite
	public List<GameObject> addObjects = new List<GameObject>();//additional game objects that will be hidden behind the ships main sprite when taking a hit
	public List<string> addTags = new List<string>();//tags for gameobjects in addObjects

	// Use this for initialization
	void Start () 
	{
		spriteRend = GetComponent<SpriteRenderer>();
		doTakeHit = true;
		doChangeSprite = true;
		notoKTimer = 0;
	}

	void Update()
	{
		killObject();
		if (notoKTimer <= Time.time)//if enough time has elapsed since being hit, display normal sprite again
		{
			changeSprite(true);
			//move all additional objects back to their original sorting layer
			for (int i = 0; i < addObjects.Count; i++)
			{
				//addObjects[i].GetComponent<SpriteRenderer>().sortingOrder = 0;
				addObjects[i].GetComponent<SpriteRenderer>().sortingLayerName = addTags[i];
			}
		}
	}

	//kills this object if it's health is zero
	private void killObject()
	{
		//checks to see if spikeHerm is dead
		if (health <= 0)
		{
			if (gameObject.tag.Equals("ship"))
				gameObject.SetActive(false);
			else
			{
				Destroy(gameObject);
				for (int i = 0; i < addObjects.Count; i++)
				{
					Destroy(addObjects[i]);
				}
			}
		}
	}

	//detects bullets on the first frame they collide with the enemy
	private void OnTriggerEnter2D(Collider2D other)
	{
		takeHit(other);
	}

	//records the damage from being hit by a player bullet
	private void takeHit(Collider2D other)
	{
		//player projectiles
		if (other.CompareTag("PlayerBullet"))
			health -= 100;

		//enemy projectiles
		if (other.gameObject.CompareTag("Enemy Bullet")) 
			health -= 100;

		if (other.gameObject.CompareTag("LasHermLas"))
			health -= 300;

		changeSprite(false);
		//move all additional objects behind the main damaged sprite.
		for (int i = 0; i < addObjects.Count; i++)
		{
			//addObjects[i].GetComponent<SpriteRenderer>().sortingOrder = 0;
			addObjects[i].GetComponent<SpriteRenderer>().sortingLayerName = "Default";
		}
		notoKTimer = Time.time + .1f;
	}

	//changes the enemies sprite to either oK or notoK
	//   other: the projectile which has collided with the enemy
	//   okay: true when enemy is not colliding with any bullets, false otherwise
	private void changeSprite(bool okay)
	{
		if (!okay && spriteRend.sprite != notoK)
			spriteRend.sprite = notoK;
		else if(okay && spriteRend.sprite != oK)
			spriteRend.sprite = oK;
	}
}
