using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NumberGenerator : MonoBehaviour
{
    int frameCount = 0;

    public Camera mainCamera;
    public RenderTexture renderTexture;
    public Text text;

    private Texture2D texture2D;

    FileManager fm = new FileManager();

    public string path = "";
    public string pathIntro = "";

    int introFrameCount;

    string[] introFrameNames;

    private void Start()
    {
        frameCount = 0;
        texture2D = new Texture2D(mainCamera.pixelWidth, mainCamera.pixelHeight, TextureFormat.RGBA32, false);

        fm.CreateDirectory(path);

        introFrameCount = fm.LoadFiles(pathIntro, "png", System.IO.SearchOption.TopDirectoryOnly);
        fm.fileInfos = fm.fileInfos.OrderBy(x => x.CreationTime).ToArray();
        introFrameNames = new string[introFrameCount];

        for (int i = 0; i < introFrameCount; i++)
        {
            introFrameNames[i] = fm.RemoveExtensionFromFileInfo(fm.fileInfos[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (frameCount >= introFrameCount)
            return;

        if (Time.frameCount < 10)
            return;

        text.text = frameCount.ToString();
    }

    private void OnPostRender()
    {
        if (frameCount >= introFrameCount)
            return;

        if (Time.frameCount < 10)
            return;

        texture2D.ReadPixels(mainCamera.pixelRect, 0, 0);
        texture2D.Apply();

        fm.SaveTextureToPng(texture2D, path, introFrameNames[frameCount]);

        frameCount++;

        if (frameCount >= introFrameCount)
            Debug.LogWarning("DONE");
    }
}
