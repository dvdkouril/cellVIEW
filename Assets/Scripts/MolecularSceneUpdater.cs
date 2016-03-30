using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices; // because of DllImport
using System; // because of IntPtr

//[ExecuteInEditMode]
public class MolecularSceneUpdater : MonoBehaviour {

    public string pBuf;
    public IntPtr hMapFile;
    public IntPtr shMemBuf;

    
    [DllImport("SharedMemDll")]
    private static extern IntPtr prepareSharedMemory(out IntPtr hMapFile); // this is going to output hMapFile and pBuf
    //private static extern void prepareSharedMemory(out IntPtr hMapFile, out string pBuf); // this is going to output hMapFile and pBuf
    [DllImport("SharedMemDll")]
    private static extern void readSharedMemory(string pBuf); // Actually... this might even be not needed at all!
    [DllImport("SharedMemDll")]
    private static extern void cleanupSharedMemory(IntPtr hMapFile, string pBuf);
    

    //[DllImport("ShMemOpen")]
    //private static extern void prepareSharedMemory(out IntPtr hMapFile, out string pBuf); // this is going to output hMapFile and pBuf
    //[DllImport("ShMemRead")]
    //private static extern void readSharedMemory(string pBuf); // Actually... this might even be not needed at all!
    //// char * readSharedMemory(LPCTSTR pBuf)
    //[DllImport("ShMemClose")]
    //private static extern void cleanupSharedMemory(IntPtr hMapFile, string pBuf);
    //// void cleanupSharedMemory(HANDLE hMapFile, LPCTSTR pBuf)

    void OnEnable () // it might be better to make this OnEnable()
    {
        //Debug.Log("MolecularSceneUpdater: OnEnable()");
        // DLL function call
        shMemBuf = prepareSharedMemory(out this.hMapFile);
        Debug.Log(PtrToStringUtf8(shMemBuf));
        //readSharedMemory(this.pBuf);
    }
	
	void Update ()
    {

        //Debug.Log("MolecularSceneUpdater: Update()");
        // DLL function call
        //char* readSharedMemory(LPCTSTR pBuf);
        //Debug.Log("pBuf = " + this.pBuf);
        string str = PtrToStringUtf8(shMemBuf);
        Debug.Log(str);

        char[] delimiters = { ' ' };
        var tokens = str.Split(delimiters);

        // remove trash after first \n
        int index = tokens[2].IndexOf("\n");
        if (index > 0)
            tokens[2] = tokens[2].Substring(0, index);

        var x = float.Parse(tokens[0]);
        var y = float.Parse(tokens[1]);
        var z = float.Parse(tokens[2]);

        GameObject cb = GameObject.Find("Cube");
        cb.transform.position = new Vector3(x, y, z);

    }

    void OnDisable ()
    {
        //Debug.Log("MolecularSceneUpdater: OnDisable()");
        // DLL function call
        cleanupSharedMemory(this.hMapFile, this.pBuf);
    }

    private static string PtrToStringUtf8(IntPtr ptr) // aPtr is nul-terminated
    {
        if (ptr == IntPtr.Zero)
            return "";
        int len = 0;
        while (System.Runtime.InteropServices.Marshal.ReadByte(ptr, len) != 0)
            len++;
        if (len == 0)
            return "";
        byte[] array = new byte[len];
        System.Runtime.InteropServices.Marshal.Copy(ptr, array, 0, len);
        return System.Text.Encoding.UTF8.GetString(array);
    }
}
