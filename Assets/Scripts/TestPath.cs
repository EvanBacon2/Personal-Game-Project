using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPath : MonoBehaviour 
{
	public bool found = false;
	GameObject ship;
	PathMaker pathMakr;


	// Use this for initialization
	void Start () 
	{
		ship = GameObject.Find("Ship");
		//pathMakr = new PathMaker(this.transform.position, ship.transform.position, ship.transform, ship.GetComponent<Collider2D>());
		//path = pathMakr.makePath();
		//if (pathMakr.goalNodeFound)
			//found = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (found)
		{
			
			/*for (int i = 0; i < path.Count - 1; i++)
			{
				Debug.DrawLine(path[i].getNodePos(), path[i + 1].getNodePos());
			}*/

		}
	}
}
