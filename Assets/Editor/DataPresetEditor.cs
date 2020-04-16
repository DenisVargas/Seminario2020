using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DataPresetEditor : EditorWindow
{
    [MenuItem("GamePlay/Data Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow(typeof(DataPresetEditor)) as DataPresetEditor;
        var content = new GUIContent();
        content.text = "Data Editor";
        window.titleContent = content;
    }

    private void OnGUI()
    {
        //Mostrar contenido actual del enum.
        //Rehacer el enuim.
        //Crear Scriptable Object Correspondiente.
    }
}
