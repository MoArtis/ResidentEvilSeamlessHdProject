using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BgTk
{
    public partial class BgToolkit
    {
        public IEnumerator RecreateTextures(string processedPath, string bgInfoPath, string alphaChannelPath,
            string resultsPath, DumpFormat dumpFormat, System.Action<ProgressInfo> progressCb, System.Action doneCb)
        {
            reportSb.Clear();
            reportSb.AppendLine("== Texture recreation report ==");
            reportSb.AppendLine("== Textures recreation Started! ==");
            reportSb.AppendLine("Selected Format: " + dumpFormat.name);

            progressCb(new ProgressInfo("Loading Bg Infos", 0, 0, 0f));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            taskTime = Time.unscaledTime;

            processedPath = Path.Combine(processedPath, game.ToString());
            alphaChannelPath = Path.Combine(alphaChannelPath, game.ToString());
            bgInfoPath = Path.Combine(bgInfoPath, game.ToString());

            resultsPath = Path.Combine(resultsPath, dumpFormat.name);
            fm.CreateDirectory(resultsPath);

            var bgInfosCount = fm.LoadFiles(bgInfoPath, "json");

            if (bgInfosCount <= 0)
            {
                reportSb.AppendLine("== Texture recreation aborted - No Bg Info ==");
                doneCb();
                yield break;
            }

            var bgInfos = fm.GetObjectsFromFiles<BgInfo>();

            var processedTexCount = fm.LoadFiles(processedPath, "png", SearchOption.AllDirectories);

            if (processedTexCount <= 0)
            {
                reportSb.AppendLine("== Texture recreation aborted - No Processed Texture ==");
                doneCb();
                yield break;
            }

            progressCb(new ProgressInfo(string.Concat("Recreating Textures - ", bgInfos[0].namePrefix), 1, bgInfosCount,
                0 / (float)(bgInfosCount - 1)));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            for (var i = 0; i < bgInfosCount; i++)
            {
                var bgInfo = bgInfos[i];

                //Get the right processed background
                var expectedBgName = string.Concat(bgInfo.namePrefix, ".png");
                var bgTexFileInfo = fm.fileInfos.FirstOrDefault(x => x.Name == expectedBgName);
                if (bgTexFileInfo == null)
                {
                    reportSb.AppendLine(string.Concat("Missing upscaled BG texture: ", bgInfo.namePrefix));
                    continue;
                }

                progressCb(new ProgressInfo(string.Concat("Recreating Textures - ", bgInfo.namePrefix), i + 1,
                    bgInfosCount, i / (float)(bgInfosCount - 1)));
                yield return new WaitForEndOfFrame();

                Texture2D processedTexAms = null;
                var processedTex = fm.GetTextureFromFileInfo(bgTexFileInfo);

                //float texRatioFloat = processedTex.width / (float)bgInfo.bgTexSize.x;
                var bgRatioFloat = processedTex.width / (float)bgInfo.bgTexSize.x;
                var maskRatioFloat = processedTex.width / (float)baseDumpFormat.maskUsageSize.x;

                if (bgRatioFloat - Mathf.Floor(bgRatioFloat) != 0f ||
                    maskRatioFloat - Mathf.Floor(maskRatioFloat) != 0f)
                {
                    reportSb.AppendLine(string.Concat(
                        "Error: This tool is not compatible with non integer scaling. Please fix this processed texture:",
                        processedTex.name));
                    Object.Destroy(processedTex);
                    continue;
                }

                var bgRatio = Mathf.RoundToInt(bgRatioFloat);
                var maskRatio = Mathf.RoundToInt(maskRatioFloat);

                CompensatePixelShift(dumpFormat.texPixelShift, processedTex, maskRatio);

                //Dynamic bg size only works for full background with no parts then... it's not good.
                //I really hope Resident evil 3 or something doesn't have backgrounds texture with different size AND splitted in some bullshit way.
                //If so I will need to analyze the background files during the matching phase and save all that data in the BG info instead.

                BgTexturePart[] bgParts;
                if (dumpFormat.bgParts == null || dumpFormat.bgParts.Length == 0)
                {
                    var fullBgPart = new BgTexturePart();
                    fullBgPart.size = new Vector2Int(bgInfo.bgTexSize.x, bgInfo.bgTexSize.y);
                    fullBgPart.patches = new[] {new Patch(0, 0, 0, 0, bgInfo.bgTexSize.x, bgInfo.bgTexSize.y)};
                    bgParts = new[] {fullBgPart};
                }
                else
                {
                    bgParts = dumpFormat.bgParts;
                }

                DumpMatch dumpMatch;
                if (bgInfo.texDumpMatches.Count(x => x.formatName == dumpFormat.name) <= 0)
                    dumpMatch = bgInfo.texDumpMatches.FirstOrDefault(
                        x => x.formatName == dumpFormat.alternateFormatName);
                else
                    dumpMatch = bgInfo.texDumpMatches.FirstOrDefault(x => x.formatName == dumpFormat.name);

                if (string.IsNullOrEmpty(dumpMatch.formatName))
                {
                    Object.Destroy(processedTex);
                    continue;
                }

                var matchGroupTexCount = bgParts.Length + (bgInfo.hasMask ? 1 : 0);
                //int matchGroupCount = dumpMatch.texNames.Length / matchGroupTexCount;

                //progressCb(new ProgressInfo(string.Concat(bgInfo.namePrefix, " - Recreating BG textures"), i + 1, bgInfosCount, i / (float)(bgInfosCount - 1)));
                //yield return new WaitForEndOfFrame();

                //Generate the Bg textures
                for (var j = 0; j < bgParts.Length; j++)
                {
                    var bgPartTex = new Texture2D(
                        Mathf.RoundToInt(bgParts[j].size.x * bgRatio),
                        Mathf.RoundToInt(bgParts[j].size.y * bgRatio),
                        TextureFormat.RGBA32, false);

                    bgPartTex.Fill(new Color32(0, 0, 0, 255));

                    for (var k = 0; k < bgParts[j].patches.Length; k++)
                    {
                        var p = bgParts[j].patches[k];
                        p.Scale(bgRatio);

                        var pColors = processedTex.GetPixels(
                            p.dstPos.x, p.dstPos.y,
                            p.size.x, p.size.y);

                        if (bgParts[j].needGapCompensation && k == 1)
                        {
                            bgPartTex.SetPixels(
                                p.srcPos.x, p.srcPos.y + 1,
                                p.size.x, p.size.y, pColors);
                        }
                        else
                        {
                            bgPartTex.SetPixels(
                                p.srcPos.x, p.srcPos.y,
                                p.size.x, p.size.y, pColors);
                        }
                    }

                    for (var k = 0; k < dumpMatch.partIndices.Length; k++)
                    {
                        if (dumpMatch.partIndices[k] != j) continue;

                        switch (dumpFormat.BgFormat)
                        {
                            case ImageFormat.Png:
                                fm.SaveTextureToPng(bgPartTex, resultsPath, dumpMatch.texNames[k]);
                                break;
                            case ImageFormat.Jpg:
                                fm.SaveTextureToJPG(bgPartTex, resultsPath, dumpMatch.texNames[k],
                                    dumpFormat.jpgQuality);
                                break;
                            case ImageFormat.DdsBc7:
                                fm.SaveTextureToDds(bgPartTex, resultsPath, dumpMatch.texNames[k]);
                                break;
                            case ImageFormat.DdsBc3:
                                fm.SaveTextureToDds(bgPartTex, resultsPath, dumpMatch.texNames[k]);
                                break;
                        }
                    }

                    Object.Destroy(bgPartTex);
                }

                //2. Generate the mask texture
                if (bgInfo.hasMask)
                {
                    //progressCb(new ProgressInfo(string.Concat(bgInfo.namePrefix, " - Recreating mask"), i+1, bgInfosCount, i / (float)(bgInfosCount-1)));
                    //yield return new WaitForEndOfFrame();

                    //One per group of masks
                    var alphaChannels = new Texture2D[bgInfo.groupsCount];
                    var hasMissingAlphaChannels = false;
                    for (var g = 0; g < bgInfo.groupsCount; g++)
                    {
                        alphaChannels[g] = fm.GetTextureFromPath(Path.Combine(alphaChannelPath,
                            string.Concat(bgInfo.namePrefix, "_", g)));

                        if (alphaChannels[g] != null) continue;

                        reportSb.AppendLine(string.Concat(bgInfo.namePrefix,
                            " is missing the alpha channel texture ", g, "."));

                        hasMissingAlphaChannels = true;
                    }

                    if (hasMissingAlphaChannels)
                    {
                        Object.Destroy(processedTex);
                        reportSb.AppendLine(string.Concat("Missing alpha channel textures: ", bgInfo.namePrefix));
                        continue;
                    }

                    if (bgInfo.useProcessedMaskTex)
                    {
                        //Object.Destroy(processedTex);
                        processedTexAms = fm.GetTextureFromPath(Path.Combine(processedPath,
                            string.Concat(bgInfo.namePrefix, altMaskSourceSuffix)));
                        if (processedTexAms == null)
                        {
                            processedTexAms = fm.GetTextureFromPath(Path.Combine(processedPath, "AMS",
                                string.Concat(bgInfo.namePrefix, altMaskSourceSuffix)));
                            if (processedTexAms == null)
                            {
                                reportSb.AppendLine(string.Concat("Missing special mask source textures: ",
                                    bgInfo.namePrefix));
                                continue;
                            }
                        }

                        CompensatePixelShift(dumpFormat.texPixelShift, processedTexAms, maskRatio);
                    }

                    //Reconstruct the mask texture itself based on processed BG and the smoothed alpha texture
                    var texWidth = Mathf.RoundToInt((dumpFormat.maskForcedSize.x != 0
                        ? dumpFormat.maskForcedSize.x
                        : bgInfo.maskTexSize.x) * maskRatio);

                    var texHeight = Mathf.RoundToInt((dumpFormat.maskForcedSize.x != 0
                        ? dumpFormat.maskForcedSize.y
                        : bgInfo.maskTexSize.y) * maskRatio);

                    var maskTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
                    maskTex.Fill(new Color32());

                    Texture2D alternateMaskTex = null;
                    if (dumpFormat.isMonochromaticMask && bgInfo.useProcessedMaskTex)
                    {
                        alternateMaskTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
                        alternateMaskTex.Fill(new Color32());
                    }


                    for (var j = 0; j < bgInfo.masks.Length; j++)
                    {
                        var mask = bgInfo.masks[j];
                        mask.patch.Scale(maskRatio);

                        Color[] pColors;
                        var isAltMaskPatch = false;

                        if (bgInfo.useProcessedMaskTex == false || mask.ignoreAltMaskSource)
                        {
                            if (dumpFormat.isMonochromaticMask)
                            {
                                pColors = new Color[mask.patch.size.x * mask.patch.size.y];
                                for (var k = 0; k < pColors.Length; k++)
                                {
                                    pColors[k] = Color.white;
                                }
                            }
                            else
                            {
                                pColors = processedTex.GetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y,
                                    mask.patch.size.x, mask.patch.size.y);
                            }
                        }
                        else
                        {
                            if (!dumpFormat.isMonochromaticMask)
                            {
                                pColors = processedTexAms.GetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y,
                                    mask.patch.size.x, mask.patch.size.y);
                            }
                            else
                            {
                                var amsHistogram = new Histogram(3, 16, 1f);
                                var origHistogram = new Histogram(3, 16, 1f);

                                pColors = processedTex.GetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y,
                                    mask.patch.size.x, mask.patch.size.y);

                                origHistogram.AddValues(pColors);

                                pColors = processedTexAms.GetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y,
                                    mask.patch.size.x, mask.patch.size.y);

                                amsHistogram.AddValues(pColors);

                                var histogramMatchValue = amsHistogram.Compare(origHistogram);
                                isAltMaskPatch = histogramMatchValue <
                                                 dumpFormat.monoMaskAmsHistogramMinMatchValue - Mathf.Epsilon;

                                Debug.Log($"{bgInfo.namePrefix}_mask_{j}_{histogramMatchValue:0.00000}");

                                if (!isAltMaskPatch)
                                {
                                    for (var k = 0; k < pColors.Length; k++)
                                    {
                                        pColors[k] = Color.white;
                                    }
                                }
                                else
                                {
                                    var amsTestTex = new Texture2D(mask.patch.size.x, mask.patch.size.y,
                                        TextureFormat.RGBA32, false);

                                    amsTestTex.SetPixels(pColors);
                                    fm.SaveTextureToPng(amsTestTex, resultsPath,
                                        $"{bgInfo.namePrefix}_mask_{j}_{histogramMatchValue}");
                                }
                            }
                        }

                        var aColors = alphaChannels[mask.groupIndex].GetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y,
                            mask.patch.size.x, mask.patch.size.y);

                        for (var k = 0; k < pColors.Length; k++)
                        {
                            pColors[k].a = aColors[k].a;
                        }

                        var srcPosY = mask.patch.srcPos.y + (maskTex.height - bgInfo.maskTexSize.y * maskRatio);

                        if (isAltMaskPatch)
                        {
                            alternateMaskTex.SetPixels(mask.patch.srcPos.x, srcPosY, mask.patch.size.x,
                                mask.patch.size.y,
                                pColors);
                        }
                        else
                        {
                            maskTex.SetPixels(mask.patch.srcPos.x, srcPosY, mask.patch.size.x, mask.patch.size.y,
                                pColors);
                        }
                    }

                    for (var j = 0; j < dumpMatch.partIndices.Length; j++)
                    {
                        if (dumpMatch.partIndices[j] == matchGroupTexCount - 1)
                        {
                            if (alternateMaskTex != null)
                            {
                                if (dumpFormat.MaskFormat == ImageFormat.Png)
                                    fm.SaveTextureToPng(alternateMaskTex, resultsPath, $"{dumpMatch.texNames[j]}_alt");
                                else
                                    fm.SaveTextureToDds(alternateMaskTex, resultsPath, $"{dumpMatch.texNames[j]}_alt");
                            }

                            if (dumpFormat.MaskFormat == ImageFormat.Png)
                                fm.SaveTextureToPng(maskTex, resultsPath, dumpMatch.texNames[j]);
                            else
                                fm.SaveTextureToDds(maskTex, resultsPath, dumpMatch.texNames[j]);
                        }
                    }

                    //Clean up the mess :) 
                    //(and yes don't reuse the texture object, since other games might need dynamic texture size 
                    //AND texture.resize IS a hidden constructor too)
                    Object.Destroy(maskTex);
                    for (var g = 0; g < bgInfo.groupsCount; g++)
                    {
                        Object.Destroy(alphaChannels[g]);
                    }
                }

                Object.Destroy(processedTex);
                Object.Destroy(processedTexAms);
            }

            yield return new WaitForEndOfFrame();

            reportSb.AppendLine(string.Format("== Textures recreation Done! ({0} seconds) ==",
                (Time.unscaledTime - taskTime).ToString("#.0")));

            fm.OpenFolder(resultsPath);

            doneCb();
        }
    }
}