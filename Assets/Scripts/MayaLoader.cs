using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

/*
    Provides functionality for loading data exported by CellViewExporter from Maya 
*/

public static class MayaLoader {

    public static List<Vector3> positions;
    public static List<Vector4> rotations;
    public static List<String> ids;

    public static void loadFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new Exception("File not found at: " + path);
        }

        int i = 0;
        foreach (var line in File.ReadAllLines(path))
        {
            //Debug.Log(line);
            /* TODO:
                - read position info
                - read rotation info
                - read id info
                - create dummy game object and put it at position
            */

            char[] delimiters = { ' ' };
            var tokens = line.Split(delimiters);

            var x = float.Parse(tokens[0]);
            var y = float.Parse(tokens[1]);
            var z = float.Parse(tokens[2]);

            string objName = "Object" + i;
            if (GameObject.Find(objName) == null) // object doesn't exist yet
            {
                GameObject newObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newObj.transform.position = new Vector3(x, y, z);
                newObj.name = objName;
            } else
            {
                GameObject obj = GameObject.Find(objName);
                obj.transform.position = new Vector3(x, y, z);
            }

            // Debug printout
            if (i == 0)
            {
                foreach (var tok in tokens)
                {
                    Debug.Log(tok);
                }
            }
            i++;
        }
    }

}
