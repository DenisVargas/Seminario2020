using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using IA.PathFinding;
using System.Linq;

[CustomEditor(typeof(NodeGraphBuilder))]
public class NGBInspector : Editor
{
    NodeGraphBuilder ins;
    public static float distance = 0;

    private void OnEnable()
    {
        ins = target as NodeGraphBuilder;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("GraphID"));

        if (GUILayout.Button("Rename and Reasign ID in All Children"))
        {
            int Count = 0;
            foreach (var child in ins.GetComponentsInChildren<Node>())
            {
                Count++;
                child.gameObject.name = $"N{Count}";
                child.ID = Count;
            }
        }

        distance = EditorGUILayout.FloatField("Maximun Distance", distance);

        if(GUILayout.Button("Connect nodes using Distance"))
        {
            var nodes = FindObjectsOfType<Node>();
            //https://answers.unity.com/questions/1719405/modifying-and-saving-scriptable-objects-setdirty-v.html
            //https://docs.unity3d.com/ScriptReference/EditorUtility.SetDirty.html
            Undo.RecordObjects(nodes, "Added node connections");
            foreach (var node in nodes)
            {
                var closerNodes = nodes.Where((n) => (n != node && Vector3.Distance(node.transform.position, n.transform.position) < distance && !node.Connections.Contains(n)));
                node.Connections.AddRange(closerNodes);
            }
        }
    }

    IEnumerator testRoutine()
    {
        yield return null;
    }
}
