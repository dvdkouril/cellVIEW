using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Focuson : MonoBehaviour
{
	float timerForDoubleClick = 0.0f;
	float delay = 0.3f;
	bool isDoubleClick = false;
	
	void Update()
	{
		if (isDoubleClick == true)
		{
			timerForDoubleClick += Time.deltaTime;
		}
		
		
		if (timerForDoubleClick >= delay)
		{
			timerForDoubleClick = 0.0f;
			isDoubleClick = false;
		}
		
	}
	
	
	void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(0) && isDoubleClick == false)
		{
			Debug.Log ("Mouse clicked once");
			isDoubleClick = true;
		}
	}
	
	void OnMouseDown()
	{
		Camera.main.GetComponent<NavigateCamera>().hardFocusFlag = false;
		if (Input.GetMouseButtonDown (0)) {
			Debug.Log("ISWORKING!!!!");
			Camera.main.GetComponent<NavigateCamera>().hardFocusFlag = true;
		}
		if (isDoubleClick == true && timerForDoubleClick < delay)
		{
			Debug.Log("ISWORKING!!!!");
			Camera.main.GetComponent<NavigateCamera>().hardFocusFlag = true;
		}
	}
}