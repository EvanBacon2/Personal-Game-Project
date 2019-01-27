using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script which does the work of actually creating a path
//Contains class PathNode which can be used for creating objects
//that represent path nodes and store all information relevant to them
//Also contains class PathMaker which contains methods for constructing paths

public class PathMaker
{
	public bool goalNodeFound;
	private LayerMask wallMask = LayerMask.GetMask("Wall");//Physics layer that all Wall objects are in
	private LayerMask playerMask = LayerMask.GetMask("Player");//Physics layer that the player's ship is in
	private LayerMask losMask;//Combination of wallMask and playerMask
	private int pathID = Random.Range(1,1000);//used for identifying paths when printing path info in the console

	int[] parentNodes;//at an index i stores the index of nodeList[i]'s parentNode
	float[] gVals;//at an index i stores the gVal of nodeList[i]
	float[] hVals;//at an index i stores the hVal of nodeList[i]
	float[] fVals;//at an index i stores the fVal of nodeList[i]

	public PathMaker()
	{
		losMask = wallMask | playerMask;
	}

	public List<Vector2> makePath(Vector2 startPos, Vector2 goalPos, Transform Target, Collider2D targetCol, string thisTag)
	{
		List<Vector2> nodeList = new List<Vector2>(TestObjectSpawner.nodeList);//list of all nodes within the game scene
		nodeList.Add(startPos);
		nodeList.Add(goalPos);
		parentNodes = new int[nodeList.Count];//at an index i stores the index of nodeList[i]'s parentNode
		gVals = new float[nodeList.Count];//at an index i stores the gVal of nodeList[i]
		hVals = new float[nodeList.Count];//at an index i stores the hVal of nodeList[i]
		fVals = new float[nodeList.Count];//at an index i stores the fVal of nodeList[i]
		List<int> openList = new List<int>();//list of nodes still to be explored
		List<int> closedList = new List<int>();//list of nodes already explored
		List<Vector2> chosenNodes = new List<Vector2>();//list of all nodes in the final path
		int startNode = nodeList.IndexOf(startPos);
		int goalNode = nodeList.IndexOf(goalPos);
		Vector2 losPair = new Vector2();
		goalNodeFound = false;
		bool backwardsPath = false;//true when path is being drawn from goalPos to startPos

		for (int i = 0; i < parentNodes.Length; i++)
		{
			parentNodes[i] = -1;
		}
			
		int minNode = determinePathDirection(startNode, goalNode, nodeList);
		//goalNode had a node closer to it, as such the path will be drawn from goalNode to startNode
		if (nodeList[nodeList.Count - 1].x == 0)
		{
			backwardsPath = true;
			startNode = nodeList.IndexOf(goalPos);
			goalNode = nodeList.IndexOf(startPos);
		}
		nodeList.RemoveAt(nodeList.Count - 1);

		openList.Add(startNode);
		enterNodeInfo(startNode, nodeList.Count, 0, Vector2.Distance(startPos, goalPos));

		int pathRun = 0;//keeps track of how many times the following while loop was run
		while (openList.Count != 0 && !goalNodeFound)
		{
			pathRun++;
			int minFNode = openList[0];

			for (int i = 1; i < openList.Count; i++)
			{
				if (fVals[openList[i]] < fVals[minFNode])
					minFNode = openList[i];
			}
			openList.Remove(minFNode);
			//Debug.Log("minFNode chosen: " + nodeList[minFNode]);//Only uncommented when debugging
			List<int> succList = new List<int>();//List of nodes within line of sight of the current node being explored that might be added to openList
			Vector2 goal;
			if (backwardsPath)
				goal = startPos;
			else
				goal = Target.position;

			Vector2 minFPos = nodeList[minFNode];
			float goalDist = Vector2.Distance(goal, minFPos);
			Vector2 goalDir = new Vector2(goal.x - minFPos.x, goal.y - minFPos.y);
			RaycastHit2D goalLosRay = Physics2D.Raycast(minFPos, goalDir, goalDist, losMask);

			//data for the rayCast used to check if the player is in Los, only uncommented when debugging
			//Debug.Log("Target " + Target.position);
			//Debug.Log("minFPos " + minFPos);
			//Debug.Log("targetDist " + targetDist);
			//Debug.Log("targetDir " + targetDir);
			//Debug.Log("targetLosRay " + targetLosRay.point + " " + targetLosRay.transform.tag);

			//check to see if the player is within line of sight of he current node being explored
			if ((!backwardsPath && goalLosRay && goalLosRay.transform.CompareTag(Target.tag)) || (backwardsPath && !goalLosRay))
			{
				succList.Add(goalNode);
				parentNodes[goalNode] = minFNode;
			}
				
			if (minFPos.Equals(nodeList[startNode]))//If at startNode, find the closest node to startNode and if it is in line of sight add it and only it to succList so that 
			{							 //the path will start being formed from that node
				Vector2 minNodePos = nodeList[minNode];
				succList.Add(minNode);
				enterNodeInfo(minNode, startNode, Vector2.Distance(minNodePos, minFPos), Vector2.Distance(minNodePos, nodeList[goalNode]));

			} else//search through all of the other nodes in the scene, if they are within line of sight, and them to succList
			{
				bool inLos = false;//whether or not the two nodes are within line of sight
				for (int i = 0; i < nodeList.Count - 2; i++)
				{
					if (minFNode < i)
						inLos = TestObjectSpawner.nodeLosLog[((minFNode + 1) * 1000) + i + 1];
					else if (minFNode > i)
						inLos = TestObjectSpawner.nodeLosLog[((i + 1) * 1000) + minFNode + 1];
					if (inLos && minFNode != i)
					{
						succList.Add(i);
					}
				}
			}
				
			//log all nodes in succList
			/*for (int i = 0; i < succList.Count; i++)
			{
				Debug.Log(nodeList[succList[i]] + " succ " + pathRun);
			}*/
			int count = 0;//so while loop will terminate once all nodes in succList have been looked at
			int openCount = openList.Count;
			while (!goalNodeFound && count < succList.Count)//search through all nodes in succList to see if any should be added to openList
			{
				int succNode = succList[count];
				if (succNode == goalNode)
				{
					goalNodeFound = true;
					break;
				}

				float tempGVal = gVals[minFNode] + Vector2.Distance(nodeList[succNode], nodeList[minFNode]);
				float tempHVal = Vector2.Distance(nodeList[succNode], nodeList[goalNode]);
				float tempFVal = tempGVal + tempHVal;
					
				bool validNode = true;//true when node should be added to the openList
				bool replaceNode = false;//true when node is already in openList but a new instance of the node with a lower f value has been found
				int replaceInt = 0;//index of the node in openList that needs to get replaced

				//search through all of the nodes in closedList, if a node with the same position and lower f value than succNode is found than succNode
				//is no longer valid and will not be added to the open list
				for (int i = 0; i < closedList.Count; i++)
				{
					validNode = nodeList[closedList[i]].x != nodeList[succNode].x && 
								nodeList[closedList[i]].y != nodeList[succNode].y || 
								tempFVal < fVals[closedList[i]];
					if (!validNode)
						break;
				}

				if (validNode)
				{
					//search through all the nodes in openList, if a node with if a node with the same position and lower f value than succNode 
					//is found than succNode is no longer valid.  If a node with the same position and greater f value than succNode is found than 
					//succNode will replace it in the openList
					for (int i = 0; i < openList.Count; i++)
					{
						if(!validNode)
							break;
						bool inOpen = nodeList[openList[i]].x == nodeList[succNode].x &&
									  nodeList[openList[i]].y == nodeList[succNode].y;

						validNode = !inOpen || tempFVal < fVals[openList[i]];
						replaceNode = inOpen && tempFVal < fVals[openList[i]];
						if (replaceNode)
						{
							replaceInt = i;
							break;
						}
					}

					if (replaceNode)
					{
						openList.Remove(openList[replaceInt]);
					}

					if (validNode)
					{
						openList.Add(succNode);
						enterNodeInfo(succNode, minFNode, tempGVal, tempHVal);
					}
				}
				count++;
			}
		
			closedList.Add(minFNode);
		}

		if (goalNodeFound)
			fillChosenNodes(nodeList, chosenNodes, parentNodes, startNode, goalNode, backwardsPath);
		else
			Debug.Log("Path not Found " + pathRun + " " + pathID);
		
		return chosenNodes;
	}

