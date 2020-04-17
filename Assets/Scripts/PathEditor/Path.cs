using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector]
    List<Vector2> _points;
    [SerializeField, HideInInspector]
    bool _isClosed;
    [SerializeField, HideInInspector]
    bool _autoSetControlPoints;

    public Path(Vector2 Center)
    {
        _points = new List<Vector2>()
        {
            Center + Vector2.left,
            Center + (Vector2.left + Vector2.up) * 0.5f,
            Center + (Vector2.right + Vector2.down) * 0.5f,
            Center + Vector2.right
        };
    }

    public Vector2 this[int t]
    {
        get => _points[t];
    }

    public bool AutoSetControlPoints
    {
        get => _autoSetControlPoints;
        set
        {
            if (_autoSetControlPoints != value)
            {
                _autoSetControlPoints = value;

                if (_autoSetControlPoints)
                {
                    AutoSetAllControlPoints();
                }
            }
        }
    }

    public int NumPoints
    {
        get => _points.Count;
    }

    public int NumSegments
    {
        get => _points.Count / 3;
    }

    public bool IsClosed
    {
        get => _isClosed;
        set
        {
            _isClosed = value;
            if (_isClosed)
            {
                _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
                _points.Add(_points[0] * 2 - _points[1]);
                if (_autoSetControlPoints)
                {
                    AutoSetAnchorControlPoints(0);
                    AutoSetAnchorControlPoints(_points.Count - 3);
                }
            }
            else
            {
                _points.RemoveRange(_points.Count - 2, 2);
                if (_autoSetControlPoints)
                {
                    AutoSetStartAndEndControls();
                }
            }
        }
    }

    public void AddSegment(Vector2 anchorPos)
    {
        //P3 + (P3-P2) = P3*2 - P2
        _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
        //El segundo punto de control esta a la mitad del camino del 6to elemento.
        _points.Add((_points[_points.Count - 1] + anchorPos) * 0.5f);
        _points.Add(anchorPos);

        if (_autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(_points.Count - 1);
        }
    }

    public void SplitSegment(Vector2 anchorPos, int segmentIndex)
    {
        _points.InsertRange(segmentIndex * 3 + 2, new Vector2[] { Vector2.zero, anchorPos, Vector2.zero });
        if (AutoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(segmentIndex * 3 + 3);
        }
        else
        {
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
        }
    }

    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments > 2 || !_isClosed && NumSegments > 1)
        {
            if (anchorIndex == 0)
            {
                if (_isClosed)
                {
                    _points[_points.Count - 1] = _points[2];
                }
                _points.RemoveRange(anchorIndex - 2, 3);
            }
            else if (anchorIndex == _points.Count - 1 && !_isClosed)
            {
                _points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                _points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public Vector2[] GetPositionInSegment(int i)
    {
        return new Vector2[]
        {
            _points[i * 3],
            _points[i*3 + 1],
            _points[i*3 + 2],
            _points[LoopIndex(i*3 + 3)]
        };
    }

    public void MovePoint(int index, Vector2 pos)
    {
        Vector2 deltaMove = pos - _points[index];
        _points[index] = pos;

        if (index % 3 == 0)
        {
            if (_autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(index);
                return;
            }

            if (index + 1 < _points.Count || _isClosed)
            {
                _points[LoopIndex(index + 1)] += deltaMove;
            }
            if (index - 1 >= 0 || _isClosed)
            {
                _points[LoopIndex(index - 1)] += deltaMove;
            }
        }
        else
        {
            bool nextPointIsAnchor = (index + 1) % 3 == 0;
            int correspondingControlIndex = LoopIndex(nextPointIsAnchor ? index + 2 : index - 2);
            int anchorIndex = LoopIndex(nextPointIsAnchor ? index + 1 : index - 1);

            if (!IsClosed) return;

            float dst = (_points[anchorIndex] - _points[correspondingControlIndex]).magnitude;
            Vector2 dir = (_points[anchorIndex] - pos).normalized;
            _points[correspondingControlIndex] = _points[anchorIndex] + dir * dst;
        }
    }

    public int LoopIndex(int i)
    {
        int count = _points.Count;
        return i < 0 ? count - (Mathf.Abs(i) % count) : i % count;
    }

    void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if (i >= 0 && i < _points.Count || _isClosed)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }
        AutoSetStartAndEndControls();
    }

    void AutoSetAnchorControlPoints(int AnchorIndex)
    {
        Vector2 anchorPos = _points[AnchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (AnchorIndex - 3 >= 0 || _isClosed)
        {
            Vector2 offset = _points[LoopIndex(AnchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }

        if (AnchorIndex + 3 >= 0 || _isClosed)
        {
            Vector2 offset = _points[LoopIndex(AnchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = AnchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < _points.Count || _isClosed)
            {
                _points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * 0.5f;
            }
        }
    }

    void AutoSetStartAndEndControls()
    {
        if (!_isClosed)
        {
            _points[1] = (_points[0] + _points[2]) * 0.5f;
            _points[_points.Count - 2] = (_points[_points.Count - 1] + _points[_points.Count - 3] * 0.5f);
        }
    }

    void AutoSetAllControlPoints()
    {
        for (int i = 0; i < _points.Count; i += 3)
        {
            AutoSetAnchorControlPoints(i);
        }
        AutoSetStartAndEndControls();
    }
}
