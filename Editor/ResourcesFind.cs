using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class ResourcesFind : Editor
{
    [MenuItem("Assets/Tool/查找shader引用的cg文件", false)]
    public static void FindCg()
    {
        var objs = Selection.objects;

        var objList = new HashSet<UnityEngine.Object>();
        foreach (var obj in objs)
        {
            string path = AssetDatabase.GetAssetPath(obj);

            if (!path.Contains(".shader"))
                return;

            var paths = AssetDatabase.GetAllAssetPaths();

            var list = new HashSet<string>();

            foreach (var assetPath in paths)
            {
                if (assetPath.EndsWith(".cginc") || assetPath.EndsWith(".hlsl"))
                {
                    list.Add(assetPath);
                }
            }

            string str = File.ReadAllText(path);
            var mathces = Regex.Matches(str, "#include\\s?\"(.*?)\"");

            foreach (Match item in mathces)
            {
                var r = item.Groups[1].Value;

                foreach (var cgincPath in list)
                {
                    if (cgincPath.Contains(r))
                    {
                        objList.Add(AssetDatabase.LoadMainAssetAtPath(cgincPath));
                    }
                }
            }
        }

        foreach (var obj in objList)
        {
            Debug.Log(AssetDatabase.GetAssetPath(obj));
        }

        Selection.objects = objList.ToList().ToArray();
    }

    [MenuItem("Assets/Tool/查找hlsl引用的shader文件", false)]
    public static void FindShaderFromInclude()
    {
        UnityEngine.Object obj = Selection.activeObject;

        string path = AssetDatabase.GetAssetPath(obj);

        if (!path.Contains(".cginc") && !path.Contains(".hlsl"))
            return;

        var paths = AssetDatabase.GetAllAssetPaths();

        var shaderPathList = new HashSet<string>();

        foreach (var assetPath in paths)
        {
            if (assetPath.EndsWith(".shader") || assetPath.EndsWith(".Shader") || assetPath.EndsWith(".SHADER"))
            {
                shaderPathList.Add(assetPath);
            }
        }

        var objList = new List<UnityEngine.Object>();

        foreach (var shaderPath in shaderPathList)
        {
            string str = File.ReadAllText(shaderPath);

            if (str.Contains(Path.GetFileName(path)))
            {
                Debug.Log(shaderPath);
                objList.Add(AssetDatabase.LoadMainAssetAtPath(shaderPath));
            }
        }

        Selection.objects = objList.ToArray();
    }
    
    [MenuItem("GameObject/TTT/阵列100个")]
    public static void InstanceGrid()
    {
        UnityEngine.Object obj = Selection.activeObject;
        int row = 10;
        int space = 20;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < row; j++)
            {
                var go = Instantiate(obj as GameObject);
                Undo.RegisterCreatedObjectUndo(go,"创建单个go : " + go.name);
                go.transform.position += go.transform.right * i * space + go.transform.forward * j * space;
                go.transform.name += $"_{i * row + j}";
            }
        }
     
    }
    
    [MenuItem("GameObject/TTT/阵列1000个")]
    public static void InstanceGrid0()
    {
        UnityEngine.Object obj = Selection.activeObject;
        int row = 30;
        float space = 1f;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < row; j++)
            {
                var go = Instantiate(obj as GameObject);
                Undo.RegisterCreatedObjectUndo(go,"创建单个go : " + go.name);
                go.transform.position += go.transform.right * i * space + go.transform.forward * j * space;
                go.transform.name += $"_{i * row + j}";
            }
        }
     
    }
}
