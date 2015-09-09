using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class UImanager : MonoBehaviour {
	//populate recipe liste
	//handle button callback
	public ListBox recipe_liste_ui;
	public ListBox settings_ui;
	public ListBox cuteObject;
	public ListBox cuteObjectType;

	public HandleSelection _sel;
	public RecipeTreeUI recipe_ingredient_ui;
	//public TreeViewControl cutObjectFilter;

	public GameObject panel_canvas;
	public int maxwidth;
	public JSONNode AllRecipes;
	public Texture2D cursorTexture;
	public float speed;
	public GameObject progressBar;
	public RectTransform treeviewHolder;

	private RectTransform panel;
	private string[] recipe_liste;
	private GameObject camerafx_panel;
	private CanvasGroup panel_fx;

	private bool camera_toggle=false;
	private Text progressLabel;
	private RectTransform progressBarRec;
	private float maxW;

	private bool _treeVisible=false;

	private int prev_cross=-1;
	private int current_cross=-1;
	private int current_panel=-1;
	private Sprite[] sprites;
	void  gatherRecipes(){
		if (AllRecipes == null) 
			AllRecipes = Helper.GetAllRecipeInfo ();
		if (recipe_liste==null) {
			var listesRecipe = AllRecipes.GetAllKeys();
			listesRecipe.Add ("Browse");
			recipe_liste=listesRecipe.ToArray();
		}
	}
	
	// Use this for initialization
	void Awake () {
		gatherRecipes ();
		recipe_liste_ui.optionStrings = recipe_liste;
		recipe_liste_ui.optionSprites = null;
		recipe_liste_ui.optionInts = new int[recipe_liste.Length];
		for (int i=0; i < recipe_liste.Length; i++)
			recipe_liste_ui.optionInts [i] = i;
		recipe_liste_ui.SetSelectedIndex (0);
		//recipe_liste_ui.valueInt = 0;
		//recipe_liste_ui.valueString = recipe_liste [0];
		panel = panel_canvas.GetComponent<RectTransform> ();
		camerafx_panel = GameObject.Find ("PanelFX");
		if (camerafx_panel != null )
			panel_fx = camerafx_panel.GetComponent<CanvasGroup>();
		camera_toggle = false;
		progressLabel = progressBar.GetComponentInChildren<Text> ();
		progressBarRec = progressBar.GetComponentInChildren<RectTransform> ();
		maxW = progressBar.GetComponent<RectTransform> ().rect.width;
		sprites = Resources.LoadAll<Sprite>("Textures");
		cuteObjectType.onValueChanged = cuteObjectTypeChanged;
		cuteObject.onValueChanged = cuteObjectChanged;
		//treeviewHolder = GameObject.Find ("TreeViewPlaceHolder").GetComponent<RectTransform> ();
		//shoulld we load asynchrone the scene while user look at the info+control?
	}

	public void cuteObjectTypeChanged(ListBox lb){
		if (cuteObject.valueInt < SceneManager.Instance.CutObjects.Count) {
			if (lb.valueInt == 0)
				SceneManager.Instance.CutObjects [cuteObject.valueInt].CutType = CutType.Plane;
			else if (lb.valueInt == 1)
				SceneManager.Instance.CutObjects [cuteObject.valueInt].CutType = CutType.Sphere;
			else if (lb.valueInt == 2)
				SceneManager.Instance.CutObjects [cuteObject.valueInt].CutType = CutType.Cube;
			else
				SceneManager.Instance.CutObjects [cuteObject.valueInt].CutType = CutType.Plane;
		}
		cuteObject.optionSprites [cuteObject.valueInt] = lb.optionSprites [lb.valueInt];
		//cuteObject.Reset();
		//cuteObject.SetSelectedIndex (cuteObject.valueInt);
	}

	public void toggleDisplayCutObj(bool value) {
		SceneManager.Instance.CutObjects [cuteObject.valueInt].Display = value;
	}

	public void toggleDisplayAllCutObject(bool value){
		//undisplay all ?
		for (int i =0; i < SceneManager.Instance.CutObjects.Count; i++) {
			SceneManager.Instance.CutObjects [i].Display = value;
		} 
	}

	public void toggleDisplayTreeCutObj(bool value) {
		Debug.Log (current_cross.ToString ());
		SceneManager.Instance.CutObjects [current_cross].toggleTree(value);
		_treeVisible = value;
	}

	public void toggleDescription(bool val){
		if (_sel != null)
			_sel.cgroup.alpha = (float)Convert.ToInt32(val);
	}

	public void cuteObjectChanged(ListBox lb){
		prev_cross = current_cross;
		current_cross = cuteObject.valueInt;
		int ty = (int)SceneManager.Instance.CutObjects [cuteObject.valueInt].CutType;
		//cuteObjectType.valueInt = ty;
		//cuteObjectType.valueString = cuteObjectType.optionStrings[ty];
		//cuteObjectType.valueSprite = cuteObjectType.optionSprites[ty];
		cuteObjectType.SetSelectedIndex(ty);
		SceneManager.Instance.CutObjects [prev_cross].hideTree ();
		SceneManager.Instance.CutObjects [cuteObject.valueInt].showTree (treeviewHolder.position,treeviewHolder.sizeDelta);
		//display the protein filter tree at the correct position
		//toggleDisplayTreeCutObj
		if (_treeVisible) SceneManager.Instance.CutObjects [cuteObject.valueInt].toggleTree (true);
	}

	public void initProgressBar(){
		if (progressLabel == null) {
			progressLabel = progressBar.GetComponentInChildren<Text> ();
			progressBarRec = progressBar.GetComponentInChildren<RectTransform> ();
			maxW = progressBar.GetComponent<RectTransform> ().sizeDelta.x;
		}
	}
	public void resetProgressBar(){
		//reset to zero and hide
		progressBar.SetActive (false);
		progressLabel.text = "";
		progressBarRec.sizeDelta = new Vector2(0,progressBarRec.sizeDelta.y);
	}

	public void setProgress (float value, string label=""){
		progressLabel.text = label;
		progressBarRec.sizeDelta = new Vector2(value * maxW ,progressBarRec.sizeDelta.y);
	}

	public void startProgress (string label){
		Debug.Log ("start with label "+label);
		progressBar.SetActive (true);
		initProgressBar ();
		progressLabel.text = label;
		progressBarRec.sizeDelta = new Vector2(0,progressBarRec.sizeDelta.y);
	}
	//button callback
	public void buttonCallback(string buttonName){
		if (buttonName == "clear") {
			SceneManager.Instance.ClearScene();
		}
		if (buttonName == "load") {
			//Cursor.SetCursor(cursorTexture,Vector2.zero,CursorMode.Auto);
			//startProgress("loading scene "+recipe_liste_ui.valueString);
			Debug.Log("*****");
			Debug.Log("Load");
			SceneManager.Instance.ClearScene();
			SceneManager.Instance.AllRecipes = AllRecipes;
			SceneManager.Instance.sceneid = recipe_liste_ui.valueInt;
			Debug.Log(SceneManager.Instance.sceneid.ToString());
			CellPackLoader.UI_manager = this;
			//asynchrone?
			CellPackLoader.LoadCellPackResults();
			//this is going to adapt the treeView
			//populate the treeView if any
			if (recipe_ingredient_ui!= null){
				recipe_ingredient_ui.populateRecipeJson (CellPackLoader.resultData);
			}
			//Cursor.SetCursor(null,Vector2.zero,CursorMode.Auto);
			//resetProgressBar();
		}
		if (buttonName == "quit") {
			//clear
			SceneManager.Instance.ClearScene();
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
		if (buttonName == "cutobjectadd") {
			SceneManager.Instance.AddCutObject(0);
			//update the cutObject List
			//add a TreeView + parameter
		}
		if (buttonName == "start") {
			//load HIV+Blood
			//SceneManager.Instance.ClearScene();
			SceneManager.Instance.AllRecipes = AllRecipes;
			SceneManager.Instance.sceneid = 3;//hiv+blood
			//Debug.Log(SceneManager.Instance.sceneid.ToString());
			CellPackLoader.UI_manager = this;
			CellPackLoader.LoadCellPackResults(false);
			if (recipe_ingredient_ui!= null){
				recipe_ingredient_ui.populateRecipeJson (CellPackLoader.resultData);
				//recipe_ingredient_ui.populateRecipe (PersistantSettings.Instance.hierarchy);
			}
		}
		if (buttonName == "unselect") {
			if (Camera.main.GetComponent<NavigateCamera>().handleMode){
				TransformHandle _selectedTransformHandle = Camera.main.GetComponent<NavigateCamera> ()._selectedTransformHandle;
				if (_selectedTransformHandle!=null){ 
					_selectedTransformHandle.Disable();
					_selectedTransformHandle = null;
				}
				Camera.main.GetComponent<NavigateCamera>().handleMode = false;
			}
			else {
				SceneManager.Instance.SetSelectedElement(-1);
			}

		}
	}

	//animate the panel for any selection in BoxList
	public void animatePanelCB(bool toggle){
		if (toggle)StartCoroutine(annimatePanelIn());
		else StartCoroutine(annimatePanelOut());
	}

	public void switchPanel(int panel_active){
		// 0 is camera setting
		// 1 is cross section setting
		if (current_panel == -1)
			panel_fx.alpha = 1;//.gameObject.SetActive (true);
		foreach (Transform child in panel_fx.transform) {
			child.gameObject.SetActive(false);
		}
		panel_fx.transform.GetChild (panel_active+1).gameObject.SetActive (true);
		panel_fx.transform.GetChild (0).gameObject.SetActive (true);
		current_panel = panel_active;
		Debug.Log ("switch panel " + current_panel.ToString () + " " + current_cross.ToString ());
		if (current_panel == 0) {
			//hide any current tree
			//cuteObject.valueInt
			if (current_cross != -1) {
				SceneManager.Instance.CutObjects [current_cross].toggleTree (false);
				Debug.Log ("hide ? " + current_cross.ToString () + " " + SceneManager.Instance.CutObjects [current_cross].ToString ());
			}
		} else {
			if (current_cross != -1) {
				SceneManager.Instance.CutObjects [current_cross].toggleTree (true);
				Debug.Log ("hide ? " + current_cross.ToString () + " " + SceneManager.Instance.CutObjects [current_cross].ToString ());
			}
		}
	}

	private IEnumerator annimatePanelOut()    {
		//Debug.Log (toggle);
			while (panel_fx.alpha > 0) {
				panel_fx.alpha -= Time.deltaTime * speed;
				//if (panel_fx.alpha < 0) panel_fx.alpha = 0;
				camerafx_panel.transform.rotation = Quaternion.AngleAxis (-90.0f*(1-panel_fx.alpha), Vector3.right);//Quaternion.Slerp (startRotation, endRotation, panel_fx.alpha);
				//transform.RotateAround(Vector3.zero, Vector3.right, 90 * Time.deltaTime);
				yield return null;
				//i += Time.deltaTime;
			}
			//panel_fx.alpha=0.0f;
		}
	private IEnumerator annimatePanelIn()    {
			while(panel_fx.alpha <= 1){
				panel_fx.alpha += Time.deltaTime * speed;
				//	if (panel_fx.alpha > 1) panel_fx.alpha = 1;
				camerafx_panel.transform.rotation = Quaternion.AngleAxis (-90.0f*(1-panel_fx.alpha), Vector3.right);//Quaternion.Slerp(startRotation, endRotation, panel_fx.alpha );
				//transform.RotateAround(Vector3.zero, Vector3.right, 90 * Time.deltaTime);
				yield return null;
				//i += Time.deltaTime;
			}
		}


	public void toggleContourCallback(bool v){
		PersistantSettings.Instance.ContourOptions = Convert.ToInt32(!v);	
	}
	public void toggleLODCallback(bool v){
		PersistantSettings.Instance.EnableLod = v	;
	}

	public void deleteCuteObj(){
		if (cuteObject.valueString == null)
			return;
		GameObject objectToDelete = GameObject.Find(cuteObject.valueString);
		if (objectToDelete != null )
			GameObject.DestroyObject (objectToDelete);
	}
	public void hideSettingPanel(){
		current_panel = -1;
		if (settings_ui!= null) settings_ui.valueString = null;
		//panel_fx.gameObject.SetActive (false);
		panel_fx.alpha = 0;
		SceneManager.Instance.CutObjects [current_cross].toggleTree (false);
	}

	public void toggleCuteObjectSelected(bool value){
		TransformHandle _selectedTransformHandle = Camera.main.GetComponent<NavigateCamera> ()._selectedTransformHandle;
		if (value) {
			if (_selectedTransformHandle!=null) 
				_selectedTransformHandle.Disable();
			_selectedTransformHandle = SceneManager.Instance.CutObjects [cuteObject.valueInt].handle;
			_selectedTransformHandle.Enable();
		} else {
			_selectedTransformHandle.Disable();
			_selectedTransformHandle=null;
		}
	}

	// Update is called once per frame
	void Update () {
		//TODO make possible to change the width of the panel  in game mode
		//update the treeView size according the panel size
		//or the other way around
		if (recipe_ingredient_ui.m_myTreeView.Width != (int)panel.rect.width-10)
			recipe_ingredient_ui.m_myTreeView.Width = (int)panel.rect.width-10;
		if (recipe_ingredient_ui.m_myTreeView.Height != (int)(panel.rect.height - recipe_ingredient_ui.m_myTreeView.Y))
			recipe_ingredient_ui.m_myTreeView.Height = (int)(panel.rect.height - recipe_ingredient_ui.m_myTreeView.Y);

		//panel.gameObject.SetActive( recipe_ingredient_ui.gameObject.activeSelf );

		maxwidth = recipe_ingredient_ui.m_myTreeView.maxWidth;
		//change panel.rect
		if ((maxwidth != 0) && (panel.sizeDelta.x!=maxwidth)){
			Vector3 pos = panel.position ;
			Vector2 sd = panel.sizeDelta;
			//panel.sizeDelta = new Vector2 (maxwidth, panel.sizeDelta.y);	//item.Width = item.maxWidth;
			//panel.position = new Vector3(pos.x+maxwidth-sd.x,pos.y,pos.z); 
		}
		if (settings_ui != null) {
			if ((settings_ui.valueInt != current_panel) && (settings_ui.valueString != null)) {
				switchPanel (settings_ui.valueInt);
				//animatePanelCB(true);
			}
		}
		//update the ListBOx of cutting object
		if (cuteObject.optionInts.Length != SceneManager.Instance.CutObjects.Count) {
			cuteObject.optionInts=new int[SceneManager.Instance.CutObjects.Count];
			cuteObject.optionStrings=new string[SceneManager.Instance.CutObjects.Count];
			cuteObject.optionSprites=new Sprite[SceneManager.Instance.CutObjects.Count];

			for (int i = 0; i < SceneManager.Instance.CutObjects.Count;i++){
				cuteObject.optionInts[i]=i;
				cuteObject.optionStrings[i]=SceneManager.Instance.CutObjects[i].gameObject.name;
				cuteObject.optionSprites[i]=cuteObjectType.optionSprites[(int)SceneManager.Instance.CutObjects[i].CutType];
			}
		}
		if (cuteObject.valueInt != current_cross) {
			//update or use on ValueChanged
			//do something
			current_cross = cuteObject.valueInt;
			//display the object tree. should we attach the tree to the gameObjectCutPlan ? and just display it
		}
		if (cuteObject.valueInt < SceneManager.Instance.CutObjects.Count) {
			if (SceneManager.Instance.CutObjects [cuteObject.valueInt].tree_isVisible)
				SceneManager.Instance.CutObjects [cuteObject.valueInt].showTree (treeviewHolder.position, treeviewHolder.rect.size);
		}
		//check if current selected cutObjecy different from the active one in the menu
		TransformHandle _selectedTransformHandle = Camera.main.GetComponent<NavigateCamera> ()._selectedTransformHandle;
		if (_selectedTransformHandle != null) {
			if (_selectedTransformHandle.cutobject.name!=cuteObject.valueString){
				cuteObject.SetValue(_selectedTransformHandle.cutobject.tagid);
			}
		}
	}
}
