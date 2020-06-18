using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGraph : MonoBehaviour
{
	private static Dictionary<int, Vector2> nodeList;//maps node ID's to their of the locations in the game scene
	private static Dictionary<int, bool> nodeLosLog;//maps a key calculated from two node ID's to boolean indicating whether or not they are in LOS
													//keys are of the form i * 10^n + j where i and j are Node ID's and n is some positive integer such that
													//10^n > maxNodeID / 10
	private static int[,] wallNodes;
	private static int[] paddings;


	//Constructor for when node placement is based off of objects AABB
	public NodeGraph(List<GameObject> Walls, int padding)
	{
		nodeList = new Dictionary<int, Vector2>();
		nodeLosLog = new Dictionary<int, bool>();
		wallNodes = new int[Walls.Count, 4];
		NodeGraph.paddings = new int[4];
		for (int i = 0; i < paddings.Length; i++)
		{
			paddings[i] = padding;
		}
	}

	/*Constructor for when node placement is based off of objects AABB, 
	* and some corner of the AABB needs unique padding.
	* 
	* Param:
	*	Walls - List of all objects to be populated with nodes
	*	paddings
	*/
	public NodeGraph(List<GameObject> Walls, int[] paddings)
	{
		nodeList = new Dictionary<int, Vector2>();
		nodeLosLog = new Dictionary<int, bool>();
		wallNodes = new int[Walls.Count, 4];
		NodeGraph.paddings = paddings;
	}

	// Start is called before the first frame update
	void Start()
    {
		
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
