using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [Header("Agents")]
    public List<Agent> blueAgents;
    public List<Agent> redAgents;
    public List<Boid> boids;
    
    [Header("Agent Settings")]
    public float maxSpeed = 10;
    [Range(0.01f, 1f)]
    public float maxForce = 1;
    public float globalViewDistance = 5;
    public float globalCohesionWeight = 0.8f;
    public float globalAlignWeight = 0.5f;
    public float globalSeparationWeight = 0.3f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}