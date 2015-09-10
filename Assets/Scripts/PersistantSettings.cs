using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

[ExecuteInEditMode]
public class PersistantSettings : MonoBehaviour
{
    public string LastSceneLoaded;

    // Base settings
    public float Scale = 0.065f;
    public int ContourOptions;
    public float ContourStrength;
    public bool DebugObjectCulling;
    public bool EnableOcclusionCulling;

    //DNA/RNA settings
    public bool EnableDNAConstraints;
    public float DistanceContraint;
    public float AngularConstraint;

    // Cross section
    public bool EnableCrossSection;
    public float CrossSectionPlaneDistance = 0;
    public Vector3 CrossSectionPlaneNormal;

    // Lod infos
    public bool EnableLod;
    public float FirstLevelOffset = 0;
    public Vector4[] LodLevels = new Vector4[8];
    
	// Hierachy info
	public class cpNode
	{
		public string Name  { get; set; }
		public List<cpNode> Children { get; set; }
	}

	public cpNode hierarchy;

	// Brownian motion
	public bool EnableBrownianMotion;
	public float SpeedFactor = 0;
	public float MoveFactor = 0;
	public float RotateFactor = 0;

	// Declare the DisplaySettings as a singleton
    private static PersistantSettings _instance = null;
    public static PersistantSettings Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = FindObjectOfType<PersistantSettings>();
            if (_instance == null)
            {
                var go = GameObject.Find("_PeristantSettings");
                if (go != null) DestroyImmediate(go);

                go = new GameObject("_PeristantSettings") { hideFlags = HideFlags.HideInInspector };
                _instance = go.AddComponent<PersistantSettings>();
            }
            return _instance;
        }
    }
	public  void storeObjectInHierarchy(JSONNode recipeData, cpNode parent){
		for (int j = 0; j < recipeData["ingredients"].Count; j++)
		{
			cpNode item = new cpNode();
			item.Name = recipeData["ingredients"][j]["name"];
			parent.Children.Add(item);
		}
	}

	public void storeHierachy(JSONNode resultData){
		hierarchy = new cpNode ();
		hierarchy.Name = resultData ["recipe"] ["name"];
		hierarchy.Children = new List<cpNode> ();
		if (resultData ["cytoplasme"] != null) {
			cpNode cyto = new cpNode();
			cyto.Name = "cytoplasme";
			cyto.Children = new List<cpNode> ();
			hierarchy.Children.Add (cyto);
			storeObjectInHierarchy(resultData["cytoplasme"],cyto);
		}
		for (int i = 0; i < resultData["compartments"].Count; i++)
		{
			cpNode comp = new cpNode();
			comp.Name =resultData["compartments"].GetKey(i);
			comp.Children = new List<cpNode> ();
			hierarchy.Children.Add (comp);
			if (resultData["compartments"][i] ["interior"] != null) {
				cpNode interior = new cpNode();
				interior.Name ="interior"+ i.ToString();
				interior.Children = new List<cpNode> ();
				storeObjectInHierarchy(resultData["compartments"][i] ["interior"],interior);
				comp.Children.Add (interior);
			}
			if (resultData["compartments"][i] ["surface"] != null) {
				cpNode surface = new cpNode();
				surface.Name ="surface"+ i.ToString();
				surface.Children = new List<cpNode> ();
				storeObjectInHierarchy(resultData["compartments"][i] ["surface"],surface);
				comp.Children.Add (surface);
			}
		}
	}
}
