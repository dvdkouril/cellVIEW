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
    public Vector3 DampTargetPosition = new Vector3(0, 0, 0);
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
    
    private float deltaTime = 0;
    private float lastUpdateTime = 0;
    private float deltaScroll;
    
    public bool freeMoveMode = false;
    public bool lockInteractions = false;
    public bool animateCameraIn = false;
    public bool animateCameraOut = false;

    public Vector3 StoredPosition;

	//
    void Update()
    {
        deltaTime = Time.realtimeSinceStartup - lastUpdateTime;
        lastUpdateTime = Time.realtimeSinceStartup;
        float smoothTime = 0.5f;              

        //*********//

        if (animateCameraIn)
        {
            freeMoveMode = false;
            lockInteractions = true;

            //TargetPosition=DampTargetPosition;
			float d = Vector3.Distance(transform.position,TargetPosition);
			PersistantSettings.Instance.SpeedFactor = (d*2.0f)/320.0f;
            if (Vector3.Distance(transform.position, DampTargetPosition) < 1.0f)
            {
                transform.position = Vector3.SmoothDamp(transform.position, DampTargetPosition, ref velocity, smoothTime * 5);
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, DampTargetPosition, ref velocity, smoothTime);
            }

            if (Vector3.Distance(transform.position, DampTargetPosition) < 0.2f)
            {
                animateCameraIn = false;
                lockInteractions = false;
				PersistantSettings.Instance.SpeedFactor = 0.2f;
            }
        }

        //*********//

        if (animateCameraOut)
        {
            lockInteractions = true;
			StoredPosition = new Vector3 (-320, 0, 0);
			TargetPosition = new Vector3 (0, 0, 0);

            //TargetPosition=DampTargetPosition;

            transform.forward = Vector3.Normalize(TargetPosition - transform.position);

            //var rotation = Quaternion.Euler(EulerAngleY, EulerAngleX, 0.0f);
            //var position = TargetPosition - rotation * Vector3.forward * Vector3.Distance(TargetPosition, transform.position);

            //transform.rotation = rotation;
            //transform.position = position;

			float d = Vector3.Distance(transform.position,TargetPosition);
			PersistantSettings.Instance.SpeedFactor = (d*2.0f)/320.0f;

			if (Vector3.Distance(transform.position, StoredPosition) < 1.0f)
            {
                transform.position = Vector3.SmoothDamp(transform.position, StoredPosition, ref velocity, smoothTime * 5);
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, StoredPosition, ref velocity, smoothTime);
            }

            if (Vector3.Distance(transform.position, StoredPosition) < 0.25f)
            {
                animateCameraOut = false;
                lockInteractions = false;
                //freeMoveMode = true;
				//PersistantSettings.Instance.SpeedFactor = 2.0f;
            }
        }

        //*********//

        if(freeMoveMode)
        {
            if (forward)
            {
                transform.position += gameObject.transform.forward * TranslationSpeed * deltaTime;
            }

            if (backward)
            {
                transform.position -= gameObject.transform.forward * TranslationSpeed * deltaTime;
            }

            if (right)
            {
                transform.position += gameObject.transform.right * TranslationSpeed * deltaTime;
            }

            if (left)
            {
                transform.position -= gameObject.transform.right * TranslationSpeed * deltaTime;
            }

            if (freeMoveMode)
            {
                TargetPosition = transform.position + transform.forward * 2;
                Distance = Vector3.Distance(transform.position, TargetPosition);
            }
        }
    }

    private float leftClickTimeStart;
    private float rightClickTimeStart;
    private float doubleClickTimeStart;

    public bool hardFocusFlag = false;

	public void resetCamera(){

		SceneManager.Instance.SetSelectedElement(-1);
		animateCameraIn = false;
		lockInteractions = false;
		//animateCameraOut = true;
		PersistantSettings.Instance.NearCullPlane = 0;
		StoredPosition = new Vector3 (-320, 0, 0);
		TargetPosition = new Vector3 (0, 0, 0);
		transform.position = new Vector3 (-320, 0, 0);
		transform.rotation = Quaternion.AngleAxis (90, Vector3.up);
		PersistantSettings.Instance.SpeedFactor = 2.0f;
	}

    private void OnGUI()
	{
		if (Event.current.keyCode == KeyCode.R)
		{
			resetCamera();
		}
		if (lockInteractions) return;

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

        // Arc ball rotation
        if (Event.current.type == EventType.mouseDrag && Event.current.button == 0)
        {
			EulerAngleX += Event.current.delta.x * AcrBallRotationSpeed;
			EulerAngleY += Event.current.delta.y * AcrBallRotationSpeed;

            transform.rotation *= Quaternion.Euler(Event.current.delta.y * AcrBallRotationSpeed, Event.current.delta.x * AcrBallRotationSpeed, 0.0f);
            var angles = transform.rotation.eulerAngles;
            angles.z = 0;
            transform.rotation = Quaternion.Euler(angles);
            var position = TargetPosition - transform.rotation * Vector3.forward * Vector3.Distance(TargetPosition, transform.position);

			
			transform.position = position;
		}

		if (Event.current.type == EventType.mouseDrag && Event.current.button == 2)//freeMoveMode && 
        {
            TargetPosition += transform.up * Event.current.delta.y * PannigSpeed;
            transform.position += transform.up * Event.current.delta.y * PannigSpeed;

            TargetPosition -= transform.right * Event.current.delta.x * PannigSpeed;
            transform.position -= transform.right * Event.current.delta.x * PannigSpeed;
        }

		if (Event.current.type == EventType.ScrollWheel)//freeMoveMode && 
        {
            transform.position -= transform.forward * Event.current.delta.y * ZoomingSpeed;
			//distance to target;
			float d = Vector3.Distance(transform.position,TargetPosition);
			PersistantSettings.Instance.SpeedFactor = (d*2.0f)/320.0f;
			Debug.Log (PersistantSettings.Instance.SpeedFactor.ToString());
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

        bool doubleClick = false;
        
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            var delta = Time.realtimeSinceStartup - doubleClickTimeStart;
            Debug.Log(delta);
            if (delta < 0.25f)
            {
                doubleClick = true;
            }

            doubleClickTimeStart = Time.realtimeSinceStartup;
        }

        //*****//

        bool rightClick = false;

        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            rightClickTimeStart = Time.realtimeSinceStartup;
        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 1)
        {
            rightClickTimeStart = 0;
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
        {
            var delta = Time.realtimeSinceStartup - rightClickTimeStart;
                        
            if (delta < 0.5f)
            {
                rightClick = true;
            }
        }
              

        //*****//

        bool leftClick = false;

	    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
	    {
            leftClickTimeStart = Time.realtimeSinceStartup;
	    }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            leftClickTimeStart = 0;
        }

        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            var delta = Time.realtimeSinceStartup - leftClickTimeStart;
            if (delta < 0.5f)
            {
                leftClick = true;
            }
        }

        //*****//

        if (rightClick && !freeMoveMode)
        {
            SceneManager.Instance.SetSelectedElement(-1);
            animateCameraOut = true;
            PersistantSettings.Instance.NearCullPlane = 0;
        }

        //if (!freeMoveMode) return;

        if (doubleClick || leftClick)
        {
			Debug.Log ("object picking");
			var mousePos = Event.current.mousePosition;

            render._mousePos = mousePos;
            render._rightMouseDown = true;
            hardFocusFlag = doubleClick;

            //Ray CameraRay = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, Screen.height - mousePos.y, 0));
            //RaycastHit hit;

            //// If we hit an object
            //if (Physics.Raycast(CameraRay, out hit, 10000))
            //{
            //    var transformHandle = hit.collider.gameObject.GetComponent<TransformHandle>();
            //    // If we hit a new selectable object
            //    if (transformHandle != null && transformHandle != _selectedTransformHandle)
            //    {
            //        if (hit.collider.gameObject.GetComponent<CutObject>().Display){
            //            if (_selectedTransformHandle != null)
            //            {
            //                //Debug.Log("Reset");
            //                _selectedTransformHandle.Disable();
            //            }
            //            //Debug.Log("Selected transform: " + transformHandle.gameObject.name);
            //            transformHandle.Enable();
            //            transformHandle.SetSelectionState(_currentState);
            //            _selectedTransformHandle = transformHandle;
            //            handleMode = true;
            //        }
            //        else {
            //            render._mousePos = mousePos;
            //            render._rightMouseDown = true;
            //        }
            //    }
            //    // If we hit a non-selectable object
            //    else if (transformHandle == null && _selectedTransformHandle != null)
            //    {
            //        //Debug.Log("Reset");
            //        _selectedTransformHandle.Disable();
            //        _selectedTransformHandle = null;
            //        handleMode=false;
            //    }
            //    else {
            //        render._mousePos = mousePos;
            //        render._rightMouseDown = true;
            //    }
            //}
            //// If we miss a hit
            //else if (_selectedTransformHandle != null)
            //{
            //    //Debug.Log("Reset");
            //    _selectedTransformHandle.Disable();
            //    _selectedTransformHandle = null;
            //    handleMode=false;
            //    //check if hit a  protein
            //    render._mousePos = mousePos;

            //    render._rightMouseDown = true;
            //}
            //else {
            //    render._mousePos = mousePos;
            //    render._rightMouseDown = true;
            //}
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

