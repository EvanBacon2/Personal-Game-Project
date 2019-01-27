using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Script which does all the work related to placing all of the path nodes in a scene
//To give a basic overview of how nodes are place, first each asteroid object in the scene
//is given four nodes placed to the N, E, S, an W of it.  After all of these nodes are placed
//in what is thier default position the script then begins to check for any possible cases where
//node deletion/combination/addition might be needed.
public class TestObjectSpawner : MonoBehaviour 
{
	public static Asteroid asters;//contains asteroids locations and a method for instantiating asteroids
	private int asterCount;//total # of asteroids
	private int nodeGap = 35;//distance between a node and the edge of an asteroids AABB
	private GameObject[] asteroids;//array of all asteroids in the scene
	private Vector2[,] asterNodes;//array of the cordinates for the nodes of each asteroid
	private Vector2[,] addedAsterNodes;//array of the cordinates for the added nodes of each asteroid
	public static List<Vector2> nodeList = new List<Vector2>();
	public static Dictionary<int, bool> nodeLosLog = new Dictionary<int, bool>();//keys are of the form i + j
	//where i and j are the indexes of the two nodes in nodeList, values are true when the nodes are in los and false otherwise
	private bool[,] asterDeletes;//keeps track of which nodes on each asteroid have been deleted
	private bool[,] asterCombs;//keeps track of which nodes on each asteroid have been combined
	private bool[,] asterAdds;//keeps track of which nodes on each asteroid have been given additions
	private GameObject LOSBlocker;//object that will hold all trigger colliders used to block los checks.
	private LayerMask losMask;//contains all physics layers a los raycast wil check against
	private LayerMask wallMask;//contains all objects which act as walls/barriers that block the players movement
	private LayerMask playerMask;//contains the players ship and any objects associated with it. i.e. its shields

	//Common Parameters
	//	anchorID: ID of the anchor asteroid
	//	orbitID: ID of the orbit asteroid
	//	anchorSide: side of the anchor asteroid that the node of interest lies on
	//	orbitSide: sid of the orbit asteroids that the nodes of interest lie on
	//	anchorBounds: dimensions of the anchor asteroid's AABB
	//	orbitBounds: dimensions of the orbit asteroids's AABB

	void Start() 
	{
		asters = ScriptableObject.CreateInstance(typeof(Asteroid)) as Asteroid;
		asterCount = asters.asterPos.Count;
		asteroids = new GameObject[asterCount];
		asterNodes = new Vector2[asterCount, 8];//[0] = North, [1] = NE, [2] = East, [3] = SE, [4] = South, [5] = SW, [6] = West, [7] = NW
		addedAsterNodes = new Vector2[asterCount, 8];
		asterDeletes = new bool[asterCount, 8];
		asterCombs = new bool[asterCount, 8];
		asterAdds = new bool[asterCount, 8];
		LOSBlocker = GameObject.Find("LOS Blocker");
		wallMask = LayerMask.GetMask("Wall");//Physics layer that all Wall objects are in
		playerMask = LayerMask.GetMask("Player");//Physics layer that the player's ship is in
		losMask = wallMask | playerMask;//Combination of wallMask and playerMask

		for (int i = 0; i < asterCount; i++)//load asteroids into scene
		{
			asteroids[i] = asters.createAsteroid(i, 2);
			spawnInitNodes(i);
		}

		asterSearch(true, false, false);//search for nodes that need deleting
		asterSearch(false, true, false);//search for nodes that need combining
		asterSearch(false, false, true);//search for asteroids that need additional nodes
		fillNodeLosLog();//does a los check between every node pair in the scene and stores the result in a dictionary
	}

	//Spawns 4 nodes for each cardinal direction around an asteroid
	//	asterID: number corresponding to an asteroid's index in asteroids
	private void spawnInitNodes(int asterID) 
	{
		Bounds asterBounds = asteroids[asterID].GetComponent<Collider2D>().bounds;
		asterNodes[asterID, 0] = new Vector2(asterBounds.center.x, asterBounds.max.y + nodeGap);//North Node
		addToNodeList(asterNodes[asterID, 0]);
		asterNodes[asterID, 2] = new Vector2(asterBounds.max.x + nodeGap, asterBounds.center.y);//East Node
		addToNodeList(asterNodes[asterID, 2]);
		asterNodes[asterID, 4] = new Vector2(asterBounds.center.x, asterBounds.min.y - nodeGap);//South Node
		addToNodeList(asterNodes[asterID,4]);
		asterNodes[asterID, 6] = new Vector2(asterBounds.min.x - nodeGap, asterBounds.center.y);//West Node
		addToNodeList(asterNodes[asterID,6]);
		asterNodes[asterID, 1] = new Vector2(asterBounds.max.x + nodeGap, asterBounds.max.y + nodeGap);//North East Node
		addToNodeList(asterNodes[asterID, 1]);
		asterNodes[asterID, 3] = new Vector2(asterBounds.max.x + nodeGap, asterBounds.min.y - nodeGap);//South East Node
		addToNodeList(asterNodes[asterID, 3]);
		asterNodes[asterID, 5] = new Vector2(asterBounds.min.x - nodeGap, asterBounds.min.y - nodeGap);//South West Node
		addToNodeList(asterNodes[asterID, 5]);
		asterNodes[asterID, 7] = new Vector2(asterBounds.min.x - nodeGap, asterBounds.max.y + nodeGap);//North West Node
		addToNodeList(asterNodes[asterID, 7]);
		//create place holder nodes for all of the diagonal nodes of this asteroid, asteroids will not have
		//diagonal nodes unless they go through a digaonal combination or addition
		for (int i = 0; i < 8; i++)
		{
			addedAsterNodes[asterID, i] = asterBounds.center;
		}
	}

