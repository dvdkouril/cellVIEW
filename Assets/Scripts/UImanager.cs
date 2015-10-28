using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
	public ToggleGroup crossmode;
	public List<Toggle> toggles_handle;
	public HandleSelection _sel;
	public RecipeTreeUI recipe_ingredient_ui;
	//public TreeViewControl cutObjectFilter;
	public NavigateCamera nvcamera;
	public GameObject panel_canvas;
	public int maxwidth;
	public JSONNode AllRecipes;
	public Texture2D cursorTexture;
	public float speed;
	public GameObject progressBar;
	public RectTransform treeviewHolder;
	public RectTransform toolBarRect;
	public SSAOPro ssao1;
	public SSAOPro ssao2;

	public GameObject showonly;

	private bool started = false;
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
	private bool cross_advmode = false; // advanced mode for cross section in UI
	private bool jitter = true;
	private float jitter_time = 0.0f;
	private bool coroutine_done = true;
	private bool jitter_started = false;
	private bool back_to_ori = false;
	private List<Vector4> random_pos = new List<Vector4>();
	private List<Vector4> store_pos = new List<Vector4>();
	public GameObject helpPanel;
	public Tutorial tuto;

	public void showOnly(string elem){
		
	}

	public void toggleJitter(bool value){
		jitter = value;
	}

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
		if (nvcamera == null)
			nvcamera = Camera.main.GetComponent<NavigateCamera> ();
	}

	void Start()
    {
		if (nvcamera == null)
			nvcamera = Camera.main.GetComponent<NavigateCamera> ();
	}

	public void cuteObjectTypeChanged(ListBox lb){
		Debug.Log ("cuteObjectTypeChanged "+current_cross.ToString());
		if ((current_cross < SceneManager.Instance.CutObjects.Count)&&(current_cross!=-1)) {
			if (lb.valueInt == 0)
				SceneManager.Instance.CutObjects [current_cross].CutType = CutType.Plane;
			else if (lb.valueInt == 1)
				SceneManager.Instance.CutObjects [current_cross].CutType = CutType.Sphere;
			else if (lb.valueInt == 2)
				SceneManager.Instance.CutObjects [current_cross].CutType = CutType.Cube;
			else
				SceneManager.Instance.CutObjects [current_cross].CutType = CutType.Plane;
			cuteObject.optionSprites [current_cross-1] = lb.optionSprites [lb.valueInt];
			cuteObject.SetSelectedIndex (current_cross-1);
		}
		//cuteObject.Reset();
		//cuteObject.SetSelectedIndex (cuteObject.valueInt);
	}
	

	public void cuteObjectChanged(ListBox lb){
		prev_cross = current_cross;
		current_cross = cuteObject.valueInt+1;
		int ty = (int)SceneManager.Instance.CutObjects [current_cross].CutType;
		//cuteObjectType.valueInt = ty;
		//cuteObjectType.valueString = cuteObjectType.optionStrings[ty];
		//cuteObjectType.valueSprite = cuteObjectType.optionSprites[ty];
		//cuteObjectType.SetSelectedIndex(ty);
		if (prev_cross!=-1) SceneManager.Instance.CutObjects [prev_cross].hideTree ();
		SceneManager.Instance.CutObjects [current_cross].showTree (treeviewHolder.position,treeviewHolder.sizeDelta);
		//display the protein filter tree at the correct position
		//toggleDisplayTreeCutObj
		if (_treeVisible) SceneManager.Instance.CutObjects [current_cross].toggleTree (true);
		toggleCuteObjectSelected (true);
	}

    public void toggleHelpPanel(bool value)
    {
		helpPanel.SetActive (value);
    }

    public void toggleDisplayCutObj(bool value) {
		SceneManager.Instance.CutObjects [current_cross].Display = value;
	}
	
	public void toggleDisplayAllCutObject(bool value){
		//undisplay all ?
		for (int i =1; i < SceneManager.Instance.CutObjects.Count; i++) {
			SceneManager.Instance.CutObjects [i].Display = value;
		} 
	}

	public void toggleCrossMode(bool value){
		cross_advmode = value;
	}

	public void toggleDisplayTreeCutObj(bool value) {
		if (cross_advmode) {
			Debug.Log (current_cross.ToString ());
			if (current_cross != -1)
				SceneManager.Instance.CutObjects [current_cross].toggleTree (value);
			_treeVisible = value;
		}
	}
	
	public void toggleDescription(bool val)
    {
		if (_sel != null)
			_sel.cgroup.alpha = (float)Convert.ToInt32(val);

	    
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

	public void toggleHandleMode(int i){
		//NavigateCamera nc = Camera.main.GetComponent<NavigateCamera> ();
		switch (i) {
			case 0: 
				nvcamera._currentState = SelectionState.Translate;
				break;
			case 1:
				nvcamera._currentState = SelectionState.Rotate;
				break;
			case 2:
				nvcamera._currentState = SelectionState.Scale;
				break;
			default:
				nvcamera._currentState = SelectionState.Translate;
				break;
			}
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
			if (SceneManager.Instance.CutObjects.Count == 2){
				//first one
				SceneManager.Instance.CutObjects[1].CutType = CutType.Cube;
				SceneManager.Instance.CutObjects[1].gameObject.transform.position = new Vector3(50,0,0);
				SceneManager.Instance.CutObjects[1].gameObject.transform.localScale = new Vector3(100,200,300);
			}
			//update the cutObject List
			//add a TreeView + parameter
		}
		if (buttonName == "start") {
			//start the app
			SceneManager.Instance.AllRecipes = AllRecipes;
			//SceneManager.Instance.sceneid = 3;//hiv+blood
			//Debug.Log(SceneManager.Instance.sceneid.ToString());
			CellPackLoader.UI_manager = this;
			//CellPackLoader.LoadCellPackResults(false);
			if (recipe_ingredient_ui!= null){
				recipe_ingredient_ui.populateRecipeGameObject (GameObject.Find (SceneManager.Instance.scene_name));
				//recipe_ingredient_ui.populateRecipeJson (CellPackLoader.resultData);
				//recipe_ingredient_ui.populateRecipe (PersistantSettings.Instance.hierarchy);
			}
			started = true;
			nvcamera.lockInteractions = false;
		}
		if (buttonName == "unselect") {
			if (nvcamera.handleMode){
				TransformHandle _selectedTransformHandle = nvcamera._selectedTransformHandle;
				if (_selectedTransformHandle!=null){ 
					_selectedTransformHandle.Disable();
					_selectedTransformHandle = null;
				}
				nvcamera.handleMode = false;
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
		//remove from the CutObj.panel
		cuteObject.Reset ();
	}

	public void hideSettingPanel(){
		current_panel = -1;
		if (settings_ui!= null) settings_ui.valueString = null;
		//panel_fx.gameObject.SetActive (false);
		panel_fx.alpha = 0;
		SceneManager.Instance.CutObjects [current_cross].toggleTree (false);
	}

	public void toggleCuteObjectSelected(bool value){
		for (int i=1; i<SceneManager.Instance.CutObjects.Count; i++) {
			SceneManager.Instance.CutObjects [i].handle.Disable();
		}
		if (value) {
			nvcamera._selectedTransformHandle = SceneManager.Instance.CutObjects [current_cross].handle;
			nvcamera._selectedTransformHandle.Enable();
			nvcamera.handleMode = true;
		}
	}

	public void toggleSSAO1(bool value){
		ssao1.enabled = value;
	}
	public void toggleSSAO2(bool value){
		ssao2.enabled = value;
	}

	public void toggleBrownianMotion(bool value){
		PersistantSettings.Instance.EnableBrownianMotion = value;
	}

	// Update is called once per frame
	void OnGUI (){
		//check if mouse in the toolbar or the tutorial if actif
		if ((toolBarRect.rect.Contains (Event.current.mousePosition))||(tuto.tutoRect.rect.Contains (Event.current.mousePosition))) {
			nvcamera.lockInteractions = true;
		} else {
			nvcamera.lockInteractions = false;
		}
		if (crossmode == null)
			return;
		if (Event.current.keyCode == KeyCode.Alpha1)
		{
			toggles_handle[0].isOn = true;
			crossmode.NotifyToggleOn(toggles_handle[0]);
		}
		
		if (Event.current.keyCode == KeyCode.Alpha2)
		{
			toggles_handle[1].isOn = true;
			crossmode.NotifyToggleOn(toggles_handle[1]);
		}
		
		if (Event.current.keyCode == KeyCode.Alpha3)
		{
			toggles_handle[2].isOn = true;
			crossmode.NotifyToggleOn(toggles_handle[2]);
		}
	}
	//function MoveObject (thisTransform : Transform, startPos : Vector3, endPos : Vector3, time : float) : IEnumerator {
	//	var i = 0.0;
	//	var rate = 1.0/time;
	//	while (i < 1.0) {
	//		i += Time.deltaTime * rate;
	//		thisTransform.position = Vector3.Lerp(startPos, endPos, i);
	//		yield; 
	//	}
	//}
	//what the end position
	//startCoRoutine ?
	public IEnumerator MoveToPosition(float duration)
	{
		coroutine_done = false;
		float speed = PersistantSettings.Instance.SpeedFactor;
		float scale = PersistantSettings.Instance.MoveFactor*5;
		float elapsedTime = 0;
		Vector3 startingPos;
		List<Vector4> random_dir = new List<Vector4> (SceneManager.Instance.CurveControlPointsPositions);
		bool inited = false;
		while (elapsedTime < duration)
		{
			List<Vector4> new_pos = new List<Vector4> (SceneManager.Instance.CurveControlPointsPositions);
			for (int i=0;i<SceneManager.Instance.CurveControlPointsPositions.Count;i++){
				Vector3 pos = new Vector3(SceneManager.Instance.CurveControlPointsPositions[i].x,SceneManager.Instance.CurveControlPointsPositions[i].y,SceneManager.Instance.CurveControlPointsPositions[i].z);
				Vector3 rpos;
				if (!inited) {
					random_dir[i] = new Vector3(( UnityEngine.Random.value-0.5f) , ( UnityEngine.Random.value-0.5f) , 
					                  (UnityEngine.Random.value-0.5f) )*scale;
				}
				rpos = random_dir[i];
				pos = Vector3.Lerp(pos,pos + rpos, (elapsedTime / duration));
				SceneManager.Instance.CurveControlPointsPositions[i] = new Vector4(pos.x,pos.y,pos.z,SceneManager.Instance.CurveControlPointsPositions[i].z) ;
			}
			if (!inited) 
				inited = true;
			elapsedTime += Time.deltaTime;
			ComputeBufferManager.Instance.CurveControlPointsPositions.SetData(SceneManager.Instance.CurveControlPointsPositions.ToArray());
			yield return null;
			//List<Vector4> normals = SceneManager.Instance.GetSmoothNormals(SceneManager.Instance.CurveControlPointsPositions);		
		}
		coroutine_done = true;
	}

	public void randomChangeCurve(){
		float duration = 2.0f;//1/PersistantSettings.Instance.SpeedFactor;
		//coroutine_done
		if (store_pos.Count == 0)
			store_pos = new List<Vector4> (SceneManager.Instance.CurveControlPointsPositions);
		if (random_pos.Count ==0)
			random_pos = new List<Vector4> (SceneManager.Instance.CurveControlPointsPositions);
		float speed = PersistantSettings.Instance.SpeedFactor*10;
		float scale = PersistantSettings.Instance.MoveFactor * 5 * PersistantSettings.Instance.Scale;
		int ingredientId = SceneManager.Instance.CurveIngredientsNames.IndexOf ("interior2_HIV_RNA");
		if (ingredientId < 0)
			return;
		if (SceneManager.Instance.CurveIngredientToggleFlags [ingredientId] == 0)
			return;
		//random_pos = new List<Vector4> (SceneManager.Instance.CurveControlPointsPositions);

		for (int i=0;i<SceneManager.Instance.CurveControlPointsPositions.Count;i++){
			Vector3 pos = new Vector3(SceneManager.Instance.CurveControlPointsPositions[i].x,SceneManager.Instance.CurveControlPointsPositions[i].y,SceneManager.Instance.CurveControlPointsPositions[i].z);
			Vector3 target = Vector3.zero;//new Vector3(store_pos.x,store_pos.y,store_pos.z);
			Vector3 origin = Vector3.zero;//new Vector3(pos.x,pos.y,pos.z);
			if (!jitter_started) {
				Vector3 rand = new Vector3(( UnityEngine.Random.value-0.5f) , ( UnityEngine.Random.value-0.5f) , 
				                           (UnityEngine.Random.value-0.5f) )*scale;//UnityEngine.Random.onUnitSphere*scale;
					random_pos[i] = new Vector4(rand.x,rand.y,rand.z,1 );
			}
			if (back_to_ori){
				//go from current position->store_pos
				target = new Vector3(store_pos[i].x,store_pos[i].y,store_pos[i].z);
				origin = new Vector3(pos.x,pos.y,pos.z);
			}
			else {
				//go toward the random position
				target = new Vector3(store_pos[i].x+random_pos[i].x,store_pos[i].y+random_pos[i].y,store_pos[i].z+random_pos[i].z);
				origin =  new Vector3(pos.x,pos.y,pos.z);//new Vector3(pos.x,pos.y,pos.z);
			}
			Vector3 rpos = random_pos[i];
			//pos = Vector3.Lerp(pos,pos + rpos, (jitter_time / duration));
			pos = Vector3.Lerp(origin,target, Time.deltaTime*2.0f);//(jitter_time / duration));
			SceneManager.Instance.CurveControlPointsPositions[i] = new Vector4(pos.x,pos.y,pos.z,SceneManager.Instance.CurveControlPointsPositions[i].z) ;
		}
		jitter_time += Time.deltaTime*speed;
		if (!jitter_started)
			jitter_started = true; 
		if (jitter_time > duration) {
			jitter_started=false;
			jitter_time = 0.0f;
			back_to_ori=!back_to_ori;
		}
		//List<Vector4> normals = SceneManager.Instance.GetSmoothNormals(SceneManager.Instance.CurveControlPointsPositions);		
		ComputeBufferManager.Instance.CurveControlPointsPositions.SetData(SceneManager.Instance.CurveControlPointsPositions.ToArray());
		//ComputeBufferManager.Instance.CurveControlPointsNormals.SetData(normals.ToArray());
	}

	void Update () 
    {

		//mouse over tooltip or button ? righclick button show only

        if (!started) nvcamera.lockInteractions = false ;
		//lock if the mouse is in the toolbar?

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
		if (cross_advmode) {
			if ((cuteObject.optionInts.Length != SceneManager.Instance.CutObjects.Count) && (SceneManager.Instance.CutObjects.Count > 0)) {
				cuteObject.optionInts = new int[SceneManager.Instance.CutObjects.Count];
				cuteObject.optionStrings = new string[SceneManager.Instance.CutObjects.Count];
				cuteObject.optionSprites = new Sprite[SceneManager.Instance.CutObjects.Count];

				for (int i = 0; i < SceneManager.Instance.CutObjects.Count; i++) {
					//if (SceneManager.Instance.CutObjects[i].gameObject.name == "CutSelection") continue;
					cuteObject.optionInts [i] = i;
					cuteObject.optionStrings [i] = SceneManager.Instance.CutObjects [i].gameObject.name;
					cuteObject.optionSprites [i] = cuteObjectType.optionSprites [(int)SceneManager.Instance.CutObjects [i].CutType];
				}
			}
			if (cuteObject.valueInt != current_cross) {
				//update or use on ValueChanged
				//do something
				current_cross = cuteObject.valueInt + 1;
				//display the object tree. should we attach the tree to the gameObjectCutPlan ? and just display it
			}
			if ((current_cross < SceneManager.Instance.CutObjects.Count) && (current_cross != -1)) {
				if (SceneManager.Instance.CutObjects [current_cross].tree_isVisible)
					SceneManager.Instance.CutObjects [current_cross].showTree (treeviewHolder.position, treeviewHolder.rect.size);
			}
			//check if current selected cutObjecy different from the active one in the menu
			if (nvcamera._selectedTransformHandle != null) {
				if (nvcamera._selectedTransformHandle.cutobject.name!=cuteObject.valueString){
					cuteObject.SetValue(nvcamera._selectedTransformHandle.cutobject.tagid-1);
				}
			}
		}

		if (!started)
			recipe_ingredient_ui.m_myTreeView.toggleInGame (false);
		if (jitter) {
			//jitter the RNA
			randomChangeCurve();
			//if (coroutine_done) {
			//	coroutine_done = false;
			//	StartCoroutine(MoveToPosition(1.0f));//*PersistantSettings.Instance.SpeedFactor));
			//}
		}
	}
}
