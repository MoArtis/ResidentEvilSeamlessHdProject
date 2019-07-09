using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using xBRZNet;

namespace BgTk
{
    public class BgToolkit
    {
        //TODO - A Config struct that can be configured / stored from another class / object
        public BgToolkit(Game game, DumpFormat baseDumpFormat, string maskSuffix, string altMaskSourceSuffix, bool prettifyJsonOnSave)
        {
            this.game = game;
            this.baseDumpFormat = baseDumpFormat;
            this.prettifyJsonOnSave = prettifyJsonOnSave;
            this.maskSuffix = maskSuffix;
            this.altMaskSourceSuffix = altMaskSourceSuffix;

        }

        protected Game game;
        protected DumpFormat baseDumpFormat;
        protected bool prettifyJsonOnSave;
        protected string maskSuffix;
        protected string altMaskSourceSuffix;

        protected FileManager fm = new FileManager();

        protected float taskTime;

        public StringBuilder reportSb = new StringBuilder();

        public IEnumerator GenerateMaskSource(string bgInfoPath, string dumpTexPath, System.Action<ProgressInfo> progressCb, System.Action doneCb)
        {
            reportSb.Clear();
            reportSb.AppendLine("== Mask Source Generation report ==");
            reportSb.AppendLine("== Mask Source Generation Started! ==");

            taskTime = Time.unscaledTime;

            progressCb(new ProgressInfo("Loading BgInfos", 0, 0, 0f));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Load all the bg infos for later
            int bgInfoCount = fm.LoadFiles(Path.Combine(bgInfoPath, game.ToString()), "json");
            BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();

            dumpTexPath = Path.Combine(dumpTexPath, baseDumpFormat.name);

            for (int i = 0; i < bgInfoCount; i++)
            {
                if (bgInfos[i].hasMask == false || bgInfos[i].useProcessedMaskTex == false)
                    continue;

                BgInfo bgInfo = bgInfos[i];

                int scaleRatio = bgInfo.bgTexSize.y / baseDumpFormat.maskUsageSize.y;

                progressCb(new ProgressInfo("Generating special Mask source", i + 1, bgInfoCount, i / (float)(bgInfoCount - 1)));
                yield return new WaitForEndOfFrame();

                Texture2D bgTex = fm.GetTextureFromPath(Path.Combine(dumpTexPath, bgInfo.namePrefix));
                Texture2D srcMaskTex = fm.GetTextureFromPath(Path.Combine(dumpTexPath, string.Concat(bgInfo.namePrefix, maskSuffix)));
                Texture2D maskTex = ScaleTexture(srcMaskTex, scaleRatio);
                //fm.SaveTextureToPng(maskTex, dumpTexPath, srcMaskTex.name + "_us");

                if (bgTex == null)
                {
                    reportSb.AppendLine(string.Concat(bgInfo.namePrefix, " is missing its BG texture in the dump folder (", dumpTexPath, ")"));
                    continue;
                }

                if (maskTex == null)
                {
                    reportSb.AppendLine(string.Concat(bgInfo.namePrefix, " is missing its Mask texture in the dump folder (", dumpTexPath, ")"));
                    Object.Destroy(bgTex);
                    continue;
                }

                //Reversing the mask order, it solves some cases where mask groups are multilayered (Only ROOM_109_02 and 103_00)
                if (bgInfo.isReversedMaskOrder)
                    bgInfo.masks = bgInfo.masks.Reverse().ToArray();

                //Read all the mask patches from the mask texture and apply them to the bgTex, then save it into a new special texture.
                for (int j = 0; j < bgInfo.masks.Length; j++)
                {
                    Mask mask = bgInfo.masks[j];

                    Color[] maskColors = maskTex.GetPixels(mask.patch.srcPos.x * scaleRatio, mask.patch.srcPos.y * scaleRatio, mask.patch.size.x * scaleRatio, mask.patch.size.y * scaleRatio);

                    Color[] bgColors = bgTex.GetPixels(mask.patch.dstPos.x * scaleRatio, mask.patch.dstPos.y * scaleRatio, mask.patch.size.x * scaleRatio, mask.patch.size.y * scaleRatio);

                    for (int k = 0; k < maskColors.Length; k++)
                    {
                        //if opaque // no semi transparency at this stage
                        if (maskColors[k].a < 0.5f)
                        {
                            maskColors[k] = bgColors[k];
                        }
                    }

                    bgTex.SetPixels(mask.patch.dstPos.x * scaleRatio, mask.patch.dstPos.y * scaleRatio, mask.patch.size.x * scaleRatio, mask.patch.size.y * scaleRatio, maskColors);
                }

                //Todo - store the special mask suffix as a variable
                fm.SaveTextureToPng(bgTex, dumpTexPath, string.Concat(bgInfo.namePrefix, altMaskSourceSuffix));

                Object.Destroy(maskTex);
                Object.Destroy(bgTex);
                Object.Destroy(srcMaskTex);
            }

            reportSb.AppendLine(string.Format("== Mask source generation done! ({0} seconds) ==", (Time.unscaledTime - taskTime).ToString("#.0")));

            fm.OpenFolder(dumpTexPath);

            doneCb();
        }

        public IEnumerator GenerateAlphaChannel(string bgInfoPath, string alphaChannelPath, AlphaChannelConfig config, System.Action<ProgressInfo> progressCb, System.Action doneCb)
        {
            reportSb.Clear();
            reportSb.AppendLine("== Alpha channel generation report ==");
            reportSb.AppendLine("== Alpha channel generation Started! ==");

            progressCb(new ProgressInfo("Loading Bg Infos", 0, 0, 0f));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            taskTime = Time.unscaledTime;

            xBRZScaler xBrzScaler = new xBRZScaler();

            byte boostValue = (byte)(config.boostValue * 255f);
            byte clipValue = (byte)(config.clipValue * 255f);

            alphaChannelPath = Path.Combine(alphaChannelPath, game.ToString());
            fm.CreateDirectory(alphaChannelPath);

            bgInfoPath = Path.Combine(bgInfoPath, game.ToString());
            int bgInfosCount = fm.LoadFiles(bgInfoPath, "json");
            if (bgInfosCount <= 0)
            {
                reportSb.AppendLine("== Alpha channel generation aborted! - No Bg Info ==");
                doneCb();
            }

            for (int i = 0; i < bgInfosCount; i++)
            {
                BgInfo bgInfo = fm.GetObjectFromFileIndex<BgInfo>(i);

                if (bgInfo.hasMask == false)
                {
                    if (i % 100 == 0)
                    {
                        progressCb(new ProgressInfo(bgInfo.namePrefix, i + 1, bgInfosCount, i / (float)bgInfosCount));
                        yield return new WaitForEndOfFrame();
                    }
                }
                else
                {
                    progressCb(new ProgressInfo(bgInfo.namePrefix, i + 1, bgInfosCount, i / (float)bgInfosCount));
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();

                    for (int g = 0; g < bgInfo.groupsCount; g++)
                    {
                        string alphaChannelName = string.Concat(bgInfo.namePrefix, "_", g);

                        Texture2D alphaTex = new Texture2D(
                            //bgInfo.bgTexSize.x,
                            //bgInfo.bgTexSize.y,
                            baseDumpFormat.maskUsageSize.x,
                            baseDumpFormat.maskUsageSize.y,
                            TextureFormat.RGBA32, false);
                        alphaTex.wrapMode = TextureWrapMode.Clamp;

                        Texture2D smoothAlphaTex = new Texture2D(
                            //bgInfo.bgTexSize.x * config.scaleRatio,
                            //bgInfo.bgTexSize.y * config.scaleRatio,
                            baseDumpFormat.maskUsageSize.x * config.scaleRatio,
                            baseDumpFormat.maskUsageSize.y * config.scaleRatio,
                            TextureFormat.RGBA32, false);

                        Color opaqueColor = Color.blue;
                        Color transparentColor = new Color();

                        alphaTex.Fill(transparentColor);

                        for (int j = 0; j < bgInfo.masks.Length; j++)
                        {
                            Mask mask = bgInfo.masks[j];

                            if (mask.groupIndex != g)
                                continue;

                            Color[] maskColors = alphaTex.GetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y, mask.patch.size.x, mask.patch.size.y);
                            for (int k = 0; k < mask.opaqueIndices.Length; k++)
                            {
                                int index = mask.opaqueIndices[k];
                                if (index == -1)
                                {
                                    int firstIndex = mask.opaqueIndices[k - 1] + 1;
                                    int lastIndex = mask.opaqueIndices[k + 1];
                                    for (int l = 0; l < lastIndex - firstIndex + 1; l++)
                                    {
                                        maskColors[firstIndex + l] = opaqueColor;
                                    }
                                }
                                else
                                {
                                    maskColors[index] = opaqueColor;
                                }
                            }
                            alphaTex.SetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y, mask.patch.size.x, mask.patch.size.y, maskColors);
                        }

                        if (config.saveDebugTextures)
                            fm.SaveTextureToPng(alphaTex, alphaChannelPath, string.Concat(alphaChannelName, "_DEBUG_1_Original"));

                        if (config.scalingType == ScalingType.xBRZ && config.scaleRatio > 1 && config.scaleRatio <= 5)
                        {
                            Color32[] xBrzColors = xBrzScaler.ScaleImage(alphaTex, config.scaleRatio);
                            for (int j = 0; j < xBrzColors.Length; j++)
                            {
                                xBrzColors[j].a = xBrzColors[j].b;
                            }
                            smoothAlphaTex.SetPixels32(xBrzColors);
                        }
                        else
                        {
                            if (config.scaleRatio <= 1)
                            {
                                smoothAlphaTex.SetPixels32(alphaTex.GetPixels32());
                            }
                            else
                            {
                                alphaTex.filterMode = FilterMode.Bilinear;

                                //Bilinear filtering trick
                                for (int y = 0; y < smoothAlphaTex.height; y++)
                                {
                                    for (int x = 0; x < smoothAlphaTex.width; x++)
                                    {
                                        float xFrac = (x + 0.5f) / (smoothAlphaTex.width - 1);
                                        float yFrac = (y + 0.5f) / (smoothAlphaTex.height - 1);

                                        Color c;
                                        if (config.scalingType == ScalingType.Bilinear)
                                        {
                                            c = alphaTex.GetPixelBilinear(xFrac, yFrac);
                                        }
                                        else
                                        {
                                            c = alphaTex.GetPixel(Mathf.FloorToInt(x / (float)config.scaleRatio), Mathf.FloorToInt(y / (float)config.scaleRatio));
                                        }

                                        smoothAlphaTex.SetPixel(x, y, c);
                                    }
                                }
                            }
                        }

                        if (config.saveDebugTextures)
                            fm.SaveTextureToPng(smoothAlphaTex, alphaChannelPath, string.Concat(alphaChannelName, "_DEBUG_2_Scaled"));

                        if (config.hasBlur)
                        {
                            smoothAlphaTex.Blur(config.blurRadius, config.blurIteration);

                            if (config.saveDebugTextures)
                                fm.SaveTextureToPng(smoothAlphaTex, alphaChannelPath, string.Concat(alphaChannelName, "_DEBUG_3_Scaled+Blurred"));
                        }


                        //Alpha clipping and boost for sharper edges
                        if (clipValue > byte.MinValue || boostValue > byte.MinValue)
                        {
                            Color32[] colors = smoothAlphaTex.GetPixels32();
                            for (int j = 0; j < colors.Length; j++)
                            {
                                Color32 c = colors[j];

                                c.a = c.a <= clipValue ? byte.MinValue : c.a;

                                if (c.a > byte.MinValue)
                                {
                                    c.a = (byte)Mathf.Clamp(c.a + boostValue, c.a, 255);
                                }

                                colors[j] = c;
                            }
                            smoothAlphaTex.SetPixels32(colors);

                            if (config.saveDebugTextures)
                                fm.SaveTextureToPng(smoothAlphaTex, alphaChannelPath, string.Concat(alphaChannelName, "_DEBUG_4_Scaled+Blurred+ClipBoost"));
                        }

                        fm.SaveTextureToPng(smoothAlphaTex, alphaChannelPath, alphaChannelName);

                        Object.Destroy(alphaTex);
                        Object.Destroy(smoothAlphaTex);
                    }
                }
            }

