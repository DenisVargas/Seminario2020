using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using IA.PathFinding;

[CustomEditor(typeof(Node)), CanEditMultipleObjects]
public class NodeInspector : Editor
{
    Node inspectedNode;
    List<Node> inspectedNodes;

    private void OnEnable()
    {
        inspectedNode = target as Node;

        inspectedNodes = new List<Node>();

        if (targets.Length == 1)
            inspectedNodes.Add(inspectedNode);

        if (targets.Length > 1)
        {
            foreach (var ins in targets)
                inspectedNodes.Add((Node)ins);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("Custom Node Editor");
        GUI.color = Color.green;
        GUILayoutOption[] bops = new GUILayoutOption[] { GUILayout.Height(50)};
        if (GUILayout.Button("Connect Selection to this", bops))
        {
            foreach (var go in Selection.gameObjects)
            {
                var node = go.GetComponent<Node>();
                if (node != null && !node.Connections.Contains(inspectedNode) && node != inspectedNode)
                {
                    node.Connections.Add(inspectedNode);
                    inspectedNode.Connections.Add(node);
                }
            }
        }

        GUI.color = Color.yellow;
        if (GUILayout.Button("Remove selected connections", bops))
        {
            foreach (var go in Selection.gameObjects)
            {
                var node = go.GetComponent<Node>();
                if (node != null && node.Connections.Contains(inspectedNode) && node != inspectedNode)
                {
                    inspectedNode.Connections.Remove(node);
                    node.Connections.Remove(inspectedNode);
                }
            }
        }
        GUI.color = Color.red;
        if (GUILayout.Button("Clear Selected Connections"))
            ClearInspectedConnections();

        GUI.color = Color.red;
        if (GUILayout.Button("Delete Selected Nodes"))
            DeleteSelectedNodes();

        GUI.color = Color.white;
        if (inspectedNodes != null && inspectedNodes.Count == 2)
        {
            if (GUILayout.Button("Subdivide"))
            {
                Node A = inspectedNodes[0];
                Node B = inspectedNodes[1];

                //Desconectamos.
                if (A.Connections.Contains(B))
                    A.Connections.Remove(B);
                if (B.Connections.Contains(A))
                    B.Connections.Remove(A);

                //Sacamos la posicion intermedia entre ambos.
                Vector3 pos = Vector3.Lerp(A.transform.position, B.transform.position, 0.5f);
                //Creamos un nuevo gameObject con el componente Nodo.
                GameObject empty = new GameObject("New Node");
                empty.transform.position = pos;
                Node newNode = empty.AddComponent<Node>();
                newNode.Connections.Add(A);
                newNode.Connections.Add(B);

                A.Connections.Add(newNode);
                B.Connections.Add(newNode);
            }
        }
    }

    private void ClearInspectedConnections()
    {
        if (inspectedNodes.Count > 1)
        {
            Undo.RecordObjects(inspectedNodes.ToArray(), "Cleared Selected Connections");
            foreach (var node in inspectedNodes)
            {
                ClearNodeConnection(node);
            }
        }
        if (inspectedNodes.Count == 1)
        {
            Undo.RecordObject(inspectedNode, "Cleared Connections");
            ClearNodeConnection(inspectedNode);
        }
    }
    private void ClearNodeConnection(Node node)
    {
        Undo.RecordObject(node, "Clear Removed Connections");
        foreach (var connection in node.Connections)
        {
            if (connection.Connections.Contains(node))
            {
                Undo.RecordObject(connection, "Removed Connection");
                connection.Connections.Remove(node);
            }
        }
        node.Connections.Clear();
    }
    void DeleteSelectedNodes()
    {
        if (inspectedNodes.Count == 1)
        {
            ClearNodeConnection(inspectedNode);
            Undo.DestroyObjectImmediate(inspectedNode.gameObject);
        }
        if (inspectedNodes.Count > 1)
        {
            ClearInspectedConnections();

            foreach (var node in inspectedNodes)
            {
                var toDelete = node.gameObject;
                //toDestroy.Add(node.gameObject);
                Undo.DestroyObjectImmediate(toDelete);
            }
        }
    }
}
