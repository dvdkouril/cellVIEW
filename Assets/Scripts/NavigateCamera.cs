using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Component = UnityEngine.Component;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class NavigateCamera : MonoBehaviour
{

	[HideInInspector]
    public SelectionState _currentState = SelectionState.Translate;
    
	[HideInInspector]
	public TransformHandle _selectedTransformHandle;
	[HideInInspector]
	public bool handleMode=false;

    const float DefaultDistance = 5.0f;

    public Vector3 TargetPosition;
	public Vector3 DampTargetPosition;
    public float AcrBallRotationSpeed = 0.25f;
    public float FpsRotationSpeed = 0.25f;
    public float TranslationSpeed = 2.0f;
    public float ZoomingSpeed = 2.0f;
    public float PannigSpeed = 0.25f;
    
    public float Distance;
    public float EulerAngleX;
    public float EulerAngleY;

	public SceneRenderer render;
    /*****/

    private GameObject Target;
    private bool forward;
    private bool backward;
    private bool right;
    private bool left;
	private Vector3 velocity = Vector3.zero;
    /*****/

    void OnEnable()
    {
        #if UNITY_EDITOR
        if (!EditorApplication.isPlaying) EditorApplication.update += Update;
        #endif
    }

    private float deltaTime = 0;
    private float lastUpdateTime = 0;

    private float deltaScroll;

    void Update()
    {
        deltaTime = Time.realtimeSinceStartup - lastUpdateTime;
        lastUpdateTime = Time.realtimeSinceStartup;
		float smoothTime = 1.0f; 
		if (DampTargetPosition != Vector3.zero) {
			//TargetPosition=DampTargetPosition;
			transform.position = Vector3.SmoothDamp(transform.position,DampTargetPosition,ref velocity,smoothTime);
			if (Vector3.Distance(transform.position,DampTargetPosition) < 1.00f) {
				DampTargetPosition = Vector3.zero;
			}
			//transform.position = TargetPosition - transform.forward * Distance;
		}
        //Debug.Log(deltaTime);

        if (forward)
        {
            TargetPosition += gameObject.transform.forward * TranslationSpeed * deltaTime; 
            transform.position += gameObject.transform.forward * TranslationSpeed * deltaTime; 
        }

        if (backward)
        {
            TargetPosition -= gameObject.transform.forward * TranslationSpeed * deltaTime;
            transform.position -= gameObject.transform.forward * TranslationSpeed * deltaTime; 
        }

        if (right)
        {
            TargetPosition += gameObject.transform.right * TranslationSpeed * deltaTime;
            transform.position += gameObject.transform.right * TranslationSpeed * deltaTime; 
        }

        if (left)
        {
            TargetPosition -= gameObject.transform.right * TranslationSpeed * deltaTime;
            transform.position -= gameObject.transform.right * TranslationSpeed * deltaTime; 
        }

    }

	public void CenterView (){
		Distance = 320.0f;
		TargetPosition = Vector3.zero;
		transform.position = TargetPosition - transform.forward * Distance;
	}
	
	private void OnGUI()
	{
		#if UNITY_EDITOR
        if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
        {
            EditorUtility.SetDirty(this); // this is important, if omitted, "Mouse down" will not be display
        }
        #endif

		//if handleMode and the mouse is on the TreeView do nothing
		if ( _selectedTransformHandle != null ){
			if (_selectedTransformHandle.cutobject.tree_isVisible){
				Debug.Log ("focus where ?"+Event.current.mousePosition.ToString ());
				if (_selectedTransformHandle.cutobject.tree_hasFocus(Event.current.mousePosition)){
					return;
				}
			}
		}

		bool arc_ball_altkey = handleMode? Event.current.alt:!Event.current.alt;
			// Arc ball rotation
		if (arc_ball_altkey && Event.current.type == EventType.mouseDrag && Event.current.button == 0) {
			EulerAngleX += Event.current.delta.x * AcrBallRotationSpeed;
			EulerAngleY += Event.current.delta.y * AcrBallRotationSpeed; 

			var rotation = Quaternion.Euler (EulerAngleY, EulerAngleX, 0.0f);
			//var position = TargetPosition + rotation * Vector3.back * Distance;//back ?
			var position = TargetPosition - rotation * Vector3.forward * Distance;//back ?
			transform.rotation = rotation;
			transform.position = position;
		}

		// Fps rotation
		if (Event.current.control && Event.current.type == EventType.mouseDrag && Event.current.button == 0) {
			EulerAngleX += Event.current.delta.x * FpsRotationSpeed;
			EulerAngleY += Event.current.delta.y * FpsRotationSpeed; 

			var rotation = Quaternion.Euler (EulerAngleY, EulerAngleX, 0.0f);

			transform.rotation = rotation;
			TargetPosition = transform.position + transform.forward * Distance;
		}

		if (arc_ball_altkey && Event.current.type == EventType.mouseDrag && Event.current.button == 2) {
			TargetPosition += transform.up * Event.current.delta.y * PannigSpeed;
			transform.position += transform.up * Event.current.delta.y * PannigSpeed;

			TargetPosition -= transform.right * Event.current.delta.x * PannigSpeed;
			transform.position -= transform.right * Event.current.delta.x * PannigSpeed; 
		}

		if (Event.current.type == EventType.ScrollWheel) {
			Distance += Event.current.delta.y * ZoomingSpeed;
			transform.position = TargetPosition - transform.forward * Distance;

			if (Distance < 0) {
				TargetPosition = transform.position + transform.forward * DefaultDistance;
				Distance = Vector3.Distance (TargetPosition, transform.position);
			}
			var d = Input.GetAxis("Mouse ScrollWheel");
			if (d < 0) {
				//zooming out
				SceneManager.Instance.SetSelectedElement(-1);
			}
		}
        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.R)
            {
				if (handleMode) {
					//reset current handle to 0,0,0
					_selectedTransformHandle.transform.position = Vector3.zero;
					_selectedTransformHandle.transform.rotation = Quaternion.identity;
					_selectedTransformHandle.transform.localScale = Vector3.one*50;//depend on the type
					if (_selectedTransformHandle.gameObject.GetComponent<CutObject>().CutType == 0 )
						_selectedTransformHandle.transform.localScale = Vector3.one*10;
				}
				else {
                	Distance = 320.0f;
                	TargetPosition = Vector3.zero;
                	transform.position = TargetPosition - transform.forward * Distance;
				}
				SceneManager.Instance.SetSelectedElement(-1);//unselect everything ?
			}
            //if (Event.current.keyCode == KeyCode.F)
            //{
            //    if (!Target)
            //    {
            //        Target = GameObject.Find("Selected Element");
            //    }

            //    if (Target)
            //    {
            //        //Distance = 75;
            //        TargetPosition = Target.gameObject.transform.position;
            //        transform.position = TargetPosition - transform.forward * Distance;
            //    }
            //}
        }

        if (Event.current.keyCode == KeyCode.W)
        {
            forward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.S)
        {
            backward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.A)
        {
            left = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.D)
        {
            right = Event.current.type == EventType.KeyDown;
        }

        //*********//
        // Object picking
        //*********//

        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
			Debug.Log ("object picking");
			var mousePos = Event.current.mousePosition;
            Ray CameraRay = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, Screen.height - mousePos.y, 0));
            RaycastHit hit;

            // If we hit an object
            if (Physics.Raycast(CameraRay, out hit, 10000))
            {
                var transformHandle = hit.collider.gameObject.GetComponent<TransformHandle>();
                // If we hit a new selectable object
                if (transformHandle != null && transformHandle != _selectedTransformHandle)
                {
					if (hit.collider.gameObject.GetComponent<CutObject>().Display){
	                    if (_selectedTransformHandle != null)
	                    {
	                        //Debug.Log("Reset");
	                        _selectedTransformHandle.Disable();
	                    }
	                    //Debug.Log("Selected transform: " + transformHandle.gameObject.name);
	                    transformHandle.Enable();
	                    transformHandle.SetSelectionState(_currentState);
	                    _selectedTransformHandle = transformHandle;
						handleMode = true;
					}
					else {
						render._mousePos = mousePos;
						render._rightMouseDown = true;
					}
				}
                // If we hit a non-selectable object
                else if (transformHandle == null && _selectedTransformHandle != null)
                {
                    //Debug.Log("Reset");
                    _selectedTransformHandle.Disable();
                    _selectedTransformHandle = null;
					handleMode=false;
                }
				else {
					render._mousePos = mousePos;
					render._rightMouseDown = true;
				}
            }
            // If we miss a hit
            else if (_selectedTransformHandle != null)
            {
                //Debug.Log("Reset");
                _selectedTransformHandle.Disable();
                _selectedTransformHandle = null;
				handleMode=false;
            	//check if hit a  protein
				render._mousePos = mousePos;

				render._rightMouseDown = true;
			}
			else {
				render._mousePos = mousePos;
				render._rightMouseDown = true;
			}
        }

        if (Event.current.keyCode == KeyCode.Alpha1)
        {
            _currentState = SelectionState.Translate;
        }

        if (Event.current.keyCode == KeyCode.Alpha2)
        {
            _currentState = SelectionState.Rotate;
        }

        if (Event.current.keyCode == KeyCode.Alpha3)
        {
            _currentState = SelectionState.Scale;
        }

        if (_selectedTransformHandle)
        {
            _selectedTransformHandle.SetSelectionState(_currentState);
        }
    }
}