	//Goes through every possible pair of nodes and checks if they are in line of sight
	//results from these checks are stored in nodeLosLog, see nodeLosLog's initilization for key/value meanings
	private void fillNodeLosLog()
	{
		for (int i = 1; i <= nodeList.Count; i++)
		{
			for (int j = i + 1; j <= nodeList.Count; j++)
			{
				float losDist = Vector2.Distance(nodeList[i - 1], nodeList[j - 1]);
				Vector2 losDir = new Vector2(nodeList[j - 1].x - nodeList[i - 1].x, nodeList[j - 1].y - nodeList[i - 1].y);
				RaycastHit2D losRay = Physics2D.Raycast(nodeList[i - 1], losDir, losDist, wallMask);
				int losPair = (i * 1000) + j;
				//Debug.Log(losPair);
				nodeLosLog.Add(losPair, !losRay);
			}
		}
		Debug.Log(nodeList.Count);
	}

	void Update() 
	{
		displayNodes();
	}

	//displays all of the current nodes in the scene
	private void displayNodes() 
	{
		//displays nodes using nodeList, used by default
		for (int i = 0; i < nodeList.Count; i++)
		{
			Vector2 startLocV = new Vector2(nodeList[i].x, nodeList[i].y + 2);
			Vector2 startLocH = new Vector2(nodeList[i].x + 2, nodeList[i].y);
			//creates a green cross denoting a nodes location
			Debug.DrawRay(startLocV, new Vector2(0,-4), Color.green);
			Debug.DrawRay(startLocH, new Vector2(-4,0), Color.green);
		}

		//displays lines between every node pair that is within los of
		//one another, used for debugging
		/*for (int i = 1; i <= nodeList.Count; i++)
		{
			for (int j = i + 1; j <= nodeList.Count; j++)
			{
				if (nodeLosLog[(i * 100) + j])
				{
					Debug.Log((i * 100) + j);
					Debug.DrawLine(nodeList[i - 1], nodeList[j - 1]);
				}
			}
		}*/
	
		//displays nodes using asterNodes, only used to check and see if nodeList
		//is being properly updated. ie are the any nodes that aren't being removed or
		//added to nodeList when they should
		/*for (int i = 0; i < asterCount; i++)
		{
			for (int j = 0; j < 8; j ++)
			{
				Color col;
				if (j % 2 == 0)
					col = Color.green;
				else
					col = Color.blue;
				if (asterNodes[i, j] != null)
				{
					Vector2 startLocV = new Vector2(asterNodes[i, j].x, asterNodes[i, j].y + 2);
					Vector2 startLocH = new Vector2(asterNodes[i, j].x + 2, asterNodes[i, j].y);
					Debug.DrawRay(startLocV, new Vector2(0,-4), col);
					Debug.DrawRay(startLocH, new Vector2(-4,0), col);

					Vector2 addedStartLocV = new Vector2(addedAsterNodes[i, j].x, addedAsterNodes[i, j].y + 2);
					Vector2 addedStartLocH = new Vector2(addedAsterNodes[i, j].x + 2, addedAsterNodes[i, j].y);
					Debug.DrawRay(addedStartLocV, new Vector2(0,-4), Color.red);
					Debug.DrawRay(addedStartLocH, new Vector2(-4,0), Color.red);
				}
			}
		}*/
	}

