using UnityEditor;
using UnityEngine;
using System.Collections;

public class ListBoxEditor : MonoBehaviour {

	// Add a menu item named "Do Something" to MyMenu in the menu bar.
	[MenuItem ("GameObject/UI/ListBox")]
	static void ListBox () {
		string[] guids = AssetDatabase.FindAssets("ListBoxMedium t:prefab");
		if(guids.Length <= 0) Debug.Log("The prefab called ListBoxPrefab was not found in the project");
		else {
			string path = AssetDatabase.GUIDToAssetPath(guids[0]);

			Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

			GameObject  newListBox = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
			if(newListBox != null) {
				newListBox.name = "ListBox";
				GameObject currentlySelectedObject = Selection.activeGameObject;
				if(currentlySelectedObject != null) {
					newListBox.transform.parent = currentlySelectedObject.transform;
				}
				Selection.activeGameObject = newListBox;
			}
		}
    }
}
