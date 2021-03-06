using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Agent : Entity
{
    [Header("Properties")]
    public Leader leader;
    public AgentTeam team;
    
    private float _viewDistance;
    private float _cohesionWeight;
    private float _alignWeight;
    private float _separationWeight;
    private float _separationRadius = 1.5f;

    private void Start()
    {
        if (team == AgentTeam.Blue)
            GameManager.instance.blueAgents.Add(this);
        else
            GameManager.instance.redAgents.Add(this);

        float randomX = Random.Range(-10, 10);
        float randomZ = Random.Range(-10, 10);

        Vector3 desired = new Vector3(randomX, 0, randomZ);
        desired.Normalize();
        desired *= maxSpeed;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);

        ApplyForce(steering);
    }
    
    private void Update()
    {
        UpdateValues();
        Move();
    }
    
    private void UpdateValues()
    {
        maxSpeed = GameManager.instance.maxSpeed;
        maxForce = GameManager.instance.maxForce;
        _viewDistance = GameManager.instance.globalViewDistance;
        _cohesionWeight = GameManager.instance.globalCohesionWeight;
        _alignWeight = GameManager.instance.globalAlignWeight;
        _separationWeight = GameManager.instance.globalSeparationWeight;
    }

    private void Move()
    {
        if (!CheckObstacles())
        {
            if (ApplyFOV(leader.transform.position))
            {
                FollowLeader();   
            }
            else
            {
                if (_path == null || _path.Count <= 0)
                    SetPath(leader.transform.position);

                MoveThroughNodes();
            }
        }
        else
        {
            // Recalculate the path
            SetPath(leader.transform.position);
        }
    }

    private void FollowLeader()
    {
        _path?.Clear();
        _currentPathNode = 0;
        
        Seek(leader.transform.position);
        
        ApplyForce(CalculateSteering(SteeringType.Cohesion) * _cohesionWeight +
                   CalculateSteering(SteeringType.Align) * _alignWeight +
                   CalculateSteering(SteeringType.Separation) * _separationWeight);

        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;
    }

    private Vector3 CalculateSteering(SteeringType type)
    {
        Vector3 desired = new Vector3();
        int visibleBoids = 0;

        List<Agent> agents = team == AgentTeam.Blue ? GameManager.instance.blueAgents : GameManager.instance.redAgents;
        
        foreach (var agent in agents)
        {
            if (agent != null && agent != this)
            {
                Vector3 dist = agent.transform.position - transform.position;
                if (dist.magnitude < _viewDistance && (type == SteeringType.Align || type == SteeringType.Cohesion))
                {
                    if (type == SteeringType.Align)
                    {
                        desired.x += agent.GetVelocity().x;
                        desired.z += agent.GetVelocity().z;
                    }
                    else if (type == SteeringType.Cohesion)
                    {
                        desired.x += agent.transform.position.x;
                        desired.z += agent.transform.position.z;   
                    }

                    visibleBoids++;
                }
                else if (dist.magnitude < _separationRadius && type == SteeringType.Separation)
                {
                    desired.x += dist.x;
                    desired.z += dist.z;
                    
                    visibleBoids++;
                }
            }
        }
        
        if (visibleBoids == 0) 
            return desired;

        desired /= visibleBoids;

        if (type == SteeringType.Cohesion)
            desired -= transform.position;
        
        desired.Normalize();
        desired *= maxSpeed;

        if (type == SteeringType.Separation)
            desired *= -1;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(desired, maxForce);

        return steering;
    }

    private enum SteeringType
    {
        Separation,
        Align,
        Cohesion
    }
    
    public enum AgentTeam
    {
        Blue,
        Red
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _viewDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);
    }
}