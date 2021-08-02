using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Pathfinding")]
    public LayerMask obstacleMask;
    [Header("Field of View")]
    public float viewRadius;
    public float angleRadius;
    [Header("Obstacle Avoidance")]
    public GameObject futureObj;
    [Range(0.01f, 1f)]
    public float futureTime;
    [Header("Movement")]
    public float maxSpeed;
    [Range(0.01f, 1f)]
    public float maxForce;
    
    protected Vector3 _velocity;
    protected Node _startingNode;
    protected Node _goalNode;
    protected List<Transform> _visibleNodes = new List<Transform>();
    
    protected List<Node> ConstructPath()
    {
        PriorityQueue frontier = new PriorityQueue();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> costSoFar = new Dictionary<Node, float>();

        frontier.Put(_startingNode, 0);
        cameFrom.Add(_startingNode, null);
        costSoFar.Add(_startingNode, 0);

        while (frontier.Count() != 0)
        {
            Node current = frontier.Get();

            if (current == _goalNode)
            {
                List<Node> path = new List<Node>();
                while (current != _startingNode)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                
                path = SmoothPath(path);
                return path;
            }

            foreach (var next in current.GetNeighbors())
            {
                if (!next.isBlocked)
                {
                    float newCost = costSoFar[current] + next.cost;
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {

                        costSoFar[next] = newCost;
                        float priority = newCost + Heuristic(next.transform.position);
                        frontier.Put(next, priority);
                        cameFrom[next] = current;
                    }
                }

            }
        }
        return null;
    }

    private float Heuristic(Vector3 pos)
    {
        return Mathf.Abs((_goalNode.transform.position - pos).magnitude);
    }

    private List<Node> SmoothPath(List<Node> p)
    {
        if (p.Count <= 1) return p;

        List<Node> newPath = p;
        newPath.Reverse();
        int index = 0;
        int count = newPath.Count;

        while (index <= count - 1)
        {
            int grandParent = index + 2;
            int parent = index + 1;
            if (grandParent > count - 1 || parent > count - 1) break;

            if (InSight(newPath[index].transform.position, newPath[grandParent].transform.position))
            {
                newPath.Remove(newPath[parent]);
                count = newPath.Count;
            }
            else
            {
                index++;
            }
        }

        return newPath;
    }

    private bool InSight(Vector3 start, Vector3 end)
    {
        Vector3 dirToTarget = end - start;
        return !Physics.Raycast(start, dirToTarget, dirToTarget.magnitude, obstacleMask);
    }
    
    /// <summary>
    /// Returns the entity's closest node 
    /// </summary>
    /// <returns></returns>
    public Node GetNearbyNode()
    {
        GameObject nearbyNode = null;
        
        List<Node> targetsInViewRadius = FindObjectsOfType<Node>().ToList();
        
        float distance = 999f;
        
        foreach (var item in targetsInViewRadius)
        {
            Vector3 nodeDistance = item.transform.position - transform.position;
            
            if (nodeDistance.magnitude < distance)
            {
                distance = nodeDistance.magnitude;
                nearbyNode = item.gameObject;
            }
        }
        
        return nearbyNode.GetComponent<Node>();
    }
    
    /// <summary>
    /// Checks if the entity is seeing the target
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    protected bool ApplyFOV(Vector3 targetPos)
    {
        Vector3 dirToTarget = targetPos - transform.position;
        
        if (dirToTarget.magnitude <= viewRadius)
        {
            if (Vector3.Angle(transform.forward, dirToTarget) < angleRadius / 2)
            {
                if (!Physics.Raycast(transform.position, dirToTarget, dirToTarget.magnitude,
                    obstacleMask))
                    return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Returns true if detects obstacles
    /// </summary>
    /// <returns></returns>
    protected bool CheckObstacles()
    {
        ApplyForce(GetDirectionForce(transform.forward));
        Vector3 futurePos = transform.position + _velocity * futureTime;
        futureObj.transform.position = futurePos;

        if (!InSight(transform.position, futurePos))
        {
            ApplyForce(GetDirectionForce(-transform.right));
            return true;
        }
        
        return false;
    }
    
    private Vector3 GetDirectionForce(Vector3 dir)
    {
        Vector3 desired = dir;
        desired.Normalize();
        desired *= maxSpeed;

        desired = Vector3.ClampMagnitude(desired, maxForce);
        return desired;
    }
    
    protected void ApplyForce(Vector3 force)
    {
        _velocity = Vector3.ClampMagnitude(_velocity + force, maxSpeed);
    }
    
    public Vector3 GetVelocity()
    {
        return _velocity;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}
