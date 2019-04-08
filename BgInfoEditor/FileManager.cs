using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;
using System.Diagnostics;

public class FileManager
{
    public FileInfo[] fileInfos;

    public byte[] GetBytesFromFile(int index)
    {
        return File.ReadAllBytes(fileInfos[index].FullName);
    }

    public int LoadFiles(string path, string extension)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        fileInfos = dirInfo.GetFiles("*." + extension, SearchOption.TopDirectoryOnly);
        return fileInfos.Length;
    }

    public void OpenFolder(string path)
    {
        Process.Start(path);
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

    //public Texture2D GetTextureFromFileIndex(int index)
    //{
    //    if (fileInfos == null || index >= fileInfos.Length)
    //        return null;

    //    return GetTextureFromFileInfo(fileInfos[index]);
    //}

    public BitmapImage GetBitmapFromFileInfo(FileInfo fileInfo)
    {
        BitmapImage bi = new BitmapImage();

        bi.BeginInit();
        bi.UriSource = new Uri(fileInfo.FullName);
        bi.EndInit();

        if (bi == null)
        {
            MessageBox.Show("Something is wrong with the processed Texture named " + fileInfo.Name, "", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        return bi;
    }

    public BitmapImage GetBitmapFromPath(string path, string extension = ".png")
    {
        FileInfo fi = new FileInfo(string.Concat(path, extension));

        if (fi.Exists == false)
            return null;

        return GetBitmapFromFileInfo(fi);
    }

    //public void SaveTextureToPng(Texture2D tex, string path, string fileName)
    //{
    //    byte[] data = ImageConversion.EncodeToPNG(tex);
    //    string fullName = string.Concat(path, "/", fileName, ".png");
    //    File.WriteAllBytes(fullName, data);
    //}

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

        return JsonConvert.DeserializeObject<T>(jsonData);
    }

    public void SaveToJson<T>(T obj, string path, string filename, bool prettyPrint = false)
    {
        string jsonStr = JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None);
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
