using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices; // because of DllImport
using System; // because of IntPtr

//[ExecuteInEditMode]
public class MolecularSceneUpdater : MonoBehaviour {

    public string pBuf;
    public IntPtr hMapFile;

    [DllImport("ShMemOpen")]
    private static extern void prepareSharedMemory(out IntPtr hMapFile, out string pBuf); // this is going to output hMapFile and pBuf
    [DllImport("ShMemRead")]
    private static extern void readSharedMemory(string pBuf); // Actually... this might even be not needed at all!
    // char * readSharedMemory(LPCTSTR pBuf)
    [DllImport("ShMemClose")]
    private static extern void cleanupSharedMemory(IntPtr hMapFile, string pBuf);
    // void cleanupSharedMemory(HANDLE hMapFile, LPCTSTR pBuf)

    void OnEnable () // it might be better to make this OnEnable()
    {
        Debug.Log("MolecularSceneUpdater: OnEnable()");
        // DLL function call
        prepareSharedMemory(out this.hMapFile, out this.pBuf);
        readSharedMemory(this.pBuf);
    }
	
	void Update ()
    {
        Debug.Log("MolecularSceneUpdater: Update()");
        // DLL function call
        //char* readSharedMemory(LPCTSTR pBuf);
    }

    void OnDisable ()
    {
        Debug.Log("MolecularSceneUpdater: OnDisable()");
        // DLL function call
        cleanupSharedMemory(this.hMapFile, this.pBuf);
    }
}
