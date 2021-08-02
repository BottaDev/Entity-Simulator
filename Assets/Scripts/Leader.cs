using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngineInternal;

public class Leader : Entity
{
    public float stoppingDistance = 0.2f;
    public LayerMask clickMask;

    public List<Node> _path;
    private Vector3 _targetPosition;
    private int _currentPathNode = 0;

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
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, clickMask))
            {
                _targetPosition = hit.point;

                if (!ApplyFOV(_targetPosition))
                {
                    _startingNode = GetNearbyNode();
                    _goalNode = GetNearbyTargetNode();
                    
                    _path = ConstructPath();   
                }
            }
        }        
    }
    
    private void Move()
    {
        if (!CheckObstacles())
        {
            if (_path.Count <= 0)
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
    
    /// <summary>
    ///  Moves through the nodes of the Theta* path 
    /// </summary>
    private void MoveThroughNodes()
    {
        Vector3 pointDistance = _path[_currentPathNode].transform.position - transform.position;
        pointDistance = new Vector3(pointDistance.x, 0, pointDistance.z);
        
        if (pointDistance.magnitude < stoppingDistance)
        {
            _currentPathNode++;
            if (_currentPathNode > _path.Count - 1)
            {
                _currentPathNode = 0;
                _path.Clear();
                return;   
            }
        }
        
        Seek(_path[_currentPathNode].transform.position);
        
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
    }
    
    private void Seek(Vector3 newPosition)
    {
        Vector3 desired;
        desired = newPosition - transform.position;
        desired.Normalize();
        desired *= maxSpeed;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);
        
        _velocity = Vector3.ClampMagnitude(_velocity + steering, maxSpeed);
        _velocity = new Vector3(_velocity.x, 0f, _velocity.z);
    }

    /// <summary>
    /// Returns the nearby node from the click position
    /// </summary>
    /// <returns></returns>
    private Node GetNearbyTargetNode()
    {
        GameObject nearbyNode = null;

        List<Node> allNodes = FindObjectsOfType<Node>().ToList();

        float distance = 999f;
        
        foreach (var item in allNodes)
        {
            Vector3 nodeDistance = item.transform.position - _targetPosition;

            if (nodeDistance.magnitude < distance)
            {
                distance = nodeDistance.magnitude;
                nearbyNode = item.gameObject;
            }
        }
        
        return nearbyNode.GetComponent<Node>();
    }
}
