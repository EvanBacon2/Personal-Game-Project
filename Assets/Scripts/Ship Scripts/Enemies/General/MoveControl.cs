using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveControl : MonoBehaviour 
{
	/** Objects/Lists **/
	public Transform Target;//object that this object will move towards and follow
	Rigidbody2D rb2d;
	private List<Vector2> targetPath = new List<Vector2>();//path towards Target created by pathMaker
	public List<string> targetTags = new List<string>();//Tags of all objects associated with Target
	private DamageControl dmgControl;
	private List<GameObject> addObjects;//additional objects associated with this object

	/** Rotation **/
	public float rotSpeed;

    /** Pathfinding **/
    public bool sightClear;
	public LayerMask sightMask;
	public LayerMask enemyMask;
	public LayerMask wallMask;
	private PathMaker pathMakr;
	private bool drawPath;
	public float nodeTriggerRadius;
	private int pathStep;//keeps track of which node in the path this object should be moving towards
	private Vector2 currentPathNode;
    private Vector2 lastFramePathNode;

	/** Movement **/
	private Vector2 pos;
	public int moveSpeed;
	//Mod fields are used to increase the relative strength of a given force
	public float targetForceMod;
	public float pathForceMod;
	public float helpForceMod;
	public float enemyForceMod;
	public float wallForceMod;
    public float clearForceMod;
	public float targetDist;
	//Radius fields are used to specificy the size of the area in objects are searched for
	public float helpForceRadius;
    public float helpDashRadius;
	public float enemyForceRadius;
	public float wallForceRadius;
	public float distPriorityMod;
    //fields for Dash Frames
    public bool dashFrame;
    private float dashEndTimeStamp;
    private float dashRechargeTimeStamp;
    private Vector2[] dashList;
    private int dashTo;//holds the index in dashList that will accessed when snapping

	/** Debugging **/
	public bool showVelocity;
	public bool showTargetForce;
	public bool showPathForce;
	public bool showHelpForce;
	public bool showEnemyForce;
	public bool showWallForce;
    public bool showClearForce;
	public bool showPath;

	void Start()
	{
		Target = GameObject.Find("Ship").transform;
		rb2d = GetComponent<Rigidbody2D>();
		pathMakr = new PathMaker();
		dmgControl = GetComponent<DamageControl>();
		addObjects = dmgControl.addObjects;
        dashList = new Vector2[6];
	}

	void Update () 
	{
		if (drawPath && showPath)
		{
			if (pathStep == 0)//draw a line from the objects position to the first path node in targetPath
				Debug.DrawLine(transform.position, targetPath[0]);
			for (int i = pathStep; i < targetPath.Count - 1; i++)
			{
				Debug.DrawLine(targetPath[i], targetPath[i + 1]);
			}
		}

		if (showVelocity)
		{
			Debug.DrawRay(pos, rb2d.velocity.normalized * 100);
		}
	}

	void FixedUpdate()
	{
		pos = transform.position;
		sightClear = inLOS();
        if (!sightClear)
        {
            if (targetPath.Count == 0 || redrawPath(targetPath[targetPath.Count - 2], Target) || redrawPath(currentPathNode, transform))
                generatePath();                 

            if (pathStep < targetPath.Count - 1 && Vector2.Distance(this.transform.position, currentPathNode) < nodeTriggerRadius)
            {
                pathStep++;
                currentPathNode = targetPath[pathStep];
            }
        }
        else
            clearPath();

        setTotalForce();
		//rb2d.AddForce(createTotalForce().normalized * moveSpeed);
	}

    void LateUpdate()
    {
        lastFramePathNode = currentPathNode;
    }

    //Checks to see if this object is within line of sight of Target
    public bool inLOS()
	{
        RaycastHit2D los = Physics2D.Raycast(transform.position, Target.position - transform.position, 
			                                  Vector2.Distance(transform.position, Target.position), sightMask);

        return los && targetTags.Contains(los.transform.tag);
	}

	//Checks to see if this objects path needs to be redrawn by checking to see if body is within LOS of checkNode
	//   body: either Target or the object this script is attached to
	//   checkNode: path node that body will be check with for LOS
	private bool redrawPath(Vector2 checkNode, Transform body)
	{
		RaycastHit2D redrawCheck;

		if (body.tag.Equals(Target.tag))
		{
			redrawCheck = Physics2D.Raycast(checkNode, new Vector2(body.position.x - checkNode.x, body.position.y - checkNode.y), 
				Vector2.Distance(checkNode, body.position), sightMask);
			if (redrawCheck && !targetTags.Contains(redrawCheck.transform.tag))
				return true;
		}
		else
		{
			redrawCheck = Physics2D.Raycast(checkNode, new Vector2(body.position.x - checkNode.x, body.position.y - checkNode.y), 
				Vector2.Distance(checkNode, body.position));
			if (redrawCheck && !redrawCheck.transform.CompareTag(this.tag))
				return true;
		}

		//used to determine why path is being redrawn
		//Debug.Log(body.tag);
		return false;
	}

	//generates a path from this object to Target
	public void generatePath()
	{
		targetPath = pathMakr.makePath(transform.position, Target.position, Target, Target.gameObject.GetComponent<Collider2D>(), gameObject.tag);

		//prints the location of each pathNode included in targetPath
		for (int i = 0; i < targetPath.Count; i++)
		{
			//Debug.Log(targetPath[i].getNodePos());
		}

		if (pathMakr.goalNodeFound)
			drawPath = true;

		currentPathNode = targetPath[0];
		pathStep = 0;
	}

	public void compareNodeAngles()
	{
		Vector2 objectToTarget = new Vector2(Target.position.x - pos.x, Target.position.y - pos.y);
		Vector2 objectToCurrentNode = new Vector2(currentPathNode.x - pos.x, currentPathNode.y - pos.y);
		Vector2 objectToNextNode = new Vector2(targetPath[pathStep + 1].x - pos.x, targetPath[pathStep + 1].y - pos.y);

		float currentNodeAng = Vector2.Angle(objectToTarget, objectToCurrentNode);
		float nextNodeAng = Vector2.Angle(objectToTarget, objectToNextNode);

		if (nextNodeAng < currentNodeAng)
		{
			pathStep++;
			currentPathNode = targetPath[pathStep];
		}
	}

	//resets all fields relatied to pathfinding to default values
	private void clearPath()
	{
		targetPath.Clear();
		currentPathNode = new Vector2();
		pathStep = 0;
		drawPath = false;
	}

	//creats the final single vector that will push this object
	private void setTotalForce()
	{
		Vector2 targetForce = new Vector2();
		Vector2 pathForce = new Vector2();
		Vector2 mainForce = new Vector2();//targetForce when sightClear, pathForce when !sightClear

		if (sightClear)//object well be moving towards their target
		{
			targetForce = createTargetForce();
			mainForce = targetForce;
		}
		else//object will be moving to their currentPathNode
		{
			pathForce = createPathForce();
			mainForce = pathForce;
		}
		Vector2 totalForce = (targetForce * targetForceMod) + 
			                 (pathForce * pathForceMod) + 
			                 (createEnemyForce() * enemyForceMod) + 
			                 (createWallForce() * wallForceMod) +
                             (createClearForce() * clearForceMod);

        if (!sightClear)
            totalForce += createHelpForce(mainForce) * helpForceMod;

        //if (!dashFrame)
       // {
            float mod = Mathf.Abs(Vector2.Angle(rb2d.velocity, totalForce)) / 180f * 500;
            rb2d.AddForce(totalForce.normalized * (moveSpeed + mod));
        //}
        /*else
        {
            if (dashEndTimeStamp > Time.time && dashRechargeTimeStamp <= Time.time)
                rb2d.velocity = dashList[0].normalized * moveSpeed;
            else
            {
                dashFrame = false;
                dashList = new Vector2[6];
                dashRechargeTimeStamp = Time.time + 1;
            }
        }*/

    }


	//Force that will push this object directly to or away from Target
	//COLOR: RED
	private Vector2 createTargetForce()
	{
		Vector2 targetForce = new Vector2();
		if (Vector2.Distance(transform.position, Target.position) < targetDist)// if object is to close to Target
			targetForce = new Vector2(transform.position.x - Target.position.x, transform.position.y - Target.position.y).normalized;
		else//if object is to far away
			targetForce = new Vector2(-(transform.position.x - Target.position.x), -(transform.position.y - Target.position.y)).normalized;

		if (showTargetForce)
			Debug.DrawLine(pos, pos + (targetForce * 100), Color.red);

		return targetForce;
	}

	//Force that will push this object directly to currentPathNode
	//COLOR: BLUE
	private Vector2 createPathForce()
	{
		Vector2 pathForce = new Vector2(currentPathNode.x - transform.position.x, currentPathNode.y - transform.position.y).normalized;
        if (showPathForce)
            Debug.DrawLine(pos, pos + (pathForce * 100), Color.blue);
        return pathForce;
	}

	//Force that will push this object in a direction parallel to the surface of what ever wall its closest to within a certain radius
	//COLOR: GREEN
	private Vector2 createHelpForce(Vector2 mainForce)
	{
		Vector2 helpForce = new Vector2();

        Collider2D[] helpDashNeeded = Physics2D.OverlapCircleAll(transform.position, helpDashRadius, wallMask);
        if (helpDashNeeded.Length > 0)
            helpForce = helpMoveFromWall(helpDashNeeded, mainForce);
        if (showHelpForce)
            Debug.DrawLine(pos, pos + (helpForce * 100), Color.green);

        if (helpForce.x != 0 || helpForce.y != 0)
        {
            dashFrame = true;
            dashEndTimeStamp = Time.time + .06f;
            dashList[0] = helpForce;
        }
        else
        {
            Collider2D[] helpNeeded = Physics2D.OverlapCircleAll(transform.position, helpForceRadius, wallMask);
            if (helpNeeded.Length > 0)
                helpForce = helpMoveFromWall(helpNeeded, mainForce);
            if (showHelpForce)
                Debug.DrawLine(pos, pos + (helpForce * 100), Color.green);
        }
        return helpForce;
	}

	//Force that will push this object away from other enemies
	//COLOR: ORANGE
	private Vector2 createEnemyForce()
	{
		Vector2 enemyForce = pushFromEnemies();
		if (showEnemyForce)
			Debug.DrawLine(pos, pos + (enemyForce * 100), new Color(255, 165, 0));//orange
		return enemyForce;
	}

	//Force that will push this object away from wall objects
	//COLOR: MAGENTA
	private Vector2 createWallForce()
	{
		Vector2 wallForce = pushFromWalls();
		if (showWallForce)
			Debug.DrawLine(pos, pos + (wallForce * 100), Color.magenta);
		return wallForce;
	}

    //Force that will push this object away from any walls partially blocking its view of Target
    //when in los
    //COLOR: YELLOW
    private Vector2 createClearForce()
    {
        Vector2 clearForce = clear2Dest();
        if (showClearForce)
            Debug.DrawLine(pos, pos + (clearForce * 100), Color.yellow);
        return clearForce;
    }

	//Creates the helpForce
	//   helpNeeded: list of colliders belonging to wall objects within a circle of radius helpCheckRadius
	//   mainForce: either targetForce or parhForce
	private Vector2 helpMoveFromWall(Collider2D[] helpNeeded, Vector2 mainForce) 
	{
		Vector2 helpForce = new Vector2();
		List<Vector2> helpNormals = new List<Vector2>();

        if ((pos.x > (helpNeeded[0].bounds.min.x + 10) && pos.x < (helpNeeded[0].bounds.max.x - 10)) ||
                (pos.y > (helpNeeded[0].bounds.min.y + 10) && pos.y < (helpNeeded[0].bounds.max.y - 10)) && helpNeeded.Length != 0)
        { 
                RaycastHit2D normalFinder = Physics2D.Raycast(transform.position, helpNeeded[0].transform.position - transform.position, Mathf.Infinity, wallMask);
                helpNormals.Add(normalFinder.normal);
		}

        if (helpNormals.Count != 0)
        {
            helpForce = helpNormals[0];
            float helpAng = (Vector2.Angle(new Vector2(1, 0), helpForce) - 90) * Mathf.Deg2Rad;
            helpForce.x = Mathf.Abs(Mathf.Cos(helpAng)) * mainForce.x;
            helpForce.y = Mathf.Abs(Mathf.Sin(helpAng)) * mainForce.y;
        }
        
		return helpForce.normalized;
	}

    //Creates a force that will push this object away from all enemies within a circle with radius enemyForceRadius
    private Vector2 pushFromEnemies()
    {
        Vector2 enemyForce = new Vector2();

        Collider2D[] inRange = Physics2D.OverlapCircleAll(transform.position, enemyForceRadius, enemyMask);//find all objects within range
        for (int i = 0; i < inRange.Length; i++)
        {
            //if the object is not this object or one of its additional objects
            if (inRange[i].gameObject != gameObject && !addObjects.Contains(inRange[i].gameObject))
            {
                Transform ObjectTrans = inRange[i].transform;
                Vector2 pushForce = transform.position - ObjectTrans.position;
                enemyForce += pushForce.normalized;
            }
        }

        return enemyForce.normalized;
    }

	//Creates a force that will push this object away from all objects of a certain layermask within a circle
	//   mask: layermask of objects being pushed away from
	//   radius: radius of the circle used to check for objects
	private Vector2 pushFromWalls() 
	{
		Vector2 pushForce = new Vector2();

		Collider2D[] inRange = Physics2D.OverlapCircleAll(transform.position, wallForceRadius, wallMask);//find all objects within range
		for (int i = 0; i < inRange.Length; i++)
		{
			//if the object is not this object or one of its additional objects
			if (inRange[i].gameObject != gameObject && !addObjects.Contains(inRange[i].gameObject))
			{
				Transform ObjectTrans = inRange[i].transform;
				Vector2 normalCheckDir = new Vector2(ObjectTrans.position.x - transform.position.x, ObjectTrans.position.y - transform.position.y);
				//used to get the normal vector of the surface being pushed away from
				RaycastHit2D normalCheck = Physics2D.Raycast(transform.position, normalCheckDir, Mathf.Infinity, wallMask);
				//objects that are closer will be pushed away from harder
				//reflect a vector with the direction of this objects velocity vector off of the normal vector of the surface being pushed away from
				Vector2 reflection = Vector2.Reflect(rb2d.velocity, normalCheck.normal);
                Vector2 pushAway = transform.position - normalCheck.transform.position;

                pushForce += Vector2.Reflect(rb2d.velocity, normalCheck.normal).normalized;
                pushForce += pushAway.normalized;
			}
		}

		return pushForce.normalized;
	}

    private Vector2 clear2Dest()
    {
        Vector2 clearForce = new Vector2();
        Vector2 clearForceDest = new Vector2();
        if (sightClear)
            clearForceDest = new Vector2(Target.position.x, Target.position.y);
        else
            clearForceDest = currentPathNode;

        Vector2 this2DestDir = (clearForceDest - new Vector2(transform.position.x, transform.position.y)).normalized;
        Vector2 clearCheckStartW = new Vector2(transform.position.x + (20 * Mathf.Cos((transform.localEulerAngles.z + 180) * Mathf.Deg2Rad)),
                                               transform.position.y + (20 * Mathf.Sin((transform.localEulerAngles.z + 180) * Mathf.Deg2Rad)));
        RaycastHit2D clearCheckW = Physics2D.Raycast(clearCheckStartW, this2DestDir, Vector2.Distance(transform.position, Target.position), wallMask);
        Vector2 clearCheckStartE = new Vector2(transform.position.x + (20 * Mathf.Cos((transform.localEulerAngles.z) * Mathf.Deg2Rad)),
                                               transform.position.y + (20 * Mathf.Sin((transform.localEulerAngles.z) * Mathf.Deg2Rad)));
        RaycastHit2D clearCheckE = Physics2D.Raycast(clearCheckStartE, this2DestDir, Vector2.Distance(transform.position, Target.position), wallMask);

        //Debug.DrawRay(clearCheckStartW, this2DestDir * Vector2.Distance(transform.position, clearForceDest));
        //Debug.DrawRay(clearCheckStartE, this2DestDir * Vector2.Distance(transform.position, clearForceDest));

        float rad90 = 90 * Mathf.Deg2Rad;
        float destAng = Mathf.Acos(this2DestDir.x);
        if (this2DestDir.y < 0)
            destAng = -destAng;

        if (clearCheckW && !clearCheckE)
            clearForce += new Vector2(Mathf.Cos(destAng + rad90), Mathf.Sin(destAng + rad90));
        if (clearCheckE && !clearCheckW)
            clearForce += new Vector2(Mathf.Cos(destAng - rad90), Mathf.Sin(destAng - rad90));
        //Debug.Log(destAng * Mathf.Rad2Deg);
        //Debug.Log(Mathf.Acos(this2DestDir.x));

        return clearForce;
    }

	//Updates this objects rotation
	//  turnAngeOffset: when 0 will point the west facing part of this objects sprite towards Target, when 90 will point the south facing part
	//                  when -90 will point the east facing part.
	public void setRotation(float turnAngleOffset)
	{
		Vector2 pointD = Target.position - transform.position;
		float turnAngle = Mathf.Atan2(pointD.y, pointD.x) * Mathf.Rad2Deg;
		Quaternion lookRotation = Quaternion.AngleAxis(turnAngle + turnAngleOffset, transform.forward);
		transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotSpeed * Time.deltaTime);
	}
}
