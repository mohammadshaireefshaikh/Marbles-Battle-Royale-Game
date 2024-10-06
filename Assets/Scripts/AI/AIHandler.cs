using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AIHandler : MonoBehaviour
{
    //Target
    Transform target;
    HPHandler targetHPHandler;

    //Navmesh info
    NavMeshPath navMeshPath;
    bool isCompletePath = false;

    //Character potitions on the last path 
    Vector3 lastRecreatePathPotition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        navMeshPath = new NavMeshPath();
    }

    void SetTarget()
    {
        GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag("Player");

        target = potentialTargets[Random.Range(0, potentialTargets.Length)].transform;
        targetHPHandler = target.GetComponent<HPHandler>();

        //Avoid targetting ourselves
        if (transform == target)
            target = null;
    }

    public Vector3 GetDirectionToTarget(out float distanceToTarget)
    {
        if (navMeshPath == null)
        {
            distanceToTarget = 1000;
            return Vector3.zero;
        }

        distanceToTarget = 0;

        //Check that we have a target
        if (target == null)
            SetTarget();

        if(targetHPHandler.isDead)
            SetTarget();

        //Check again that we have found a valid target
        if (target == null)
            return Vector3.zero;

        //Draw the path
        DebugDrawNavMeshPath();

        //Caculate the distance to the target
        distanceToTarget = (target.position - transform.position).magnitude;

        //Avoid recalcuating the path all the time, re-use the old path if we have not moved enough 
        if ((transform.position - lastRecreatePathPotition).magnitude < 1)
            return GetVectorToPath();

        //Calculate the path
        isCompletePath = NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, navMeshPath);

        return GetVectorToPath();

    }

    Vector3 GetVectorToPath()
    {
        Vector3 vectorToPath = Vector3.zero;

        //Take the 2nd navmesh corner and use that as the path, if no navigation is found then just follow the target directly. 
        if (navMeshPath.corners.Length > 1)
            vectorToPath = navMeshPath.corners[1] - transform.position;
        else vectorToPath = target.position - transform.position;

        vectorToPath.Normalize();

        return vectorToPath;
    }

    void DebugDrawNavMeshPath()
    {
        if (navMeshPath == null) return;

        //Draw the path for debugging
        for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
        {
            Color pathLineColor = Color.white;

            if (!isCompletePath)
                pathLineColor = Color.magenta;

            Vector3 corner1Position = navMeshPath.corners[i];

            //On the first part of the path use the characters position instead. 
            if (i == 0)
                corner1Position = transform.position;

            //Draw the path
            Debug.DrawLine(corner1Position, navMeshPath.corners[i + 1], pathLineColor);
        }
    }


}
