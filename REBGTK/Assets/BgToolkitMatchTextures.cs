using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BgTk
{
    public partial class BgToolkit
    {
        public IEnumerator MatchTextures(string bgInfoPath, string dumpTexPath, DumpFormat dumpFormat,
            TextureMatchingConfig config, System.Action<ProgressInfo> progressCb, System.Action doneCb)
        {
            if (dumpFormat.Equals(baseDumpFormat))
            {
                Debug.LogWarning(string.Format(
                    "The current base texture format ({0}) is the same as the selected texture format for matching ({1}). This is useless.",
                    baseDumpFormat.name, dumpFormat.name));
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
            var bgInfoCount = fm.LoadFiles(bgInfoPath, "json");
            var bgInfos = fm.GetObjectsFromFiles<BgInfo>();

            //Load all the file info of the textures to match
            var mcTexCount = fm.LoadFiles(Path.Combine(dumpTexPath, dumpFormat.name), "png",
                SearchOption.AllDirectories);

            var candidatesList = new List<MatchCandidate>();

            var texDuplicatesCount = 0;
            var unmatchedTexCount = 0;
            var unmatchedMcCount = 0;

            //1. Prepare the MatchCandidates
            for (var i = 0; i < mcTexCount; i++)
            {
                if (i % 50 == 0)
                {
                    progressCb(
                        new ProgressInfo("Preparing candidates", i + 1, mcTexCount, i / (float)(mcTexCount - 1)));
                    yield return new WaitForEndOfFrame();
                }

                var mcTex = fm.GetTextureFromFileIndex(i);

                var mc = new MatchCandidate();

                mc.md5 = fm.GetMd5(mcTex.GetRawTextureData());
                mc.fileInfoIndices = new int[] {i};
                mc.texSize.x = mcTex.width;
                mc.texSize.y = mcTex.height;
                mc.bgInfoMatchIndex = new List<int>();
                mc.bgInfoMatchValue = new List<float>();

                //Check for duplicates
                var isDuplicate = false;
                for (var j = 0; j < candidatesList.Count; j++)
                {
                    if (candidatesList[j].md5 == mc.md5)
                    {
                        reportSb.AppendLine(string.Concat(mcTex.name, " is a duplicate of ",
                            fm.fileInfos[candidatesList[j].fileInfoIndices[0]].Name));
                        texDuplicatesCount++;

                        //int[] indices = new int[candidatesList[j].fileInfoIndices.Length + 1];
                        //for (int k = 0; k < candidatesList[j].fileInfoIndices.Length; k++)
                        //{
                        //    indices[k] = candidatesList[j].fileInfoIndices[k];
                        //}
                        //indices[indices.Length - 1] = j;
                        var indices = candidatesList[j].fileInfoIndices.Concat(new int[] {i}).ToArray();

                        var duplicatedMc = candidatesList[j];
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
                IdentifyCandidate(dumpFormat, ref mc, mcTex);

                //RE3 WARNING - That doesn't WORK AT ALL ON RE3 GAMECUBE
                //Create a special patch for mask texture, thus It will only pick colors into the non fully transparent area.
                var partPatch = new Patch();
                if (mc.isMask)
                {
                    //For some dumping format like Peixoto Before, starting the matching process,
                    // transparency needs to be added by replacing pure black pixels with transparent ones. 
                    if (dumpFormat.useBlackAsTransparent)
                    {
                        var colors = mcTex.GetPixels32();
                        for (int j = 0; j < colors.Length; j++)
                        {
                            var c = colors[j];
                            if (c.r + c.g + c.b <= byte.MinValue)
                            {
                                c.a = byte.MinValue;
                                colors[j] = c;
                            }
                        }

                        mcTex.SetPixels32(colors);
                        mcTex.Apply();

                        // Save the texture with proper transparent pixels.
                        // var fi = fm.fileInfos[i];
                        // Debug.Log(fi.FullName);
                        // var testName = $"{Path.GetFileNameWithoutExtension(fi.Name)}";
                        // fm.SaveTextureToPng(mcTex, fi.DirectoryName, testName);
                    }

                    if (config.inconsistentMaskSize)
                    {
                        partPatch = new Patch(0, mcTex.height - 23, 0, mcTex.height - 23, 128, 23);
                    }
                    else
                    {
                        var line = mcTex.GetPixels(12, 0, 1, mcTex.height);
                        var foundFlag = false;
                        for (var k = 0; k < line.Length; k++)
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
                            reportSb.AppendLine(string.Concat(mcTex.name,
                                " seems fully transparent mask. Match it manually."));
                            continue;
                        }

                        line = mcTex.GetPixels(0, mcTex.height - 4, mcTex.width, 1);
                        foundFlag = false;
                        for (var k = mcTex.width - 1; k >= 0; k--)
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
                            reportSb.AppendLine(string.Concat(mcTex.name,
                                " seems fully transparent mask. Match it manually."));
                            continue;
                        }
                    }
                }

                //Generate the histogram data
                var histGenAttemptsCount = 0;
                var isMonochromatic = false;

                mc.histograms = new Histogram[config.histogramPatchCount];
                mc.HistPatches = new Patch[config.histogramPatchCount];
                //mc.histBgPartPatchIndices = new int[config.histogramPatchCount];

                for (var j = 0; j < config.histogramPatchCount; j++)
                {
                    if (mc.isMask == false)
                    {
                        if (mc.bgPartIndex != -1)
                        {
                            var bgPartPatchIndex = Random.Range(0, dumpFormat.bgParts[mc.bgPartIndex].patches.Length);
                            partPatch = dumpFormat.bgParts[mc.bgPartIndex].patches[bgPartPatchIndex];
                        }
                        else
                        {
                            partPatch = new Patch(0, 0, 0, 0, mc.texSize.x, mc.texSize.y);
                        }
                    }

                    var p = partPatch;

                    p.size = config.histogramPatchSize;

                    var patchPos = partPatch.size - config.histogramPatchSize;
                    patchPos.x = Random.Range(0, patchPos.x);
                    patchPos.y = Random.Range(0, patchPos.y);

                    p.Move(patchPos);

                    var histogram = new Histogram(mc.isMask ? 4 : 3, config.histogramStepCount, 1f);

                    var patchColors = mcTex.GetPixels(p.srcPos.x, p.srcPos.y, p.size.x, p.size.y);

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
                        var test = new Texture2D(p.size.x, p.size.y);
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
                    reportSb.AppendLine(string.Concat(mcTex.name,
                        " is too consistent visually to be analyzed properly. Match it manually."));
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
            var candidates = candidatesList.ToArray();
            var baseDumpPath = Path.Combine(dumpTexPath, baseDumpFormat.name);
            //int mcMatchesCount = 0;
            var texMatchesCount = 0;
            for (var i = 0; i < bgInfoCount; i++)
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

                var bgTex = fm.GetTextureFromPath(Path.Combine(baseDumpPath, bgInfos[i].texDumpMatches[0].texNames[0]));
                var maskTex = bgInfos[i].hasMask
                    ? fm.GetTextureFromPath(Path.Combine(baseDumpPath, bgInfos[i].texDumpMatches[0].texNames[1]))
                    : null;

                //TODO - max possible match for one bg info and one bg part, List suck ass and realistically I never saw 3 textures exactly the same (and there is no duplicate on GC)
                var bestMatchValues =
                    new List<float>[dumpFormat.bgParts.Length == 0 ? 2 : dumpFormat.bgParts.Length + 1];
                var bestMatchCandidateIndices =
                    new List<int>[dumpFormat.bgParts.Length == 0 ? 2 : dumpFormat.bgParts.Length + 1];

                for (var j = 0; j < bestMatchValues.Length; j++)
                {
                    bestMatchValues[j] = new List<float>();
                    bestMatchCandidateIndices[j] = new List<int>();
                }

                for (var j = 0; j < candidates.Length; j++)
                {
                    //if (candidates[j].isMatched)
                    //    continue;

                    var mc = candidates[j];

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
                        //This BG texture size comparison was certainly added for re3 but why?
                        //It's actually horrible for RE2 and 1. Thus I added a BG parts length check. 
                        if (dumpFormat.bgParts.Length <= 1 && (bgTex.width != mc.texSize.x || bgTex.height != mc.texSize.y))
                            continue;
                    }

                    var mcMatchValue = 0f;

                    var isImpossibleMatch = false;
                    for (var k = 0; k < mc.HistPatches.Length; k++)
                    {
                        var p = mc.HistPatches[k];

                        mc.tempHistogram.Reset();

                        Color[] pixels;
                        if (mc.isMask)
                        {
                            if (config.inconsistentMaskSize)
                            {
                                var dstPosY = maskTex.height - 23 + (p.dstPos.y - (256 - 23));
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
                            var test = new Texture2D(p.size.x, p.size.y);
                            test.SetPixels(pixels);
                            fm.SaveTextureToPng(test, "./test", (mc.isMask ? maskTex.name : bgTex.name) + "_" + k);
                            Object.Destroy(test);
                        }

                        var patchMatchValue = mc.histograms[k].Compare(mc.tempHistogram);

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
                    if (mcMatchValue >= config.candidateMinMatchValue
                    ) // && (isFirstValue || mcMatchValue >= bestMatchValues[mc.bgPartIndex][0]))
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
                var partCount = bestMatchValues.Length;
                //bool hasMatches = false;
                for (var j = 0; j < partCount; j++)
                {
                    var matchCount = bestMatchValues[j].Count;
                    for (var k = 0; k < matchCount; k++)
                    {
                        //it should be defaulted to 0 anyway.
                        if (bestMatchValues[j][k] >= config.candidateMinMatchValue)
                        {
                            //candidates[bestMatchCandidateIndices[j][k]].isMatched = true;
                            var mc = candidates[bestMatchCandidateIndices[j][k]];
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
            for (var i = 0; i < candidates.Length; i++)
            {
                var mc = candidates[i];
                if (mc.bgInfoMatchIndex.Count <= 0)
                {
                    //Report the unmatchable candidates
                    unmatchedMcCount++;
                    reportSb.Append(string.Concat("Candidate ", i, " can't find a match: "));
                    for (var j = 0; j < candidates[i].fileInfoIndices.Length; j++)
                    {
                        unmatchedTexCount++;
                        reportSb.Append(string.Concat("[", fm.fileInfos[candidates[i].fileInfoIndices[j]].Name, "]"));
                    }

                    reportSb.AppendLine();
                }
                else
                {
                    var bestBgInfoMatchValue = 0f;
                    var bestBgInfoIndex = 0;
                    for (var j = 0; j < mc.bgInfoMatchIndex.Count; j++)
                    {
                        if (mc.bgInfoMatchValue[j] > bestBgInfoMatchValue)
                        {
                            bestBgInfoMatchValue = mc.bgInfoMatchValue[j];
                            bestBgInfoIndex = mc.bgInfoMatchIndex[j];
                        }
                    }

                    //Update and save the best bg info match 
                    for (var j = 0; j < mc.fileInfoIndices.Length; j++)
                    {
                        var texName = fm.RemoveExtensionFromFileInfo(fm.fileInfos[mc.fileInfoIndices[j]]);

                        bgInfos[bestBgInfoIndex].AddDumpMatch(dumpFormat.name, texName, mc.bgPartIndex);
                        texMatchesCount++;
                    }

                    reportSb.Append(string.Concat("Candidate ", i, " matched: "));
                    for (var j = 0; j < candidates[i].fileInfoIndices.Length; j++)
                    {
                        reportSb.Append(string.Concat("[", fm.fileInfos[candidates[i].fileInfoIndices[j]].Name, "]"));
                    }

                    reportSb.Append(string.Concat(" - ", bgInfos[bestBgInfoIndex].namePrefix));
                    reportSb.Append(string.Concat(" - ", bestBgInfoMatchValue.ToString("0.00")));
                    reportSb.AppendLine();

                    fm.SaveToJson(bgInfos[bestBgInfoIndex], bgInfoPath, bgInfos[bestBgInfoIndex].GetFileName(),
                        prettifyJsonOnSave);
                }
            }

            reportSb.AppendLine(string.Format(
                "{0} matches for {1} textures ({2} duplicates). {3} unmatchable textures from {4} candidates.",
                texMatchesCount, mcTexCount, texDuplicatesCount, unmatchedTexCount, unmatchedMcCount));
            reportSb.AppendLine(string.Format("== Texture Matching done! ({0} seconds) ==",
                (Time.unscaledTime - taskTime).ToString("#.0")));

            fm.OpenFolder(bgInfoPath);

            doneCb();
        }

        private static Color peixotoBgBandColor = new Color(0.415687f, 0.443138f, 0.611765f, 1f);
        private static Color peixotoRightBgColor = new Color(0.031372f, 0.0f, 0.0f, 1f);

        private void IdentifyCandidatePeixoto(DumpFormat dumpFormat, ref MatchCandidate mc, Texture2D mcTex)
        {
            //As all the textures are squared and the BG ones are 2 256x256 textures,
            // I can quickly check that as an early identification of mask textures.
            if (mcTex.width != 256)
            {
                mc.bgPartIndex = 2;
                mc.isMask = true;
                return;
            }

            var bottomLeftColors = mcTex.GetPixels(0, 0, 16, 16);
            var avgColor = bottomLeftColors.GetAverage();
            var isBgPart = avgColor.Compare(peixotoBgBandColor) >= 0.999f;
            if (isBgPart)
            {
                var topRightColors = mcTex.GetPixels(mcTex.width - 32, mcTex.height - 32, 32, 32);
                avgColor = topRightColors.GetAverage();
                var isRightBgPart = avgColor.Compare(peixotoRightBgColor) >= 0.999f;

                mc.isMask = false;
                mc.bgPartIndex = isRightBgPart ? 1 : 0;
            }
            else
            {
                mc.bgPartIndex = 2;
                mc.isMask = true;
            }

            //sRGB or Linear doesn't seem to make any difference for these old assets.
            // Let's keep the unity default, sRGB.

            // var name = Path.GetFileNameWithoutExtension(fm.fileInfos[mc.fileInfoIndices[0]].Name);
            // var testSrgb = new Texture2D(16, 16, TextureFormat.RGBA32, false, false);
            // testSrgb.SetPixels(bottomLeftColors);
            // fm.SaveTextureToPng(testSrgb, "./", $"{name}_srgb");
            // Object.Destroy(testSrgb);

            // var testLinear = new Texture2D(16, 16, TextureFormat.RGBA32, false, true);
            // testLinear.SetPixels(bottomLeftColors);
            // fm.SaveTextureToPng(testSrgb, "./", $"{name}_linear");
            // Object.Destroy(testLinear);
        }

        private void IdentifyCandidate(DumpFormat dumpFormat, ref MatchCandidate mc, Texture2D mcTex)
        {
            if (dumpFormat.usePeixotoCandidateIdentification)
            {
                IdentifyCandidatePeixoto(dumpFormat, ref mc, mcTex);
                return;
            }

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
                    for (var j = 0; j < dumpFormat.bgParts.Length; j++)
                    {
                        if (mcTex.width != dumpFormat.bgParts[j].size.x ||
                            mcTex.height != dumpFormat.bgParts[j].size.y) continue;

                        mc.bgPartIndex = j;
                        break;
                    }
                }
            }
        }
    }
}

public static class ColorExtensions
{
    public static Color GetAverage(this Color[] colors)
    {
        var avgC = Color.clear;
        for (var i = 0; i < colors.Length; i++)
        {
            avgC += colors[i];
        }

        return avgC / colors.Length;
    }

    public static float Compare(this Color a, Color b)
    {
        var diff = Mathf.Abs(a.r - b.r) +
                   Mathf.Abs(a.g - b.g) +
                   Mathf.Abs(a.b - b.b) +
                   Mathf.Abs(a.a - b.a);
        return 1.0f - diff / 4f;
    }
}