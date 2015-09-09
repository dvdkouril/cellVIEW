using UnityEngine;
using System;
using System.Collections;
using SimpleJSON;

public class RecipeTreeUI : MonoBehaviour {

	public bool filter_cut = false;
	public TreeViewControl m_myTreeView;
	private int anid = 0;//counter of item
	private HandleSelection _sel;
	public CutObject cutobject;
	public delegate void changeItem(TreeViewItem item,params object[] argsRest);

	// Use this for initialization
	public void Awake(){
		_sel = GameObject.Find ("_Selection").GetComponent<HandleSelection>();
	}


	public void Start () {
		//foreach (TreeViewItem item in m_myTreeView.Items) 
		//	ApplyFunctionRec (AddEvents,item);
    }

	public void toggleChecked(TreeViewItem item, params object[] argsRest ){
		bool value = (bool)argsRest[0];
		item.IsChecked = value;
		int itemid = SceneManager.Instance.ProteinNames.IndexOf (item.Parent.Header + "_" + item.Header);
		//apply to the object visibility
		string ingname = item.Parent.Header + "_" + item.Header;
		Debug.Log (item.Parent.Header + "_" + item.Header);
		Debug.Log ("toggle " + itemid.ToString ());
		if (itemid == -1) {
			itemid = SceneManager.Instance.CurveIngredientsNames.IndexOf (item.Parent.Header + "_" + item.Header);
			Debug.Log ("toggle " + itemid.ToString ());
			if (itemid == -1) {
				return;
			} else {
				if (!filter_cut)SceneManager.Instance.CurveIngredientToggleFlags [itemid] = Convert.ToInt32 (value);
			}
		} else {
			if (!filter_cut)SceneManager.Instance.ProteinToggleFlags [itemid] = Convert.ToInt32 (value);
		}
		if (filter_cut) {
			cutobject.toggleCutItme(ingname,value);
		}
	}

	void ApplyFunctionRec(changeItem mehod,TreeViewItem item){
		if (item.HasChildItems ()) {
			foreach (TreeViewItem i in item.Items) 
				ApplyFunctionRec (mehod, i);
		} else {
			mehod (item);
		}
	}

	void ApplyFunctionRecValue(changeItem mehod,TreeViewItem item, bool value){
		if (item.HasChildItems ()) {
			foreach (TreeViewItem i in item.Items) 
				ApplyFunctionRecValue(mehod,i,value);
		}
		mehod (item,value);
	}
	public void HandlerFilterCut(object sender, System.EventArgs args)
	{
		//Debug.Log(string.Format("{0} detected: {1}", args.GetType().Name, (sender as TreeViewItem).Header));
		TreeViewItem item = sender as TreeViewItem;
		bool update = false;
		if (args.GetType ().Name == "CheckedEventArgs") {
			//toggle on
			ApplyFunctionRecValue (toggleChecked, item, true);
			update = true;
		}
		if (args.GetType ().Name == "UncheckedEventArgs") {
			//toggle off
			ApplyFunctionRecValue (toggleChecked, item, false);
			update = true;
		}
		if (args.GetType ().Name == "SelectedEventArgs") {
			//Debug.Log ("ok selected "+item.Header);
			if (!item.HasChildItems ()) {
			}
		}
	}

	public void Handler(object sender, System.EventArgs args)
    {
        //Debug.Log(string.Format("{0} detected: {1}", args.GetType().Name, (sender as TreeViewItem).Header));
		TreeViewItem item = sender as TreeViewItem;
		bool update=false;
		if (args.GetType ().Name == "CheckedEventArgs") {
			//toggle on
			ApplyFunctionRecValue(toggleChecked,item,true);
			update=true;
		}
		if (args.GetType ().Name == "UncheckedEventArgs") {
			//toggle off
			ApplyFunctionRecValue(toggleChecked,item,false);
			update=true;
		}
		if (args.GetType ().Name == "SelectedEventArgs") {
			//Debug.Log ("ok selected "+item.Header);
			if (!item.HasChildItems()){
				//SceneManager.Instance.SetSelectedElement(itemid);
				//Debug.Log ("update label with "+item.Header);
				SceneManager.Instance.SelectedElement=-2;
				_sel.UpdateDescription(item.Header);
				var ingredientId = SceneManager.Instance.ProteinNames.IndexOf(item.Parent.Header + "_" + item.Header);
				//all ingredient instance should have state put highlighted 
				for (int i=0;i <SceneManager.Instance.ProteinInstanceInfos.Count;i++){
					if (SceneManager.Instance.ProteinInstanceInfos[i].x == ingredientId)
						SceneManager.Instance.ProteinInstanceInfos[i] = new Vector4(SceneManager.Instance.ProteinInstanceInfos[i].x, 1, SceneManager.Instance.ProteinInstanceInfos[i].z); 
					else SceneManager.Instance.ProteinInstanceInfos[i] = new Vector4(SceneManager.Instance.ProteinInstanceInfos[i].x, 3, SceneManager.Instance.ProteinInstanceInfos[i].z); 
				}
				ComputeBufferManager.Instance.ProteinInstanceInfos.SetData(SceneManager.Instance.ProteinInstanceInfos.ToArray());
				//update camera position to center
				//Helper.FocusCameraOnGameObject(Camera.main,Vector4.zero,5.0f/PersistantSettings.Instance.Scale);
			}
			else {
				SceneManager.Instance.SelectedElement=-1;
				for (int i=0;i <SceneManager.Instance.ProteinInstanceInfos.Count;i++){
					SceneManager.Instance.ProteinInstanceInfos[i] = new Vector4(SceneManager.Instance.ProteinInstanceInfos[i].x, 0, SceneManager.Instance.ProteinInstanceInfos[i].z); 
				}
				ComputeBufferManager.Instance.ProteinInstanceInfos.SetData(SceneManager.Instance.ProteinInstanceInfos.ToArray());
			}
		}
		//if (args.GetType ().Name == "UnselectedEventArgs") {
			//toggle off
		//	SceneManager.Instance.SetSelectedElement(-1);
		//}
		//if selected should show the description ?
		if (update )
			SceneManager.Instance.UploadIngredientToggleData();
    }

