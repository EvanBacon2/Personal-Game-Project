using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Interface implemented by any object which needs path nodes.  Implementing this Interface 
 * allows this object to be added to a NodeGraph.
 */
public interface IFindable 
{
	/*
	 * true if this objects nodes are created using padding, or hard coded locations
	 */
	bool padded
	{
		get;
		set;
	}

	/*
	 * Specifiecs the distance between an objects nodes and the corners of its AABB. 
	 * Only used if padded is set to true.
	 */
	int padding
	{
		get;
		set;
	}

	/*
	 * A list of Vector2's which specify the exact position of each node for an object
	 * Only used if padded is set to false.
	 * Should only be used for objects with more complex shapes where simply placing nodes at
	 * the AABB corners won't lead to an optimal layout of nodes.
	 */
	List<Vector2> points
	{
		get;
	}

}
