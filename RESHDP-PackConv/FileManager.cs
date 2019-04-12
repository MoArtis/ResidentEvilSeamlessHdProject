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
using System.Diagnostics;
using FolderBrowserDialog = Ookii.Dialogs.Wpf.VistaFolderBrowserDialog;
using System.Security.Permissions;
using System.Security;

public class FileManager
{
    public FileInfo[] fileInfos;

    public byte[] GetBytesFromFile(int index)
    {
        return File.ReadAllBytes(fileInfos[index].FullName);
    }

    public int LoadFiles(string path, string extension, SearchOption searchOption)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        fileInfos = dirInfo.GetFiles("*." + extension, searchOption);
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
            //MessageBox.Show("Something is wrong with the processed Texture named " + fileInfo.Name, "", MessageBoxButton.OK, MessageBoxImage.Error);
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

        return GetObjectFromFileInfo<T>(fileInfos[index]);
    }

    public T GetObjectFromFileInfo<T>(FileInfo fi)
    {
        string jsonData = File.ReadAllText(fi.FullName);

        if (jsonData == null || jsonData == "")
            return default;

        return JsonConvert.DeserializeObject<T>(jsonData);
    }

    public T GetObjectFromPath<T>(string path)
    {
        FileInfo fi = new FileInfo(path);

        if (fi.Exists == false)
            return default;

        return GetObjectFromFileInfo<T>(fi);
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

    public bool SelectFolder(out string folderPath, string description = "", string directory = "", bool showNewFolderButton = false)
    {
        var fbd = new FolderBrowserDialog
        {
            Description = description,
            ShowNewFolderButton = showNewFolderButton
        };

        if (!string.IsNullOrEmpty(directory))
        {
            fbd.SelectedPath = GetDirectoryPath(directory);
        }

        bool result = fbd.ShowDialog().GetValueOrDefault();

        folderPath = result ? fbd.SelectedPath : string.Empty;

        return result;
    }

    public string GetDirectoryPath(string directory)
    {
        var directoryPath = Path.GetFullPath(directory);
        if (!directoryPath.EndsWith("\\"))
        {
            directoryPath += "\\";
        }
        if (Path.GetPathRoot(directoryPath) == directoryPath)
        {
            return directory;
        }
        return Path.GetDirectoryName(directoryPath) + Path.DirectorySeparatorChar;
    }

    private static bool HasWriteAccess(string directory)
    {
        var permission = new FileIOPermission(FileIOPermissionAccess.Write, directory);
        var permissionSet = new PermissionSet(PermissionState.None);
        permissionSet.AddPermission(permission);

        return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
    }
}
