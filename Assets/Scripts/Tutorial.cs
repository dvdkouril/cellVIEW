using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Tutorial : MonoBehaviour {
	//guide the user in the first of the app if he want
	// Use this for initialization
	public int current_step;
	public UImanager ui_manager;
	public RectTransform tutoRect;
	public CanvasGroup cg;
	public Text label;

	public GameObject skip_button;
	private bool in_tutorial = false;
	private int nbStep;

	[HideInInspector]
	public List<string> labels;
	//we use one canvas with a Label, image, and continue bouton.
	//we wait for the action to be done to continue

	void Start () {
		nbStep = labels.Count;
		labels = new List<string> ();
		string aString = "Welcome, we are going to guide you through the interface and help you navigate the scene representing the HIV virus surrounded by plasma molecule.";
		labels.Add (aString);
		aString =  "Familiarize yourself with the camera, left-click and drag your mouse to manipulate the scene and look around";
		labels.Add (aString);
		aString =  "left-click on a particular proteine. The protein is highligted, and her description appears in the right panel. You can hide the descrition panel using the toggle in the tool bar, or by clickin the buton inside the description panel";
		labels.Add (aString);
		aString =  "double-clic on a particular proteine. The camera zoom closer to it. Right-click anywhere or click the reset view button in the tool bar to exit the zoom view.";
		labels.Add (aString);
		aString =  "In order to see all the proteins presented here, use the hierarchy tree. Toggle the hierarchy tree visibility using the first icon in the tool bar";
		labels.Add (aString);
		aString = "You can use the tree to toggle the visibility of any element in the scene by using their checkbox. You can highlight all instance of one type of proteins by selecting an entry in tree. If you double click only the selected protein remains visible. Double click again to make all the other proteins reappear.";
		labels.Add (aString);
		aString = "Our last step is to see how the use of cross section can also help when visualising such dense environement. Click the toggle cross section in the tool bar. And click on the Plane Preset. The scene is now cut in half and we can see clearly a cross section of the HIV virus.";
		labels.Add (aString);
		aString = "With the plane cross secion active, toggle the setting panel using the button in the tool bar. Activate the depth cueing and zoom in to a satisfying effect that look like David Goodcell's painting.";
		labels.Add (aString);
		aString = "That's it play around, and let us how we can improve your experience.";
		label.text = labels [0];
	}

	// Update is called once per frame
	void Update () {

	}

	public void startTutorial(){
		//show the first step
		cg.alpha = 1;
		in_tutorial = true;
	}

	public void stopTutorial(){
		cg.alpha = 0;
		in_tutorial = false;
	}

	public void buttonContinue(){
		//go to the next step
		current_step += 1;
		nextStep ();
	}

	public void nextStep(){
		if (current_step == 1) {
			//hide the skip button
			skip_button.SetActive(false);
		}
		if (current_step == labels.Count) {
			stopTutorial();
		}
		else 
			label.text = labels [current_step];
	}
}
