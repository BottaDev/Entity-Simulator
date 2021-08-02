using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boid : MonoBehaviour
{
    public Leader leader;
    public float maxSpeed;
    [Range(0.01f, 1f)]
    public float maxForce;

    public GameObject futureObj;
    [Range(0.01f, 1f)]
    public float futureTime;
    
    private float _viewDistance;
    private float _cohesionWeight;
    private float _alignWeight;
    private float _separationWeight;
    private float _separationRadius = 1.5f;
    private Vector3 _velocity;

    private void Start()
    {
        GameManager.instance.boids.Add(this);

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
        _viewDistance = GameManager.instance.globalViewDistance;
        _cohesionWeight = GameManager.instance.globalCohesionWeight;
        _alignWeight = GameManager.instance.globalAlignWeight;
        _separationWeight = GameManager.instance.globalSeparationWeight;
    }

    private void Move()
    {
        Seek();
        ApplyForce(GetDirectionForce(transform.forward));
        Vector3 futurePos = transform.position + _velocity * futureTime;
        futureObj.transform.position = futurePos;

        if (!InSight(transform.position, futurePos))
        {
            ApplyForce(GetDirectionForce(-transform.right));
        }
        else
        {
            ApplyForce(CalculateSteering(SteeringType.Cohesion) * _cohesionWeight +
                       CalculateSteering(SteeringType.Align) * _alignWeight +
                       CalculateSteering(SteeringType.Separation) * _separationWeight);    
        }


        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity.normalized;
    }
    
    // Moves to the leader position
    private void Seek()
    {
        Vector3 desired;
        desired = leader.transform.position - transform.position;
        desired.Normalize();
        desired *= leader.maxSpeed;

        Vector3 steering = desired - leader.GetVelocity();
        steering = Vector3.ClampMagnitude(steering, leader.maxForce);

        _velocity = Vector3.ClampMagnitude(_velocity + steering, maxSpeed);
        _velocity = new Vector3(_velocity.x, 0f, _velocity.z);
    }
    
    private Vector3 GetDirectionForce(Vector3 dir)
    {
        Vector3 desired = dir;
        desired.Normalize();
        desired *= maxSpeed;

        desired = Vector3.ClampMagnitude(desired, maxForce);
        return desired;
    }
    
    private bool InSight(Vector3 start, Vector3 end)
    {
        Vector3 dirToTarget = end - start;
        if (!Physics.Raycast(start, dirToTarget, dirToTarget.magnitude, 9)) 
            return true;
        else 
            return false;
    }
    
    private Vector3 CalculateSteering(SteeringType type)
    {
        Vector3 desired = new Vector3();
        int visibleBoids = 0;

        foreach (var boid in GameManager.instance.boids)
        {
            if (boid != null && boid != this)
            {
                Vector3 dist = boid.transform.position - transform.position;
                if (dist.magnitude < _viewDistance && (type == SteeringType.Align || type == SteeringType.Cohesion))
                {
                    if (type == SteeringType.Align)
                    {
                        desired.x += boid.GetVelocity().x;
                        desired.z += boid.GetVelocity().z;
                    }
                    else if (type == SteeringType.Cohesion)
                    {
                        desired.x += boid.transform.position.x;
                        desired.z += boid.transform.position.z;   
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

    private void ApplyForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, maxSpeed);
    }

    private enum SteeringType
    {
        Separation,
        Align,
        Cohesion
    }

    public Vector3 GetVelocity()
    {
        return _velocity;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _viewDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _separationRadius);
    }
}