using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileObstacle : MonoBehaviour
{
    public float speed;
    public float stoppingDistance;
    public List<Transform> wayPoints;

    private int _currentWayPoint = 0;
    
    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (wayPoints.Count <= 0)
            return;

        if (Vector3.Distance(wayPoints[_currentWayPoint].position, transform.position) < stoppingDistance)
        {
            _currentWayPoint++;
            if (_currentWayPoint > wayPoints.Count - 1)
                _currentWayPoint = 0;
        }

        transform.position = Vector3.MoveTowards(transform.position, wayPoints[_currentWayPoint].position, speed * Time.deltaTime);
    }
}
