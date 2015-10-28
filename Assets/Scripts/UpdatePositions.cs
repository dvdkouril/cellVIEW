using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;

[ExecuteInEditMode]
public class UpdatePositions : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PersistantSettings.Instance.EnableBrownianMotion = false;
	}
	
	// Update is called once per frame
	void Update () {
		//grab child position and update buffer position ?
		//deactivate the jitter ? or use the jitter and evaluate collision ?
		//HandleUtility.Repaint ();
		int i = 0;
		foreach (Transform child in transform) {
			SceneManager.Instance.ProteinInstancePositions[i]= child.position;
			SceneManager.Instance.ProteinInstanceRotations[i]= Helper.QuanternionToVector4(child.rotation);
			i+=1;
		}
		ComputeBufferManager.Instance.ProteinInstancePositions.SetData(SceneManager.Instance.ProteinInstancePositions.ToArray());
		ComputeBufferManager.Instance.ProteinInstanceRotations.SetData(SceneManager.Instance.ProteinInstanceRotations.ToArray());
	}
}
