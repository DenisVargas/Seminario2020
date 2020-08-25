using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using IA.PathFinding;

[CustomEditor(typeof(NodeGraphBuilder))]
public class NGBInspector : Editor
{
    NodeGraphBuilder ins;

    private void OnEnable()
    {
        ins = target as NodeGraphBuilder;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
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
    }
}
