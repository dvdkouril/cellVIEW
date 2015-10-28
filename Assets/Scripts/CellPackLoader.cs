using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public static class CellPackLoader
{
	public static JSONNode resultData;
	public static int current_color;
	public static List<Vector3> ColorsPalette;
	public static List<Vector3> ColorsPalette2;
	public static Dictionary<int,List<int>> usedColors;
	public static UImanager UI_manager;
	private static bool use_rigid_body = false;

	public static void AddRecipeIngredientsGameObject(JSONNode recipeData,GameObject parent){
		for (int j = 0; j < recipeData["ingredients"].Count; j++)
		{
			string iname = recipeData["ingredients"][j]["name"];
			if (iname.StartsWith("HIV")) 
				if (iname.Contains("NC")){
					iname = "HIV_"+iname.Split('_')[1]+"_"+iname.Split('_')[2];
				}
				else if (iname.Contains("P6_VPR"))
					iname = "HIV_"+iname.Split('_')[1]+"_"+iname.Split('_')[2];
				else 
					iname = "HIV_"+iname.Split('_')[1];
			if (!SceneManager.Instance.ProteinNames.Contains(parent.name+"_"+iname))
			{
				Debug.Log (parent.name+"_"+iname);
				continue;
			}
			Debug.Log ("create "+iname);
			var jitem = new GameObject(iname);
			jitem.transform.parent=parent.transform;
			if ((recipeData["ingredients"][j]["radii"] != null )&&(use_rigid_body)){
				//build child as rigid body
				var atomSpheres = Helper.gatherSphereTree(recipeData["ingredients"][j])[0];
				//build the prefab
				GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);// new GameObject(iname+"_0");
				//prefab.transform.localScale = new Vector3()
				//add sphere collision compoinent
				Rigidbody rb = prefab.AddComponent<Rigidbody>();
				rb.useGravity = false;
				rb.velocity = Vector3.zero;
				rb.drag = 100;
				rb.angularDrag =100;
				foreach (Vector4 sph in atomSpheres) {
					SphereCollider sc = prefab.AddComponent<SphereCollider>();
					sc.radius = sph.w;
					sc.center = new Vector3(sph.x,sph.y,sph.z);
					//sc.attachedRigidbody = rb;
				}
				GameObject rbroot = GameObject.Find (SceneManager.Instance.scene_name);
				prefab.transform.parent = rbroot.transform;
				prefab.hideFlags = HideFlags.HideInHierarchy;
				//add rigid body component
				//instantiate
				for (int k = 0; k < recipeData["ingredients"][j]["results"].Count; k++)
				{
					var p = recipeData["ingredients"][j]["results"][k][0];
					var r = recipeData["ingredients"][j]["results"][k][1];
					
					var position = new Vector3(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat);
					var rotation = new Quaternion(r[0].AsFloat, r[1].AsFloat, r[2].AsFloat, r[3].AsFloat);
					
					var mat = Helper.quaternion_matrix(rotation);
					var euler = Helper.euler_from_matrix(mat);
					rotation = Helper.MayaRotationToUnity(euler);
					//instantiate
					if (k==0){
						prefab.transform.position = position;
						prefab.transform.rotation = rotation;
					}
					else {
						GameObject inst = GameObject.Instantiate(prefab, position, rotation) as GameObject;
						inst.transform.parent = rbroot.transform;
						inst.hideFlags = HideFlags.HideInHierarchy;
						inst.name = iname+"_"+k.ToString();
					}
					//SceneManager.Instance.AddIngredientInstance(name, position, rotation);
				}
			}
			//add children invisible instance with collider and rigidBody with no gravity
			//collider should be the primitive from cellPACK
		}
	}
	
	public static void buildHierarchy(JSONNode resultData){
		SceneManager.Instance.scene_name = resultData ["recipe"] ["name"];
		var root = new GameObject(resultData["recipe"]["name"]);//in case we want to have more than one recipe loaded
		//create empty null object or sphere ?
		if (use_rigid_body) {
			GameObject rb_root = new GameObject (resultData ["recipe"] ["name"] + "_rigidbody");
		}
		if (resultData["cytoplasme"] != null)
		{
			var cyto = new GameObject("cytoplasme");
			cyto.transform.parent = root.transform;
			AddRecipeIngredientsGameObject(resultData["cytoplasme"], cyto);
		}
		
		for (int i = 0; i < resultData["compartments"].Count; i++)
		{
			var comp = new GameObject(resultData["compartments"].GetKey(i));
			comp.transform.parent = root.transform;
			var surface = new GameObject("surface"+ i.ToString());
			surface.transform.parent = comp.transform;
			AddRecipeIngredientsGameObject(resultData["compartments"][i]["surface"],surface);
			var interior = new GameObject("interior"+ i.ToString());
			interior.transform.parent = comp.transform;
			AddRecipeIngredientsGameObject(resultData["compartments"][i]["interior"], interior);
		}
	}
    public static void LoadCellPackResults(bool load=true)
    {
           //#if UNITY_EDITOR
			Debug.Log("Loading");
            var directory = "";
			var path = "";

            if (string.IsNullOrEmpty(PersistantSettings.Instance.LastSceneLoaded) || !Directory.Exists(Path.GetDirectoryName(PersistantSettings.Instance.LastSceneLoaded)))
            {
                directory = Application.dataPath;
            }
            else
            {
                directory = Path.GetDirectoryName(PersistantSettings.Instance.LastSceneLoaded);
            }
			if (SceneManager.Instance.sceneid==SceneManager.Instance.AllRecipes.Count){
				#if UNITY_EDITOR
				path = EditorUtility.OpenFilePanel("Select .cpr", directory, "cpr");
            	if (string.IsNullOrEmpty(path)) return;
				#endif
			}
			else {
				string url = SceneManager.Instance.AllRecipes[SceneManager.Instance.sceneid][0]["resultfile"];
				url=url.Replace("autoPACKserver","https://raw.githubusercontent.com/mesoscope/cellPACK_data/master/cellPACK_database_1.1.0/");
				//fetch the results file from the server
				path = Helper.GetResultsFile(url);
			}
            PersistantSettings.Instance.LastSceneLoaded = path;

			//change cursor to loading
			if (SceneManager.Instance.AllIngredients == null) 
				SceneManager.Instance.AllIngredients = Helper.GetAllIngredientsInfo ();
            if (load) {
				LoadIngredients (path);
				//restore cursor

				Debug.Log ("*****");
				Debug.Log ("Total protein atoms number: " + SceneManager.Instance.TotalNumProteinAtoms);
				if (SceneManager.Instance.scene_name.Contains("HIV")){
					LoadRna();
					LoadLipidsTest();
				}
				// create problem with cut object
				// Upload scene data to the GPU
				SceneManager.Instance.UploadAllData ();
				SceneManager.Instance.SetCutObjects ();//set everything
			} else {
				var cellPackSceneJsonPath = path;//Application.dataPath + "/../Data/HIV/cellPACK/BloodHIV1.0_mixed_fixed_nc1.json";
				if (!File.Exists(cellPackSceneJsonPath)) throw new Exception("No file found at: " + cellPackSceneJsonPath);
				//this assume a result file from cellpack, not a recipe file.
				resultData = Helper.ParseJson(cellPackSceneJsonPath);
			}
			
			#if UNITY_EDITOR
			if (UI_manager.recipe_ingredient_ui!= null){
				UI_manager.recipe_ingredient_ui.populateRecipeJson (resultData);
			}
			#endif
			PersistantSettings.Instance.storeHierachy (resultData);
   }

    public static void LoadIngredients(string recipePath)
    {
        Debug.Log("*****");
        Debug.Log("Loading scene: " + recipePath);
        
        var cellPackSceneJsonPath = recipePath;//Application.dataPath + "/../Data/HIV/cellPACK/BloodHIV1.0_mixed_fixed_nc1.json";
        if (!File.Exists(cellPackSceneJsonPath)) throw new Exception("No file found at: " + cellPackSceneJsonPath);
		//this assume a result file from cellpack, not a recipe file.
        resultData = Helper.ParseJson(cellPackSceneJsonPath);



		int nCompartemnts = 0;
        //we can traverse the json dictionary and gather ingredient source (PDB,center), sphereTree, instance.geometry if we want.
        //the recipe is optional as it will gave more information than just the result file.

        //idea: use secondary color scheme for compartments, and analogous color for ingredient from the recipe baseColor
        current_color = 0;
        //first grab the total number of object
        int nIngredients = 0;
        if (resultData ["cytoplasme"] != null) {
			nIngredients += resultData ["cytoplasme"] ["ingredients"].Count;
			nCompartemnts+=1;
		}
        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            nIngredients += resultData["compartments"][i]["interior"]["ingredients"].Count;
            nIngredients += resultData["compartments"][i]["surface"]["ingredients"].Count;
			nCompartemnts+=2;
        }
		if (nCompartemnts < 2)
			nCompartemnts = 2;
        //generate the palette
        //ColorsPalette   = ColorGenerator.Generate(nIngredients).Skip(2).ToList(); 
        //ColorsPalette = ColorGenerator.Generate(8).Skip(2).ToList();//.Skip(2).ToList();
        //List<Vector3> startKmeans = new List<Vector3>(ColorsPalette);
        //paletteGenerator.initKmeans (startKmeans);
		DateTime start = DateTime.Now;
		// the code that you want to measure comes here
        usedColors = new Dictionary<int, List<int>>();
        ColorsPalette2 = ColorPaletteGenerator.generate(
				6, // Colors
                ColorPaletteGenerator.testfunction,
                false, // Using Force Vector instead of k-Means
                50, // Steps (quality)
				false
                );
		/*
		 * TimeSpan timeItTook = DateTime.Now - start;
		Debug.Log ("time to generate palette " + timeItTook.ToString ());
		foreach (Vector3 v in ColorsPalette2) {
			Debug.Log ("color "+v.ToString());
			var c = ColorPaletteGenerator.lab2rgb(v);
			Debug.Log ("color rgb "+c.ToString());
		}*/
        // Sort colors by differenciation first
        //ColorsPalette2 = paletteGenerator.diffSort(ColorsPalette2);
        //check if cytoplasme present
        Color baseColor = new Color(1.0f, 107.0f / 255.0f, 66.0f / 255.0f);
        if (resultData["cytoplasme"] != null)
        {
            usedColors.Add(current_color, new List<int>());
            baseColor = new Color(1.0f, 107.0f / 255.0f, 66.0f / 255.0f);
            AddRecipeIngredients(resultData["cytoplasme"]["ingredients"], baseColor, "cytoplasme");
            current_color += 1;
        }

        for (int i = 0; i < resultData["compartments"].Count; i++)
        {
            baseColor = new Color(148.0f / 255.0f, 66.0f / 255.0f, 255.0f / 255.0f);
            usedColors.Add(current_color, new List<int>());	
            AddRecipeIngredients(resultData["compartments"][i]["interior"]["ingredients"], baseColor, "interior" + i.ToString());
            current_color += 1;
            baseColor = new Color(173.0f / 255.0f, 255.0f / 255.0f, 66.0f / 255.0f);
            usedColors.Add(current_color, new List<int>());
            AddRecipeIngredients(resultData["compartments"][i]["surface"]["ingredients"], baseColor, "surface" + i.ToString());
            current_color += 1;
        }
		//this is comment as I manually change a lot of name of the recipe to a better reading for the NSF competition. 
		//buildHierarchy (resultData);
    }
	//coroutine?
	public static void AddRecipeIngredients(JSONNode recipeDictionary, Color baseColor, string prefix)
    {
		for (int j = 0; j < recipeDictionary.Count; j++)
		{
			//UImanager.setProgress((float)j/(float)recipeDictionary.Count,"adding "+recipeDictionary["name"]);
            if (recipeDictionary[j]["nbCurve"] != null)
            {
                AddCurveIngredients(recipeDictionary[j], prefix);
            }
            else
            {
                AddProteinIngredient(recipeDictionary[j], prefix);
            }
        }
	}

    public static void AddProteinIngredient(JSONNode ingredientDictionary, string prefix)
    {
		string iname = ingredientDictionary ["name"].Value;
		if (iname.StartsWith("HIV")) 
			if (iname.Contains("_NC"))
				iname = "HIV_"+iname.Split('_')[1]+"_"+iname.Split ('_')[2];
			else if (iname.Contains("P6_VPR"))
				iname = "HIV_"+iname.Split('_')[1]+"_"+iname.Split ('_')[2];
			else 
				iname = "HIV_"+iname.Split('_')[1];
        var name = prefix + "_" + iname;
        var biomt = (bool)ingredientDictionary["source"]["biomt"].AsBool;
        var center = (bool)ingredientDictionary["source"]["transform"]["center"].AsBool;
        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");
		List<Vector4> atomSpheres;
		List<Matrix4x4> biomtTransforms = new List<Matrix4x4>();
		Vector3 biomtCenter = Vector3.zero;
		bool containsACarbonOnly = false;
		bool oneLOD = false;
		if ((pdbName == "") || (pdbName == "null") || (pdbName == "None")||pdbName.StartsWith("EMDB")) {
			//check for sphere file//information in the file. if not in file is it on disk ? on repo ?
			//possibly read the actuall recipe definition ?
			//check if bin exist
			var filePath = PdbLoader.DefaultPdbDirectory + ingredientDictionary["name"] + ".bin";
			if (File.Exists(filePath)){
				atomSpheres = new List<Vector4>();
				var points = Helper.ReadBytesAsFloats(filePath);
				for (var i = 0; i < points.Length; i += 4) {
					var currentAtom = new Vector4 (points [i], points [i + 1], points [i + 2], points [i + 3]);
					atomSpheres.Add (currentAtom);
				}
				containsACarbonOnly = true;
				oneLOD = true;
			}
			else if (ingredientDictionary ["radii"] != null) {
				atomSpheres = Helper.gatherSphereTree(ingredientDictionary)[0];
				Debug.Log ("nbprim "+atomSpheres.Count.ToString());//one sphere
				oneLOD = true;
			} else {
				float radius = 30.0f;
				if (name.Contains("dLDL"))
					radius = 108.08f;//or use the mesh? or make sphere from the mesh ?
				if (name.Contains("iLDL"))
					radius = 105.41f;//or use the mesh? or make sphere from the mesh ?
				atomSpheres = new List<Vector4>();
				atomSpheres.Add (new Vector4(0,0,0,radius));
				//No LOD since only one sphere
				oneLOD = true;
			}
		} else {
			//if (pdbName.StartsWith("EMDB")) return;
			//if (pdbName.Contains("1PI7_1vpu_biounit")) return;//??
			// Load atom set from pdb file
			var atomSet = PdbLoader.LoadAtomSet(pdbName);
			
			// If the set is empty return
			if (atomSet.Count == 0) return;
		
			atomSpheres = AtomHelper.GetAtomSpheres(atomSet);
			containsACarbonOnly = AtomHelper.ContainsACarbonOnly(atomSet);
		}
		var centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;       
		
		// Center atoms
		AtomHelper.OffsetSpheres(ref atomSpheres, centerPosition);
		
		// Compute bounds
		var bounds = AtomHelper.ComputeBounds(atomSpheres);

		biomtTransforms = biomt ? PdbLoader.LoadBiomtTransforms(pdbName) : new List<Matrix4x4>();
		biomtCenter = AtomHelper.GetBiomtCenter(biomtTransforms,centerPosition);

		//if (!pdbName.Contains("1TWT_1TWV")) return;
        
        // Disable biomts until loading problem is resolved
        //if (!biomt) return;
        

        // Get ingredient color
        // TODO: Move color palette code into dedicated function
        var cid = ColorPaletteGenerator.GetRandomUniqFromSample(current_color, usedColors[current_color]);
        usedColors[current_color].Add(cid);
        var sample = ColorPaletteGenerator.colorSamples[cid];
        var c = ColorPaletteGenerator.lab2rgb(sample) / 255.0f;
        var color = new Color(c[0], c[1], c[2]);

        // Define cluster decimation levels
        var clusterLevels = (containsACarbonOnly)
            ? new List<float>() {0.85f, 0.25f, 0.1f}
            : new List<float>() {0.15f, 0.10f, 0.05f};
		if (oneLOD)
			clusterLevels = new List<float> () {1, 1, 1};
        // Add ingredient type
        //SceneManager.Instance.AddIngredient(name, bounds, atomSpheres, color);
	
		SceneManager.Instance.AddIngredient(name, bounds, atomSpheres, color, clusterLevels,oneLOD);
        int instanceCount = 0;
        
        for (int k = 0; k < ingredientDictionary["results"].Count; k++)
        {
            var p = ingredientDictionary["results"][k][0];
            var r = ingredientDictionary["results"][k][1];

            var position = new Vector3(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat);
            var rotation = new Quaternion(r[0].AsFloat, r[1].AsFloat, r[2].AsFloat, r[3].AsFloat);

            var mat = Helper.quaternion_matrix(rotation);
            var euler = Helper.euler_from_matrix(mat);
            rotation = Helper.MayaRotationToUnity(euler);

            if (!biomt)
            {
                // Find centered position
                if (!center) position += Helper.QuaternionTransform(rotation, centerPosition);
                SceneManager.Instance.AddIngredientInstance(name, position, rotation);
                instanceCount++;
            }
            else
            {
                foreach (var transform in biomtTransforms)
                {
					var biomteuler = Helper.euler_from_matrix(transform);
					var rotBiomt = Helper.MayaRotationToUnity(biomteuler);
					var offset = Helper.QuaternionTransform(rotBiomt,centerPosition);//Helper.RotationMatrixToQuaternion(matBiomt), GetCenter());
					var posBiomt = new Vector3(-transform.m03, transform.m13, transform.m23)+offset - biomtCenter;

					var biomtOffset = Helper.RotationMatrixToQuaternion(transform) * centerPosition;
					var biomtInstanceRot = rotation * rotBiomt;//Helper.RotationMatrixToQuaternion(transform);
					var biomtInstancePos = rotation * posBiomt + position;

					SceneManager.Instance.AddIngredientInstance(name, biomtInstancePos, biomtInstanceRot);
                    instanceCount++;
                }
            }
        }

        Debug.Log("*****");
        Debug.Log("Added ingredient: " + name);
        if (containsACarbonOnly) Debug.Log("Alpha-carbons only");
        Debug.Log("Pdb name: " + pdbName + " *** " + "Num atoms: " + atomSpheres.Count + " *** " + "Num instances: " + instanceCount + " *** " + "Total atom count: " + atomSpheres.Count * instanceCount);
    }

    public static void AddCurveIngredients(JSONNode ingredientDictionary, string prefix)
    {
        //in case there is curveN, grab the data if more than 4 points
        //use the given PDB for the representation.
        var numCurves = ingredientDictionary["nbCurve"].AsInt;
        var curveIngredientName = prefix + "_" + ingredientDictionary["name"].Value;
        var pdbName = ingredientDictionary["source"]["pdb"].Value.Replace(".pdb", "");

        SceneManager.Instance.AddCurveIngredient(curveIngredientName, pdbName);
        
        for (int i = 0; i < numCurves; i++)
        {
            //if (i < nCurve-10) continue;
            var controlPoints = new List<Vector4>();
            if (ingredientDictionary["curve" + i.ToString()].Count < 4) continue;

            for (int k = 0; k < ingredientDictionary["curve" + i.ToString()].Count; k++)
            {
                var p = ingredientDictionary["curve" + i.ToString()][k];
                controlPoints.Add(new Vector4(-p[0].AsFloat, p[1].AsFloat, p[2].AsFloat, 1));
            }

            SceneManager.Instance.AddCurve(curveIngredientName, controlPoints);
            //break;
        }

        Debug.Log("*****");
        Debug.Log("Added curve ingredient: " + curveIngredientName);
        Debug.Log("Num curves: " + numCurves);
    }
	
    public static void DebugMethod()
    {
        Debug.Log("Hello World");
    }

	public static void LoadRna()
	{
		var rnaControlPointsPath = Application.dataPath + "/../Data/proteins/rna_allpoints.txt";
		if (!File.Exists(rnaControlPointsPath)) throw new Exception("No file found at: " + rnaControlPointsPath);
		
		var controlPoints = new List<Vector4>();
		foreach (var line in File.ReadAllLines(rnaControlPointsPath))
		{
			var split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var x = float.Parse(split[0]);
			var y = float.Parse(split[1]);
			var z = float.Parse(split[2]);
			
			//should use -Z pdb are right-handed
			controlPoints.Add(new Vector4(-x, y, z, 1));
		}
		SceneManager.Instance.AddCurveIngredient("interior2_HIV_RNA", "RNA_U_Base");
		SceneManager.Instance.AddCurve("interior2_HIV_RNA", controlPoints);
		//Debug.Log ("added RNA " + atomSpheres.Count.ToString () + " " + controlPoints.Count.ToString ());
		GameObject rna = new GameObject("HIV_RNA");
		rna.transform.parent = GameObject.Find (SceneManager.Instance.scene_name).transform;
	}

	public static void LoadMycoDna()
	{
		var rnaControlPointsPath = Application.dataPath + "/../Data/proteins/tps_path_all.txt";
		if (!File.Exists(rnaControlPointsPath)) throw new Exception("No file found at: " + rnaControlPointsPath);
		
		var controlPoints = new List<Vector4>();
		foreach (var line in File.ReadAllLines(rnaControlPointsPath))
		{
			var split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var x = float.Parse(split[0]);
			var y = float.Parse(split[1]);
			var z = float.Parse(split[2]);
			
			//should use -Z pdb are right-handed
			controlPoints.Add(new Vector4(-x, y, z, 1));
		}
		SceneManager.Instance.AddCurveIngredient("DNA", "dna_single_base");
		SceneManager.Instance.AddCurve("DNA", controlPoints);
		//Debug.Log ("added RNA " + atomSpheres.Count.ToString () + " " + controlPoints.Count.ToString ());
		//GameObject rna = new GameObject("DNA");
		//rna.transform.parent = GameObject.Find (SceneManager.Instance.scene_name).transform;
	}
	public static void LoadLipidsTest(){
		//use Library compute from RMSD
		//read atomic info
		
		Dictionary<string,List<List<float>>> tri_res = new Dictionary<string,List<List<float>>> ();
		
		var Lib = Helper.ReadBytesAsFloats(Application.dataPath + "/../Data/membrane/library_hiv.bin");
		int step = 5;//resid,x,y,z,atomtype	
		int count = 0;
		List<Vector4> atomSpheres=new List<Vector4>();
		int previd = 0;
		Bounds bounds;
		List<List<Vector4>> atomClusters;
		Color ingrColor;
		Vector3 centerPosition;
		var clusterLevels = new List<float>() {0.80f, 0.55f, 0.21f};
		var mb = new GameObject("membrane");
		mb.transform.parent = GameObject.Find (SceneManager.Instance.scene_name).transform;//or it could be a compartment

		for (var i = 0; i < Lib.Length; i += step) {
			var currentAtom = new Vector4 (Lib [i + 1], Lib [i + 2], Lib [i + 3], AtomHelper.AtomRadii [(int)Lib [i + 4]]);
			var resid = (int)Lib [i];
			//Debug.Log (resid.ToString()+" "+previd.ToString()+" "+atomSpheres.Count.ToString());
			if (previd != resid) {		
				centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;
				atomClusters = new List<List<Vector4>> ();
				// Center atoms
				AtomHelper.OffsetSpheres(ref atomSpheres, centerPosition);
				// Compute bounds
				bounds = AtomHelper.ComputeBounds(atomSpheres);
				//PdbLoader.OffsetPoints (ref atomSpheres, bounds.center);//center
				//List<Vector4> atomCl1 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/2 , 1.0f);
				//List<Vector4> atomCl2 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/4 , 1.0f);
				//List<Vector4> atomCl3 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/8 , 1.0f);
				//atomClusters.Add (atomCl1);
				//atomClusters.Add (atomCl2);
				//atomClusters.Add (atomCl3);
				ingrColor = new Color (0, 1, 0);
				// Define cluster decimation levels
				SceneManager.Instance.AddIngredient ("lipids" + previd.ToString (), bounds, atomSpheres, ingrColor, clusterLevels);
				atomSpheres.Clear ();
				var lipid = new GameObject("lipids" + previd.ToString ());
				lipid.transform.parent = mb.transform;
				lipid.hideFlags = HideFlags.HideInHierarchy;
				//Debug.Log ("added lipids" + previd.ToString ());
			}
			atomSpheres.Add (currentAtom);
			previd=resid;
		}
		//add the last one
		centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;//PdbLoader.GetBounds (atomSpheres);
		//atomClusters = new List<List<Vector4>> ();
		// Center atoms
		AtomHelper.OffsetSpheres(ref atomSpheres, centerPosition);
		// Compute bounds
		bounds = AtomHelper.ComputeBounds(atomSpheres);

		//bounds = PdbLoader.GetBounds (atomSpheres);
		//atomClusters = new List<List<Vector4>> ();
		//check the cluster radius, seems too large
		//List<Vector4> atomClustersL1 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/2 , 1.0f);
		//List<Vector4> atomClustersL2 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/4 , 1.0f);
		//List<Vector4> atomClustersL3 = PdbLoader.ClusterAtomsPointsKmeans(atomSpheres,atomSpheres.Count/8 , 1.0f);
		//PdbLoader.OffsetPoints (ref atomSpheres, bounds.center);//center
		//PdbLoader.OffsetPoints(ref atomClustersL1, bounds.center);
		//PdbLoader.OffsetPoints(ref atomClustersL2, bounds.center);
		//PdbLoader.OffsetPoints(ref atomClustersL3, bounds.center);
		//
		//atomClusters.Add (atomClustersL1);
		//atomClusters.Add (atomClustersL2);
		//atomClusters.Add (atomClustersL3);
		ingrColor = new Color (0, 1, 0);
		SceneManager.Instance.AddIngredient ("lipids" + previd.ToString (), bounds, atomSpheres, ingrColor, clusterLevels);
		atomSpheres.Clear ();
		Debug.Log ("added lipids" + previd.ToString ());
		var lipid1 = new GameObject("lipids" + previd.ToString ());
		lipid1.transform.parent = mb.transform;
		lipid1.hideFlags = HideFlags.HideInHierarchy;

		var Lipids = Helper.ReadBytesAsFloats(Application.dataPath + "/../Data/membrane/lipid_pos_hiv.bin");
		step=8;//rid,pos,rot,
		Debug.Log ("total lipids " + (Lipids.Length / step).ToString ());
		for (var i = 0; i < Lipids.Length; i += step) {
			var position = new Vector3(-Lipids[i+1], Lipids[i+2],Lipids[i+3]);
			var rotation = new Quaternion(Lipids[i+4], Lipids[i+5], Lipids[i+6], Lipids[i+7]);			
			//Debug.Log (rotation.ToString ());
			var mat = Helper.quaternion_matrix(rotation);//.transpose;
			var euler = Helper.euler_from_matrix(mat);
			//Debug.Log (position.ToString()+ " " +euler.ToString ());
			rotation = Helper.MayaRotationToUnity(euler);//-Y-Z
			var resid = (int)Lipids [i];
			SceneManager.Instance.AddIngredientInstance("lipids" + resid.ToString (), position, rotation);
		}
		//we will need some specal case for that, maybe an array with the indice of the lipids Protein object
	}
	//what about distance grid ?

	public static void LoadLipidsTest_lw(){
		//read the lipid plane geometry, and generate a buffer with all the lipid by id.
		//atoms[i].residueId
		//read the triangle files (id,pos,rot,start-end)
		//make this binary
		string resfile = Application.dataPath + "/../Data/membrane/testinsltancelipids.txt";//Application.dataPath + "/../Data/membrane/residhiv.txt"
		string trifile = Application.dataPath + "/../Data/membrane/tridata.txt";//Application.dataPath + "/../Data/membrane/tridatahiv.txt"
		Dictionary<string,List<List<float>>> tri_res = new Dictionary<string,List<List<float>>> ();
		var Lines = File.ReadAllLines(resfile).ToList ();
		int count = 0;
		int linenb = 0;
		foreach (var line in File.ReadAllLines(trifile)) {
			var elems = line.Split();
			//Debug.Log (line);
			var triid = (int)float.Parse(elems[0]);//the triangle indice
			float x1 = float.Parse(elems[1]);
			float y1 = float.Parse(elems[2]);
			float z1 = float.Parse(elems[3]);
			float x2 = float.Parse(elems[4]);
			float y2 = float.Parse(elems[5]);
			float z2 = float.Parse(elems[6]);
			float qx = float.Parse(elems[7]);
			float qy = float.Parse(elems[8]);
			float qz = float.Parse(elems[9]);
			float qw = float.Parse(elems[10]);
			//var start = (int)float.Parse(elems[11]);//start lipid id
			//var end = (int)float.Parse(elems[12]);//end lipid id
			//if (start >0) start -=1;
			//List<int>  listresidues =  Lines[start:end].Select(int.Parse).ToList();
			//if ( end <= start ) continue;
			//var lines = Lines.Skip(start).Take(end-start+1).ToList();//enumerator
			//Debug.Log (start.ToString()+" "+(end-start+1).ToString()+" "+lines.Count.ToString());
			string[] resid = Lines[linenb].Split ();//one line is a bunch of resid separate by space
			foreach (string l in resid) {
				Debug.Log ("resid "+l);
				var tr = new List<float>();
				tr.Add (x1);tr.Add (y1);tr.Add (z1);tr.Add (x2);tr.Add (y2);tr.Add (z2);tr.Add (qx);tr.Add (qy);tr.Add (qz);tr.Add (qw);
				if (!tri_res.ContainsKey(l)){
					var transforms = new List<List<float>>();
					transforms.Add (tr);
					tri_res.Add (l,transforms);
				}
				else {
					tri_res[l].Add (tr);
				}
			}
			linenb+=1;
			/*foreach (string l in lines) {
				Debug.Log (l);
				var tr = new List<float>();
				tr.Add (x1);tr.Add (y1);tr.Add (z1);tr.Add (x2);tr.Add (y2);tr.Add (z2);tr.Add (qx);tr.Add (qy);tr.Add (qz);tr.Add (qw);
				if (!tri_res.ContainsKey(l)){
					var transforms = new List<List<float>>();
					transforms.Add (tr);
					tri_res.Add (l,transforms);
				}
				else {
					tri_res[l].Add (tr);
				}
				if ((int.Parse(l) == 73) ||(int.Parse(l) == 74)){
					Debug.Log ("debug 73/74 "+l);
					Debug.Log (line);
					Debug.Log (tri_res[l][0].ToString ());
				}
			}*/
		}
		//now foreach lipids create an object and its instance
		Debug.Log ("REad "+tri_res.Count.ToString ());
		var atoms = PdbLoader.ReadAtomData(Application.dataPath + "/../Data/membrane/lipid_example.pdb");
		//atoms[i].residueId
		var clusters = new List<Vector4>();
		var atomSpheres = new List<Vector4>();
		
		var residueCount = 0;
		
		for (int i = 0; i < atoms.Count; i++)
		{
			var symbolId = Array.IndexOf(AtomHelper.AtomSymbols, atoms[i].symbol);
			if (symbolId < 0) symbolId = 0;
			
			atomSpheres.Add(new Vector4(atoms[i].position.x, atoms[i].position.y, atoms[i].position.z, AtomHelper.AtomRadii[symbolId]));
			//center is at the center of the lipid triangle not the center of the lipid residues
			if (i == atoms.Count -1 || atoms[i].residueId != atoms[i + 1].residueId)
			{	
				//if this residues is in the dictionary 
				//if (tri_res.ContainsKey( residueCount.ToString() )){
				if (tri_res.ContainsKey( atoms[i].residueId )){
					Debug.Log ("resid "+atoms[i].residueId);
					var centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;
					//var bounds = AtomHelper.ComputeBounds(atomSpheres);
					var atomClusters = new List<List<Vector4>>();
					//dont center
					AtomHelper.OffsetSpheres(ref atomSpheres,  centerPosition);
					var bounds = AtomHelper.ComputeBounds(atomSpheres);
					//PdbLoader.OffsetPoints(ref atomClustersL1, bounds.center);
					//PdbLoader.OffsetPoints(ref atomClustersL2, bounds.center);
					//PdbLoader.OffsetPoints(ref atomClustersL3, bounds.center);
					
					var clusterLevels = new List<float>() {0.15f, 0.10f, 0.05f};

					// Add ingredient to scene manager
					//Color ingrColor = ColorsPalette[current_color];// colorList.Current;
					
					Color ingrColor = new Color(0,1,0);
					//Debug.Log ("color "+current_color+" "+N+" "+ingrColor.ToString());
					//should try to pick most disctinct one ?
					//shouldnt use the pdbName for the name of the ingredient, but rather the actual name
					SceneManager.Instance.AddIngredient("lipids_"+atoms[i].residueId, bounds, atomSpheres, ingrColor, clusterLevels);
					//SceneManager.Instance.AddIngredient("lipids"+residueCount.ToString(), bounds, atomSpheres,ingrColor,atomClusters);
					int inst=0;
					foreach (List<float> tr in tri_res[atoms[i].residueId]){//residueCount.ToString()]){
						var position = new Vector3(-tr[3], tr[4],tr[5]);
						var ori = new Vector3(-tr[0], tr[1],tr[2]);
						var rotation = new Quaternion(tr[6], tr[7], tr[8], tr[9]);
						//var offset = rotation*(new Vector3(-bounds.center[0], bounds.center[1],bounds.center[2])-new Vector3(tr[0], tr[1],tr[2]));
						var mat = Helper.quaternion_matrix(rotation).transpose;
						var euler = Helper.euler_from_matrix(mat);
						rotation = Helper.MayaRotationToUnity(euler);
						//offset = new Vector3(-offset[0],offset[1],offset[2]);
						var off = centerPosition-ori;//-bounds.center;
						//if (Vector3.Distance(ori,bounds.center) > 100.0f) {
						//	Debug.Log ("probem triangle residue");
						//	Debug.Log (inst.ToString()+" "+residueCount.ToString()+" "+ori.ToString()+" "+bounds.center.ToString());
						//	continue;
						//}
						var offset = Helper.QuaternionTransform(rotation, off);//rotation *(bounds.center - ori);
						Debug.Log ("off is "+off.ToString()+" "+ori.ToString()+" "+centerPosition.ToString());
						SceneManager.Instance.AddIngredientInstance("lipids_"+atoms[i].residueId, position+offset, rotation);
					}
					//create a object
					//Debug.Log ("rid "+atoms[i].residueId+" "+residueCount.ToString());
					//clusters.AddRange(atomSpheres);
				}
				atomSpheres.Clear();
				residueCount ++;
				//if (residueCount >= 200) break;
			}
		}
		//read the lipid id for each triangle.
		//generate a buffer lipid ID->instance ?
		//we could alos use a file Lidid ID pos-rot
		//
	}
	public static void LoadLipidsTest_bin(){
		//read the lipid plane geometry, and generate a buffer with all the lipid by id.
		//atoms[i].residueId
		//read the triangle files (id,pos,rot,start-end)
		//make this binary
		string resfile = Application.dataPath + "/../Data/membrane/testinsltancelipids.txt";//Application.dataPath + "/../Data/membrane/residhiv.txt"
		string trifile = Application.dataPath + "/../Data/membrane/tridata.bin";//Application.dataPath + "/../Data/membrane/tridatahiv.txt"
		Dictionary<string,List<List<float>>> tri_res = new Dictionary<string,List<List<float>>> ();
		var Lines = File.ReadAllLines(resfile).ToList ();
		//var Lipids = Helper.ReadBytesAsFloats(resfile);
		var Triangles = Helper.ReadBytesAsFloats(trifile);
		int steptri = 11;

		int count = 0;
		int linenb = 0;
		for (var i = 0; i < Triangles.Length; i += steptri) {
			var triid = Triangles[i];//the triangle indice
			float x1 = Triangles[i+1];
			float y1 = Triangles[i+2];
			float z1 = Triangles[i+3];
			float x2 = Triangles[i+4];
			float y2 = Triangles[i+5];
			float z2 = Triangles[i+6];
			float qx = Triangles[i+7];
			float qy = Triangles[i+8];
			float qz = Triangles[i+9];
			float qw = Triangles[i+10];

			string[] resid = Lines[linenb].Split ();//one line is a bunch of resid separate by space
			foreach (string l in resid) {
				Debug.Log ("resid "+l);
				var tr = new List<float>();
				tr.Add (x1);tr.Add (y1);tr.Add (z1);tr.Add (x2);tr.Add (y2);tr.Add (z2);tr.Add (qx);tr.Add (qy);tr.Add (qz);tr.Add (qw);
				if (!tri_res.ContainsKey(l)){
					var transforms = new List<List<float>>();
					transforms.Add (tr);
					tri_res.Add (l,transforms);
				}
				else {
					tri_res[l].Add (tr);
				}
			}
			linenb+=1;
		}
		//now foreach lipids create an object and its instance
		Debug.Log ("REad "+tri_res.Count.ToString ());
		var atoms = PdbLoader.ReadAtomData(Application.dataPath + "/../Data/membrane/lipid_example.pdb");//could it be faster with binary ?
		//atoms[i].residueId
		var clusters = new List<Vector4>();
		var atomSpheres = new List<Vector4>();
		
		var residueCount = 0;
		
		for (int i = 0; i < atoms.Count; i++)
		{
			var symbolId = Array.IndexOf(AtomHelper.AtomSymbols, atoms[i].symbol);
			if (symbolId < 0) symbolId = 0;
			
			atomSpheres.Add(new Vector4(atoms[i].position.x, atoms[i].position.y, atoms[i].position.z, AtomHelper.AtomRadii[symbolId]));
			//center is at the center of the lipid triangle not the center of the lipid residues
			if (i == atoms.Count -1 || atoms[i].residueId != atoms[i + 1].residueId)
			{	
				Debug.Log ("resid "+atoms[i].residueId);
				//if this residues is in the dictionary 
				//if (tri_res.ContainsKey( residueCount.ToString() )){
				if (tri_res.ContainsKey( atoms[i].residueId )){
					var bounds = AtomHelper.ComputeBounds(atomSpheres);
					var atomClusters = new List<List<Vector4>>();
					//dont center
					AtomHelper.OffsetSpheres(ref atomSpheres,  bounds.center);
					//PdbLoader.OffsetPoints(ref atomClustersL1, bounds.center);
					//PdbLoader.OffsetPoints(ref atomClustersL2, bounds.center);
					//PdbLoader.OffsetPoints(ref atomClustersL3, bounds.center);
					
					var clusterLevels = new List<float>() {0.15f, 0.10f, 0.05f};
					
					// Add ingredient to scene manager
					//Color ingrColor = ColorsPalette[current_color];// colorList.Current;
					
					Color ingrColor = new Color(0,1,0);
					//Debug.Log ("color "+current_color+" "+N+" "+ingrColor.ToString());
					//should try to pick most disctinct one ?
					//shouldnt use the pdbName for the name of the ingredient, but rather the actual name
					SceneManager.Instance.AddIngredient("lipids_"+atoms[i].residueId, bounds, atomSpheres, ingrColor, clusterLevels);
					//SceneManager.Instance.AddIngredient("lipids"+residueCount.ToString(), bounds, atomSpheres,ingrColor,atomClusters);
					int inst=0;
					foreach (List<float> tr in tri_res[atoms[i].residueId]){//residueCount.ToString()]){
						var position = new Vector3(-tr[3], tr[4],tr[5]);
						var ori = new Vector3(-tr[0], tr[1],tr[2]);
						var rotation = new Quaternion(tr[6], tr[7], tr[8], tr[9]);
						//var offset = rotation*(new Vector3(-bounds.center[0], bounds.center[1],bounds.center[2])-new Vector3(tr[0], tr[1],tr[2]));
						var mat = Helper.quaternion_matrix(rotation).transpose;
						var euler = Helper.euler_from_matrix(mat);
						rotation = Helper.MayaRotationToUnity(euler);
						//offset = new Vector3(-offset[0],offset[1],offset[2]);
						var off = (bounds.center*-1.0f)-ori;//-bounds.center;
						//if (Vector3.Distance(ori,bounds.center) > 100.0f) {
						//	Debug.Log ("probem triangle residue");
						//	Debug.Log (inst.ToString()+" "+residueCount.ToString()+" "+ori.ToString()+" "+bounds.center.ToString());
						//	continue;
						//}
						var offset = Helper.QuaternionTransform(rotation, ori - bounds.center);//rotation *(bounds.center - ori);
						//the rotation is acutally the alignement to the normal vector ?
						Debug.Log (inst.ToString()+" "+atoms[i].residueId+" "+ori.ToString()+" "+bounds.center.ToString());
						//Debug.Log ((ori - bounds.center).ToString());
						//Debug.Log (offset.ToString());
						inst+=1;
						SceneManager.Instance.AddIngredientInstance("lipids_"+atoms[i].residueId, position, rotation);
					}
					//create a object
					//Debug.Log ("rid "+atoms[i].residueId+" "+residueCount.ToString());
					//clusters.AddRange(atomSpheres);
				}
				atomSpheres.Clear();
				residueCount ++;
				//if (residueCount >= 200) break;
			}
		}
		//read the lipid id for each triangle.
		//generate a buffer lipid ID->instance ?
		//we could alos use a file Lidid ID pos-rot
		//
	}
	public static void AddIngredients(string iname, string pdbName, Color baseColor,string prefix, bool biomt)
	{

		List<Vector4> atomSpheres;
		List<Matrix4x4> biomtTransforms = new List<Matrix4x4>();
		Vector3 biomtCenter = Vector3.zero;
		bool containsACarbonOnly = false;
		bool oneLOD = false;
		//if (pdbName.StartsWith("EMDB")) return;
		//if (pdbName.Contains("1PI7_1vpu_biounit")) return;//??
		// Load atom set from pdb file
		var atomSet = PdbLoader.LoadAtomSet(pdbName);
		
		// If the set is empty return
		if (atomSet.Count == 0) return;
		
		atomSpheres = AtomHelper.GetAtomSpheres(atomSet);
		containsACarbonOnly = AtomHelper.ContainsACarbonOnly(atomSet);

		var centerPosition = AtomHelper.ComputeBounds(atomSpheres).center;       
		
		// Center atoms
		AtomHelper.OffsetSpheres(ref atomSpheres, centerPosition);
		
		// Compute bounds
		var bounds = AtomHelper.ComputeBounds(atomSpheres);
		
		biomtTransforms = biomt ? PdbLoader.LoadBiomtTransforms(pdbName) : new List<Matrix4x4>();
		biomtCenter = AtomHelper.GetBiomtCenter(biomtTransforms,centerPosition);

		Color ingrColor = baseColor;

		// Define cluster decimation levels
		var clusterLevels = (containsACarbonOnly)
		? new List<float>() {0.85f, 0.25f, 0.1f}
		: new List<float>() {0.15f, 0.10f, 0.05f};
		//if (oneLOD)
		//clusterLevels = new List<float> () {1, 1, 1};
		// Add ingredient type
		//SceneManager.Instance.AddIngredient(name, bounds, atomSpheres, color);
		
		SceneManager.Instance.AddIngredient(iname, bounds, atomSpheres, ingrColor, clusterLevels,oneLOD);
	}

	public static void LoadPrototype(){
		
		//txt file not adequate, use binary
		var posrotpath = Application.dataPath + "/../Data/packing_results/haltongrid.bin";
		if (!File.Exists(posrotpath)) throw new Exception("No file found at: " + posrotpath);
		//int count = 0;
		var Data = Helper.ReadBytesAsFloats(posrotpath);
		int step = 7;//3 translation 4 rotation	
		string name="test1";
		bool second = true;
		int count = 0;
		for (var i = 0; i < Data.Length; i += step)
			//foreach (var line in File.ReadAllLines(posrotpath))
		{
			if (i==0) {
				AddIngredients ("test1", "1atu", new Color (1.0f, 107.0f / 255.0f, 66.0f / 255.0f), "inner",false);
				AddIngredients ("test2", "3j1z", new Color (0.0f, 107.0f / 255.0f, 66.0f / 255.0f), "inner",false);
				name="test1";
				Debug.Log ("ok ingredient loaded and build ?");
			}
			//else if ((i>=Data.Length/2)&&(second)) {
			//	AddIngredients ("test2", "3j1z", new Color (0.0f, 107.0f / 255.0f, 66.0f / 255.0f), "inner",false);
			//	name="test2";
			//	second = false;
			//}
			if ((count % 2)==0) name = "test1";
			else name = "test2";
			//name="test1";
			//if (i < Data.Length/2 ){name = "test1";}
			//else {name="test2";}
			
			//var split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var x = Data[i];//float.Parse(split[0]);
			var y = Data[i+1];//float.Parse(split[1]);
			var z = Data[i+2];//float.Parse(split[2]);
			var qx = Data[i+3];//float.Parse(split[3]);
			var qy = Data[i+4];//float.Parse(split[4]);
			var qz = Data[i+5];//float.Parse(split[5]);
			var qw = Data[i+6];//float.Parse(split[6]);
			
			var position = new Vector3(-x, y,z);
			var rotation = new Quaternion(qx, qy, qz, qw);
			
			var mat = Helper.quaternion_matrix(rotation);
			var euler = Helper.euler_from_matrix(mat);
			rotation = Helper.MayaRotationToUnity(euler);
			
			SceneManager.Instance.AddIngredientInstance(name, position, rotation);
			count+=1;
			//#if UNITY_EDITOR too slow
			//EditorUtility.DisplayProgressBar("Parsing", "Parsing...", (float)count/1000000.0f);
			//#endif
			//count+=1;
		}
		//#if UNITY_EDITOR
		//EditorUtility.ClearProgressBar();
		//#endif
	}
}
