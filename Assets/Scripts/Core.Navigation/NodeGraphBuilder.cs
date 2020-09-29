using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.PathFinding;

[System.Serializable]
public class NodeGraphBuilder : MonoBehaviour
{
    public int GraphID = 0;
    [SerializeField]
    Dictionary<int, Node> NodesByID = new Dictionary<int, Node>();

    private void Awake()
    {
        ListAllNodes();
    }

    public Node getNode(int index)
    {
        if (NodesByID.ContainsKey(index))
            return NodesByID[index];

        return null;
    }

    public void ListAllNodes()
    {
        foreach (var node in GetComponentsInChildren<Node>())
        {
            if (!NodesByID.ContainsKey(node.ID))
                NodesByID.Add(node.ID, node);
        }
    }
}