	//Searches through every pair of asteroid, either deleting or combining nodes based on combine's value
	//delete: true when asterSearch is being called to check for nodes that need to be deleted
	//combine: true when asterSearch is being called to check for nodes that need to be combined
	//add: true when asterSearch is being called to check for nodes that need to be added
	private void asterSearch(bool delete, bool combine, bool add)
	{
		//Cardinal vs. Diagonal Asteroids
		//to check if two asteroids are diagonally alligned the arc tangent of the difference in y position divided
		//by the difference in x posiiton is found, or more simply Mathf.Atan(xDiff, yDiff). this angle, once converted to degrees,
		//is then modded by 90.  If the end result is between 40 and 50 degrees then the two asteroids are said to be aligned along a diagonal
		//otherwise they are aligned along a cardinal.

		//distances used to check if two asteroids are within the required distance range for an action to take place.  Cardinal distances are used
		//when asteroids are aligned along a cardinal, and diagonal when they are aligned along a diagonal
		int cardinalDeleteDist = 85;
		int diagonalDeleteDist = 121;
		int cardinalCombineDist = 150;
		int diagonalCombineDist = 213;
		int minCardinalAddDist = 150;
		int maxCardinalAddDist = 215;
		int minDiagonalAddDist = 225;
		int maxDiagonalAddDist = 319;

		for (int i = 0; i < asterCount - 1; i++)
		{
			List<Bounds>[] deleteList = new List<Bounds>[8];//all the asteroids that are close enough for deletion, organized by their orientation relative to
															//the anchor asteroid. [0] = North [1] = East etc...
			List<int>[] deleteIDs = new List<int>[8];//asterID's for asteroids in deleteList
			List<Bounds>[] combList = new List<Bounds>[8];//all the asteroids that are close enough for combination, organized by their orientation relative to
														  //the anchor asteroid. [0] = North [1] = East etc...
			List<int>[] combIDs = new List<int>[8];//asterID's for asteroids in combList
			List<Bounds>[] addList = new List<Bounds>[8];//all the asteroids that are close enough for addition, organized by their orientation relative to
			//the anchor asteroid. [0] = North [1] = East etc...
			List<int>[] addIDs = new List<int>[8];//asterID's for asteroids in addList
			int[] anchorSides = new int[8] {4, 5, 6, 7, 0, 1, 2, 3};//each index represents a value for orbitSide and contains the corresponding anchorSide that
			//the orbitSide would face. eg index 0 is for when orbitSide is the north side, in this case the orbit asteroid would be facing the anchor asteroids
			//south side, thus the value of 4.

			for (int j = 0; j < 8; j++)//initiate all of the indices in the lists
			{
				deleteList[j] = new List<Bounds>();
				deleteIDs[j] = new List<int>();
				combList[j] = new List<Bounds>();
				combIDs[j] = new List<int>();
				addList[j] = new List<Bounds>();
				addIDs[j] = new List<int>();
			}

			Bounds anchorBounds = asteroids[i].GetComponent<Collider2D>().bounds;
			for (int j = i + 1; j < asterCount; j++)//search through all the other asteroids
			{
				Bounds orbitBounds = asteroids[j].GetComponent<Collider2D>().bounds;
				float asterDist = Vector2.Distance(anchorBounds.center, orbitBounds.center);

				if (delete && asterDist < diagonalDeleteDist)//criteria for node deletion
				{
					int orbitSide = addToChangeList(i, j, anchorBounds, orbitBounds, 0, cardinalDeleteDist, deleteList, true, false);
					if (orbitSide != -1)//orbitSide couldn't be found, result of one of the nodes having been previously modified
						deleteIDs[orbitSide].Add(j);
				}
				if (combine && asterDist > cardinalDeleteDist && asterDist < diagonalCombineDist)//criteria for node combination
				{
					int orbitSide = addToChangeList(i, j, anchorBounds, orbitBounds, 0, cardinalCombineDist, combList, false, false);
					if (orbitSide != -1)//orbitSide couldn't be found, result of one of the nodes having been previously modified
						combIDs[orbitSide].Add(j);
				}
				if (add && asterDist >= minCardinalAddDist && asterDist < maxDiagonalAddDist)//criteria for node addition
				{
					int orbitSide = addToChangeList(i, j, anchorBounds, orbitBounds, minDiagonalAddDist, maxCardinalAddDist, addList, false, true);
					if (orbitSide != -1)//orbitSide couldn't be found, result of one of the nodes having been previously modified
						addIDs[orbitSide].Add(j);
				} 
			}
				
			for (int j = 0; j < 8; j++)//for each side of the anchor asteroid check if the anchorNode and corresponding orbitNode should be deleted
			{
				if (deleteList[j].Count != 0)
				{
					if (j % 2 == 0)//node is on a cardinal direction
						nodeDelete(i, deleteIDs[j], anchorSides[j], j, anchorBounds, deleteList[j]);
					else//node is on a diagonal direction
						nodeDeleteDiagonal(i, deleteIDs[j], anchorSides[j], j, anchorBounds, deleteList[j]);
				}
			}
			for (int j = 0; j < 8; j++)//for each side of the anchor asteroid check if the anchorNode and corresponding orbitNode should be combined
			{
				if (combList[j].Count != 0)
				{
					if (j % 2 == 0)//node is on a cardinal direction
						nodeCombination(i, combIDs[j], anchorSides[j], j, anchorBounds, combList[j]);
					else//node is on a diagonal direction
						nodeCombinationDiagonal(i, combIDs[j], anchorSides[j], j, anchorBounds, combList[j]);
				}
			}
			for (int j = 0; j < 8; j++)//for each side of the anchor asteroid check if a node should be added to that side
			{
				if (addList[j].Count != 0)
				{
					if (j % 2 == 0)//node is on a cardinal direction
						nodeAddition(i, addIDs[j], anchorSides[j], j, anchorBounds);
					else//node is on a diagonal direction
						nodeAdditionDiagonal(i, addIDs[j], anchorSides[j], j, anchorBounds);
				}
			}
		}
	}
		
