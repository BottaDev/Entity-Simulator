using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

public class Leader : MonoBehaviour
{
    public float maxSpeed = 10f;
    [Range(0.01f, 1f)]
    public float maxForce;
    public float stoppingDistance = 0.2f;
    public LayerMask mask;
    
    private Vector3 _velocity;
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
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask))
                _targetPosition = hit.point;
        }        
    }
    
    private void Move()
    {
        Vector3 pointDistance = _targetPosition - transform.position;
        pointDistance = new Vector3(pointDistance.x, 0, pointDistance.z);   // Fix y axi
        
        if (pointDistance.magnitude > stoppingDistance)
        {
            Seek(_targetPosition);
            
            transform.position += _velocity * Time.deltaTime;
            transform.forward = _velocity;
        }
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

    public Vector3 GetVelocity()
    {
        return _velocity;
    }
}
