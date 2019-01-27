using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : ScriptableObject 
{
	public GameObject[] Asteroids = new GameObject[3];
	public List<Vector2> asterPos = new List<Vector2>(); /*{new Vector2(-507,265), new Vector2(-353,231), new Vector2(-253,269), new Vector2(-428,126), new Vector2(-282,125),
		new Vector2(-660, 103), new Vector2(-403, 103), new Vector2(-368, 490), new Vector2(-340, 79), new Vector2(-427, 57), new Vector2(-577, -38),
		new Vector2(-91, 547), new Vector2(-274, 554), new Vector2(-684, 416), new Vector2(-679, 238), new Vector2(-520, 486), new Vector2(-514, 543),
		new Vector2(-213, 388), new Vector2(-531, 427), new Vector2(-338, 336), new Vector2(-569, 138), new Vector2(-429, 366)};


	//{new Vector2(-500, 265), new Vector2(-650, 415)};
	//{new Vector2(-500,265), new Vector2(-560,215), new Vector2(-620,265)};
	//{new Vector2(-507,265), new Vector2(-353,231), new Vector2(-253,269), new Vector2(-428,126), new Vector2(-282,125)};

	/*{new Vector2(-500, 100), new Vector2(-500, 150), new Vector2(-500, 200), new Vector2(-500, 250),
		new Vector2(-620, 100), new Vector2(-620, 150), new Vector2(-620, 200), new Vector2(-620, 250),
		new Vector2(-500, 350), new Vector2(-450, 350), new Vector2(-400, 350), new Vector2(-350, 350), new Vector2(-300, 350),
		new Vector2(-450, 250), new Vector2(-400, 250), new Vector2(-350, 250), new Vector2(-300, 250), new Vector2(-300, 300),
		new Vector2(-670, 250), new Vector2(-720, 250), new Vector2(-770, 250), new Vector2(-820, 250), new Vector2(-870, 250), new Vector2(-920, 250),
		new Vector2(-620, 350), new Vector2(-670, 350), new Vector2(-720, 350), new Vector2(-770, 350), new Vector2(-820, 350), new Vector2(-870, 350), new Vector2(-920, 350),
		new Vector2(-920, 300), new Vector2(-560, 350)
	};*/

	/*{new Vector2(-670, 250), new Vector2(-720, 250), new Vector2(-770, 250), new Vector2(-820, 250), new Vector2(-870, 250), new Vector2(-920, 250),
		new Vector2(-920, 300), new Vector2(-920, 350), new Vector2(-920, 400), new Vector2(-920, 450), new Vector2(-920, 500), new Vector2(-920, 550), new Vector2(-920, 600), 
		new Vector2(-670, 300), new Vector2(-670, 350), new Vector2(-670, 400), new Vector2(-670, 450), new Vector2(-670, 500), new Vector2(-670, 550), new Vector2(-670, 600), new Vector2(-670, 650), new Vector2(-670, 700)};
	*/
	// Use this for initialization
	void Awake () 
	{
	    Asteroids[0] = Resources.Load("Asteroids/Test Asteroid 1 x 1") as GameObject;
	    Asteroids[1] = Resources.Load("Asteroids/Test Asteroid 1 x 2") as GameObject;
        Asteroids[2] = Resources.Load("Asteroids/Test Asteroid 2 x 2") as GameObject;
		Transform[] asterboids = GameObject.Find("Walls").GetComponentsInChildren<Transform>();
		GameObject.Find("Walls").SetActive(false);
		for (int i = 1; i < asterboids.Length; i++)
		{
			asterPos.Add(asterboids[i].position);
		}
	}

	public GameObject createAsteroid(int asterID, int asterSize) 
	{
		float LocX = asterPos[asterID].x;
		float LocY = asterPos[asterID].y;
		Vector2 Loc = new Vector2(LocX, LocY);
		return Instantiate(Asteroids[asterSize], Loc, Asteroids[asterSize].transform.rotation);
	}
}
