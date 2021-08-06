using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngineInternal;

public class Leader : Entity
{
    [Header("Properties")]
    public Agent.AgentTeam team;
    public LayerMask clickMask;

    private Vector3 _targetPosition;

    private void Start()
    {
        _targetPosition = transform.position;
    }

    private void Update()
    {
        GetClick();
        Move();
    }

    private void GetClick()
    {
        if ((Input.GetMouseButtonDown(0) && team == Agent.AgentTeam.Blue) || (Input.GetMouseButtonDown(1) && team == Agent.AgentTeam.Red))
        {
            RaycastHit hit;

            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, obstacleMask) && 
                Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, clickMask))
            {
                _targetPosition = hit.point;

                if (!ApplyFOV(_targetPosition))
                {
                    _startingNode = GetNearbyNode();
                    _goalNode = GetNearbyTargetNode(_targetPosition);
                    
                    _path = ConstructPath();   
                }
            }
        }        
    }
    
    private void Move()
    {
        if (!CheckObstacles())
        {
            if (_path == null|| _path.Count <= 0)
                MoveToVisiblePos();
            else
                MoveThroughNodes();
        }
    }

    private void MoveToVisiblePos()
    {
        Vector3 pointDistance = _targetPosition - transform.position;
        pointDistance = new Vector3(pointDistance.x, 0, pointDistance.z);
        
        if (pointDistance.magnitude > stoppingDistance)
        {
            Seek(_targetPosition);
            
            transform.position += _velocity * Time.deltaTime;
            transform.forward = _velocity;
        }   
    }
}
