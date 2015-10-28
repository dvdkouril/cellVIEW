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
	public Text TextUI2;// description_ui;
	public RecipeTreeUI tree;
	public CanvasGroup cgroup;
	public Toggle toggle;
	public CutObject cute;
	public SphereCollider collider;
	public RectTransform panel;
	public RectTransform rec ;

	private int prev_id;

	void Start () {
		//cgroup = TextUI.GetComponentInParent<CanvasGroup> ();
	}
	
	string filterForPrefix(string name){
		string iname = name;
		var elem = name.Split('_');
		if (elem [0].StartsWith ("cytoplasme") || elem [0].StartsWith ("interior") || elem [0].StartsWith ("surface")) {
			iname = name.Replace(elem[0]+"_","");
			if (iname.Contains("NC")){
				iname = iname+"_"+elem[2];
			}
		}
		//need the orginal name or filter the orginal name
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
		Debug.Log ("Selected "+iname);
		if (iname.Contains("lipid")) {
			iname = "lipids";
		}
		description = SceneManager.Instance.AllIngredients [iname] ["description"];
		if ((description == null) || (string.Equals (description, ""))) {
			if (iname.Contains("lipid"))
				description = "A lipid molecule which is the main component of the lipid bilayer, a thin polar membrane. These membranes are flat sheets that form a continuous barrier around all cells. The cell membranes of almost all living organisms and many viruses are made of a lipid bilayer, as are the membranes surrounding the cell nucleus and other sub-cellular structures. The lipid bilayer is the barrier that keeps ions, proteins and other molecules where they are needed and prevents them from diffusing into areas where they should not be.";
			else if (iname.Contains("SP2"))
				description = "SP2 - (spacer protein 2) is a small domain of 14 residues linking NC and P6 in HIV-1 Gag.";
			else 
				description = new_iname;
		}
		//RectTransform labelrec =  TextUI.GetComponentInChildren<Text>().gameObject.GetComponent<RectTransform>();
		TextUI.GetComponentInChildren<Text>().text = description;
		TextUI2.text = description;
		//update the parent width 
		//RectTransform rec = TextUI.GetComponent<RectTransform> ();
		if (SceneManager.Instance.SelectedElement >= 0) {
			collider.radius = SceneManager.Instance.ProteinBoundingSpheres [(int)SceneManager.Instance.ProteinInstanceInfos [SceneManager.Instance.SelectedElement].x] * PersistantSettings.Instance.Scale;
			transform.position = SceneManager.Instance.ProteinInstancePositions [SceneManager.Instance.SelectedElement] * PersistantSettings.Instance.Scale;
		}
		//panel.rect.Set (rec.rect.x, rec.rect.y, rec.rect.width, rec.rect.height);//set(top,left,width,height);
		//panel.sizeDelta = rec.sizeDelta;

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
			if (prev_id == SceneManager.Instance.SelectedElement) return;
			//if (string.Equals( new_iname,iname)) {return;}
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
				if (cute != null) cute.toggleAllCutItme(false);
			}
		}
		prev_id = SceneManager.Instance.SelectedElement;
		//update the imageSize
		//panel.rect.Set (rec.rect.x, rec.rect.y, rec.rect.width, rec.rect.height);//set(top,left,width,height);
		//panel.sizeDelta = rec.sizeDelta;
		//panel.hasChanged;
		//panel.right
		//panel.
	}
}
