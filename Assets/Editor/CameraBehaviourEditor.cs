﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraBehaviour))]
public class CameraBehaviourEditor : Editor
{
    CameraBehaviour ins;
    bool EditLimitsOn = false;
    int selected = 0;

    //Distance Slider Editor.
    Transform childCamera;
    float cameraLocalPosition = 10;
    float minCameraDistance = 5;
    float maxCameraDistance = 50;

    private void OnEnable()
    {
        ins = target as CameraBehaviour;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        GUIStyle BoldText = new GUIStyle();
        BoldText.fontStyle = FontStyle.Bold;

        GUILayout.Space(20f);

        EditorGUILayout.LabelField("Camera Locking", BoldText);
        string[] options = { "Free Camera", "Locked To Target" };
        selected = GUILayout.SelectionGrid(selected, options, 2);
        switch (selected)
        {
            case 0:
                ins.freeCamera = true;
                break;
            case 1:
                ins.freeCamera = false;
                break;
            default:
                break;
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Target"));
        if (GUILayout.Button("Pick First Player Controller in Scene"))
        {
            var finded = GameObject.Find("Player");
            if (!finded) Debug.LogError("Player has not been found");
            else ins.Target = finded.transform;
        }
        EditorGUILayout.Space();


        EditorGUILayout.LabelField("Velocities", BoldText);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("zoomVelocity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("panSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"));

        EditorGUILayout.LabelField("Limits and Borders", BoldText);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("panBorderThickness"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("panBorderThickness"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("panLimits"));
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.Space();

        if (!EditLimitsOn)
        {
            if (GUILayout.Button("Edit Camera Pan Limits"))
            {
                ActiveEditorTracker.sharedTracker.isLocked = true;
                Debug.Log("Funciona el botón.");
                ActiveEditorTracker.sharedTracker.ForceRebuild();
                EditLimitsOn = true;
            }
        }
        else
        {
            GUI.color = Color.green;
            if (GUILayout.Button("Exit Edit Camera pan Limits"))
            {
                Selection.activeGameObject = ins.gameObject;
                ActiveEditorTracker.sharedTracker.isLocked = false;
                ActiveEditorTracker.sharedTracker.ForceRebuild();
                EditLimitsOn = false;
            }
        }

        DrawCameraDistanceEditor();
    }

    private void DrawCameraDistanceEditor()
    {
        EditorGUILayout.Space();
        GUIStyle Title = new GUIStyle();
        Title.fontStyle = FontStyle.Bold;
        Title.alignment = TextAnchor.LowerLeft;
        //Title.fontSize = 19;
        GUILayoutOption[] TitleOptions = new GUILayoutOption[] { GUILayout.MinWidth(10), GUILayout.MaxWidth(19) };
        EditorGUILayout.LabelField("Camera Distance from Parent", Title, TitleOptions);
        EditorGUILayout.Space();

        GUILayoutOption[] SliderLenght = new GUILayoutOption[] { GUILayout.MinWidth(340) };
        cameraLocalPosition = EditorGUILayout.Slider(cameraLocalPosition, minCameraDistance, maxCameraDistance, SliderLenght);

        EditorGUILayout.BeginHorizontal();

        GUILayoutOption[] LabelOpt = new GUILayoutOption[] { GUILayout.MinWidth(120) };

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Minimun Distance", LabelOpt);
        minCameraDistance = EditorGUILayout.FloatField(minCameraDistance, LabelOpt);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Maximun Distance", LabelOpt);
        maxCameraDistance = EditorGUILayout.FloatField(maxCameraDistance, LabelOpt);
        EditorGUILayout.EndVertical();


        EditorGUILayout.EndHorizontal();
    }

    private void OnSceneGUI()
    {
        Vector3 dirToCamera = (Camera.main.transform.position - ins.transform.position);
        float distance = dirToCamera.magnitude;
        dirToCamera.Normalize();

        Handles.color = Color.cyan;
        Handles.DrawLine(ins.transform.position, Camera.main.transform.position);

        Handles.RectangleHandleCap(1, Vector3.zero, Quaternion.identity, 30, EventType.MouseDrag);
    }
}
