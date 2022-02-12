using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BgTk;
using System.Linq;

public class BgToolkitManager : MonoBehaviour
{

    public enum State
    {
        None,
        MainMenu,
        GenerateBgInfos,
        GenerateAlphaChannels,
        RecreateTextures,
        MatchTextures,
        GenerateMaskSources
    }

    protected BgToolkit toolkit;
    protected FileManager fm;

    [SerializeField] protected string maskSuffix;
    [SerializeField] protected string altMaskSourceSuffix;

    [SerializeField] protected string dumpTexturesPath;
    [SerializeField] protected string rdtFilesPath;
    [SerializeField] protected string bgInfoPath;
    [SerializeField] protected string dumpFormatsPath;
    [SerializeField] protected string alphaChannelPath;
    [SerializeField] protected string processedPath;
    [SerializeField] protected string resultsPath;
    [SerializeField] protected string reportsPath;

    protected State currentState;

    [SerializeField] protected ProgressBar progressBar;
    [SerializeField] protected MainMenu mainMenu;

    [SerializeField] protected bool prettifyJsonOnSave;

    protected int matchTexDumpFormatIndex;
    protected int recreateTexDumpFormatIndex;
    protected DumpFormat[] dumpFormats;

    [SerializeField] protected AlphaChannelConfig alphaConfig;

    [SerializeField] protected TextureMatchingConfig texMatchingConfig;

    private void Awake()
    {
        fm = new FileManager();
    }

