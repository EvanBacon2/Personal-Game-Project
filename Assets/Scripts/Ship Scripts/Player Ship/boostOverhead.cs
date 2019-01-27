using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class boostOverhead : MonoBehaviour 
{
	private static GameObject[] boostBars;
	public GameObject bar1;
	public GameObject bar2;
	public GameObject bar3;
	private Queue<float> regenQueue;
	private Color readyColor = new Color (230f, 255, 0);
	public float regenTime;
	private int boostLeft = 2;

	void Start() 
	{
		boostBars = new GameObject[] {bar1, bar2, bar3};
		regenQueue = new Queue<float>();
	}

	void Update () 
	{
		if (regenQueue.Count != 0 && regenQueue.Peek() <= Time.time) 
		{
			boostLeft++;
			boostBars[boostLeft].GetComponent<Image>().color = readyColor;
			regenQueue.Dequeue();
		}
	}

	public void useBoost() 
	{
		this.regenQueue.Enqueue(Time.time + regenTime);
        boostBars[this.boostLeft].GetComponent<Image>().color = Color.black;
	    this.boostLeft--;
	}

	public int getBoost() {
		return boostLeft;
	}
}
