using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum CutType
{
    Plane = 0,
    Sphere = 1,
    Cube = 2,
    Cylinder = 3,
    Cone = 4
};

[System.Serializable]
public class CutItem
{
    public string Name;
    public bool State;
}

[ExecuteInEditMode]
public class CutObject : MonoBehaviour
{
    public Material CutObjectMaterial;
	public bool tree_isVisible = true;
    public CutType CutType;
	public int tagid;
	public string name;

	public TransformHandle handle;

    [HideInInspector]
    public CutType PreviousCutType;
    
    public bool Display = true;
    
    [Range(0,1)]
    public float Value1;

    [Range(0, 1)]
    public float Value2;

    [HideInInspector]
    public List<CutItem> ProteinCutFilters = new List<CutItem>();

	private TreeViewControl _tree;
	private RecipeTreeUI _tree_ui;

	public void SetCutItems(List<string> names)
    {
        foreach(var name in names)
        {
            ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
        }
    }

	public void RemoveCutItem (string name){
		CutItem toRemove=null;
		foreach(CutItem cu in ProteinCutFilters){
			if (string.Equals(cu.Name,name)){
				toRemove = cu;
				break;
			}
		}
		if (toRemove != null)ProteinCutFilters.Remove(toRemove);
	}

	public  void AddCutItem (string name){
		ProteinCutFilters.Add(new CutItem() { Name = name, State = true });
	}

	public void toggleCutItme (string name, bool toggle){
		foreach(CutItem cu in ProteinCutFilters){
			if (string.Equals(cu.Name,name)){
				cu.State = toggle;
				break;
			}
		}
	}

	public void toggleAllCutItme (bool toggle){
		foreach(CutItem cu in ProteinCutFilters){
				cu.State = toggle;
		}
	}

    void Awake()
    {
        Debug.Log("Init cut object");
		if (ProteinCutFilters.Count == 0)
        //ProteinCutFilters.Clear();//?maybe shouldnt clear on Awake ?
        	SetCutItems(SceneManager.Instance.ProteinNames);
		_tree = GetComponent<TreeViewControl> ();
		_tree_ui = GetComponent<RecipeTreeUI> ();
	}

    void OnEnable()
    {
        // Register this object in the cut object cache
        if (!SceneManager.Instance.CutObjects.Contains(this))
        {
            SceneManager.Instance.CutObjects.Add(this);
        }
		//check the tree
		if (_tree.enabled) setTree ();
    }

    void OnDisable()
    {
        // De-register this object in the cut object cache
        if (SceneManager.CheckInstance() && SceneManager.Instance.CutObjects.Contains(this))
        {
            SceneManager.Instance.CutObjects.Remove(this);
        }

    }

	public void toggleTree(bool value){
		_tree.DisplayOnGame = value;
		tree_isVisible = value;
	}

	public void showTree(Vector3 pos,Vector2 size){
		_tree.DisplayOnGame = true;
		_tree.Width = (int)size.x-20;
		_tree.Height = (int)size.y-30;
		_tree.X = (int)pos.x+10;
		_tree.Y = (Screen.height - (int)size.y)+10;//invert ?
		tree_isVisible = true;
		Debug.Log ("should show tree");
	}

	public void hideTree(){
		Debug.Log ("should be hided");
		_tree.DisplayOnGame = false;
		tree_isVisible = false;
	}

	public void setTree(){
		_tree_ui.ClearTree ();
		GameObject root = GameObject.Find (SceneManager.Instance.scene_name);
		if (root != null) {
			_tree_ui.populateRecipeGameObject (root);
		}
		//if (CellPackLoader.resultData != null)
			//_tree_ui.populateRecipeJson (CellPackLoader.resultData);
			//_tree_ui.populateRecipe (PersistantSettings.Instance.hierarchy);
		else {
			Debug.Log ("cellPackResult not availble");
		}
		hideTree ();
		tree_isVisible = false;
	}

	public bool tree_hasFocus(Vector2 mousepos){
		Rect rect = new Rect(_tree.X-60, _tree.Y-60, _tree.Width+90, _tree.Height+90);
		return rect.Contains(mousepos);
	}

    // This function is meant to keep exisiting cut item and to preserve their original state
    // While removing items which are not present in the source list and that are present in the destination list
    // While also adding elements from the input list and which are not present in the destination list
    public void ResetCutItems(List<string> names)
    {
        // find elements present in source but not in destination
        // these elements will be added to the desitnation afterwards
        var AB = new List<string>();
        foreach (var a in names)
        {
            var contains = false;
            foreach (var b in ProteinCutFilters.Where(b => b.Name == a))
            {
                contains = true;
            }

            if (!contains) AB.Add(a);
        }

        // find elements present in the destination but not in the source
        // these elements will be removed from the input source
        var BA = new List<string>();
        foreach (var b in ProteinCutFilters)
        {
            var contains = false;
            foreach (var a in names.Where(a => b.Name == a))
            {
                contains = true;
            }

            if (!contains) BA.Add(b.Name);
        }

        // add new elements
        foreach (var a in AB)
        {
            ProteinCutFilters.Add(new CutItem() { Name = a, State = true });
        }

        // remove old elements
        foreach (var b in BA)
        {
            // find index of the element to remove 
            var index = -1;
            for (var i = 0; i < ProteinCutFilters.Count; i++)
            {
                if (ProteinCutFilters[i].Name != b) continue;
                index = i;
                break;
            }

            if(index == -1) throw new Exception();

            ProteinCutFilters.RemoveAt(index);
        }
    }
    
    void OnRenderObject()
    {
        if (!Display || Camera.current == null) return;

        if (CutType != PreviousCutType || gameObject.GetComponent<MeshFilter>().sharedMesh == null)
        {
            SetMesh();
            PreviousCutType = CutType;
        }

        var depthBuffer = RenderTexture.GetTemporary(Camera.current.pixelWidth, Camera.current.pixelHeight, 32, RenderTextureFormat.Depth);

        if(Camera.current == Camera.main)Graphics.SetRenderTarget(Graphics.activeColorBuffer, depthBuffer.depthBuffer);

        CutObjectMaterial.SetPass(0);
        Graphics.DrawMeshNow(gameObject.GetComponent<MeshFilter>().sharedMesh, transform.localToWorldMatrix);

        RenderTexture.ReleaseTemporary(depthBuffer);
    }

    public void SetMesh()
    {
        switch (CutType)
        {
            case CutType.Plane:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Plane") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;

            case CutType.Sphere:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Sphere") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<SphereCollider>();
                break;

            case CutType.Cube:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cube") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;

            case CutType.Cone:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cone") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;

            case CutType.Cylinder:
                gameObject.GetComponent<MeshFilter>().sharedMesh = Resources.Load("Meshes/Cylinder") as Mesh;
                DestroyImmediate(gameObject.GetComponent<Collider>());
                gameObject.AddComponent<MeshCollider>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