	//Adds an asteroid to combList so one of its nodes can later be combined with an anchorNode
	//  changeList: contains all of the orbit asteroids that need a specific modification made to their nodes
	//returns the side of the orbit asteroid that the potential combination node lies on
	private int addToChangeList(int anchorID, int orbitID, Bounds anchorBounds, Bounds orbitBounds, int minDist, int cardinalChangeDist, 
								List<Bounds>[] changeList, bool delete, bool addition) 
	{
		float asterDist = Vector2.Distance(orbitBounds.center, anchorBounds.center);
		float xDiff = orbitBounds.center.x - anchorBounds.center.x;
		float yDiff = orbitBounds.center.y - anchorBounds.center.y;
		float anchorOrbitAng = Mathf.Atan(yDiff / xDiff) * Mathf.Rad2Deg;
		float targetAngDiff = Mathf.Abs(anchorOrbitAng % 90);
		int orbitSide = -1;

		if (targetAngDiff > 40 && targetAngDiff <= 50 && asterDist >= minDist)//if angle between the x axis and the line through the center of the anchor 
		{											 						  //and orbit asteroid is within 5 degrees of 45, 135, 225, or 315 degrees
			orbitSide = addToChangeListDiagonal(anchorID, orbitID, anchorBounds, orbitBounds, xDiff, yDiff, changeList, delete, addition);
			return orbitSide;
		}

		if (asterDist < cardinalChangeDist)
		{
			if (Mathf.Abs(xDiff) > Mathf.Abs(yDiff))
			{
				if (xDiff < 0 && isUnChanged(anchorID, orbitID, 6, 2, anchorBounds, orbitBounds, delete, addition))
				{											 //orbit asteroid is to the West of the anchor asteroid
					changeList[2].Add(orbitBounds);
					orbitSide = 2;
				} else if (xDiff > 0 && isUnChanged(anchorID, orbitID, 2, 6, anchorBounds, orbitBounds, delete, addition))
				{                                                   //orbit asteroid is to the East of the anchor asteroid
					changeList[6].Add(orbitBounds);
					orbitSide = 6;
				}
			} else if (Mathf.Abs(xDiff) < Mathf.Abs(yDiff))
			{
				if (yDiff < 0 && isUnChanged(anchorID, orbitID, 4, 0, anchorBounds, orbitBounds, delete, addition))
				{                                           //orbit asteroid is to the South of the anchor asteroid
					changeList[0].Add(orbitBounds);
					orbitSide = 0;
				} else if (yDiff > 0 && isUnChanged(anchorID, orbitID, 0, 4, anchorBounds, orbitBounds, delete, addition))
				{                                                  //orbit asteroid is to the North of the anchor asteroid
					changeList[4].Add(orbitBounds);
					orbitSide = 4;
				}
			}
		}
		return orbitSide;
	}

	//checks if a anchor/orbit pair of nodes has been previously combined or deleted.
	//  delete: true when asterSearch was called to look for nodes that should be deleted
	//  addition: true when asterSearch was called to look for places where nodes should be added
	//returns whether or not both the anchorNode and orbitNode are unChanged
	private bool isUnChanged(int anchorID, int orbitID, int anchorSide, int orbitSide, Bounds anchorBounds, Bounds orbitBounds, bool delete, bool addition)
	{
		bool result = true;
		if (!delete)//if currently performing combination/addition check to see if any of the nodes have already been deleted
			result = !asterDeletes[anchorID, anchorSide] && !asterDeletes[orbitID, orbitSide];
		if (!addition)//if currently performing combination, check to see if any of the nodes hvae already been combined
			result = result && !asterCombs[anchorID, anchorSide] && !asterCombs[orbitID, orbitSide];
		
		//if currently performing addition, check to see if anchorSide or orbitSide already contains an added node
		result = result && !asterAdds[anchorID, anchorSide] && !asterAdds[orbitID, orbitSide];
		return result;
	}

	//adds an orbit asteroid to a changeList so that the appropriate modication can be made to its nodes
	//	xDiff: difference between the x cords of the anchor and orbit asteroids center
	//	yDiff: difference between the y cords of the anchor and orbit asteroids center
	//  changeList: contains all of the orbit asteroids that need a specific modification made to their nodes
	//  delete: true when asterSearch was called to look for nodes that should be deleted
	//  addition: true when asterSearch was called to look for places where nodes should be added
	//returns the side of the orbit asteroid that the potential combination node lies on
	private int addToChangeListDiagonal(int anchorID, int orbitID, Bounds anchorBounds, Bounds orbitBounds, float xDiff, float yDiff, 
										List<Bounds>[] changeList, bool delete, bool addition)
	{
		int orbitSide = -1;
		if (xDiff > 0 && yDiff > 0 && isUnchangedDiagonal(anchorID, orbitID, 0, 2, 4, 6, anchorBounds, orbitBounds, delete, addition))
		{																		  //orbit asteroid is to the NE of the anchor asteroid
			changeList[5].Add(orbitBounds);	
			orbitSide = 5;
		} else if (xDiff > 0 && yDiff < 0 && isUnchangedDiagonal(anchorID, orbitID, 4, 2, 0, 6, anchorBounds, orbitBounds, delete, addition))
		{																				 //orbit asteroid is to the SE of the anchor asteroid
			changeList[7].Add(orbitBounds);
			orbitSide = 7;
		} else if (xDiff < 0 && yDiff < 0 && isUnchangedDiagonal(anchorID, orbitID, 4, 6, 0, 2, anchorBounds, orbitBounds, delete, addition))
		{																				 //orbit asteroid is to the SW of the anchor asteroid
			changeList[1].Add(orbitBounds);
			orbitSide = 1;
		} else if (xDiff < 0 && yDiff > 0 && isUnchangedDiagonal(anchorID, orbitID, 0, 6, 4, 2, anchorBounds, orbitBounds, delete, addition))
		{                                                                                //orbit asteroid is to the NW of the anchor asteroid
			changeList[3].Add(orbitBounds);
			orbitSide = 3;
		}
		return orbitSide;
	}