    private void Start()
    {
        fm.CreateDirectory(dumpTexturesPath);
        fm.CreateDirectory(rdtFilesPath);
        fm.CreateDirectory(bgInfoPath);
        fm.CreateDirectory(dumpFormatsPath);
        fm.CreateDirectory(alphaChannelPath);
        fm.CreateDirectory(processedPath);
        fm.CreateDirectory(resultsPath);
        fm.CreateDirectory(reportsPath);

        ChangeState(State.MainMenu);

        //int bgInfoCount = fm.LoadFiles(System.IO.Path.Combine(bgInfoPath, "RE3"), "json");
        //BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();

        //System.Text.StringBuilder sb = new System.Text.StringBuilder();

        //for (int i = 0; i < bgInfoCount; i++)
        //{
        //    sb.AppendLine(string.Format("- /RE3_bgs/{0}", bgInfos[i].namePrefix));
        //}

        //System.IO.File.WriteAllText("./RE3_BGs.txt", sb.ToString());

        //Trash code to generate RE3 OSD Room ID
        //int bgInfoCount = fm.LoadFiles(System.IO.Path.Combine(bgInfoPath, "RE3"), "json");
        //BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();
        //System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //System.Text.StringBuilder sbRoomIds = new System.Text.StringBuilder();

        //for (int i = 0; i < bgInfoCount; i++)
        //{
        //    if (bgInfos[i].texDumpMatches.Length <= 1)
        //        continue;

        //    sbRoomIds.Clear();

        //    for (int j = 0; j < bgInfos[i].texDumpMatches[0].partIndices.Length; j++)
        //    {
        //        if(bgInfos[i].texDumpMatches[0].partIndices[j] == 0)
        //        {
        //            sbRoomIds.Append(bgInfos[i].texDumpMatches[0].texNames[j]);
        //            sbRoomIds.Append(" ");
        //        }
        //    }

        //    for (int j = 0; j < bgInfos[i].texDumpMatches[1].partIndices.Length; j++)
        //    {

        //        if (bgInfos[i].texDumpMatches[1].partIndices[j] == 0)
        //        {
        //            sb.AppendLine(string.Format("reThreeRoomOsdMap[\"{0}\"] = \"{1}\";", bgInfos[i].texDumpMatches[1].texNames[j], sbRoomIds.ToString()));
        //        }

        //    }
        //}
        //System.IO.File.WriteAllText("./RE3_OSD_CODE.txt", sb.ToString());

        //Trash code to find a stupid missing background
        //int processedBgCount = fm.LoadFiles(System.IO.Path.Combine(processedPath, "RE2"), "png");
        //string[] bgNames = new string[processedBgCount];
        //for (int i = 0; i < processedBgCount; i++)
        //{
        //    bgNames[i] = fm.RemoveExtensionFromFileInfo(fm.fileInfos[i]);
        //}

        //int bgInfoCount = fm.LoadFiles(System.IO.Path.Combine(bgInfoPath, "RE2"), "json");
        //BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();
        //for (int i = 0; i < bgNames.Length; i++)
        //{
        //    bool found = false;
        //    for (int j = 0; j < bgInfoCount; j++)
        //    {
        //        if (bgInfos[j].namePrefix == bgNames[i])
        //        {
        //            if (bgInfos[j].texDumpMatches[1].texNames.Length >= 2)
        //            {
        //                found = true;
        //                break;
        //            }
        //        }
        //    }
        //    if (found == false)
        //    {
        //        Debug.LogError(bgNames[i] + " is pure trash...");
        //    }
        //}

        //Trash code to reverse determine the PC names of the upscaled GC named backgrounds on RE3
        //int bgInfoCount = fm.LoadFiles(System.IO.Path.Combine(bgInfoPath, "RE3"), "json");
        //BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();

        //int bgCount = fm.LoadFiles(System.IO.Path.Combine(processedPath, "RE3"), "png", System.IO.SearchOption.AllDirectories);

        //for (int i = 0; i < bgCount; i++)
        //{
        //    string bgName = fm.RemoveExtensionFromFileInfo(fm.fileInfos[i]);
        //    bool isFound = false;

        //    for (int j = 0; j < bgInfoCount; j++)
        //    {
        //        BgInfo bgInfo = bgInfos[j];
        //        for (int k = 0; k < bgInfo.texDumpMatches.Length; k++)
        //        {
        //            if (bgInfo.texDumpMatches[k].formatName == "RE3 GameCube")
        //            {
        //                for (int l = 0; l < bgInfo.texDumpMatches[k].texNames.Length; l++)
        //                {
        //                    if (bgInfo.texDumpMatches[k].texNames[l] == bgName)
        //                    {
        //                        string texName = bgInfo.namePrefix + ".png";
        //                        string newFullName = System.IO.Path.Combine(fm.fileInfos[i].DirectoryName, texName);
        //                        Debug.Log(fm.fileInfos[i].FullName + " || " + newFullName);
        //                        System.IO.File.Move(fm.fileInfos[i].FullName, newFullName);
        //                        isFound = true;
        //                        break;
        //                    }
        //                }
        //            }
        //            if (isFound)
        //                break;
        //        }
        //        if (isFound)
        //            break;
        //    }
        //}

        //Trash code for dealing with the mixed Special masks (the ones mixing Alt mask source and the BG for reconstructing the mask) 
        //int bgInfoCount = fm.LoadFiles(bgInfoPath, "json");
        //BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();
        //for (int i = 0; i < bgInfoCount; i++)
        //{
        //    BgInfo bgInfo = bgInfos[i];

        //    if (bgInfo.hasMask == false || bgInfo.useProcessedMaskTex == false)
        //        continue;

        //    int[] indices = { };

        //    for (int j = 0; j < bgInfo.masks.Length; j++)
        //    {
        //        if (indices.Contains(bgInfo.masks[j].groupIndex))
        //        {
        //            bgInfo.masks[j].ignoreAltMaskSource = false;
        //        }
        //    }

        //    fm.SaveToJson(bgInfo, bgInfoPath, bgInfo.GetFileName(), prettifyJsonOnSave);
        //}

        //Trash code to convert the old opaque indices of mask into the new efficient one - Don't uncomment that, it will fuck everything up in your bginfo folder, potentially
        //int bgInfoCount = fm.LoadFiles(bgInfoPath, "json");
        //BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();
        //for (int i = 0; i < bgInfoCount; i++)
        //{
        //    BgInfo bgInfo = bgInfos[i];
        //    List<int> opaqueIndices = new List<int>();

        //    for (int j = 0; j < bgInfo.masks.Length; j++)
        //    {
        //        Mask mask = bgInfo.masks[j];

        //        opaqueIndices.Clear();

        //        int blockLength = 1;
        //        for (int k = 0; k < mask.opaqueIndices.Length; k++)
        //        {
        //            if (k < mask.opaqueIndices.Length - 1 && mask.opaqueIndices[k] == mask.opaqueIndices[k + 1] - 1)
        //            {
        //                blockLength++;
        //            }
        //            else
        //            {
        //                if (blockLength > 1)
        //                    opaqueIndices.Add(mask.opaqueIndices[k - blockLength + 1]);

        //                if (blockLength > 2)
        //                    opaqueIndices.Add(-1);

        //                opaqueIndices.Add(mask.opaqueIndices[k]);

        //                blockLength = 1;
        //            }
        //        }

        //        mask.opaqueIndices = opaqueIndices.ToArray();

        //        bgInfo.masks[j] = mask;
        //    }

        //    fm.SaveToJson(bgInfo, bgInfoPath, bgInfo.GetFileName(), prettifyJsonOnSave);
        //}

        //Trash code to check some matching bugs
        //System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //fm.LoadFiles(bgInfoPath, "json");
        //BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();
        //for (int i = 0; i < bgInfos.Length; i++)
        //{
        //    if (bgInfos[i].texDumpMatches == null || bgInfos[i].texDumpMatches.Length == 0)
        //    {
        //        sb.AppendLine(string.Concat(bgInfos[i].namePrefix, " has no tex Dump match at all! wtf..."));
        //    }
        //    else if (bgInfos[i].texDumpMatches.Length == 1)
        //    {
        //        //sb.AppendLine(string.Concat(bgInfos[i].namePrefix, ": No GC names"));
        //    }
        //    else if (bgInfos[i].texDumpMatches.Length == 2)
        //    {
        //        if ((bgInfos[i].texDumpMatches[1].texNames.Length > 3 && bgInfos[i].hasMask) ||
        //            (bgInfos[i].texDumpMatches[1].texNames.Length > 2 && bgInfos[i].hasMask == false))
        //        {
        //            sb.AppendLine(string.Concat(bgInfos[i].namePrefix, ": Too many GC names"));
        //        }
        //        else if ((bgInfos[i].texDumpMatches[1].texNames.Length < 3 && bgInfos[i].hasMask) ||
        //            (bgInfos[i].texDumpMatches[1].texNames.Length < 2 && bgInfos[i].hasMask == false))
        //        {
        //            //sb.AppendLine(string.Concat(bgInfos[i].namePrefix, ": Not enough GC names"));
        //        }
        //    }
        //}
        //Debug.LogWarning(sb.ToString());
    }

