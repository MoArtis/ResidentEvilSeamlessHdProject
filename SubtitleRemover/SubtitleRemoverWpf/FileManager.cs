using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using FolderBrowserDialog = Ookii.Dialogs.Wpf.VistaFolderBrowserDialog;
using OpenFileDialog = Ookii.Dialogs.Wpf.VistaOpenFileDialog;
using System.Security.Permissions;
using System.Security;
using System.Drawing.Imaging;

public class FileManager
{
    //public FileInfo[] fileInfos;

    //public T[] GetObjectsFromFiles<T>()
    //{
    //    if (fileInfos == null)
    //        return null;

    //    T[] objects = new T[fileInfos.Length];
    //    for (int i = 0; i < fileInfos.Length; i++)
    //    {
    //        objects[i] = GetObjectFromFileIndex<T>(i);
    //    }
    //    return objects;
    //}

    //public T GetObjectFromFileIndex<T>(int index)
    //{
    //    if (fileInfos == null || index >= fileInfos.Length)
    //        return default;

    //    return GetObjectFromFileInfo<T>(fileInfos[index]);
    //}

    //public void OrderFiles<T>(Func<FileInfo, T> keySelector, bool isAscending)
    //{
    //    if (isAscending)
    //        fileInfos = fileInfos.OrderBy(keySelector).ToArray();
    //    else
    //        fileInfos = fileInfos.OrderByDescending(keySelector).ToArray();
    //}

    //public byte[] GetBytesFromFile(int index)
    //{
    //    return File.ReadAllBytes(fileInfos[index].FullName);
    //}

    public static int LoadFiles(ref FileInfo[] fileInfos, string path, string extension, SearchOption searchOption)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (dirInfo.Exists == false)
            return 0;

        fileInfos = dirInfo.GetFiles("*." + extension, searchOption);
        return fileInfos.Length;
    }

    public static void OpenFolder(string path)
    {
        Process.Start("explorer.exe", path);
        //Process.Start(path);
    }

    public static DirectoryInfo CreateDirectory(string path)
    {
        if (path == null || path == "")
            return null;

        if (Directory.Exists(path) == false)
            return Directory.CreateDirectory(path);
        else
            return new DirectoryInfo(path);
    }

    public static string GetFileName(FileInfo file)
    {
        return RemoveExtensionFromFileInfo(file);
    }

    public static string RemoveExtensionFromFileInfo(FileInfo fileInfo)
    {
        return fileInfo.Name.Remove(fileInfo.Name.Length - fileInfo.Extension.Length);
    }

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

    public void SaveReportToFile(StringBuilder reportSb, string path, string context)
    {
        string fileName = string.Concat("REPORT_", context, "_", DateTime.Now.ToString("yyyy-MM-dd_HH-mm"));
        string fullName = Path.Combine(path, fileName + ".txt");
        File.WriteAllText(fullName, reportSb.ToString());
    }

    public static T[] GetOjectsFromPath<T>(string path)
    {
        FileInfo[] fis = null;
        LoadFiles(ref fis, path, "json", SearchOption.TopDirectoryOnly);
        if (fis == null)
            return null;

        T[] objects = new T[fis.Length];
        for (int i = 0; i < fis.Length; i++)
        {
            objects[i] = GetObjectFromFileInfo<T>(fis[i]);
        }

        return objects;
    }

    public static T GetObjectFromFileInfo<T>(FileInfo fi)
    {
        string jsonData = File.ReadAllText(fi.FullName);

        if (jsonData == null || jsonData == "")
            return default;

        return JsonConvert.DeserializeObject<T>(jsonData);
    }

    public static T GetObjectFromPath<T>(string path)
    {
        FileInfo fi = new FileInfo(path);

        if (fi.Exists == false)
            return default;

        return GetObjectFromFileInfo<T>(fi);
    }

    public static void SaveToJson<T>(T obj, string fullName, bool prettyPrint = false)
    {
        string jsonStr = JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None);
        File.WriteAllText(fullName, jsonStr);
    }

    public void SaveToJson<T>(T obj, string path, string filename, bool prettyPrint = false)
    {
        string jsonStr = JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None);
        string fullName = Path.Combine(path, filename + ".json");
        File.WriteAllText(fullName, jsonStr);
    }

    public static string GetMd5(object value)
    {
        return GetMd5(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
    }

    public static string GetMd5(byte[] data)
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

    //TODO multiple images

    public static bool OpenImage(out string imagePath, string description = "", string directory = "./")
    {
        return OpenFile(out imagePath, "Images|*.jpg;*.png;*.jpeg", description, directory);
    }

    public static bool OpenFile(out string filePath, string filter = "", string description = "", string directory = "./")
    {
        var ofd = new OpenFileDialog
        {
            Filter = filter,
            InitialDirectory = directory
        };

        bool result = ofd.ShowDialog().GetValueOrDefault();

        filePath = result ? ofd.FileName : string.Empty;

        return result;
    }

    public static bool SelectFolder(out string folderPath, string description = "", string directory = "", bool showNewFolderButton = false)
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

    public static string GetDirectoryPath(string directory)
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

    public static bool HasWriteAccess(string directory)
    {
        var permission = new FileIOPermission(FileIOPermissionAccess.Write, directory);
        var permissionSet = new PermissionSet(PermissionState.None);
        permissionSet.AddPermission(permission);

        return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
    }

    public static bool IsVideo(string videoPath)
    {
        if (!File.Exists(videoPath))
            return false;

        FileInfo fileInfo = new FileInfo(videoPath);

        string[] videoExtensions = new string[] { "mp4", "avi", "mov" };

        for (int i = 0; i < videoExtensions.Length; i++)
        {
            if (fileInfo.Extension.ToLower().Contains(videoExtensions[i]))
                return true;
        }

        return false;
    }

    public static bool IsImage(string imagePath)
    {
        if (!File.Exists(imagePath))
            return false;

        System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath);
        ImageFormat rawFormat = image.RawFormat;
        bool result = false;

        if (ImageFormat.Jpeg.Equals(rawFormat))
        {
            result = true;
        }
        else if (ImageFormat.Png.Equals(rawFormat))
        {
            result = true;
        }
        else if (ImageFormat.Gif.Equals(rawFormat))
        {
            result = true;
        }
        else if (ImageFormat.Bmp.Equals(rawFormat))
        {
            result = true;
        }

        image.Dispose();
        return result;
    }

}
