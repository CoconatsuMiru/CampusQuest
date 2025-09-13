using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public static NodeManager Instance;
    public List<Node> allNodes = new List<Node>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        allNodes.Clear();
        Node[] nodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
        allNodes.AddRange(nodes);
    }

    public List<Node> GetAllNodes()
    {
        return allNodes;
    }
}
