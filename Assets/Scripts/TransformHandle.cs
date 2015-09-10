using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SelectionState
{
    Translate = 1,           // Instance will not be displayed
    Rotate = 2,          // Instance will be displayed with normal color
    Scale = 3      // Instance will be displayed with highlighted color
}

public enum ControlType
{
    None = -1,
    TranslateX = 0,        
    TranslateY = 1,         
    TranslateZ = 2,
    RotateX = 3,
    RotateY = 4,
    RotateZ = 5,
    RotateInner = 6,
    RotateOuter = 7,
    ScaleX = 8,
    ScaleY = 9,
    ScaleZ = 10,
    ScaleCenter = 11,
}

[ExecuteInEditMode]
public class TransformHandle : MonoBehaviour
{
    private bool _enabled;
    private float _handleSize;
    private float _nearestDistance;
    private float _customPickDistance = 5f;
    private SelectionState _state = SelectionState.Translate;

    private ControlType _nearestControl;
    private ControlType _currentControl = ControlType.None;

	private static Vector2 s_StartMousePosition;
	private static Vector2 s_CurrentMousePosition;

	//position handle
	private static Vector3 s_StartPosition;
	//rotation handle
	private static float s_RotationDist = 0.0f;
	private static Quaternion s_StartRotation;
	//scale handle
	private static Vector3 s_StartAxis;
	private static Vector3 s_StartScale;
	private static float s_ValueDrag;

	private float snap = 0.0f;
    
	[HideInInspector]
	public bool allow_manipulation=true;//*****//
	public CutObject cutobject;

    public void Enable()
    {
        _enabled = true;
    }

    public void Disable()
    {
        _enabled = false;
    }

    public void SetSelectionState(SelectionState state)
    {
        _state = state;
    }

    public ControlType NearestControl
    {
        get
        {
            if ((double)_nearestDistance <= 5.0)
                return _nearestControl;

            return ControlType.None;
        }
        set
        {
            _nearestControl = value;
        }
    }

    public void BeginHandle()
    {
        if (Event.current.type == EventType.Layout)
        {
            _nearestDistance = 5f;
            _nearestControl = ControlType.None;
        }
    }
    
    public void AddControl(ControlType type, float distance)
    {
        if ((double)distance < (double)_customPickDistance && (double)distance > 5.0)
            distance = 5f;

        if ((double)distance > (double)_nearestDistance)
            return;

        _nearestControl = type;
        _nearestDistance = distance;
    }

    //*****//

//    void OnEnable()
//    {
//#if UNITY_EDITOR
//        if (!EditorApplication.isPlaying) EditorApplication.update += Update;
//#endif
//    }

//    void Update()
//    {
        
//    }