	//checks if all of the cardinal nodes next to the diagonal side being modified haven't been combined or deleted
	//eg. if diagonal anchorside is NE then cardinal anchorSides would be N and E, orbitSides would be S and W.
	//	anchorSide1/2: the two sides of anchor asteroid that are next to the diagonal side being modified
	//	orbitSide1/2: the two sides of aorbit asteroid that are next to the diagonal side being modified
	//  delete: true when asterSearch was called to look for nodes that should be deleted
	//  addition: true when asterSearch was called to look for places where nodes should be added
	//returns whether or not all nodes looked at are unchanged
	private bool isUnchangedDiagonal(int anchorID, int orbitID, int anchorSide1, int anchorSide2, int orbitSide1, int orbitSide2, 
									 Bounds anchorBounds, Bounds orbitBounds, bool delete, bool addition)
	{
		bool result;
		int anchorDiagonal = (anchorSide1 + anchorSide2) / 2;
		int orbitDiagonal = (orbitSide1 + orbitSide2) / 2;

		//check to see if a node hasn't already been added to the diagonal side of either asteroid
		result = isUnChanged(anchorID, orbitID, anchorDiagonal, orbitDiagonal, anchorBounds, orbitBounds, false, true);

		if (!delete)//if combining or adding nodes, check to see that none of the nodes next to the diagonals of either asteroid have been deleted
			result = result && !asterDeletes[anchorID, anchorSide1] && !asterDeletes[anchorID, anchorSide2] &&
					 !asterDeletes[orbitID, orbitSide1] && !asterDeletes[orbitID, orbitSide2];
		return result;
	}

	//Deletes nodes from aster1 and aster2 that would otherwise be located in an impassable
	//gap created by the two asteroids
	//  deleteIDs: asterID's for all of the orbit asteroids
	//  deleteList: all of the orbit asteroids on a given orbitSide that have nodes to be deleted
	private void nodeDelete(int anchorID, List<int> deleteIDs, int anchorSide, int orbitSide, Bounds anchorBounds, List<Bounds> deleteList) 
	{
		removeFromNodeList(anchorID, anchorSide);
		replaceNode(anchorBounds.center, anchorID, anchorSide, asterDeletes);
		removeFromNodeList(anchorID, ((anchorSide - 1 + 8) % 8));
		replaceNode(anchorBounds.center, anchorID, ((anchorSide - 1 + 8) % 8), asterDeletes);
		removeFromNodeList(anchorID, anchorSide + 1);
		replaceNode(anchorBounds.center, anchorID, anchorSide + 1, asterDeletes);
		for (int i = 0; i < deleteList.Count; i++)
		{
			removeFromNodeList(deleteIDs[i], orbitSide);
			replaceNode(deleteList[i].center, deleteIDs[i], orbitSide, asterDeletes);
			removeFromNodeList(deleteIDs[i], ((orbitSide - 1 + 8) % 8));
			replaceNode(deleteList[i].center, deleteIDs[i], ((orbitSide - 1 + 8) % 8), asterDeletes);
			removeFromNodeList(deleteIDs[i], orbitSide + 1);
			replaceNode(deleteList[i].center, deleteIDs[i], orbitSide + 1, asterDeletes);
			addLOSBlock(anchorID, deleteIDs[i], anchorSide);
		}
	}

	//Deletes the node on the two sides next to the corner of each asteroid that are found on the inside of the impassable gap created
	//by the two asteroids.
	//  deleteIDs: asterID's for all of the orbit asteroids
	//  deleteList: all of the orbit asteroids on a given orbitSide that have nodes to be deleted
	private void nodeDeleteDiagonal(int anchorID, List<int> deleteIDs, int anchorSide, int orbitSide, Bounds anchorBounds, List<Bounds> deleteList)
	{
		for (int i = 0; i < deleteList.Count; i++)
		{
			deleteNodeDiagonal(anchorID, deleteIDs[i], anchorSide, orbitSide, anchorBounds, deleteList[i], asterDeletes);
			addLOSBlock(anchorID, deleteIDs[i], anchorSide);
		}
	}

	//Creates a new node between the locations of a node from anchor asteroid and the orbit nodes, and deletes
	//the two old nodes.
	//  combIDs: asterID's for all of the orbit asteroids
	//  combList: all of the orbit asteroids on a given orbitSide that have nodes to be combined
	private void nodeCombination(int anchorID, List<int> combIDs, int anchorSide, int orbitSide, Bounds anchorBounds, List<Bounds> combList)
	{
		Vector2 combNode = createCombNode(anchorID, combIDs, anchorSide, orbitSide, anchorBounds);
		removeFromNodeList(anchorID, anchorSide);
		addToNodeList(combNode);
		replaceNode(combNode, anchorID, anchorSide, asterCombs);

		for (int i = 0; i < combIDs.Count; i++)
		{
			removeFromNodeList(combIDs[i], orbitSide);
			replaceNode(combNode, combIDs[i], orbitSide, asterCombs);
		}
	}

