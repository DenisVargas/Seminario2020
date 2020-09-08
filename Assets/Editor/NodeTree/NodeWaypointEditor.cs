using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using IA.Waypoints;
using IA.PathFinding;

[CustomEditor(typeof(NodeWaypoint))]
public class NodeWaypointEditor : Editor
{
    NodeWaypoint insp = null;
    bool _lock = false;
    int positionIndex = 0;

    private void OnEnable()
    {
        insp = target as NodeWaypoint;
        _lock = ActiveEditorTracker.sharedTracker.isLocked;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();

        DrawUtilities();
        DrawNodesSelector();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
    private void OnSceneGUI()
    {
        if (insp.points.Count == 0) return;
        Handles.color = Color.magenta;
        int pos = 1;

        foreach (var node  in insp.points)
        {
            if (node == null) continue;

            Handles.DrawWireDisc(node.transform.position, // position
                                 Vector3.up,  // normal
                                 0.2f);                     // radius
            Handles.DrawWireDisc(node.transform.position, // position
                                 Vector3.up,  // normal
                                 0.4f);                     // radius
            Handles.DrawWireDisc(node.transform.position, // position
                                 Vector3.up,  // normal
                                 0.6f);                     // radius
            GUIContent labelContent = new GUIContent($"Position {pos}");
            Vector3 offset = Vector3.right + Vector3.up;
            Handles.Label(node.transform.position + offset, labelContent);
            pos++;
        }
    }
    private void DrawUtilities()
    {
        EditorGUILayout.Space();

        if (insp.points.Count > 0)
        {
            if (GUILayout.Button("Allign Object to First Position"))
            {
                insp.gameObject.transform.position = insp.points[0].transform.position;
            }

            EditorGUILayout.BeginHorizontal();

            positionIndex = EditorGUILayout.IntField(positionIndex, new GUILayoutOption[] { GUILayout.MaxWidth(50f) });

            if (GUILayout.Button("Allign Object to Position at Index"))
            {
                if (positionIndex < insp.points.Count)
                    insp.gameObject.transform.position = insp.points[positionIndex].transform.position;
                else Debug.Log("Indíce inválido");
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    private void DrawNodesSelector()
    {
        EditorGUILayout.Space();

        if (_lock == false && GUILayout.Button("Lock Inspector"))
        {
            if (_lock)
            {
                Debug.Log("Lo bloquee");
                _lock = true;
            }
            CustomShorcuts.ToogleInspectorLock();
        }

        //Metemos funcionalidad de añadir nodo como parte del waypoint.
        if (_lock && GUILayout.Button("Add Selected Nodes"))
        {
            //Debug.Log("Añado la selección");
            serializedObject.Update();
            Undo.RecordObject(insp, "Node/s added");

            foreach (var item in Selection.gameObjects)
            {
                var Node = item.GetComponent<Node>();
                if (Node != null)
                {
                    if (!insp.points.Contains(Node))
                    {
                        EditorUtility.SetDirty(insp);
                        insp.points.Add(Node);
                    }
                }
                //Debug.Log(item.name);
            }
        }

        if (_lock && GUILayout.Button("Done"))
        {
            //Debug.Log("Termine we");
            _lock = false;
            CustomShorcuts.ToogleInspectorLock();
        }
    }
}