            yield return new WaitForEndOfFrame();

            reportSb.AppendLine(string.Format("== Alpha channel generation Done! ({0} seconds) ==", (Time.unscaledTime - taskTime).ToString("#.0")));

            fm.OpenFolder(alphaChannelPath);

            doneCb();

        }

        public IEnumerator MatchTextures(string bgInfoPath, string dumpTexPath, DumpFormat dumpFormat, TextureMatchingConfig config, System.Action<ProgressInfo> progressCb, System.Action doneCb)
        {
            if (dumpFormat.Equals(baseDumpFormat))
            {
                Debug.LogWarning(string.Format("The current base texture format ({0}) is the same as the selected texture format for matching ({1}). This is useless.", baseDumpFormat.name, dumpFormat.name));
                doneCb();
                yield break;
            }

            reportSb.Clear();
            reportSb.AppendLine("== Texture Matching report ==");
            reportSb.AppendLine("== Texture Matching Started! ==");
            reportSb.AppendLine("Selected Format: " + dumpFormat.name);

            taskTime = Time.unscaledTime;

            progressCb(new ProgressInfo("Loading textures and BgInfos", 0, 0, 0f));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Load all the bg infos for later
            bgInfoPath = Path.Combine(bgInfoPath, game.ToString());
            int bgInfoCount = fm.LoadFiles(bgInfoPath, "json");
            BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();

            //Load all the file info of the textures to match
            int mcTexCount = fm.LoadFiles(Path.Combine(dumpTexPath, dumpFormat.name), "png", SearchOption.AllDirectories);

            List<MatchCandidate> candidatesList = new List<MatchCandidate>();

            int texDuplicatesCount = 0;
            int unmatchedTexCount = 0;
            int unmatchedMcCount = 0;

            //1. Prepare the MatchCandidates
            for (int i = 0; i < mcTexCount; i++)
            {
                if (i % 50 == 0)
                {
                    progressCb(new ProgressInfo("Preparing candidates", i + 1, mcTexCount, i / (float)(mcTexCount - 1)));
                    yield return new WaitForEndOfFrame();
                }

                Texture2D mcTex = fm.GetTextureFromFileIndex(i);

                MatchCandidate mc = new MatchCandidate();
                mc.md5 = fm.GetMd5(mcTex.GetRawTextureData());
                mc.fileInfoIndices = new int[] { i };
                mc.texSize.x = mcTex.width;
                mc.texSize.y = mcTex.height;
                mc.bgInfoMatchIndex = new List<int>();
                mc.bgInfoMatchValue = new List<float>();

                //Check for duplicates
                bool isDuplicate = false;
                for (int j = 0; j < candidatesList.Count; j++)
                {
                    if (candidatesList[j].md5 == mc.md5)
                    {
                        reportSb.AppendLine(string.Concat(mcTex.name, " is a duplicate of ", fm.fileInfos[candidatesList[j].fileInfoIndices[0]].Name));
                        texDuplicatesCount++;

                        //int[] indices = new int[candidatesList[j].fileInfoIndices.Length + 1];
                        //for (int k = 0; k < candidatesList[j].fileInfoIndices.Length; k++)
                        //{
                        //    indices[k] = candidatesList[j].fileInfoIndices[k];
                        //}
                        //indices[indices.Length - 1] = j;
                        int[] indices = candidatesList[j].fileInfoIndices.Concat(new int[] { i }).ToArray();

                        MatchCandidate duplicatedMc = candidatesList[j];
                        duplicatedMc.SetFileInfoIndices(indices);
                        candidatesList[j] = duplicatedMc;

                        isDuplicate = true;

                        //It can't have another duplicate, impossible since I checked for every new element.
                        break;
                    }
                }

                if (isDuplicate)
                    continue;

                //Identify the candidate
                if (mcTex.GetPixel(0, 0).a < 0.01f || mcTex.GetPixel(mcTex.width - 1, 0).a < 0.01f)
                {
                    if (dumpFormat.bgParts.Length == 0)
                        mc.bgPartIndex = 1;
                    else
                        mc.bgPartIndex = dumpFormat.bgParts.Length;

                    mc.isMask = true;
                }
                else
                {
                    //TODO - What if there is no part? (Full bg only)
                    //TODO - What if different bg parts have the same size? For now, too bad... :)
                    if (dumpFormat.bgParts.Length == 0)
                    {
                        //Well we already know it's not a mask so we might be able to assume that it's a BG texture, right? ^^'
                        mc.bgPartIndex = -1;
                    }
                    else
                    {
                        for (int j = 0; j < dumpFormat.bgParts.Length; j++)
                        {
                            if (mcTex.width == dumpFormat.bgParts[j].size.x && mcTex.height == dumpFormat.bgParts[j].size.y)
                            {
                                mc.bgPartIndex = j;
                                break;
                            }
                        }
                    }
                }

                //RE3 WARNING - That doesn't WORK AT ALL ON RE3 GAMECUBE
                //Create a special patch for mask texture, thus It will only pick colors into the non fully transparent area.
                Patch partPatch = new Patch();
                if (mc.isMask)
                {
                    if (config.inconsistentMaskSize)
                    {
                        partPatch = new Patch(0, mcTex.height - 23, 0, mcTex.height - 23, 128, 23);
                    }
                    else
                    {
                        Color[] line = mcTex.GetPixels(12, 0, 1, mcTex.height);
                        bool foundFlag = false;
                        for (int k = 0; k < line.Length; k++)
                        {
                            if (line[k].a > 0.9f)
                            {
                                partPatch.srcPos.y = k;
                                partPatch.dstPos.y = k;
                                partPatch.size.y = mcTex.height - k;
                                foundFlag = true;
                                break;
                            }
                        }
                        if (foundFlag == false || partPatch.size.y < config.histogramPatchSize.y)
                        {
                            unmatchedMcCount++;
                            unmatchedTexCount += mc.fileInfoIndices.Length;
                            reportSb.AppendLine(string.Concat(mcTex.name, " seems fully transparent mask. Match it manually."));
                            continue;
                        }

                        line = mcTex.GetPixels(0, mcTex.height - 4, mcTex.width, 1);
                        foundFlag = false;
                        for (int k = mcTex.width - 1; k >= 0; k--)
                        {
                            if (line[k].a > 0.9f)
                            {
                                partPatch.size.x = k + 1;
                                foundFlag = true;
                                break;
                            }
                        }
                        if (foundFlag == false || partPatch.size.x < config.histogramPatchSize.x)
                        {
                            unmatchedMcCount++;
                            unmatchedTexCount += mc.fileInfoIndices.Length;
                            reportSb.AppendLine(string.Concat(mcTex.name, " seems fully transparent mask. Match it manually."));
                            continue;
                        }
                    }
                }

                //Generate the histogram data
                int histGenAttemptsCount = 0;
                bool isMonochromatic = false;

                mc.histograms = new Histogram[config.histogramPatchCount];
                mc.HistPatches = new Patch[config.histogramPatchCount];
                //mc.histBgPartPatchIndices = new int[config.histogramPatchCount];

                for (int j = 0; j < config.histogramPatchCount; j++)
                {
                    if (mc.isMask == false)
                    {
                        if (mc.bgPartIndex != -1)
                        {
                            int bgPartPatchIndex = Random.Range(0, dumpFormat.bgParts[mc.bgPartIndex].patches.Length);
                            partPatch = dumpFormat.bgParts[mc.bgPartIndex].patches[bgPartPatchIndex];
                        }
                        else
                        {
                            partPatch = new Patch(0, 0, 0, 0, mc.texSize.x, mc.texSize.y);
                        }
                    }

                    Patch p = partPatch;
                    p.size = config.histogramPatchSize;

                    Vector2Int patchPos = partPatch.size - config.histogramPatchSize;
                    patchPos.x = Random.Range(0, patchPos.x);
                    patchPos.y = Random.Range(0, patchPos.y);

                    p.Move(patchPos);

                    Histogram histogram = new Histogram(mc.isMask ? 4 : 3, config.histogramStepCount, 1f);

                    Color[] patchColors = mcTex.GetPixels(p.srcPos.x, p.srcPos.y, p.size.x, p.size.y);

                    histogram.AddValues(patchColors, mc.isMask);

                    //Compare the new histogram with the previous one, if there are the same the texture might be monochromatic...
                    if (j > 0)
                    {
                        if (histogram.Compare(mc.histograms[mc.histograms.Length - 1]) >= 0.999f)
                        {
                            histGenAttemptsCount++;

                            if (histGenAttemptsCount > config.histGenAttemptsMaxCount)
                            {
                                isMonochromatic = true;
                                break;
                            }

                            j--;
                            continue;
                        }
                    }

                    if (config.savePatchTexures)
                    {
                        Texture2D test = new Texture2D(p.size.x, p.size.y);
                        test.SetPixels(patchColors);
                        fm.SaveTextureToPng(test, "./test", mcTex.name + "_" + j);
                        Object.Destroy(test);
                    }

                    mc.tempHistogram = new Histogram(mc.isMask ? 4 : 3, config.histogramStepCount, 1f);
                    mc.histograms[j] = histogram;
                    mc.HistPatches[j] = p;
                }

                if (isMonochromatic)
                {
                    unmatchedMcCount++;
                    unmatchedTexCount += mc.fileInfoIndices.Length;
                    reportSb.AppendLine(string.Concat(mcTex.name, " is too consistent visually to be analyzed properly. Match it manually."));
                }
                else
                {
                    if (mc.bgPartIndex == -1)
                        mc.bgPartIndex = 0;

                    //Finally add the candidate to the list
                    candidatesList.Add(mc);
                }


                Object.Destroy(mcTex);
            }

            progressCb(new ProgressInfo("Preparing candidates", mcTexCount, mcTexCount, 1f));
            yield return new WaitForEndOfFrame();

            //2. The match candidates are now all generated, we can go through all the BgInfo and for each BgInfo, going through all the MCs to find the best match.
            //For sure this way increase risk of false positives but it is also much faster than loading entire textures exponentially...
            MatchCandidate[] candidates = candidatesList.ToArray();
            string baseDumpPath = Path.Combine(dumpTexPath, baseDumpFormat.name);
            //int mcMatchesCount = 0;
            int texMatchesCount = 0;
            for (int i = 0; i < bgInfoCount; i++)
            {
                //if (i % 10 == 0)
                //{
                progressCb(new ProgressInfo("Looking for matches", i + 1, bgInfoCount, i / (float)(bgInfoCount - 1)));
                yield return new WaitForEndOfFrame();
                //}

                if (config.resetDumpMatches)
                    bgInfos[i].ResetDumpMatches(dumpFormat.name);

                //Every candidate is matched, stop searching.
                //TODO - Track the best BgInfo into the MCandidate. This could be perfect. But requires to get rid of that and the isMatched flag stored into the MC.
                //if (mcMatchesCount >= candidates.Length)
                //    break;

                Texture2D bgTex = fm.GetTextureFromPath(Path.Combine(baseDumpPath, bgInfos[i].texDumpMatches[0].texNames[0]));
                Texture2D maskTex = bgInfos[i].hasMask ? fm.GetTextureFromPath(Path.Combine(baseDumpPath, bgInfos[i].texDumpMatches[0].texNames[1])) : null;

                //TODO - max possible match for one bg info and one bg part, List suck ass and realistically I never saw 3 textures exactly the same (and there is no duplicate on GC)
                List<float>[] bestMatchValues = new List<float>[dumpFormat.bgParts.Length == 0 ? 2 : dumpFormat.bgParts.Length + 1];
                List<int>[] bestMatchCandidateIndices = new List<int>[dumpFormat.bgParts.Length == 0 ? 2 : dumpFormat.bgParts.Length + 1];

                for (int j = 0; j < bestMatchValues.Length; j++)
                {
                    bestMatchValues[j] = new List<float>();
                    bestMatchCandidateIndices[j] = new List<int>();
                }

                for (int j = 0; j < candidates.Length; j++)
                {
                    //if (candidates[j].isMatched)
                    //    continue;

                    MatchCandidate mc = candidates[j];

                    //Quick Pruning for mask, compare the size. Masks tend to have different size.
                    if (mc.isMask)
                    {
                        if (maskTex == null)
                            continue;

                        //In RE3, the masks have a fixed resolution on GC and a variable on PC (cropped)
                        if (config.inconsistentMaskSize)
                        {
                            if (maskTex.width < 128 || maskTex.height < 23)
                                continue;
                        }
                        else
                        {
                            if (maskTex.width != mc.texSize.x || maskTex.height != mc.texSize.y)
                                continue;
                        }
                    }
                    else
                    {
                        if (bgTex.width != mc.texSize.x || bgTex.height != mc.texSize.y)
                            continue;
                    }

                    float mcMatchValue = 0f;

                    bool isImpossibleMatch = false;
                    for (int k = 0; k < mc.HistPatches.Length; k++)
                    {
                        Patch p = mc.HistPatches[k];

                        mc.tempHistogram.Reset();

                        Color[] pixels;
                        if (mc.isMask)
                        {
                            if (config.inconsistentMaskSize)
                            {
                                int dstPosY = maskTex.height - 23 + (p.dstPos.y - (256 - 23));
                                pixels = maskTex.GetPixels(p.dstPos.x, dstPosY, p.size.x, p.size.y);
                                mc.tempHistogram.AddValues(pixels, true);

                            }
                            else
                            {
                                pixels = maskTex.GetPixels(p.dstPos.x, p.dstPos.y, p.size.x, p.size.y);
                                mc.tempHistogram.AddValues(pixels, true);
                            }
                        }
                        else
                        {
                            pixels = bgTex.GetPixels(p.dstPos.x, p.dstPos.y, p.size.x, p.size.y);
                            mc.tempHistogram.AddValues(pixels);
                        }

                        if (config.savePatchTexures)
                        {
                            Texture2D test = new Texture2D(p.size.x, p.size.y);
                            test.SetPixels(pixels);
                            fm.SaveTextureToPng(test, "./test", (mc.isMask ? maskTex.name : bgTex.name) + "_" + k);
                            Object.Destroy(test);
                        }

                        float patchMatchValue = mc.histograms[k].Compare(mc.tempHistogram);

                        //if the match value of one patch is SO BAD, you can stop here and move to the next MC.
                        if (patchMatchValue <= config.patchMinMatchValue)
                        {
                            isImpossibleMatch = true;
                            break;
                        }

                        mcMatchValue += patchMatchValue;
                    }

                    if (isImpossibleMatch)
                        continue;

                    mcMatchValue = mcMatchValue / mc.HistPatches.Length;

                    //bool isFirstValue = bestMatchValues[mc.bgPartIndex].Count <= 0;
                    if (mcMatchValue >= config.candidateMinMatchValue)// && (isFirstValue || mcMatchValue >= bestMatchValues[mc.bgPartIndex][0]))
                    {
                        //To deal with multiple perfect match... but I haven't already identified duplicates via checksum?
                        //if (!isFirstValue && mcMatchValue != bestMatchValues[mc.bgPartIndex][0])
                        //{
                        //    bestMatchValues[mc.bgPartIndex].Clear();
                        //}

                        bestMatchValues[mc.bgPartIndex].Add(mcMatchValue);
                        bestMatchCandidateIndices[mc.bgPartIndex].Add(j);
                    }
                }

                //Finally we store all the best matches found for that BG info
                //Again going through the bg info (and then the candidates) produces more false positives.
                //But the opposite is crazy bad in term of performance. An exponential amount of disk fetching.

                //For each part (typically 0 = Left, 1 = right, 2 mask)
                int partCount = bestMatchValues.Length;
                //bool hasMatches = false;
                for (int j = 0; j < partCount; j++)
                {
                    int matchCount = bestMatchValues[j].Count;
                    for (int k = 0; k < matchCount; k++)
                    {
                        //it should be defaulted to 0 anyway.
                        if (bestMatchValues[j][k] >= config.candidateMinMatchValue)
                        {
                            //candidates[bestMatchCandidateIndices[j][k]].isMatched = true;
                            MatchCandidate mc = candidates[bestMatchCandidateIndices[j][k]];
                            //for (int l = 0; l < mc.fileInfoIndices.Length; l++)
                            //{
                            //    string texName = fm.RemoveExtensionFromFileInfo(fm.fileInfos[mc.fileInfoIndices[l]]);
                            //    bgInfos[i].AddDumpMatch(dumpFormat.name, texName, mc.bgPartIndex);
                            //    //hasMatches = true;
                            //    texMatchesCount++;
                            //}
                            ////mcMatchesCount++;

                            mc.bgInfoMatchValue.Add(bestMatchValues[j][k]);
                            mc.bgInfoMatchIndex.Add(i);
                        }
                    }
                }

                //if (hasMatches)
                //    fm.SaveToJson(bgInfos[i], bgInfoPath, bgInfos[i].GetFileName(), prettifyJsonOnSave);

                Object.Destroy(bgTex);
                Object.Destroy(maskTex);
            }

            //Analyze the Match candidates' possible bg info matches
            for (int i = 0; i < candidates.Length; i++)
            {
                MatchCandidate mc = candidates[i];
                if (mc.bgInfoMatchIndex.Count <= 0)
                {
                    //Report the unmatchable candidates
                    unmatchedMcCount++;
                    reportSb.Append(string.Concat("Candidate ", i, " can't find a match: "));
                    for (int j = 0; j < candidates[i].fileInfoIndices.Length; j++)
                    {
                        unmatchedTexCount++;
                        reportSb.Append(string.Concat("[", fm.fileInfos[candidates[i].fileInfoIndices[j]].Name, "]"));
                    }
                    reportSb.AppendLine();
                }
                else
                {
                    float bestBgInfoMatchValue = 0f;
                    int bestBgInfoIndex = 0;
                    for (int j = 0; j < mc.bgInfoMatchIndex.Count; j++)
                    {
                        if (mc.bgInfoMatchValue[j] > bestBgInfoMatchValue)
                        {
                            bestBgInfoMatchValue = mc.bgInfoMatchValue[j];
                            bestBgInfoIndex = mc.bgInfoMatchIndex[j];
                        }
                    }

                    //Update and save the best bg info match 
                    for (int j = 0; j < mc.fileInfoIndices.Length; j++)
                    {
                        string texName = fm.RemoveExtensionFromFileInfo(fm.fileInfos[mc.fileInfoIndices[j]]);

                        bgInfos[bestBgInfoIndex].AddDumpMatch(dumpFormat.name, texName, mc.bgPartIndex);
                        texMatchesCount++;
                    }

                    reportSb.Append(string.Concat("Candidate ", i, " matched: "));
                    for (int j = 0; j < candidates[i].fileInfoIndices.Length; j++)
                    {
                        reportSb.Append(string.Concat("[", fm.fileInfos[candidates[i].fileInfoIndices[j]].Name, "]"));
                    }
                    reportSb.Append(string.Concat(" - ", bestBgInfoMatchValue.ToString("0.00")));
                    reportSb.AppendLine();

                    fm.SaveToJson(bgInfos[bestBgInfoIndex], bgInfoPath, bgInfos[bestBgInfoIndex].GetFileName(), prettifyJsonOnSave);
                }
            }

            reportSb.AppendLine(string.Format("{0} matches for {1} textures ({2} duplicates). {3} unmatchable textures from {4} candidates.", texMatchesCount, mcTexCount, texDuplicatesCount, unmatchedTexCount, unmatchedMcCount));
            reportSb.AppendLine(string.Format("== Texture Matching done! ({0} seconds) ==", (Time.unscaledTime - taskTime).ToString("#.0")));

            fm.OpenFolder(bgInfoPath);

            doneCb();
        }

        public IEnumerator RecreateTextures(string processedPath, string bgInfoPath, string alphaChannelPath, string resultsPath, Vector2Int pixelShift, DumpFormat dumpFormat, System.Action<ProgressInfo> progressCb, System.Action doneCb)
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

            int bgInfosCount = fm.LoadFiles(bgInfoPath, "json");

            if (bgInfosCount <= 0)
            {
                reportSb.AppendLine("== Texture recreation aborted - No Bg Info ==");
                doneCb();
                yield break;
            }

            BgInfo[] bgInfos = fm.GetObjectsFromFiles<BgInfo>();

            int processedTexCount = fm.LoadFiles(processedPath, "png", SearchOption.AllDirectories);

            if (processedTexCount <= 0)
            {
                reportSb.AppendLine("== Texture recreation aborted - No Processed Texture ==");
                doneCb();
                yield break;
            }

            progressCb(new ProgressInfo(string.Concat("Recreating Textures - ", bgInfos[0].namePrefix), 1, bgInfosCount, 0 / (float)(bgInfosCount - 1)));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < bgInfosCount; i++)
            {
                BgInfo bgInfo = bgInfos[i];

                //Get the right processed background
                string expectedBgName = string.Concat(bgInfo.namePrefix, ".png");
                FileInfo bgTexFileInfo = fm.fileInfos.FirstOrDefault(x => x.Name == expectedBgName);
                if (bgTexFileInfo == null)
                {
                    reportSb.AppendLine(string.Concat("Missing upscaled BG texture: ", bgInfo.namePrefix));
                    continue;
                }

                progressCb(new ProgressInfo(string.Concat("Recreating Textures - ", bgInfo.namePrefix), i + 1, bgInfosCount, i / (float)(bgInfosCount - 1)));
                yield return new WaitForEndOfFrame();

                Texture2D processedTexAms = null;
                Texture2D processedTex = fm.GetTextureFromFileInfo(bgTexFileInfo);

                //float texRatioFloat = processedTex.width / (float)bgInfo.bgTexSize.x;
                float bgRatioFloat = processedTex.width / (float)bgInfo.bgTexSize.x;
                float maskRatioFloat = processedTex.width / (float)baseDumpFormat.maskUsageSize.x;

                if (bgRatioFloat - Mathf.Floor(bgRatioFloat) != 0f || maskRatioFloat - Mathf.Floor(maskRatioFloat) != 0f)
                {
                    reportSb.AppendLine(string.Concat("Error: This tool is not compatible with non integer scaling. Please fix this processed texture:", processedTex.name));
                    Object.Destroy(processedTex);
                    continue;
                }

                int bgRatio = Mathf.RoundToInt(bgRatioFloat);
                int maskRatio = Mathf.RoundToInt(maskRatioFloat);

                CompensatePixelShift(pixelShift, processedTex, maskRatio);

                //Dynamic bg size only works for full background with no parts then... it's not good.
                //I really hope Resident evil 3 or something doesn't have backgrounds texture with different size AND splitted in some bullshit way.
                //If so I will need to analyze the background files during the matching phase and save all that data in the BG info instead.

                BgTexturePart[] bgParts;
                if (dumpFormat.bgParts == null || dumpFormat.bgParts.Length == 0)
                {
                    BgTexturePart fullBgPart = new BgTexturePart();
                    fullBgPart.size = new Vector2Int(bgInfo.bgTexSize.x, bgInfo.bgTexSize.y);
                    fullBgPart.patches = new Patch[1] { new Patch(0, 0, 0, 0, bgInfo.bgTexSize.x, bgInfo.bgTexSize.y) };
                    bgParts = new BgTexturePart[1] { fullBgPart };
                }
                else
                {
                    bgParts = dumpFormat.bgParts;
                }

                DumpMatch dumpMatch = bgInfo.texDumpMatches.FirstOrDefault(x => x.formatName == dumpFormat.name);
                if (string.IsNullOrEmpty(dumpMatch.formatName))
                {
                    Object.Destroy(processedTex);
                    continue;
                }

                int matchGroupTexCount = bgParts.Length + (bgInfo.hasMask ? 1 : 0);
                //int matchGroupCount = dumpMatch.texNames.Length / matchGroupTexCount;

                //progressCb(new ProgressInfo(string.Concat(bgInfo.namePrefix, " - Recreating BG textures"), i + 1, bgInfosCount, i / (float)(bgInfosCount - 1)));
                //yield return new WaitForEndOfFrame();

                //Generate the Bg textures
                for (int j = 0; j < bgParts.Length; j++)
                {
                    Texture2D bgPartTex = new Texture2D(
                        Mathf.RoundToInt(bgParts[j].size.x * bgRatio),
                        Mathf.RoundToInt(bgParts[j].size.y * bgRatio),
                        TextureFormat.RGBA32, false);

                    bgPartTex.Fill(new Color32(0, 0, 0, 255));

                    for (int k = 0; k < bgParts[j].patches.Length; k++)
                    {
                        Patch p = bgParts[j].patches[k];
                        p.Scale(bgRatio);

                        Color[] pColors = processedTex.GetPixels(
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

                    for (int k = 0; k < dumpMatch.partIndices.Length; k++)
                    {
                        if (dumpMatch.partIndices[k] == j)
                            fm.SaveTextureToPng(bgPartTex, resultsPath, dumpMatch.texNames[k]);
                    }

                    Object.Destroy(bgPartTex);
                }

                //2. Generate the mask texture
                if (bgInfo.hasMask)
                {
                    //progressCb(new ProgressInfo(string.Concat(bgInfo.namePrefix, " - Recreating mask"), i+1, bgInfosCount, i / (float)(bgInfosCount-1)));
                    //yield return new WaitForEndOfFrame();

                    //One per group of masks
                    Texture2D[] alphaChannels = new Texture2D[bgInfo.groupsCount];
                    bool hasMissingAlphaChannels = false;
                    for (int g = 0; g < bgInfo.groupsCount; g++)
                    {
                        alphaChannels[g] = fm.GetTextureFromPath(Path.Combine(alphaChannelPath, string.Concat(bgInfo.namePrefix, "_", g)));
                        if (alphaChannels[g] == null)
                        {
                            reportSb.AppendLine(string.Concat(bgInfo.namePrefix, " is missing the alpha channel texture ", g, "."));
                            hasMissingAlphaChannels = true;
                        }
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
                        processedTexAms = fm.GetTextureFromPath(Path.Combine(processedPath, string.Concat(bgInfo.namePrefix, altMaskSourceSuffix)));
                        if (processedTexAms == null)
                        {
                            processedTexAms = fm.GetTextureFromPath(Path.Combine(processedPath, "AMS", string.Concat(bgInfo.namePrefix, altMaskSourceSuffix)));
                            if (processedTexAms == null)
                            {
                                reportSb.AppendLine(string.Concat("Missing special mask source textures: ", bgInfo.namePrefix));
                                continue;
                            }
                        }
                        CompensatePixelShift(pixelShift, processedTexAms, maskRatio);
                    }

                    //Reconstruct the mask texture itself based on processed BG and the smoothed alpha texture
                    Texture2D maskTex = new Texture2D(
                    Mathf.RoundToInt((dumpFormat.maskForcedSize.x != 0 ? dumpFormat.maskForcedSize.x : bgInfo.maskTexSize.x) * maskRatio),
                    Mathf.RoundToInt((dumpFormat.maskForcedSize.x != 0 ? dumpFormat.maskForcedSize.y : bgInfo.maskTexSize.y) * maskRatio),
                    TextureFormat.RGBA32, false);

                    maskTex.Fill(new Color32());

                    for (int j = 0; j < bgInfo.masks.Length; j++)
                    {
                        Mask mask = bgInfo.masks[j];
                        mask.patch.Scale(maskRatio);

                        Color[] pColors;

                        if (bgInfo.useProcessedMaskTex == false || mask.ignoreAltMaskSource)
                        {
                            pColors = processedTex.GetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y, mask.patch.size.x, mask.patch.size.y);
                        }
                        else
                        {
                            pColors = processedTexAms.GetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y, mask.patch.size.x, mask.patch.size.y);
                        }

                        Color[] aColors = alphaChannels[mask.groupIndex].GetPixels(mask.patch.dstPos.x, mask.patch.dstPos.y, mask.patch.size.x, mask.patch.size.y);
                        for (int k = 0; k < pColors.Length; k++)
                        {
                            pColors[k].a = aColors[k].a;
                        }

                        int srcPosY = mask.patch.srcPos.y + (maskTex.height - bgInfo.maskTexSize.y * maskRatio);
                        maskTex.SetPixels(mask.patch.srcPos.x, srcPosY, mask.patch.size.x, mask.patch.size.y, pColors);
                    }

                    for (int j = 0; j < dumpMatch.partIndices.Length; j++)
                    {
                        if (dumpMatch.partIndices[j] == matchGroupTexCount - 1)
                            fm.SaveTextureToPng(maskTex, resultsPath, dumpMatch.texNames[j]);
                    }

                    //Clean up the mess :) 
                    //(and yes don't reuse the texture object, since other games might need dynamic texture size 
                    //AND texture.resize IS a hidden constructor too)
                    Object.Destroy(maskTex);
                    for (int g = 0; g < bgInfo.groupsCount; g++)
                    {
                        Object.Destroy(alphaChannels[g]);
                    }
                }

                Object.Destroy(processedTex);
                Object.Destroy(processedTexAms);

            }

            yield return new WaitForEndOfFrame();

            reportSb.AppendLine(string.Format("== Textures recreation Done! ({0} seconds) ==", (Time.unscaledTime - taskTime).ToString("#.0")));

            fm.OpenFolder(resultsPath);

            doneCb();
        }

        private IEnumerator GenerateBgInfosRE2(string rdtPath, string dumpTexturesPath, string bgInfoPath, System.Action<ProgressInfo> progressCb)
        {
            RE2.RdtParser rdtParser = new RE2.RdtParser();

            //File access is slow as fuck, let me at least display the Progress bar
            progressCb(new ProgressInfo("Loading Rdt files", 0, 0, 0f));
            yield return new WaitForEndOfFrame();

            //Get all the RDT data
            int rdtFilesCount = fm.LoadFiles(rdtPath, "rdt");
            List<RE2.RdtRoom> rdtRooms = new List<RE2.RdtRoom>();

            string lastRdtRoomMd5 = "";
            for (int i = 0; i < rdtFilesCount; i++)
            {
                if (i % 100 == 0)
                {
                    progressCb(new ProgressInfo("Converting Rdt files", i + 1, rdtFilesCount, i / (float)(rdtFilesCount - 1)));
                    yield return new WaitForEndOfFrame();
                }

                byte[] data = fm.GetBytesFromFile(i);

                string rdtRoomMd5 = fm.GetMd5(data);

                //Check if player 0 and player 1 are exactly the same, if so prune player 1.
                if (lastRdtRoomMd5 != "")
                {
                    if (rdtRoomMd5 == lastRdtRoomMd5)
                    {
                        lastRdtRoomMd5 = "";
                        continue;
                    }
                }

                if (rdtParser.ParseRdtData(data, fm.fileInfos[i].Name, out RE2.RdtRoom room))
                    rdtRooms.Add(room);

                lastRdtRoomMd5 = rdtRoomMd5;
            }

            //Let the UI Refresh
            progressCb(new ProgressInfo("Matching textures", 0, 0, 1f));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Get all the CR textures
            int crTexCount = fm.LoadFiles(Path.Combine(dumpTexturesPath, baseDumpFormat.name), "png");

            //Make sure the textures are ordered by names
            fm.OrderFiles(x => x.Name, true);

            int identifiedTexCount = 0;

            BgInfo bgInfo = new BgInfo();
            List<BgInfo> bgInfos = new List<BgInfo>();
            while (identifiedTexCount < crTexCount)
            {
                FileInfo bgCandidate = fm.fileInfos[identifiedTexCount];
                FileInfo maskCandidate;

                //manage a BG only case at the end of the file array
                if (identifiedTexCount + 1 < fm.fileInfos.Length)
                    maskCandidate = fm.fileInfos[identifiedTexCount + 1];
                else
                    maskCandidate = null;

                //0 - Mask only (not good), 1 - BG only, 2 - BG + Mask
                int result = IdentifyCrTextures(bgCandidate, maskCandidate);

                switch (result)
                {
                    case 0:
                        reportSb.AppendLine(string.Concat("WARNING: ", bgCandidate.Name, " is a mask without a BG. Please check your CR folder."));
                        identifiedTexCount++;
                        continue;

                    case 1:
                        identifiedTexCount++;
                        GetBgInfoFromTexFiles(ref bgInfo, bgCandidate, null);
                        break;

                    case 2:
                        identifiedTexCount += 2;
                        GetBgInfoFromTexFiles(ref bgInfo, bgCandidate, maskCandidate);
                        break;
                }

                bgInfos.Add(bgInfo);

                progressCb(new ProgressInfo("Matching textures", identifiedTexCount, crTexCount, identifiedTexCount / (float)(crTexCount - 1)));
                yield return new WaitForEndOfFrame();
            }

            //Check and track BG info duplicates
            List<int> duplicateIndices = new List<int>();
            for (int i = 0; i < bgInfos.Count; i++)
            {
                //i + 1 because an element doesn't need to check itself and when an element check all the others, the others don't need to check the former again.
                for (int j = i + 1; j < bgInfos.Count; j++)
                {
                    if (bgInfos[i].bgMd5 == bgInfos[j].bgMd5 && bgInfos[i].maskMd5 == bgInfos[j].maskMd5)
                    {
                        reportSb.AppendLine(string.Concat("INFO:", string.Concat(bgInfos[i].namePrefix, " has a duplicate: ", bgInfos[j].namePrefix)));
                        bgInfo = bgInfos[i];

                        //Add the duplicate to the dump Matches for the base Dump format of the BG info
                        bgInfo.texDumpMatches[0].AddTexName(bgInfos[j].namePrefix, 0);

                        if (bgInfo.hasMask)
                            bgInfo.texDumpMatches[0].AddTexName(string.Concat(bgInfos[j].namePrefix, maskSuffix), 1);

                        bgInfos[i] = bgInfo;
                        duplicateIndices.Add(j);
                    }
                }
            }

            //Jesus, I could have just use distinct just before instead of the double for... but whatever.
            duplicateIndices = duplicateIndices.Distinct().ToList();
            duplicateIndices.Sort();
            duplicateIndices.Reverse();
            for (int i = 0; i < duplicateIndices.Count; i++)
            {
                bgInfos.RemoveAt(duplicateIndices[i]);
            }

            //Process RDT data
            for (int i = 0; i < bgInfos.Count; i++)
            {
                bgInfo = bgInfos[i];

                if (i % 2 == 0)
                {
                    progressCb(new ProgressInfo("Analyzing mask data", i + 1, bgInfos.Count, i / (float)(bgInfos.Count - 1)));
                    yield return new WaitForEndOfFrame();
                }

                if (bgInfo.hasMask == false)
                    continue;

                //Determine the camPos index of the BgInfo, will be useful later.
                if (int.TryParse(bgInfo.namePrefix.Substring(9, 2), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int camPosIndex))
                {
                    bgInfo.SetCamPosIndex(camPosIndex);
                }
                else
                {
                    reportSb.AppendLine(string.Format("WARNING: Unable to determine CamPos index for {0}", bgInfo.namePrefix));
                    continue;
                }

                string rdtName = bgInfo.namePrefix.Substring(0, 8);
                //RdtRoom match = rdtRooms.First(x => x.name.Contains(rdtName));
                for (int j = 0; j < rdtRooms.Count; j++)
                {
                    RE2.RdtRoom rdtRoom = rdtRooms[j];
                    if (rdtRooms[j].name.Contains(rdtName))
                    {
                        //If it is player 0, check if there is a player 1
                        if (rdtRoom.player == "0" && rdtRooms.Count > j + 1 && rdtRooms[j + 1].player == "1")
                        {
                            //Take Rdt Rooms with the most Camera Positions... Not even sure this is necessary.
                            if (rdtRoom.header.nCut < rdtRooms[j + 1].header.nCut)
                                rdtRoom = rdtRooms[j + 1];
                        }

                        //Unpack the Room masks data into the BgInfo
                        if (AddMasksFromRE2RdtRoom(ref bgInfo, rdtRoom) == false)
                            continue;

                        FileInfo maskTexFi = fm.fileInfos.FirstOrDefault(x => x.Name.Contains(bgInfo.namePrefix + maskSuffix));
                        if (maskTexFi == null)
                        {
                            reportSb.AppendLine("Warning: " + bgInfo.namePrefix + " is supposed to have a mask but the texture is not present. Check your CR folder.");
                            continue;
                        }

                        Texture2D maskTex = fm.GetTextureFromFileInfo(maskTexFi);
                        ComputeMaskTransparency(ref bgInfo, maskTex);
                        Object.Destroy(maskTex);

                        bgInfos[i] = bgInfo;

                        break;
                    }
                }
            }

            //Let the UI Refresh
            progressCb(new ProgressInfo("Saving BgInfo files", 0, 0, 1f));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Save Bg Infos
            for (int i = 0; i < bgInfos.Count; i++)
            {
                if (i % 5 == 0)
                {
                    progressCb(new ProgressInfo("Saving BgInfo files", i + 1, bgInfos.Count, i / (float)(bgInfos.Count - 1)));
                    yield return new WaitForEndOfFrame();
                }

                fm.SaveToJson(bgInfos[i], bgInfoPath, bgInfos[i].GetFileName(), prettifyJsonOnSave);
            }

            reportSb.AppendLine(string.Format("{0} BgInfos, {1} duplicates for {2} textures, {3} Rdt files ({4} Uniques)", bgInfos.Count, duplicateIndices.Count, fm.fileInfos.Length, rdtFilesCount, rdtRooms.Count));
            reportSb.AppendLine(string.Format("== BgInfo generation done! ({0} seconds) ==", (Time.unscaledTime - taskTime).ToString("#.0")));

        }

        private IEnumerator GenerateBgInfosRE3(string rdtPath, string dumpTexturesPath, string bgInfoPath, System.Action<ProgressInfo> progressCb)
        {
            RE3.RdtParser rdtParser = new RE3.RdtParser();

            //File access is slow as fuck, let me at least display the Progress bar
            progressCb(new ProgressInfo("Loading Rdt files", 0, 0, 0f));
            yield return new WaitForEndOfFrame();

            //Get all the RDT data
            int rdtFilesCount = fm.LoadFiles(rdtPath, "rdt");
            List<RE3.RdtRoom> rdtRooms = new List<RE3.RdtRoom>();

            string lastRdtRoomMd5 = "";
            for (int i = 0; i < rdtFilesCount; i++)
            {
                if (i % 100 == 0)
                {
                    progressCb(new ProgressInfo("Converting Rdt files", i + 1, rdtFilesCount, i / (float)(rdtFilesCount - 1)));
                    yield return new WaitForEndOfFrame();
                }

                byte[] data = fm.GetBytesFromFile(i);

                string rdtRoomMd5 = fm.GetMd5(data);

                //Check if player 0 and player 1 are exactly the same, if so prune player 1.
                if (lastRdtRoomMd5 != "")
                {
                    if (rdtRoomMd5 == lastRdtRoomMd5)
                    {
                        lastRdtRoomMd5 = "";
                        continue;
                    }
                }

                if (rdtParser.ParseRdtData(data, fm.fileInfos[i].Name, out RE3.RdtRoom room))
                {
                    rdtRooms.Add(room);
                }

                lastRdtRoomMd5 = rdtRoomMd5;
            }

            //Let the UI Refresh
            progressCb(new ProgressInfo("Matching textures", 0, 0, 1f));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Get all the CR textures
            int crTexCount = fm.LoadFiles(Path.Combine(dumpTexturesPath, baseDumpFormat.name), "png");

            //Make sure the textures are ordered by names
            fm.OrderFiles(x => x.Name, true);

            int identifiedTexCount = 0;

            BgInfo bgInfo = new BgInfo();
            List<BgInfo> bgInfos = new List<BgInfo>();
            List<FileInfo> bgLessMasks = new List<FileInfo>();
            while (identifiedTexCount < crTexCount)
            {
                FileInfo bgCandidate = fm.fileInfos[identifiedTexCount];
                FileInfo maskCandidate;

                //manage a BG only case at the end of the file array
                if (identifiedTexCount + 1 < fm.fileInfos.Length)
                    maskCandidate = fm.fileInfos[identifiedTexCount + 1];
                else
                    maskCandidate = null;

                //0 - Mask only (not good), 1 - BG only, 2 - BG + Mask
                int result = IdentifyCrTextures(bgCandidate, maskCandidate);

                switch (result)
                {
                    //RE3 has legit masks without backgrounds. Most of them are duplicates so let's track these correctly by comparing their MD5 with other masks.
                    //Hopefully the associated RDT files made no changes on the mask mapping data. If so I will have to create specific BgInfo for them too...
                    case 0:
                        //reportSb.AppendLine(string.Concat("WARNING: ", bgCandidate.Name, " is a mask without a BG. Please check your CR folder."));
                        bgLessMasks.Add(bgCandidate);
                        identifiedTexCount++;
                        continue;

                    case 1:
                        identifiedTexCount++;
                        GetBgInfoFromTexFiles(ref bgInfo, bgCandidate, null);
                        break;

                    case 2:
                        identifiedTexCount += 2;
                        GetBgInfoFromTexFiles(ref bgInfo, bgCandidate, maskCandidate);
                        break;
                }

                bgInfos.Add(bgInfo);

                progressCb(new ProgressInfo("Matching textures", identifiedTexCount, crTexCount, identifiedTexCount / (float)(crTexCount - 1)));
                yield return new WaitForEndOfFrame();
            }

            //Check BgLess masks for duplicates
            for (int i = 0; i < bgLessMasks.Count; i++)
            {
                string bgLessMaskMd5 = fm.GetMd5(fm.GetTextureFromFileInfo(bgLessMasks[i]).GetRawTextureData());
                bool isDuplicate = false;
                for (int j = 0; j < bgInfos.Count; j++)
                {
                    bgInfo = bgInfos[j];

                    if (bgInfo.hasMask && bgInfo.maskMd5 == bgLessMaskMd5)
                    {
                        isDuplicate = true;
                        bgInfo.texDumpMatches[0].AddTexName(fm.RemoveExtensionFromFileInfo(bgLessMasks[i]), 1);
                        bgInfos[j] = bgInfo;

                        reportSb.AppendLine(string.Concat("INFO: ", bgLessMasks[i].Name, " is a mask without a BG but was matched with " + bgInfo.namePrefix));
                        break;
                    }
                }

                if (isDuplicate == false)
                {
                    reportSb.AppendLine(string.Concat("WARNING: ", bgLessMasks[i].Name, " is a mask without a BG. Please check your CR folder."));
                }
            }

            //Check and track BG info duplicates
            List<int> duplicateIndices = new List<int>();
            for (int i = 0; i < bgInfos.Count; i++)
            {
                //i + 1 because an element doesn't need to check itself and when an element check all the others, the others don't need to check the former again.
                for (int j = i + 1; j < bgInfos.Count; j++)
                {
                    if (bgInfos[i].bgMd5 == bgInfos[j].bgMd5 && bgInfos[i].maskMd5 == bgInfos[j].maskMd5)
                    {
                        reportSb.AppendLine(string.Concat("INFO: ", string.Concat(bgInfos[i].namePrefix, " has a duplicate: ", bgInfos[j].namePrefix)));
                        bgInfo = bgInfos[i];

                        //Add the duplicate to the dump Matches for the base Dump format of the BG info
                        bgInfo.texDumpMatches[0].AddTexName(bgInfos[j].namePrefix, 0);

                        if (bgInfo.hasMask)
                            bgInfo.texDumpMatches[0].AddTexName(string.Concat(bgInfos[j].namePrefix, maskSuffix), 1);

                        bgInfos[i] = bgInfo;
                        duplicateIndices.Add(j);
                    }
                }
            }

            //Jesus, I could have just use distinct just before instead of the double for... but whatever.
            duplicateIndices = duplicateIndices.Distinct().ToList();
            duplicateIndices.Sort();
            duplicateIndices.Reverse();
            for (int i = 0; i < duplicateIndices.Count; i++)
            {
                bgInfos.RemoveAt(duplicateIndices[i]);
            }

            //Process RDT data
            for (int i = 0; i < bgInfos.Count; i++)
            {
                bgInfo = bgInfos[i];

                if (i % 2 == 0)
                {
                    progressCb(new ProgressInfo("Analyzing mask data", i + 1, bgInfos.Count, i / (float)(bgInfos.Count - 1)));
                    yield return new WaitForEndOfFrame();
                }

                if (bgInfo.hasMask == false)
                    continue;

                //Determine the camPos index of the BgInfo, will be useful later.
                if (int.TryParse(bgInfo.namePrefix.Substring(4, 2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out int camPosIndex))
                {
                    bgInfo.SetCamPosIndex(camPosIndex);
                }
                else
                {
                    reportSb.AppendLine(string.Format("WARNING: Unable to determine CamPos index for {0}", bgInfo.namePrefix));
                    continue;
                }

                string rdtName = bgInfo.namePrefix.Substring(0, 4);
                //RdtRoom match = rdtRooms.First(x => x.name.Contains(rdtName));
                for (int j = 0; j < rdtRooms.Count; j++)
                {
                    RE3.RdtRoom rdtRoom = rdtRooms[j];
                    if (rdtRooms[j].name.Contains(rdtName))
                    {
                        //If it is player 0, check if there is a player 1
                        if (rdtRoom.player == "0" && rdtRooms.Count > j + 1 && rdtRooms[j + 1].player == "1")
                        {
                            //Take Rdt Rooms with the most Camera Positions... Not even sure this is necessary.
                            if (rdtRoom.header.nCut < rdtRooms[j + 1].header.nCut)
                                rdtRoom = rdtRooms[j + 1];
                        }

                        //Unpack the Room masks data into the BgInfo
                        if (AddMasksFromRE3RdtRoom(ref bgInfo, rdtRoom) == false)
                            continue;

                        FileInfo maskTexFi = fm.fileInfos.FirstOrDefault(x => x.Name.Contains(bgInfo.namePrefix + maskSuffix));
                        if (maskTexFi == null)
                        {
                            reportSb.AppendLine("Warning: " + bgInfo.namePrefix + " is supposed to have a mask but the texture is not present. Check your CR folder.");
                            continue;
                        }

                        Texture2D maskTex = fm.GetTextureFromFileInfo(maskTexFi);
                        ComputeMaskTransparency(ref bgInfo, maskTex);
                        Object.Destroy(maskTex);

                        bgInfos[i] = bgInfo;

                        break;
                    }
                }
            }

            //Let the UI Refresh
            progressCb(new ProgressInfo("Saving BgInfo files", 0, 0, 1f));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            //Save Bg Infos
            for (int i = 0; i < bgInfos.Count; i++)
            {
                if (i % 5 == 0)
                {
                    progressCb(new ProgressInfo("Saving BgInfo files", i + 1, bgInfos.Count, i / (float)(bgInfos.Count - 1)));
                    yield return new WaitForEndOfFrame();
                }

                fm.SaveToJson(bgInfos[i], bgInfoPath, bgInfos[i].GetFileName(), prettifyJsonOnSave);
            }

            reportSb.AppendLine(string.Format("{0} BgInfos, {1} duplicates for {2} textures, {3} Rdt files ({4} Uniques)", bgInfos.Count, duplicateIndices.Count, fm.fileInfos.Length, rdtFilesCount, rdtRooms.Count));
            reportSb.AppendLine(string.Format("== BgInfo generation done! ({0} seconds) ==", (Time.unscaledTime - taskTime).ToString("#.0")));

        }

        public IEnumerator GenerateBgInfos(string rdtPath, string dumpTexturesPath, string bgInfoPath, System.Action<ProgressInfo> progressCb, System.Action doneCb)
        {
            reportSb.Clear();
            reportSb.AppendLine("== BgInfo generation Started! ==");

            taskTime = Time.unscaledTime;

            rdtPath = Path.Combine(rdtPath, game.ToString());
            bgInfoPath = Path.Combine(bgInfoPath, game.ToString());

            fm.CreateDirectory(bgInfoPath);

            Debug.Log(rdtPath);
            Debug.Log(bgInfoPath);

            switch (game)
            {
                case Game.RE2:
                    yield return GenerateBgInfosRE2(rdtPath, dumpTexturesPath, bgInfoPath, progressCb);
                    break;

                case Game.RE3:
                    yield return GenerateBgInfosRE3(rdtPath, dumpTexturesPath, bgInfoPath, progressCb);
                    break;
            }

            fm.OpenFolder(bgInfoPath);

            doneCb();
        }

        protected void ComputeMaskTransparency(ref BgInfo bgInfo, Texture2D maskTex)
        {
            //2 ways: get the entire array of pixels32 and compute the indices for each pixels or get a specific patch of pixels with GetPixels.
            //Apparently GetPixels is slower. But I also want to challenge myself computing the indices correctly in a 1d array based on a rect.
            //Let's be lazy...
            StringBuilder sb = new StringBuilder();

            List<int> opaqueIndices = new List<int>();
            Mask mask;
            Color[] colors;
            for (int i = 0; i < bgInfo.masks.Length; i++)
            {
                mask = bgInfo.masks[i];
                colors = maskTex.GetPixels(mask.patch.srcPos.x, mask.patch.srcPos.y, mask.patch.size.x, mask.patch.size.y);
                opaqueIndices.Clear();
                bool isBlock = false;
                for (int j = 0; j < colors.Length; j++)
                {
                    if (colors[j].a > 0.5f)
                    {
                        if (isBlock == false)
                            opaqueIndices.Add(j);
                        isBlock = true;
                    }
                    else
                    {
                        if (isBlock)
                        {
                            isBlock = false;

                            int previousIndex = j - 1;
                            int lastOpaqueIndex = opaqueIndices.Last();

                            //One pixel case
                            if (lastOpaqueIndex == previousIndex)
                            {
                                //Do nothing
                                continue;
                            }

                            //Two Pixel case
                            if (lastOpaqueIndex == previousIndex - 1)
                            {
                                opaqueIndices.Add(previousIndex);
                                continue;
                            }

                            //General case
                            opaqueIndices.Add(-1);
                            opaqueIndices.Add(previousIndex);
                        }
                    }
                }

                if (isBlock)
                {
                    isBlock = false;

                    int previousIndex = colors.Length - 1;
                    int lastOpaqueIndex = opaqueIndices.Last();

                    //One pixel case
                    if (lastOpaqueIndex == previousIndex)
                    {
                        //Do nothing
                    }
                    else if (lastOpaqueIndex == previousIndex - 1)
                    {
                        opaqueIndices.Add(previousIndex);
                    }
                    else
                    {
                        //General case
                        opaqueIndices.Add(-1);
                        opaqueIndices.Add(previousIndex);
                    }
                }

                mask.opaqueIndices = opaqueIndices.ToArray();
                bgInfo.masks[i] = mask;
            }
        }

        protected bool AddMasksFromRE3RdtRoom(ref BgInfo bgInfo, RE3.RdtRoom room)
        {
            int camPosIndex = bgInfo.camPosIndex;

            if (room.hasMasks[camPosIndex] == false)
            {
                reportSb.AppendLine("Warning: " + bgInfo.namePrefix + " is supposed to have a mask but the associated RDT file has no mask data... This is not normal.");
                return false;
            }

            bgInfo.masks = new Mask[room.cameraMasks[camPosIndex].count_masks];
            bgInfo.groupsCount = room.maskGroups[camPosIndex].Length;

            Vector2Int offset = new Vector2Int();
            int maskIndex = 0;
            for (int i = 0; i < bgInfo.groupsCount; i++)
            {
                offset.x = room.maskGroups[camPosIndex][i].x;
                offset.y = room.maskGroups[camPosIndex][i].y;
                int groupMaskCount = room.maskGroups[camPosIndex][i].count;

                for (int j = 0; j < groupMaskCount; j++)
                {
                    RE3.RdtMask mask = room.masks[camPosIndex][i][j];
                    bgInfo.masks[maskIndex].groupIndex = i;
                    bgInfo.masks[maskIndex].patch = new Patch(
                        mask.u,
                        bgInfo.maskTexSize.y - mask.v - mask.height,
                        offset.x + mask.x,
                        baseDumpFormat.maskUsageSize.y - offset.y - mask.y - mask.height,
                        mask.width, mask.height);

                    maskIndex++;
                }
            }

            return true;
        }

        protected bool AddMasksFromRE2RdtRoom(ref BgInfo bgInfo, RE2.RdtRoom room)
        {
            int camPosIndex = bgInfo.camPosIndex;

            if (room.hasMasks[camPosIndex] == false)
            {
                reportSb.AppendLine("Warning: " + bgInfo.namePrefix + " is supposed to have a mask but the associated RDT file has no mask data... This is not normal.");
                return false;
            }

            bgInfo.masks = new Mask[room.cameraMasks[camPosIndex].count_masks];
            bgInfo.groupsCount = room.maskGroups[camPosIndex].Length;

            Vector2Int offset = new Vector2Int();
            int maskIndex = 0;
            for (int i = 0; i < bgInfo.groupsCount; i++)
            {
                offset.x = room.maskGroups[camPosIndex][i].x;
                offset.y = room.maskGroups[camPosIndex][i].y;
                int groupMaskCount = room.maskGroups[camPosIndex][i].count;

                for (int j = 0; j < groupMaskCount; j++)
                {
                    RE2.RdtMask mask = room.masks[camPosIndex][i][j];
                    bgInfo.masks[maskIndex].groupIndex = i;
                    bgInfo.masks[maskIndex].patch = new Patch(
                        mask.u,
                        bgInfo.maskTexSize.y - mask.v - mask.height,
                        offset.x + mask.x,
                        baseDumpFormat.maskUsageSize.y - offset.y - mask.y - mask.height,
                        mask.width, mask.height);

                    maskIndex++;
                }
            }

            return true;
        }

        protected void CompensatePixelShift(Vector2Int pixelShift, Texture2D tex, int texRatio)
        {
            if (pixelShift.x != 0 || pixelShift.y != 0)
            {
                tex.wrapMode = TextureWrapMode.Clamp;

                int pixelShiftCount = Mathf.Abs(pixelShift.x) >= Mathf.Abs(pixelShift.y) ? Mathf.Abs(pixelShift.x) : Mathf.Abs(pixelShift.y);
                int shiftX = 0;
                int shiftY = 0;

                //x
                for (int j = 0; j < Mathf.Abs(pixelShift.x); j++)
                {
                    int gx = 0;
                    int sx = 0;
                    int w = 0;

                    shiftX = j * 1 * texRatio;

                    if (pixelShift.x > 0)
                    {
                        gx = shiftX;
                        sx = gx + 1;
                    }
                    else
                    {
                        sx = 0;
                        gx = sx + 1;
                    }

                    w = tex.width - shiftX - 1;

                    Color[] colors = tex.GetPixels(gx, 0, w, tex.height);
                    tex.SetPixels(sx, 0, w, tex.height, colors);
                }

                //Y
                for (int j = 0; j < Mathf.Abs(pixelShift.y); j++)
                {
                    int gy = 0;
                    int sy = 0;
                    int h = 0;

                    shiftY = j * 1 * texRatio;

                    if (pixelShift.y > 0)
                    {
                        gy = shiftY;
                        sy = gy + 1;
                    }
                    else
                    {
                        sy = 0;
                        gy = sy + 1;
                    }

                    h = tex.height - shiftY - 1;

                    Color[] colors = tex.GetPixels(0, gy, tex.width, h);
                    tex.SetPixels(0, sy, tex.width, h, colors);
                }
            }
        }

        protected void GetBgInfoFromTexFiles(ref BgInfo bgInfo, FileInfo bgFileInfo, FileInfo maskFileInfo)
        {
            bgInfo.Reset();

            Texture2D tex = fm.GetTextureFromFileInfo(bgFileInfo);

            bgInfo.namePrefix = tex.name;
            bgInfo.bgTexSize = new Vector2Int(tex.width, tex.height);
            bgInfo.bgMd5 = fm.GetMd5(tex.GetRawTextureData());

            bgInfo.texDumpMatches = new DumpMatch[] { new DumpMatch(baseDumpFormat.name, tex.name, 0) };

            Object.Destroy(tex);

            if (maskFileInfo != null)
            {
                bgInfo.hasMask = true;
                tex = fm.GetTextureFromFileInfo(maskFileInfo);
                bgInfo.maskTexSize = new Vector2Int(tex.width, tex.height);
                bgInfo.maskMd5 = fm.GetMd5(tex.GetRawTextureData());

                bgInfo.texDumpMatches[0].AddTexName(tex.name, 1);

                Object.Destroy(tex);
            }
        }

        //This is using the filename directly since the texture are dumped with a good tool
        protected int IdentifyCrTextures(FileInfo bgCandidate, FileInfo maskCandidate)
        {
            if (bgCandidate.Name.Contains(maskSuffix))
                return 0; //Skip this texture without creating a BG info

            string bgName = fm.GetFileName(bgCandidate);

            if (maskCandidate == null)
                return 1;

            string maskCandidateName = fm.GetFileName(maskCandidate);
            if (bgName + maskSuffix == maskCandidateName)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        public string GetReport()
        {
            return reportSb.ToString();
        }

        private Texture2D ScaleTexture(Texture2D source, int scaleRatio)
        {
            int tWidth = source.width * scaleRatio;
            int tHeight = source.height * scaleRatio;
            Texture2D result = new Texture2D(tWidth, tHeight, source.format, false);
            Color[] rpixels = result.GetPixels();
            for (int px = 0; px < rpixels.Length; px++)
            {
                float u = (px % tWidth) / (float)tWidth;
                float v = (px / tWidth) / (float)tHeight;

                rpixels[px] = source.GetPixel(Mathf.FloorToInt(u * (float)source.width), Mathf.FloorToInt(v * (float)source.height));
            }
            result.SetPixels(rpixels);
            result.Apply();
            return result;
        }

    }

}