	//Creates a new node between the location of a corner from anchor asteroid and the orbit corners, all nodes adjacent to the diagonal from each
	//asteroid are deleted.
	//  combIDs: asterID's for all of the orbit asteroids
	//  combList: all of the orbit asteroids that have a node on a given orbitSide to be combined
	private void nodeCombinationDiagonal(int anchorID, List<int> combIDs, int anchorSide, int orbitSide, Bounds anchorBounds, List<Bounds> combList)
	{
		Vector2 diagonalNode = createCombNodeDiagonal(anchorID, combIDs, anchorSide, orbitSide, anchorBounds);
		addToNodeList(diagonalNode);
		//replaceNodeDiagonal(diagonalNode, anchorID, anchorSide, anchorBounds, asterCombs);
		replaceNode(diagonalNode, anchorID, anchorSide, asterCombs);

		for (int i = 0; i < combIDs.Count; i++)
		{
			//replaceNodeDiagonal(diagonalNode, combIDs[i], orbitSide, combList[i], asterCombs);
			replaceNode(diagonalNode, combIDs[i], orbitSide, asterCombs);
		}
	}

	//Creates a new node between the locations of a node from anchor asteroid and the orbit nodes
	//  addIDs: asterID's for all of the orbit asteroids
	//  addList: all of the orbit asteroids on a given orbitSide that have nodes to be added
	private void nodeAddition(int anchorID, List<int> addIDs, int anchorSide, int orbitSide, Bounds anchorBounds)
	{
		Vector2 addedNode = createCombNode(anchorID, addIDs, anchorSide, orbitSide, anchorBounds);
		addedAsterNodes[anchorID, anchorSide] = addedNode;
		addToNodeList(addedNode);
		asterAdds[anchorID, anchorSide] = true;
		for (int i = 0; i < addIDs.Count; i++)
		{
			addedAsterNodes[addIDs[i], orbitSide] = addedNode;
			asterAdds[addIDs[i], orbitSide] = true;
		}
	}

	//Creates a new node between the location of a corner from anchor asteroid and the orbit corners
	//  addIDs: asterID's for all of the orbit asteroids
	//  addList: all of the orbit asteroids on a given orbitSide that have nodes to be added
	private void nodeAdditionDiagonal(int anchorID, List<int> addIDs, int anchorSide, int orbitSide, Bounds anchorBounds)
	{
		Vector2 addedNode = createCombNodeDiagonal(anchorID, addIDs, anchorSide, orbitSide, anchorBounds);
		addedAsterNodes[anchorID, anchorSide] = addedNode;
		asterAdds[anchorID, anchorSide] = true;
		addToNodeList(addedNode);
		for (int i = 0; i < addIDs.Count; i++)
		{
			addedAsterNodes[addIDs[i], orbitSide] = addedNode;
			asterAdds[addIDs[i], orbitSide] = true;
		}
	}

	//Uses the location of the anchor node on anchorSide and all of the orbit nodes on orbit to pick a location for a new
	//node to replace them.
	//returns the position of the new node
	private Vector2 createCombNode(int anchorID, List<int> orbitIDs, int anchorSide, int orbitSide, Bounds anchorBounds)
	{
		Vector2 combNode = new Vector2();
		Vector2 anchorNode = defaultNodePos(anchorBounds, anchorSide);
		List<Vector2> orbitNodes = new List<Vector2>();
		for (int i = 0; i < orbitIDs.Count; i++)
		{
			orbitNodes.Add(defaultNodePos(asteroids[orbitIDs[i]].GetComponent<Collider2D>().bounds, orbitSide));
		}
		float farMidPoint = (findFarthestNode(orbitSide, anchorNode, orbitNodes, true) + findFarthestNode(orbitSide, anchorNode, orbitNodes, false)) / 2;
		float minMidPoint = findminOrbitNode(orbitSide, anchorNode, orbitNodes);

		if (orbitSide == 0 || orbitSide == 4)
			combNode = new Vector2(farMidPoint, (minMidPoint + anchorNode.y) / 2);
		else if (orbitSide == 2 || orbitSide == 6)
			combNode = new Vector2((minMidPoint + anchorNode.x) / 2, farMidPoint);

		return combNode;
	}

	//returns the node that is closest to anchorNode along the y axis if orbitSide is N or S, or closest along the x axis if orbitSide
	//is E or W.
	//anchorNode: node on the anchor Asteroid that faces the nodes on orbitSide
	private float findminOrbitNode(int orbitSide, Vector2 anchorNode, List<Vector2> orbitNodes)
	{
		float minOrbitCord = 0;
		if (orbitSide == 0 || orbitSide == 4)//orbitNodes are on the North or South of thier asteroid
			minOrbitCord = orbitNodes[0].y;
		else if (orbitSide == 2 || orbitSide == 6)//orbitNodes are on the East or West side of their asteroid
			minOrbitCord = orbitNodes[0].x;
				
		for (int i = 1; i < orbitNodes.Count; i++)
		{
			float currOrbitCord = 0;
			if (orbitSide == 0 || orbitSide == 4)
				currOrbitCord = orbitNodes[i].y;
			else if (orbitSide == 2 || orbitSide == 6)
				currOrbitCord = orbitNodes[i].x;

			if (Mathf.Abs(currOrbitCord) < Mathf.Abs(minOrbitCord))
				minOrbitCord = currOrbitCord;
		}
		return minOrbitCord;
	}

