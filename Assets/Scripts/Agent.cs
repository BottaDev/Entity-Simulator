using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Agent : MonoBehaviour
{
    public Leader leader;
    public AgentTeam team;    
    
    private float maxSpeed;
    private float maxForce;
    private float viewDistance;
    private float cohesionWeight;
    private float alignWeight;
    private float separationWeight;
    private float separationRadius = 1.5f;
    private Vector3 _velocity;

    private void Start()
    {
        if (team == AgentTeam.Blue)
            GameManager.instance.blueAgents.Add(this);
        else if (team == AgentTeam.Red)
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
        viewDistance = GameManager.instance.globalViewDistance;
        cohesionWeight = GameManager.instance.globalCohesionWeight;
        alignWeight = GameManager.instance.globalAlignWeight;
        separationWeight = GameManager.instance.globalSeparationWeight;
    }

    private void Move()
    {
        Arrive();
        ApplyForce(CalculateSteering(SteeringType.Cohesion) * cohesionWeight +
                   CalculateSteering(SteeringType.Align) * alignWeight +
                   CalculateSteering(SteeringType.Separation) * separationWeight);
        
        /*Vector3 leaderDistance = leader.transform.position - transform.position;

        if (leaderDistance.magnitude <= viewDistance)
        {
            Arrive();
            ApplyForce(CalculateSteering(SteeringType.Cohesion) * cohesionWeight +
                       CalculateSteering(SteeringType.Align) * alignWeight +
                       CalculateSteering(SteeringType.Separation) * separationWeight);    
        }
        else
        {
            // Calculate path using Theta Star...
        }*/

        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity.normalized;
    }
    
    private Vector3 CalculateSteering(SteeringType type)
    {
        Vector3 desired = new Vector3();
        int visibleBoids = 0;

        List<Agent> teamAgents = new List<Agent>();
        
        if (team == AgentTeam.Blue)
            teamAgents = GameManager.instance.blueAgents;
        else if (team == AgentTeam.Red)
            teamAgents = GameManager.instance.redAgents;
        
        foreach (var agent in teamAgents)
        {
            if (agent != null && agent != this)
            {
                Vector3 dist = agent.transform.position - transform.position;
                if (dist.magnitude < viewDistance && (type == SteeringType.Align || type == SteeringType.Cohesion))
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
                else if (dist.magnitude < separationRadius && type == SteeringType.Separation)
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

    // Moves to the leader position
    private void Arrive()
    {
        Vector3 desired;
        desired = leader.transform.position - transform.position;

        float dist = (leader.transform.position - transform.position).magnitude;
        
        float speed = Map(dist, 0, viewDistance, 0, maxSpeed);
            
        desired.Normalize();
        desired *= speed;

        Vector3 steering = desired - _velocity;
        steering = Vector3.ClampMagnitude(steering, maxForce);

        _velocity = Vector3.ClampMagnitude(_velocity + steering, maxSpeed);
    }
    
    private void ApplyForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, maxSpeed);
    }
    
    private float Map(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        return (from - toMin) / (fromMax - fromMin) * (toMax - toMin) + fromMin;
    }
    
    public Vector3 GetVelocity()
    {
        return _velocity;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}