using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;

public class MayaLoaderWindow : EditorWindow {

    public string filePath = "C:\\Users\\dvdkouril\\Documents\\github-projects\\maya-cellview-exporter\\scene-out.txt";

    [MenuItem("MayaLoader/Maya Loader window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MayaLoaderWindow));
    }

    void OnGUI()
    {
        GUILayout.Label("Export File Path", EditorStyles.boldLabel);
        filePath = EditorGUILayout.TextField("Export Input File", filePath);

        GUILayout.Label("Submit", EditorStyles.boldLabel);

        if ( GUILayout.Button("Just do it!") )
        { // user clicked the button
            Debug.Log("Just doing it!");
            LoadMayaFile();
        }
    }


    [DllImport("SharedMemDll")]
    private static extern void load();

    void LoadMayaFile()
    {
        load();
        //MayaLoader.loadFile(filePath);
    }

}
