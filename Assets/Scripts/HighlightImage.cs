using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HighlightImage : MonoBehaviour {
	public Image image;
	private bool doit;
	// Use this for initialization
	void Start () {
	
	}

	void OnEnable()
	{
		// Register this object in the cut object cache
		doit = true;
	}
	
	void OnDisable()
	{
		// De-register this object in the cut object cache
		doit = false;
	}


	// Update is called once per frame
	void Update () {
		//image.color = new Color (image.color.r,image.color.g,image.color.b,Mathf.PingPong (Time.time, 1));
		if (doit) image.color = new Color (Mathf.PingPong (Time.time, 1),Mathf.PingPong (Time.time, 1),0,1);
	}
}