    public void ChangeGame(int gameIndex)
    {
        Game currentGame = (Game)gameIndex;
        toolkit = new BgToolkit(currentGame, GetBaseDumpFormat(currentGame), maskSuffix, altMaskSourceSuffix, prettifyJsonOnSave);

        Debug.Log("Game changed to " + currentGame);
    }

    public DumpFormat GetBaseDumpFormat(Game game)
    {
        string baseDumpFormatName = "";
        switch (game)
        {
            case Game.RE1:
                baseDumpFormatName = "RE1 Classic Rebirth";
                break;
            
            case Game.RE2:
                baseDumpFormatName = "RE2 Classic Rebirth";
                break;

            case Game.RE3:
                baseDumpFormatName = "RE3 SourceNext";
                break;
        }

        DumpFormat baseDumpFormat = new DumpFormat();

        for (int i = 0; i < dumpFormats.Length; i++)
        {
            if (dumpFormats[i].name == baseDumpFormatName)
            {
                baseDumpFormat = dumpFormats[i];
                break;
            }
        }

        if (string.IsNullOrEmpty(baseDumpFormat.name))
        {
            //ChangeState(State.Error);
            //errorMessage.ChangeMessage("The dump format named " + baseDumpFormatName + " is missing. Check the Dump Format folder.");
            Debug.LogError("The dump format named " + baseDumpFormatName + " is missing. Check the Dump Formats folder.");
            ChangeState(State.None);
        }

        return baseDumpFormat;
    }

