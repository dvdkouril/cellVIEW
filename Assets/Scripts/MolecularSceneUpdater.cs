using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices; // because of DllImport
using System; // because of IntPtr

/*
    In Update(), this class loads data from Maya scene via shared memory
*/

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
    
    void OnEnable () // it might be better to make this OnEnable()
    {
        //Debug.Log("MolecularSceneUpdater: OnEnable()");

        // DLL function call
        shMemBuf = prepareSharedMemory(out this.hMapFile);
        Debug.Log(PtrToStringUtf8(shMemBuf)); // leaving this here just because it does funny output into Console
    }

    /*
        now loading more than one obj info
    */
    void Update()
    {

        //Debug.Log("MolecularSceneUpdater: Update()");
        // some debug shit:
        //GameObject cube = GameObject.Find("Cube");
        //Quaternion q = cube.transform.rotation;


        float[] shMemContent = new float[256];

        Marshal.Copy(shMemBuf, shMemContent, 0, 256); // last param is number of array elements to copy

        int numObjsLoaded = 3;
        for (int i = 0; i < numObjsLoaded; ++i)
        {
            var pos_x = shMemContent[i*7 + 0];
            var pos_y = shMemContent[i*7 + 1];
            var pos_z = shMemContent[i*7 + 2];
            pos_z *= -1;                            // position conversion from right handed (maya) coordinate system

            var rot_x = shMemContent[i*7 + 3];
            var rot_y = shMemContent[i*7 + 4];
            var rot_z = shMemContent[i*7 + 5];
            var rot_w = shMemContent[i*7 + 6];

            GameObject cb = GameObject.Find("Cube" + i);
            if (cb == null)
            { // game object for this object has not been yet created
                cb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cb.name = "Cube" + i;
            }

            // debugging just the first instance
            if (cb.name == "Cube0")
            {
                //Debug.Log("Cube0 position: " + pos_x + " " + pos_y + " " + pos_z);
                //Debug.Log("Cube0 rotation quaternion = (" + rot_x + " " + rot_y + " " + rot_z + " " + rot_w + ")");
                //float x = cb.transform.rotation.eulerAngles.x;
                //float y = cb.transform.rotation.eulerAngles.y;
                //float z = cb.transform.rotation.eulerAngles.z;
                //Debug.Log("loaded rotation: (" + x + ", " + y + ", " + z + ")");
            }
            
            Quaternion rot = new Quaternion(rot_x, rot_y, rot_z, rot_w);
            Vector3 rotDeg = rot.eulerAngles;

            // this way it works for a single axis rotations:
            Quaternion x = Quaternion.AngleAxis(-rotDeg.x, Vector3.right);
            Quaternion y = Quaternion.AngleAxis(-rotDeg.y, Vector3.up);
            Quaternion z = Quaternion.AngleAxis(rotDeg.z, Vector3.forward);
            //rot = x * y * z;      // rotation around x changes direction midway
            //rot = x * z * y;      // same
            rot = y * x * z;      // wow, this looks like this is it
            //rot = y * z * x;      // direction change
            //rot = z * x * y;      // weird combined rotations
            //rot = z * y * x;        // direction change

            // setting the transoform on an object
            cb.transform.position = new Vector3(pos_x, pos_y, pos_z);
            cb.transform.localRotation = rot;
        }

    }

    /*
        Update function that uses the new ("binary") format of shared memory data
    */
    void Update2()
    {

        //Debug.Log("MolecularSceneUpdater: Update()");
        
        double[] shMemContent = new double[256];

        Marshal.Copy(shMemBuf, shMemContent, 0, 256); // last param is number of array elements to copy

        var x = shMemContent[0];
        var y = shMemContent[1];
        var z = shMemContent[2];

        GameObject cb = GameObject.Find("Cube");
        cb.transform.position = new Vector3((float)x, (float)y, (float)z);

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
