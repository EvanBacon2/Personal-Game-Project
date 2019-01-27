using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : ScriptableObject {

	public GameObject[] spikes = new GameObject[43];

	//spike data
	private float[] spikeRads = {/*1-10*/6.60681f, 7.31642f, 9.5462f, 8.70919f, 10.1139f , 11.2894f, 11.4983f, 10.5076f, 11.905f, 11.8072f,
		                 /*11-20*/10.8577f, 10.5475f, 11.0386f, 8.6977f, 7.85175f, 5.20865f, 3.32415f, .921954f, 3.45398f, 5.50364f,
		                 /*21-30*/6.38201f, 7.17008f, 8.31925f, 8.97385f, 8.1f, 8.58662f, 8.32406f, 7.18401f, 5.60803f, 4.38292f,
		                 /*31-40*/3.62353f, 1.92094f, 1.94165f, 3.96611f, 5.73847f, 6.54905f, 6.84471f, 6.09016f, 4.24382f, 3.2573f,
		                 /*41-43*/3.72156f, 5.5f, 5.48179f};

	private float[] spikeAngs = {/*1-10*/2.60167f, 10.2348f, 22.8045f, 34.2375f, 46.2021f, 55.4654f, 74.876f, 87.8184f, 100.649f, 117.216f,
		                 /*11-20*/130.143f, 137.69f, 146.454f, 160.523f, 173.418f, 176.697f, 195.708f, 282.5288f, 22.1093f, 24.7025f,
		                 /*21-30*/35.4334f, 50.0921f, 64.359f, 75.8089f, 90f, 104.845f, 118.72f, 141.216f, 148.861f, 145.222f,
		                 /*31-40*/152.021f, 128.66f, 78.1113f, 56.31f, 67.457f, 82.9835f, 101.802f, 119.511f, 124.439f, 107.879f,
		                 /*41-43*/83.8298f, 90f, 104.797f};

	public float[] spikeVects = {/*1-10*/333.435f, 315f, 26.5651f, 0f, 45f, 26.5651f, 63.4349f, 90f, 95f, 120f, 
		                          /*11-20*/135f, 150f, 140f, 210f, 225f, 225f, 270f, 270f, 315f, 330f, 
		                          /*21-30*/0f, 40f, 45f, 35f, 70f, 90f, 135f, 200f, 225f, 250f,
		                          /*31-40*/220f, 250f, 290f, 315f, 30f, 45f, 90f, 150f, 225f, 270f,
		                          /*41-43*/290f, 60f, 130f};

	void Awake() 
	{
		for (int i = 1; i <= 43; i++) {
			spikes[i-1] = Resources.Load("Spikes/spike" + i) as GameObject;
		}
	}

	public GameObject createSpike(int spikeID, Transform form) 
	{
		//GameObject spike = Resources.Load("spikes/spike" + spikeID) as GameObject;
		float LocX = form.position.x + (spikeRads[spikeID] * Mathf.Cos((form.eulerAngles.z + spikeAngs[spikeID]) * Mathf.Deg2Rad));
		float LocY = form.position.y + (spikeRads[spikeID] * Mathf.Sin((form.eulerAngles.z + spikeAngs[spikeID]) * Mathf.Deg2Rad));
		Vector2 Loc = new Vector2(LocX, LocY);
		return Instantiate(spikes[spikeID], Loc, form.rotation);
	}
}
