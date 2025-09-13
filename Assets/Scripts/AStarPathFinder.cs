using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinder : MonoBehaviour
{
    public static AStarPathFinder instance;

    private void Awake()
    {
        instance = this;
    }

    public List<Node> FindPath(Node startNode, Node endNode)
    {
        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        foreach (var node in FindObjectsByType<Node>(FindObjectsSortMode.None))
        {
            node.gCost = Mathf.Infinity;
            node.hCost = Mathf.Infinity;
            node.parent = null;
        }

        startNode.gCost = 0;
        startNode.hCost = Vector3.Distance(startNode.transform.position, endNode.transform.position);

        while (openSet.Count > 0)
        {
            Node current = GetLowestFCostNode(openSet);

            if (current == endNode)
                return RetracePath(startNode, endNode);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Node neighbor in current.neighbors)
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float tentativeG = current.gCost + Vector3.Distance(current.transform.position, neighbor.transform.position);

                if (tentativeG < neighbor.gCost)
                {
                    neighbor.gCost = tentativeG;
                    neighbor.hCost = Vector3.Distance(neighbor.transform.position, endNode.transform.position);
                    neighbor.parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    private Node GetLowestFCostNode(List<Node> nodes)
    {
        Node best = nodes[0];
        foreach (Node node in nodes)
        {
            if (node.FCost < best.FCost)
                best = node;
        }
        return best;
    }

    private List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }
}
