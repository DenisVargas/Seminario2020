using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraBehaviour))]
public class CameraBehaviourEditor : Editor
{
    CameraBehaviour ins;
    //bool EditLimitsOn = false;
    int selected = 0;
    int currentSelection = 0;

    //Distance Slider Editor.
    Transform childCamera;
    float cameraLocalPosition = 10;
    float minCameraDistance = 5;
    float maxCameraDistance = 50;

    private void OnEnable()
    {
        ins = target as CameraBehaviour;
        //ins.freeCamera = true;
        currentSelection = ins.freeCamera ? 0 : 1;
        selected = currentSelection;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();

        GUIStyle BoldText = new GUIStyle();
        BoldText.fontStyle = FontStyle.Bold;

        GUILayout.Space(20f);

        //Esto no esta funcionando correctamente, el resultado no se serializa.
        //Al salir del PlayMode la referencias se pierden.
        EditorGUILayout.LabelField("Camera Locking", BoldText);
        string[] options = { "Free Camera", "Locked To Target" };
        selected = GUILayout.SelectionGrid(selected, options, 2);
        if (selected != currentSelection)
        {
            Undo.RecordObject(ins, "Switched DefaultMode");
            EditorUtility.SetDirty(ins);
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
            //serializedObject.ApplyModifiedProperties();
        }

        if (ins._target != null)
        {
            EditorGUILayout.LabelField($"Current target is: {ins._target.gameObject.name}", BoldText);
        }
        else
        {
            GUI.color = Color.red;
            EditorGUILayout.LabelField("Current target has not been setted.", BoldText);
            GUI.color = Color.white;
            if (GUILayout.Button("Pick First Player Controller in Scene"))
            {
                var finded = GameObject.Find("Player");
                if (!finded) Debug.LogError("Player has not been found");
                else
                {
                    Undo.RecordObject(ins, "Player Setted");
                    EditorUtility.SetDirty(ins);
                    ins._target = finded.transform;
                }
            }
        }

        EditorGUILayout.Space();

        //EditorGUILayout.PropertyField(serializedObject.FindProperty("_target"));
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("freeCamera"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OperativeCamera"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("locked"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundMask"));

        EditorGUILayout.LabelField("Velocities", BoldText);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("panSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationSpeed"));

        EditorGUILayout.LabelField("Limits and Borders", BoldText);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mousePanBorderThickness"));
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("navigationLimits"));
        EditorGUILayout.Space();

        //TODO: Encuentra una forma de hacer la edición de los límites, personalizados.
        //EditorGUILayout.Space();
        //if (!EditLimitsOn)
        //{
        //    if (GUILayout.Button("Edit Camera Pan Limits"))
        //    {
        //        ActiveEditorTracker.sharedTracker.isLocked = true;
        //        Debug.Log("Funciona el botón.");
        //        ActiveEditorTracker.sharedTracker.ForceRebuild();
        //        EditLimitsOn = true;
        //    }
        //}
        //else
        //{
        //    GUI.color = Color.green;
        //    if (GUILayout.Button("Exit Edit Camera pan Limits"))
        //    {
        //        Selection.activeGameObject = ins.gameObject;
        //        ActiveEditorTracker.sharedTracker.isLocked = false;
        //        ActiveEditorTracker.sharedTracker.ForceRebuild();
        //        EditLimitsOn = false;
        //    }
        //}

        //TODO: Una manera de editar la inclinación y el zoom de la cámara de forma mas intuitiva.
        //DrawCameraDistanceEditor();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
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
