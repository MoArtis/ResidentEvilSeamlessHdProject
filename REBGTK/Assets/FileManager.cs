using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;
using System.Security.Cryptography;

public class FileManager
{
    public FileInfo[] fileInfos;

    public byte[] GetBytesFromFile(int index)
    {
        return File.ReadAllBytes(fileInfos[index].FullName);
    }

    public int LoadFiles(string path, string extension, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        fileInfos = dirInfo.GetFiles("*." + extension, searchOption);
        return fileInfos.Length;
    }

    public void OpenFolder(string path)
    {
        path = path.TrimEnd(new[] { '\\', '/' }); // Mac doesn't like trailing slash

        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        UnityEngine.Application.OpenURL(Path.Combine("file://", directoryInfo.FullName));
    }

    public void CreateDirectory(string path)
    {
        if (path == null || path == "")
            return;

        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
    }

    public void OrderFiles<T>(Func<FileInfo, T> keySelector, bool isAscending)
    {
        if (isAscending)
            fileInfos = fileInfos.OrderBy(keySelector).ToArray();
        else
            fileInfos = fileInfos.OrderByDescending(keySelector).ToArray();
    }

    public string GetFileName(FileInfo file)
    {
        return RemoveExtensionFromFileInfo(file);
    }

    public string RemoveExtensionFromFileInfo(FileInfo fileInfo)
    {
        return fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length);
    }

    public Texture2D GetTextureFromFileIndex(int index)
    {
        if (fileInfos == null || index >= fileInfos.Length)
            return null;

        return GetTextureFromFileInfo(fileInfos[index]);
    }

    public Texture2D GetTextureFromFileInfo(FileInfo fileInfo)
    {
        Texture2D tex = new Texture2D(0, 0, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        //tex.hideFlags = HideFlags.HideAndDontSave;
        if (tex.LoadImage(File.ReadAllBytes(fileInfo.FullName)) == false)
        {
            Debug.LogError("Something is wrong with the processed Texture named " + fileInfo.Name);
            return null;
        }

        tex.name = RemoveExtensionFromFileInfo(fileInfo);
        return tex;
    }

    public Texture2D GetTextureFromPath(string path, string extension = ".png")
    {
        FileInfo fi = new FileInfo(string.Concat(path, extension));
        if (fi.Exists == false)
            return null;

        return GetTextureFromFileInfo(fi);
    }

    public void SaveTextureToPng(Texture2D tex, string path, string fileName)
    {
        byte[] data = ImageConversion.EncodeToPNG(tex);
        string fullName = string.Concat(path, "/", fileName, ".png");
        File.WriteAllBytes(fullName, data);
    }

    public void SaveTextureToJPG(Texture2D tex, string path, string fileName, int quality)
    {
        byte[] data = ImageConversion.EncodeToJPG(tex, quality);
        string fullName = string.Concat(path, "/", fileName, ".jpg");
        File.WriteAllBytes(fullName, data);
    }

    public void SaveReportToFile(StringBuilder reportSb, string path, string context)
    {
        string fileName = string.Concat("REPORT_", context, "_", DateTime.Now.ToString("yyyy-MM-dd_HH-mm"));
        string fullName = Path.Combine(path, fileName + ".txt");
        File.WriteAllText(fullName, reportSb.ToString());
    }

    public T[] GetObjectsFromFiles<T>()
    {
        if (fileInfos == null)
            return null;

        T[] objects = new T[fileInfos.Length];
        for (int i = 0; i < fileInfos.Length; i++)
        {
            objects[i] = GetObjectFromFileIndex<T>(i);
        }
        return objects;
    }

    public T GetObjectFromFileIndex<T>(int index)
    {
        if (fileInfos == null || index >= fileInfos.Length)
            return default;

        string jsonData = File.ReadAllText(fileInfos[index].FullName);

        if (jsonData == null || jsonData == "")
            return default;

        //Debug.Log(fileInfos[index].Name);

        return JsonUtility.FromJson<T>(jsonData);
    }

    public void SaveToJson<T>(T obj, string path, string filename, bool prettyPrint = false)
    {
        string jsonStr = JsonUtility.ToJson(obj, prettyPrint);
        string fullName = Path.Combine(path, filename + ".json");
        File.WriteAllText(fullName, jsonStr);
    }

    public string GetMd5(byte[] data)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] md5Hash = md5.ComputeHash(data);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < md5Hash.Length; i++)
        {
            sb.Append(md5Hash[i].ToString("X2"));
        }

        return sb.ToString();
    }
}