	//records the info of a new node in different arrays that hold data for pathNodes
	//currNode: index of currNode in nodeList
	//parentNode: index of currNode's parent node in nodeList
	//gVal: g value of currNode
	//hVal: h value of currNode
	private void enterNodeInfo(int currNode, int parentNode, float gVal, float hVal)
	{
		parentNodes[currNode] = parentNode;
		gVals[currNode] = gVal;
		hVals[currNode] = hVal;
		fVals[currNode] = gVal + hVal;
	}

	//Finds the closest node to startNode and goalNode.  If startNode has a closer node
	//then path is drawn as normal.  If goalNode has a closer node then the path is drawn from
	//goalNode to startNode.
	//   startNode: node at the position of the object this path is being drawn for
	//   goalNode: node at the position of the target of the object this path is being drawn for
	//   nodeList: list of all path nodes in the game world.
	private int determinePathDirection(int startNode, int goalNode, List<Vector2> nodeList)
	{
		int minStartNode = 0;
		int minGoalNode = 0;
		float minStartDist = Vector2.Distance(nodeList[minStartNode], nodeList[startNode]);
		float minGoalDist = Vector2.Distance(nodeList[minGoalNode], nodeList[goalNode]);

		for (int i = 1; i < nodeList.Count - 2; i++)//search for the closest PathNode to startNode and goalNode
		{
			if (Vector2.Distance(nodeList[i], nodeList[startNode]) < minStartDist)
			{
				minStartNode = i;
				minStartDist = Vector2.Distance(nodeList[i], nodeList[startNode]);
			}

			if (Vector2.Distance(nodeList[i], nodeList[goalNode]) < minGoalDist)
			{
				minGoalNode = i;
				minGoalDist = Vector2.Distance(nodeList[i], nodeList[goalNode]);
			}
		}

		if (minStartDist < minGoalDist)
		{
			nodeList.Add(new Vector2(1, 0));//added so it can be known that minStartDist was less.
			return minStartNode;
		}
		else
		{
			nodeList.Add(new Vector2(0, 0));
			return minGoalNode;
		}
	}

