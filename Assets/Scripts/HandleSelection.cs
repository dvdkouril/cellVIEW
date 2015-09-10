using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class HandleSelection : MonoBehaviour {
	//can we grab the selected protein information here
	//and show it in a corner, either zoom structure or details information 
	// Use this for initialization
	public string iname;
	public string description;
	public GameObject TextUI;// description_ui;
	public RecipeTreeUI tree;
	public CanvasGroup cgroup;
	public Toggle toggle;
	public CutObject cute;

	void Start () {
		//cgroup = TextUI.GetComponentInParent<CanvasGroup> ();
	}
	
	string filterForPrefix(string name){
		string iname = name;
		var elem = name.Split('_');
		if (elem [0].StartsWith ("cytoplasme") || elem [0].StartsWith ("interior") || elem [0].StartsWith ("surface")) {
			iname = name.Replace(elem[0]+"_","");
		}
		return iname;
	}
	// Update is called once per frame

	public void UpdateDescription(string new_iname,bool filter=false){
		if (filter) new_iname = filterForPrefix(new_iname);
		if (toggle.isOn)
			cgroup.alpha = 1.0f;
		//if (!TextUI.gameObject.activeSelf)
		//	TextUI.gameObject.SetActive (true);
		if (SceneManager.Instance.AllIngredients == null) 
			SceneManager.Instance.AllIngredients = Helper.GetAllIngredientsInfo ();
		iname = new_iname;
		//iname = iname.Replace("_")//cytoplasme interior
		description = SceneManager.Instance.AllIngredients [iname] ["description"];
		if ((description == null) || (string.Equals (description, ""))) {
			description = new_iname;
		}
		RectTransform labelrec =  TextUI.GetComponentInChildren<Text>().gameObject.GetComponent<RectTransform>();
		TextUI.GetComponentInChildren<Text>().text = description;
		//update the parent width 
		RectTransform rec = TextUI.GetComponent<RectTransform> ();
		//rec.sizeDelta = labelrec.sizeDelta;
		//rec.sizeDelta = new Vector2(0,labelrec.sizeDelta.y);
		//rec.up = labelrec.up;
	}

	void Update () {
		if (SceneManager.Instance.SelectedElement > 0) {
			//cute.SetActive(true);
			//activate canvas
			//if (!TextUI.gameObject.activeSelf)
		    //	TextUI.gameObject.SetActive (true);
			if (SceneManager.Instance.AllIngredients == null) 
				SceneManager.Instance.AllIngredients = Helper.GetAllIngredientsInfo ();
			var new_iname = SceneManager.Instance.ProteinNames [(int)SceneManager.Instance.ProteinInstanceInfos [SceneManager.Instance.SelectedElement].x];
			//we need to remove the prefix if any
			new_iname = filterForPrefix(new_iname);
			if (string.Equals( new_iname,iname)) {return;}
			UpdateDescription(new_iname);
			var item = tree.getItemFromName(new_iname);
			tree.m_myTreeView.SelectedItem = item;
			//TextUI.GetComponent<RectTransform>()
			//description_ui.CalcHeight(description, sizeX)
		} else {
			if (SceneManager.Instance.SelectedElement == -1){
				if ( cgroup.alpha  > 0.0f )
					cgroup.alpha = 0.0f;
				//cute.SetActive(false);
				//if (TextUI.gameObject.activeSelf)
					//TextUI.gameObject.SetActive(false);
				cute.toggleAllCutItme(false);
			}
		}
		//update the imageSize
	}
}