    void AddHandlerEvent(out System.EventHandler handler)
    {
		if (!filter_cut) handler = new System.EventHandler(Handler);
		else handler = new System.EventHandler(HandlerFilterCut);
    }

	void AddEvents(TreeViewItem item,params object[] argsRest)
    {
        AddHandlerEvent(out item.Click);
        AddHandlerEvent(out item.Checked);
        AddHandlerEvent(out item.Unchecked);
        AddHandlerEvent(out item.Selected);
        AddHandlerEvent(out item.Unselected);
    }

	//calculate MaxSize Width ?
	//CalcSize(GUIContent(label));
	public void addIngredientsItemJson(JSONNode recipeData,TreeViewItem parent){
		for (int j = 0; j < recipeData["ingredients"].Count; j++)
		{
			TreeViewItem jitem =  parent.AddItem(recipeData["ingredients"][j]["name"],true,true);
			AddEvents(jitem);
			jitem.anid=anid;
			anid+=1;
		}
	}

	public void ClearTree(){
		m_myTreeView.Items.Clear ();
		m_myTreeView.SelectedItem = null;
		m_myTreeView.HoverItem = null;
		m_myTreeView.Header = "";
	}

	public TreeViewItem getItemInChild(TreeViewItem item, string name){
		TreeViewItem itemfound = null;
		if (item.HasChildItems ()) {
			foreach(TreeViewItem i in item.Items){
				itemfound = getItemInChild(i,name);
				if (itemfound!=null)
					return itemfound;
			}
		}
		if (string.Equals (item.Header, name)) {
			return item;
		}
		return itemfound;
	}


	public TreeViewItem getItemFromName(string name){
		TreeViewItem itemfound = null;
		foreach (TreeViewItem item in m_myTreeView.Items) {
			itemfound = getItemInChild(item,name);
			if (itemfound!=null) return itemfound;
		}
		return itemfound;
	}

	public void populateRecipeJson(JSONNode recipeData){
		ClearTree ();
		anid = 0;
		var item = m_myTreeView;
		item.Width = 250;
		item.Height = 500;
		item.Header = recipeData["recipe"]["name"];
		AddEvents(item.RootItem);
		//int anid = 0;
		if (recipeData ["cytoplasme"] != null) {
			TreeViewItem item1 = item.RootItem.AddItem("cytoplasme",true,true);
			AddEvents(item1);
			addIngredientsItemJson(recipeData["cytoplasme"],item1);
		}
		for (int i = 0; i < recipeData["compartments"].Count; i++)
		{
			TreeViewItem comp = item.RootItem.AddItem(recipeData["compartments"].GetKey(i),true,true);
			AddEvents(comp);
			if (recipeData["compartments"][i] ["interior"] != null) {
				TreeViewItem interior = comp.AddItem("interior"+ i.ToString(),true,true);
				AddEvents(interior);
				addIngredientsItemJson(recipeData["compartments"][i] ["interior"],interior);
			}
			if (recipeData["compartments"][i] ["surface"] != null) {
				TreeViewItem surface = comp.AddItem("surface"+ i.ToString(),true,true);
				AddEvents(surface);
				addIngredientsItemJson(recipeData["compartments"][i] ["surface"],surface);
			}
		}
	}
	public void addIngredientsItem(PersistantSettings.cpNode recipeData,TreeViewItem parent){
		for (int j = 0; j < recipeData.Children.Count; j++)
		{
			TreeViewItem jitem =  parent.AddItem(recipeData.Children[j].Name,true,true);
			AddEvents(jitem);
			jitem.anid=anid;
			anid+=1;
		}
	}

	public void populateRecipe(PersistantSettings.cpNode hierachy){
		ClearTree ();
		anid = 0;
		var item = m_myTreeView;
		item.Width = 250;
		item.Height = 500;
		Debug.Log (hierachy.Name);
		item.Header = hierachy.Name;
		AddEvents(item.RootItem);
		//int anid = 0;
		int i = 0;
		foreach (PersistantSettings.cpNode child in hierachy.Children) {
			if (string.Equals(child.Name,"cytoplasme")){
				TreeViewItem item1 = item.RootItem.AddItem("cytoplasme",true,true);
				AddEvents(item1);
				addIngredientsItem(child,item1);
			}
			else {
				//should have two child
				TreeViewItem comp = item.RootItem.AddItem(child.Name,true,true);
				AddEvents(comp);
				if (child.Children.Count != 0){
					if (child.Children[0].Children.Count != 0) {
						TreeViewItem interior = comp.AddItem("interior"+ i.ToString(),true,true);
						AddEvents(interior);
						addIngredientsItem(child.Children[0],interior);
					}
					if (child.Children[1].Children.Count != 0) {
						TreeViewItem surface = comp.AddItem("surface"+ i.ToString(),true,true);
						AddEvents(surface);
						addIngredientsItem(child.Children[1],surface);
					}
				}
				i+=1;
			}
		
		}
	}
}