    void OnGUI()
    {
//#if UNITY_EDITOR
//        if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
//        {
//            EditorUtility.SetDirty(this); // this is important, if omitted, "Mouse down" will not be display
//        }
//#endif

        if (!_enabled) return;

        _handleSize = MyHandleUtility.GetHandleSize(transform.position);
        
        BeginHandle();
		//test if treeView hasFocus
		if ((cutobject.tree_isVisible)&&(cutobject.tree_hasFocus (Event.current.mousePosition))) {
			return;
		}
		if (!Event.current.alt) {
			switch (_state) {
			case SelectionState.Scale:
				DoScaleHandle ();
				break;

			case SelectionState.Rotate:
				DoRotateHandle ();
				break;

			case SelectionState.Translate:
				DoTranslateHandle ();
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
    }

    private void DoTranslateHandle()
    {
        if (Event.current.type == EventType.mouseDown)
        {
            AddControl(ControlType.TranslateX, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.right*_handleSize));
            AddControl(ControlType.TranslateX, MyHandleUtility.DistanceToCircle(transform.position + transform.right*_handleSize, _handleSize*0.2f));
            AddControl(ControlType.TranslateY, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.up*_handleSize));
            AddControl(ControlType.TranslateY, MyHandleUtility.DistanceToCircle(transform.position + transform.up*_handleSize, _handleSize*0.2f));
            AddControl(ControlType.TranslateZ, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.forward*_handleSize));
            AddControl(ControlType.TranslateZ, MyHandleUtility.DistanceToCircle(transform.position + transform.forward*_handleSize, _handleSize*0.2f));

            _currentControl = NearestControl;
			s_CurrentMousePosition = s_StartMousePosition = Event.current.mousePosition;
			s_StartPosition = transform.position;
			//Event.current.Use();//?
        }
		if (Event.current.type == EventType.MouseDrag) {
			//toggle the navigation
			MyHandleUtility.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
			s_CurrentMousePosition += Event.current.delta;
			Vector3 slideDirection = GetAxe(_currentControl);
			float num = MyHandleUtility.SnapValue(MyHandleUtility.CalcLineTranslation(s_StartMousePosition, s_CurrentMousePosition, 
			                                                                          s_StartPosition, slideDirection), snap);
			Vector3 vector3 = MyHandleUtility.matrix.MultiplyVector(slideDirection);
			Vector3 v = MyHandleUtility.matrix.MultiplyPoint(s_StartPosition) + vector3 * num;
			transform.position = MyHandleUtility.inverseMatrix.MultiplyPoint(v);
			//	GUI.changed = true;
			//	current.Use();
			//	break;
		}
    }

    private void DoRotateHandle()
    {
		//problem with Camera.current ? why not main ?
		if (Event.current.type == EventType.MouseDown)
        {
            AddControl(ControlType.RotateInner, MyHandleUtility.DistanceToDisc(transform.position, Camera.main.transform.forward, _handleSize)/2f);
			AddControl(ControlType.RotateOuter, MyHandleUtility.DistanceToDisc(transform.position, Camera.main.transform.forward, _handleSize*1.1f)/2f);
			AddControl(ControlType.RotateX, MyHandleUtility.DistanceToArc(transform.position, transform.right, Vector3.Cross(transform.right, Camera.main.transform.forward).normalized, 180f, _handleSize)/2f);
			AddControl(ControlType.RotateY, MyHandleUtility.DistanceToArc(transform.position, transform.up, Vector3.Cross(transform.up, Camera.main.transform.forward).normalized, 180f, _handleSize)/2f);
			AddControl(ControlType.RotateZ, MyHandleUtility.DistanceToArc(transform.position, transform.forward, Vector3.Cross(transform.forward, Camera.main.transform.forward).normalized, 180f, _handleSize)/2f);

			//Disc is rotation, position, rotation * Vector3.forward
            _currentControl = NearestControl;
			float size = MyHandleUtility.GetHandleSize(transform.position);
			Vector3 slideDirection = GetAxe(_currentControl);
			s_StartPosition = MyHandleUtility.ClosestPointToDisc(transform.position, slideDirection, _handleSize);//or transform.rotation * VEctor3.forward ?
			//s_RotationDist = 0.0f;
			s_StartRotation = transform.rotation;
			s_StartAxis = slideDirection;
			s_CurrentMousePosition = s_StartMousePosition = Event.current.mousePosition;
		}
		if (Event.current.type == EventType.MouseDrag) {
			//what about freeRotate? arcBall?
			float size = MyHandleUtility.GetHandleSize(transform.position);
			Vector3 axis = GetAxe(_currentControl);
			Vector3 normalized = Vector3.Cross(axis, transform.position - s_StartPosition).normalized;
			s_CurrentMousePosition += Event.current.delta;
			s_RotationDist = (float) ((double) MyHandleUtility.CalcLineTranslation(s_StartMousePosition, s_CurrentMousePosition, 
			                                                                       s_StartPosition, normalized) / (double) _handleSize * 30.0);
			s_RotationDist = MyHandleUtility.SnapValue(s_RotationDist, snap);
			transform.rotation = Quaternion.AngleAxis(s_RotationDist * -1f, s_StartAxis) * s_StartRotation;
		}
    }

    private void DoScaleHandle()
    {
        if (Event.current.type == EventType.MouseDown)
        {
            AddControl(ControlType.ScaleCenter, MyHandleUtility.DistanceToCircle(transform.position, _handleSize*0.15f));
            AddControl(ControlType.ScaleX, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.right*_handleSize));
            AddControl(ControlType.ScaleX, MyHandleUtility.DistanceToCircle(transform.position + transform.right*_handleSize, _handleSize*0.2f));
            AddControl(ControlType.ScaleY, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.up*_handleSize));
            AddControl(ControlType.ScaleY, MyHandleUtility.DistanceToCircle(transform.position + transform.up*_handleSize, _handleSize*0.2f));
            AddControl(ControlType.ScaleZ, MyHandleUtility.DistanceToLine(transform.position, transform.position + transform.forward*_handleSize));
            AddControl(ControlType.ScaleZ, MyHandleUtility.DistanceToCircle(transform.position + transform.forward*_handleSize, _handleSize*0.2f));

            _currentControl = NearestControl;
			s_CurrentMousePosition = s_StartMousePosition = Event.current.mousePosition;
			s_StartScale = transform.localScale;
			s_ValueDrag = 0.0f;
		}
		if (Event.current.type == EventType.MouseDrag) {
			if (_currentControl==ControlType.ScaleCenter){
				s_ValueDrag += MyHandleUtility.niceMouseDelta * 0.01f;
				Vector3 value = (MyHandleUtility.SnapValue(s_ValueDrag, snap) + 1f) * s_StartScale;
				//s_ScaleDrawLength = value / s_StartScale;
				transform.localScale = value;
			}
			else {
				Vector3 direction = GetAxe(_currentControl);
				s_CurrentMousePosition += Event.current.delta;
				float num = MyHandleUtility.SnapValue((float) (1.0 + (double) MyHandleUtility.CalcLineTranslation(s_StartMousePosition, s_CurrentMousePosition, 
			                                                                                          transform.position, direction) / (double) _handleSize), snap);
				transform.localScale = s_StartScale * num;
			}
		}
    }

	private Vector3 GetAxe(ControlType control)
	{
		switch (control)
			{
			case ControlType.TranslateX:
				return transform.right;
			case ControlType.TranslateY:
				return transform.up;
			case ControlType.TranslateZ:
				return transform.forward;
			case ControlType.RotateX:
				return transform.right;
			case ControlType.RotateY:
				return transform.up;
			case ControlType.RotateZ:
				return transform.forward;
			case ControlType.RotateInner:
				return Camera.main.transform.forward;
			case ControlType.RotateOuter:
				return Camera.main.transform.forward;
			case ControlType.ScaleX:
				return transform.right;
			case ControlType.ScaleY:
				return transform.up;
			case ControlType.ScaleZ:
				return transform.forward;
			case ControlType.ScaleCenter:
				return Vector3.one;
			case ControlType.None:
				return transform.right;
			default:
				throw new Exception("Control type error");
			}
		return Vector3.zero;
	}

    private Color GetColor(ControlType control)
    {
        if (_currentControl == control) return MyHandleUtility.selectedColor;
        else
        {
            switch (control)
            {
                case ControlType.TranslateX:
                    return MyHandleUtility.xAxisColor;
                case ControlType.TranslateY:
                    return MyHandleUtility.yAxisColor;
                case ControlType.TranslateZ:
                    return MyHandleUtility.zAxisColor;
                case ControlType.RotateX:
                    return MyHandleUtility.xAxisColor;
                case ControlType.RotateY:
                    return MyHandleUtility.yAxisColor;
                case ControlType.RotateZ:
                    return MyHandleUtility.zAxisColor;
                case ControlType.RotateInner:
                    return MyHandleUtility.centerColor;
                case ControlType.RotateOuter:
                    return MyHandleUtility.centerColor;
                case ControlType.ScaleX:
                    return MyHandleUtility.xAxisColor;
                case ControlType.ScaleY:
                    return MyHandleUtility.yAxisColor;
                case ControlType.ScaleZ:
                    return MyHandleUtility.zAxisColor;
                case ControlType.ScaleCenter:
                    return MyHandleUtility.centerColor;
                case ControlType.None:
                    break;
                default:
                    throw new Exception("Control type error");
            }
        }

        return Color.magenta;
    }

    private void OnRenderObject()
    {
        if (!_enabled || Camera.current != Camera.main) return;

        _handleSize = MyHandleUtility.GetHandleSize(transform.position);

        MyHandleUtility.DrawWireMesh(GetComponent<MeshFilter>().sharedMesh, transform, new Color(0.5f, 0.8f, 0.5f));

        switch (_state)
        {
            case SelectionState.Scale:
                DrawScaleHandle();
                break;

            case SelectionState.Rotate:
                DrawRotationHandle();
                break;

            case SelectionState.Translate:
                DrawTranslateHandle();
                break;
        }
    }

    private void DrawTranslateHandle()
    {
        MyHandleUtility.DrawLine(transform.position, transform.position + transform.right*_handleSize*0.9f, GetColor(ControlType.TranslateX));
        MyHandleUtility.DrawConeCap(transform.position + transform.right*_handleSize, Quaternion.LookRotation(transform.right), _handleSize*0.2f, GetColor(ControlType.TranslateX));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.up*_handleSize*0.9f, GetColor(ControlType.TranslateY));
        MyHandleUtility.DrawConeCap(transform.position + transform.up*_handleSize, Quaternion.LookRotation(transform.up), _handleSize*0.2f, GetColor(ControlType.TranslateY));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.forward*_handleSize*0.9f, GetColor(ControlType.TranslateZ));
        MyHandleUtility.DrawConeCap(transform.position + transform.forward*_handleSize, Quaternion.LookRotation(transform.forward), _handleSize*0.2f, GetColor(ControlType.TranslateZ));
    }

    private void DrawRotationHandle()
    {
        MyHandleUtility.DrawWireDisc(transform.position, Camera.current.transform.forward, _handleSize, GetColor(ControlType.RotateInner));
        MyHandleUtility.DrawWireDisc(transform.position, Camera.current.transform.forward, _handleSize*1.1f, GetColor(ControlType.RotateOuter));
        MyHandleUtility.DrawWireArc(transform.position, transform.right, Vector3.Cross(transform.right, Camera.current.transform.forward).normalized, 180f, _handleSize, GetColor(ControlType.RotateX));
        MyHandleUtility.DrawWireArc(transform.position, transform.up, Vector3.Cross(transform.up, Camera.current.transform.forward).normalized, 180f, _handleSize, GetColor(ControlType.RotateY));
        MyHandleUtility.DrawWireArc(transform.position, transform.forward, Vector3.Cross(transform.forward, Camera.current.transform.forward).normalized, 180f, _handleSize, GetColor(ControlType.RotateZ));
    }

    private void DrawScaleHandle()
    {
        MyHandleUtility.DrawCubeCap(transform.position, transform.rotation, _handleSize*0.15f, GetColor(ControlType.ScaleCenter));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.right*_handleSize, GetColor(ControlType.ScaleX));
        MyHandleUtility.DrawCubeCap(transform.position + transform.right*_handleSize, transform.rotation, _handleSize*0.1f, GetColor(ControlType.ScaleX));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.up*_handleSize, GetColor(ControlType.ScaleY));
        MyHandleUtility.DrawCubeCap(transform.position + transform.up*_handleSize, transform.rotation, _handleSize*0.1f, GetColor(ControlType.ScaleY));

        MyHandleUtility.DrawLine(transform.position, transform.position + transform.forward*_handleSize, GetColor(ControlType.ScaleZ));
        MyHandleUtility.DrawCubeCap(transform.position + transform.forward*_handleSize, transform.rotation, _handleSize*0.1f, GetColor(ControlType.ScaleZ));
    }
}