    public void GenerateSpecialMaskSources()
    {
        ChangeState(State.GenerateMaskSources);
        // StartCoroutine(toolkit.AddTeamxNamingConvention(bgInfoPath, ProgressCallback, DoneCallback));
        StartCoroutine(toolkit.GenerateMaskSource(bgInfoPath, dumpTexturesPath, ProgressCallback, DoneCallback));
    }

    private void UpdateDumpFormats()
    {
        fm.LoadFiles(dumpFormatsPath, "json");

        if (dumpFormats == null || dumpFormats.Length != fm.fileInfos.Length)
            recreateTexDumpFormatIndex = 0;

        dumpFormats = fm.GetObjectsFromFiles<DumpFormat>();

        mainMenu.UpdateRecreateTexturesFormatOptions(dumpFormats, recreateTexDumpFormatIndex);
        mainMenu.UpdateMatchTexturesFormatOptions(dumpFormats, matchTexDumpFormatIndex);
    }

    public void ChangeMatchTexturesDumpFormatIndex(int index)
    {
        matchTexDumpFormatIndex = index;
    }

    public void ChangeRecreateTexturesDumpFormatIndex(int index)
    {
        recreateTexDumpFormatIndex = index;
    }

    public void MatchTextures()
    {
        ChangeState(State.MatchTextures);
        DumpFormat dumpFormat = dumpFormats[matchTexDumpFormatIndex];
        StartCoroutine(toolkit.MatchTextures(
            bgInfoPath, dumpTexturesPath,       //Paths
            dumpFormat, texMatchingConfig,      //Dump Format and config
            ProgressCallback, DoneCallback));   //and callbacks
    }

    public void RecreateTextures()
    {
        ChangeState(State.RecreateTextures);
        DumpFormat dumpFormat = dumpFormats[recreateTexDumpFormatIndex];
        StartCoroutine(toolkit.RecreateTextures(
            processedPath, bgInfoPath, alphaChannelPath, resultsPath,   //Paths
            dumpFormat, ProgressCallback, DoneCallback));               //Config structs and callbacks
    }

    public void GenerateAlphaChannel()
    {
        ChangeState(State.GenerateAlphaChannels);
        StartCoroutine(toolkit.GenerateAlphaChannel(
            bgInfoPath, alphaChannelPath,       //Paths
            alphaConfig,                        //Config
            ProgressCallback, DoneCallback));   //Callbacks
    }

    public void GenerateBgInfos()
    {
        ChangeState(State.GenerateBgInfos);
        StartCoroutine(toolkit.GenerateBgInfos(rdtFilesPath, dumpTexturesPath, bgInfoPath, ProgressCallback, DoneCallback));
    }

    public void DoneCallback()
    {
        Debug.Log(toolkit.reportSb.ToString());

        if (toolkit.reportSb.Length > 0)
            fm.SaveReportToFile(toolkit.reportSb, reportsPath, currentState.ToString());

        ChangeState(State.MainMenu);
    }

    public void ProgressCallback(ProgressInfo pInfo)
    {
        progressBar.ChangeCount("", pInfo.currentIndex, pInfo.totalIndex);
        progressBar.ChangeText(pInfo.label);
        progressBar.ChangeValue(pInfo.progress);
    }

    protected void LockFrameRate()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }

    protected void UnlockFrameRate()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 0;
    }

    private void ChangeState(State nState)
    {
        if (nState == currentState)
            return;

        switch (currentState)
        {
            case State.MainMenu:
                mainMenu.Hide();
                break;

            case State.GenerateBgInfos:
            case State.RecreateTextures:
            case State.MatchTextures:
            case State.GenerateAlphaChannels:
            case State.GenerateMaskSources:
                UnlockFrameRate();
                progressBar.Hide();
                break;
        }

        currentState = nState;

        switch (currentState)
        {
            case State.MainMenu:
                UpdateDumpFormats();
                LockFrameRate();
                mainMenu.Show();
                break;

            case State.GenerateBgInfos:
            case State.RecreateTextures:
            case State.MatchTextures:
            case State.GenerateAlphaChannels:
            case State.GenerateMaskSources:
                progressBar.Show();
                break;
        }
    }
}
