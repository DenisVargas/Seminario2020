using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator _creator;
    Path Path
    {
        get => _creator.path;
    }

    const float segmentSelectDistanceTreshold = 0.1f;
    int selectedSegmentIndex = -1;

    private void OnEnable()
    {
        var current = Event.current;

        _creator = target as PathCreator;

        if (_creator.path == null)
            _creator.CreatePath();
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create New"))
        {
            if (EditorUtility.DisplayDialog("Create new path", "The current path will be replaced with a new path.\nAre you sure you want to create a new path?", "Yes", "Cancel"))
            {
                Undo.RecordObject(_creator, "Create New");
                _creator.CreatePath();
            }
        }

        bool toogleClosed = GUILayout.Toggle(Path.IsClosed, "Is Closed");
        if (Path.IsClosed != toogleClosed)
        {
            Undo.RecordObject(_creator, "Toggle Closed");
            Path.IsClosed = toogleClosed;
        }

        bool autoSetControlPoints = GUILayout.Toggle(Path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != Path.AutoSetControlPoints)
        {
            Undo.RecordObject(_creator, "Toggle auto set controls");
            Path.AutoSetControlPoints = autoSetControlPoints;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
            serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// Handles the Input of the Editor.
    /// </summary>
    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if (selectedSegmentIndex != -1)
            {
                Undo.RecordObject(_creator, "Split Segment");
                Path.SplitSegment(mousePos, selectedSegmentIndex);
            }
            else if (!Path.IsClosed)
            {
                Undo.RecordObject(_creator, "Add Segment");
                Path.AddSegment(mousePos);
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDstToAnchor = _creator.anchorDiameter * 0.5f;
            int closestAnchorIndex = -1;

            for (int i = 0; i < Path.NumPoints; i++)
            {
                float dst = Vector2.Distance(mousePos, Path[i]);

                if (dst < minDstToAnchor)
                {
                    minDstToAnchor = dst;
                    closestAnchorIndex = i;
                }
            }

            if (closestAnchorIndex != -1)
            {
                Undo.RecordObject(_creator, "Delete Segment");
                Path.DeleteSegment(closestAnchorIndex);
            }
        }

        if (guiEvent.type == EventType.MouseMove)
        {
            float minDstToSegment = segmentSelectDistanceTreshold;
            int newSelectedSegmentIndex = -1;

            for (int i = 0; i < Path.NumSegments; i++)
            {
                Vector2[] points = Path.GetPositionInSegment(i);
                float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if (dst < minDstToSegment)
                {
                    minDstToSegment = dst;
                    newSelectedSegmentIndex = i;
                }
            }

            if (newSelectedSegmentIndex != selectedSegmentIndex)
            {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }
    }
    /// <summary>
    /// Draws the graphic Elements.
    /// </summary>
    void Draw()
    {

        for (int i = 0; i < Path.NumSegments; i++)
        {
            Vector2[] points = Path.GetPositionInSegment(i);

            if (_creator.displayControlPoints)
            {
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
            }
            Color segmentColor = (i == selectedSegmentIndex && Event.current.shift) ?  _creator.selectedSegmentCol : _creator.segmentCol;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColor, null, 2);
        }

        Handles.color = Color.red;
        for (int i = 0; i < Path.NumPoints; i++)
        {
            if (i % 3 == 0 || _creator.displayControlPoints)
            {
                Handles.color = i % 3 == 0 ? _creator.anchorCol : _creator.controlCol;
                float handleSize = (i % 3 == 0) ? _creator.anchorDiameter : _creator.controlDiameter;
                Vector2 newposition = Handles.FreeMoveHandle(Path[i], Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);
                if (Path[i] != newposition)
                {
                    Undo.RecordObject(_creator, "movePoint");
                    Path.MovePoint(i, newposition);
                } 
            }
        }
    }
}