	//fills chosenNodes with all of the nodes that make up the final path
	//chosenNodes: list to hold the nodes
	//startNode: the first node in the path
	//goalNode: the last node in the path
	private void fillChosenNodes(List<Vector2> nodeList, List<Vector2> chosenNodes, int[] parentNodes, int startNode, int goalNode, bool backwardsPath)
	{
		int currentNode = goalNode;

		while (currentNode != startNode)
		{
			if (backwardsPath)
				chosenNodes.Add(nodeList[currentNode]);
			else
				chosenNodes.Insert(0, nodeList[currentNode]);
			currentNode = parentNodes[currentNode];
		}
		int losCount = 0;
		Vector2 startPos = nodeList[startNode];

		//the following code modifies the path by removing any unecessary nodes that only exist because the path started from the node closest to startNode,
		//and not startNode

		//Starting with the second node in the path, do a line of sight check between said node and the startNode, keeping moving along the path
		//untill a node not in line of sight of startNode is found

		for (int i = 1; i < chosenNodes.Count - 1; i++)
		{
			RaycastHit2D losCheck = Physics2D.Raycast(startPos, new Vector2(chosenNodes[i].x - startPos.x, chosenNodes[i].y - startPos.y), 
				Vector2.Distance(nodeList[startNode], chosenNodes[i]), losMask);
			if (!losCheck)
				losCount++;
			else
				break;
		}

		//remove all nodes in the path that come before the last node within line of sight of startNode
		for (int i = 0; i < losCount; i++)
		{
			chosenNodes.Remove(chosenNodes[i]);
		}

	}
}