	//returns the node that is farthest from anchorNode along the x axis if orbitSide is N or S, or along the y acis if orbitSide 
	//is E or W
	//anchorCord: either the x or y component of anchorNode's location
	//positiveDist: if true find the orbitNode that is farthest up or to the right from anchorNode, if false that down or to the left.
	private float findFarthestNode(int orbitSide, Vector2 anchorNode, List<Vector2> orbitNodes, bool positiveDist)
	{
		float farthestOrbitCord = 0;
		if (orbitSide == 0 || orbitSide == 4)//orbitNodes are on the North or South of thier asteroid
			farthestOrbitCord = anchorNode.x;
		else if (orbitSide == 2 || orbitSide == 6)//orbitNodes are on the East or West side of their asteroid
			farthestOrbitCord = anchorNode.y;

		for (int i = 0; i < orbitNodes.Count; i++)
		{
			float currOrbitCord = 0;
			if (orbitSide == 0 || orbitSide == 4)
				currOrbitCord = orbitNodes[i].x;
			else if (orbitSide == 2 || orbitSide == 6)
				currOrbitCord = orbitNodes[i].y;
			
			if (currOrbitCord > farthestOrbitCord && positiveDist)
				farthestOrbitCord = currOrbitCord;
			else if (currOrbitCord < farthestOrbitCord && !positiveDist)
				farthestOrbitCord = currOrbitCord;
		}
		return farthestOrbitCord;
	}

	//Uses the location of the anchor asteroids Corner on anchorSide and all of the orbit corners on orbitSide 
	//to pick a location for a new node
	//returns the new node
	private Vector2 createCombNodeDiagonal(int anchorID, List<int> orbitIDs, int anchorSide, int orbitSide, Bounds anchorBounds)
	{
		Vector2 combNode = new Vector2();
		Vector2 anchorCorner = getAsterCorner(anchorID, anchorSide);
		List<Vector2> orbitCorners = new List<Vector2>();
		for (int i = 0; i < orbitIDs.Count; i++)
		{
			orbitCorners.Add(getAsterCorner(orbitIDs[i], orbitSide));
		}
		float minDist = findMinDist(anchorCorner, orbitCorners);
		if (anchorSide == 1)
			combNode = new Vector2(anchorCorner.x + minDist, anchorCorner.y + minDist);
		else if (anchorSide == 3)
			combNode = new Vector2(anchorCorner.x + minDist, anchorCorner.y - minDist);
		else if (anchorSide == 5)
			combNode = new Vector2(anchorCorner.x - minDist, anchorCorner.y - minDist);
		else if (anchorSide == 7)
			combNode = new Vector2(anchorCorner.x - minDist, anchorCorner.y + minDist);
		return combNode;
	}

	//finds the orbitCorner that is closest to anchorCorner
	//	anchorCorner: corner on anchor asteroid that lies on the diagonal being modified
	//	orbitCorners: list of corners on the orbit asteroids that lie on the diagonal being modified
	//	returns the length of the x/y combonent of the distance between anchorCorner and the closest orbitCorner
	private float findMinDist(Vector2 anchorCorner, List<Vector2> orbitCorners)
	{
		float minOrbitDist = Vector2.Distance(anchorCorner, orbitCorners[0]);

		for (int i = 1; i < orbitCorners.Count; i++)
		{
			float currOrbitDist = Vector2.Distance(anchorCorner, orbitCorners[i]);

			if (currOrbitDist < minOrbitDist)
				minOrbitDist = currOrbitDist;
		}
			
		return minOrbitDist / Mathf.Sqrt(2) / 2;
	}

	//returns the position of the corner of an asteroids AABB that lies on the diagonal being modified
	private Vector2 getAsterCorner(int asterID, int asterSide)
	{
		Bounds asterBounds = asteroids[asterID].GetComponent<Collider2D>().bounds;
		if (asterSide == 1)
			return new Vector2(asterBounds.max.x, asterBounds.max.y);
		else if (asterSide == 3)
			return new Vector2(asterBounds.max.x, asterBounds.min.y);
		else if (asterSide == 5)
			return new Vector2(asterBounds.min.x, asterBounds.min.y);
		else   //asterSide == 7
			return new Vector2(asterBounds.min.x, asterBounds.max.y);
	}

