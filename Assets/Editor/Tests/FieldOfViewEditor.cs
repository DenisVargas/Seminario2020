using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fw = target as FieldOfView;

        Handles.color = Color.white;
        Handles.DrawWireArc(fw.transform.position, Vector3.up, Vector3.forward, 360, fw.viewRadius);
        Vector3 viewAngleA = fw.DirFromAngle(-fw.ViewAngle / 2, false);
        Vector3 viewAngleB = fw.DirFromAngle(fw.ViewAngle / 2, false);

        Handles.DrawLine(fw.transform.position, fw.transform.position + viewAngleA * fw.viewRadius);
        Handles.DrawLine(fw.transform.position, fw.transform.position + viewAngleB * fw.viewRadius);

        Handles.color = Color.red;
        foreach (Transform visibleTarget in fw.visibleTargets)
            Handles.DrawLine(fw.transform.position, visibleTarget.position);
    }
}
