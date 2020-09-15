using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class CustomEditorUtilities
{
    public static T[] GetAllAssetPathsAt<T>(string relativePath)
    {
        string[] fileEntries = Directory.GetFiles($"{Application.dataPath}/{relativePath}");
        List<T> outputAssets = new List<T>();

        foreach (var FileName in fileEntries)
        {
            if (FileName.EndsWith(".meta")) continue;

            string temp = FileName.Replace("\\", "/");
            int index = temp.LastIndexOf("/");
            string localPath = "Assets/" + relativePath;

            if (index > 0)
                localPath += temp.Substring(index);

            object loadedAsset = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

            if (loadedAsset != null)
                outputAssets.Add((T)loadedAsset);
        }
        return outputAssets.ToArray();
    }

    public static List<T> GetAllAssetsAtRelativePath<T>(string relativePath)
    {
        string[] fileEntries = Directory.GetFiles($"{Application.dataPath}/{relativePath}");
        List<T> outputAssets = new List<T>();

        foreach (var FileName in fileEntries)
        {
            if (FileName.EndsWith(".meta")) continue;

            string temp = FileName.Replace("\\", "/");
            int index = temp.LastIndexOf("/");
            string localPath = "Assets/" + relativePath;

            if (index > 0)
                localPath += temp.Substring(index);

            object loadedAsset = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

            if (loadedAsset != null)
                outputAssets.Add((T)loadedAsset);
        }

        return outputAssets;
    }
}