	//Deletes the nodes on the sides next to the diagonals on which the anchor and orbit asteroid are aligned
	//	anchorSide1/2: the two sides of anchor asteroid that are next to the diagonal side being modified
	//	orbitSide1/2: the two sides of aorbit asteroid that are next to the diagonal side being modified
	//  asterDelets: list that will record which nodes were deleted.
	private void deleteNodeDiagonal(int anchorID, int orbitID, int anchorSide, int orbitSide, 
									Bounds anchorBounds, Bounds orbitBounds, bool[,] asterDeletes )
	{
		removeFromNodeList(anchorID, anchorSide);
		replaceNode(anchorBounds.center, anchorID, anchorSide, asterDeletes);
		removeFromNodeList(anchorID, anchorSide - 1);
		replaceNode(anchorBounds.center, anchorID, anchorSide - 1, asterDeletes);
		removeFromNodeList(anchorID, (anchorSide + 1) % 8);
		replaceNode(anchorBounds.center, anchorID, (anchorSide + 1) % 8, asterDeletes);

		removeFromNodeList(orbitID, orbitSide);
		replaceNode(orbitBounds.center, orbitID, orbitSide, asterDeletes);
		removeFromNodeList(orbitID, orbitSide - 1);
		replaceNode(orbitBounds.center, orbitID, orbitSide - 1, asterDeletes);
		removeFromNodeList(orbitID, (orbitSide + 1) % 8);
		replaceNode(orbitBounds.center, orbitID, (orbitSide + 1) % 8, asterDeletes);
	}

	//Takes the asteroid with asterID and sets the position of its node on asterSide to newNode
	//	newNode: position of the replacement node
	//	changeList: list that will make a recording of which node was modified and the type of modifaction
	private void replaceNode(Vector2 newNode, int asterID, int asterSide, bool[,] changeList)
	{
		asterNodes[asterID, asterSide] = newNode;
		changeList[asterID, asterSide] = true;
	}

	//Takes the asteroid with asterID and sets the position of its two nodes next to asterSide to diagonalNode
	//  diagonalNode: postion of the node to be placed on the diagonal
	//	changeList: list that will make a recording of which asteroid was modified and the type of modifaction
	private void replaceNodeDiagonal(Vector2 diagonalNode, int asterID, int asterSide, Bounds asterBounds, bool[,] changeList)
	{
		//add diagonal node to asterside
		removeFromNodeList(asterID, asterSide);
		replaceNode(diagonalNode, asterID, asterSide, asterCombs);

		removeFromNodeList(asterID, asterSide - 1);
		asterNodes[asterID, asterSide - 1] = asterBounds.center;
		removeFromNodeList(asterID, (asterSide + 1) % 8);//when asterSide == 7 the +1 side should == 0
		asterNodes[asterID, (asterSide + 1) % 8] = asterBounds.center;
	}

	//removes the node located on the side asterSide of asteroid with ID asterID
	private void removeFromNodeList(int asterID, int asterSide)
	{
		if (!asterNodes[asterID, asterSide].Equals(asteroids[asterID].GetComponent<Collider2D>().bounds.center))
			nodeList.Remove(asterNodes[asterID, asterSide]);
	}

	//Adds newNode to nodeList as long as nodeList does not already contain newNode
	//  newNode: node to be added to nodeList
	private void addToNodeList(Vector2 newNode)
	{
		if (!nodeList.Contains(newNode))
			nodeList.Add(newNode);
	}

	//Creates a trigger collider that between any impassable gaps that will block LOS checks
	private void addLOSBlock(int anchorID, int orbitID, int anchorSide)
	{
		Bounds anchorAster = asteroids[anchorID].GetComponent<SpriteRenderer>().bounds;
		Bounds orbitAster = asteroids[orbitID].GetComponent<SpriteRenderer>().bounds;
		GameObject LOSBlocker = new GameObject();
		LOSBlocker.transform.parent = asteroids[anchorID].transform;
		LOSBlocker.layer = 12;//wall layer
		LOSBlocker.AddComponent<BoxCollider2D>();
		BoxCollider2D boxy = LOSBlocker.GetComponent<BoxCollider2D>();
		boxy.isTrigger = true;
		boxy.transform.position = (asteroids[anchorID].transform.position + asteroids[orbitID].transform.position) / 2;
		float boxxy = 0;//x cordinate
		float boxyy = 0;//y cordinate

		if (anchorAster.center.x < orbitAster.min.x)//left edge of orbitAster is to the left of anchorAster's center
			boxxy = Mathf.Abs(anchorAster.max.x - orbitAster.min.x) + .1f;
		else//left edge of orbitAster is to the right of anchorAster's center
			boxxy = Mathf.Abs(anchorAster.min.x - orbitAster.max.x) + .1f;
		
		if (anchorAster.center.y < orbitAster.min.y)//bottom edge of orbitAster is above anchorAster's center
			boxyy = Mathf.Abs(anchorAster.max.y - orbitAster.min.y) + .1f;
		else//bottom edge of orbitAster is below anchorAster's center
			boxyy = Mathf.Abs(anchorAster.min.y - orbitAster.max.y) + .1f;
		boxy.size = new Vector2(boxxy, boxyy);
	}

	//Returns the default node position for a given asteroid side, position is relative
	//to position of asterBounds
	//  asterBounds: AABB of an asteroid
	//  asterSide: side of an asteroid
	private Vector2 defaultNodePos(Bounds asterBounds, int asterSide)
	{
		if (asterSide == 0)
			return new Vector2(asterBounds.center.x, asterBounds.max.y + nodeGap);
		else if (asterSide == 2)
			return new Vector2(asterBounds.max.x + nodeGap, asterBounds.center.y);
		else if (asterSide == 4)
			return new Vector2(asterBounds.center.x, asterBounds.min.y - nodeGap);
		else //asterSide == 6
			return new Vector2(asterBounds.min.x - nodeGap, asterBounds.center.y);
	}
}
