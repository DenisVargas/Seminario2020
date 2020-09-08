using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(VectorWaypoint))]
public class WaypointCustomInspector : Editor
{
    VectorWaypoint inspected;
    private ReorderableList list;
    int positionIndex = 0;

    private void OnEnable()
    {
        inspected = target as VectorWaypoint;

        list = new ReorderableList(serializedObject, serializedObject.FindProperty("points"), true, true, true, true);
        list.drawHeaderCallback = DrawListHeader;
        list.drawElementCallback = DrawReorderableList_WaypointPositions;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        serializedObject.Update();

        DrawUtilities();

        Rect begining = EditorGUILayout.GetControlRect();
        list.DoLayoutList();
        Rect ending = EditorGUILayout.GetControlRect();

        float area = ending.y - begining.y;
        begining.height = area;
        DropAreaGUI(begining);

        serializedObject.ApplyModifiedProperties();
    }

    public void DrawUtilities()
    {
        EditorGUILayout.Space();

        if (inspected.points.Count > 0)
        {
            if (GUILayout.Button("Allign Object to First Position"))
            {
                inspected.gameObject.transform.position = inspected.points[0].position;
            }

            EditorGUILayout.BeginHorizontal();

            positionIndex = EditorGUILayout.IntField(positionIndex, new GUILayoutOption[]{ GUILayout.MaxWidth(50f) });

            if (GUILayout.Button("Allign Object to Position at Index"))
            {
                if (positionIndex < inspected.points.Count)
                    inspected.gameObject.transform.position = inspected.points[positionIndex].position;
                else Debug.Log("Indíce inválido");
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    public void DrawListHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Waypoint Positions");
    }
    public void DrawReorderableList_WaypointPositions(Rect rect, int index, bool isActive, bool isFocused)
    {
        var element = list.serializedProperty.GetArrayElementAtIndex(index);
        rect.y += 2;
        float ListEndOffset = 60f;
        float maxLenght = rect.width - ListEndOffset;
        //GUIContent elementContent = new GUIContent("Hola", "Esto es un texto de prueba");
        //var positionValue = element.vector3Value;

        Rect FirtElement = new Rect(rect.x, rect.y, maxLenght / 4, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(FirtElement, $"#{index}");

        Rect SecondElement = new Rect(rect.x + FirtElement.width, rect.y, maxLenght, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(SecondElement, element, GUIContent.none);
        //Transform gameO = null;
        //gameO = (Transform)EditorGUI.ObjectField(new Rect(rect.x + FirtElement.width, rect.y, maxLenght, EditorGUIUtility.singleLineHeight), gameO, typeof(Transform), true);
    }
    public void DropAreaGUI(Rect dropArea)
    {
        Event evt = Event.current;
        //Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUIContent boxText = new GUIContent("Add Waypoint Positions!", "Drag and Drop to add new Elements");
        GUI.Box(dropArea, boxText);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    EditorUtility.SetDirty(inspected);
                    Undo.RecordObject(inspected, "AddTransforms to List");
                    foreach (Object dragged_object in DragAndDrop.objectReferences)
                    {
                        if (dragged_object.GetType().Equals(typeof(GameObject)))
                        {
                            inspected.points.Add(((GameObject)dragged_object).transform);
                        }
                        // Do On Drag Stuff here
                    }
                    serializedObject.ApplyModifiedProperties();
                }
                break;
        }
    }
}
