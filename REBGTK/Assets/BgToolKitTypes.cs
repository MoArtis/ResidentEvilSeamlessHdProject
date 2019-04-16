﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BgTk
{
    public enum BgInfoStatus
    {
        Todo,
        Done,
        Tested
    }

    public struct MatchCandidate
    {
        public int[] fileInfoIndices; //An Array of int to handle duplicates
        public string md5;
        public Vector2Int texSize;

        public Histogram[] histograms;
        //public int[] histBgPartPatchIndices;
        public Patch[] HistPatches;

        public Histogram bgHistogram;

        //public bool isMatched;
        public bool isMask;
        public int bgPartIndex;

        //TODO - convert them into array? I need to add element conveniently and the list will never be big.
        public List<int> bgInfoMatchIndex;
        public List<float> bgInfoMatchValue;

        public void SetFileInfoIndices(int[] indices)
        {
            fileInfoIndices = indices;
        }

    }

    public struct Histogram
    {
        public Histogram(int channelCount, int stepCount, float maxValue)
        {
            this.channelCount = channelCount;
            this.stepCount = stepCount;

            channels = new int[channelCount, stepCount];
            step = maxValue / stepCount;

            valueCount = 0;
        }

        public int[,] channels;
        public float step;

        private int channelCount;
        private int stepCount;
        private int valueCount;

        public void Reset()
        {
            valueCount = 0;

            //Is it better(and what do I mean by better?) than a new int[,] ?
            int bound0 = channels.GetUpperBound(0);
            int bound1 = channels.GetUpperBound(1);
            for (int i = 0; i <= bound0; i++)
            {
                for (int j = 0; j <= bound1; j++)
                {
                    channels[i, j] = 0;
                }
            }
        }

        public void AddValues(Color[] colors, bool clipTransparentPixels = false)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (clipTransparentPixels && colors[i].a <= 0.5f)
                {
                    AddValue(colors[i][3], 3);
                    continue;
                }

                for (int j = 0; j < channelCount; j++)
                {
                    AddValue(colors[i][j], j);
                }
            }
        }

        public void AddValue(float value, int channelIndex)
        {
            int stepIndex = Mathf.FloorToInt(value / step);
            stepIndex = Mathf.Min(stepIndex, stepCount - 1); //handle value = max value
            channels[channelIndex, stepIndex]++;
            valueCount++;
        }

        public float Compare(Histogram other)
        {
            if (channelCount != other.channelCount)
                return 0f;

            if (stepCount != other.stepCount)
                return 0f;

            if (valueCount != other.valueCount)
                return 0f;

            int cumulatedDiff = 0;
            for (int c = 0; c < channelCount; c++)
            {
                int cumulatedStepDiff = 0;
                for (int s = 0; s < stepCount; s++)
                {
                    int stepDiff = Mathf.Abs(channels[c, s] - other.channels[c, s]);
                    cumulatedStepDiff += stepDiff;
                }
                cumulatedDiff += (cumulatedStepDiff);
            }
            return 1f - ((float)cumulatedDiff / (float)(valueCount * channelCount));
        }
    }

    public enum ScalingType
    {
        Point,
        Bilinear,
        xBRZ
    }

    [System.Serializable]
    public struct AlphaChannelConfig
    {
        public int scaleRatio;

        public ScalingType scalingType;

        public bool hasBlur;
        public int blurRadius;
        public int blurIteration;

        public float clipValue;
        public float boostValue;

        public bool saveDebugTextures;
    }

    [System.Serializable]
    public struct TextureMatchingConfig
    {
        public int histogramPatchCount;
        public Vector2Int histogramPatchSize;
        public int histogramStepCount;
        public int histGenAttemptsMaxCount;
        public float patchMinMatchValue;
        public float candidateMinMatchValue;
    }

    [System.Serializable]
    public struct DumpMatch
    {
        public DumpMatch(string formatName, string firstTexName, int partIndex)
        {
            this.formatName = formatName;
            texNames = new string[] { firstTexName };
            partIndices = new int[] { partIndex };
        }

        public string formatName;
        public string[] texNames; //0 - Left, 1 - Right, 2 - Mask... Can it be more? Maybe in other CAPCOM games?
        public int[] partIndices;

        public void AddTexName(string name, int partIndex)
        {
            texNames = texNames.Concat(new string[] { name }).ToArray();
            partIndices = partIndices.Concat(new int[] { partIndex }).ToArray();
        }
    }

    [System.Serializable]
    public struct BgTexturePart
    {
        public string name;
        public bool needGapCompensation; //Will shift the bottom part of the right part one pixel down. Only needed for Dolphin.
        public Patch[] patches;
        public Vector2Int size;
    }

    [System.Serializable]
    public struct DumpFormat
    {
        public string name;
        public BgTexturePart[] bgParts; //if none = The BG texture remains whole

        public override bool Equals(object obj)
        {
            if (!(obj is DumpFormat))
            {
                return false;
            }

            var format = (DumpFormat)obj;
            return name == format.name;
        }

        public override int GetHashCode()
        {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(name);
        }
    }

    public struct ProgressInfo
    {
        public ProgressInfo(string label, int currentIndex, int totalIndex, float progress)
        {
            this.label = label;
            this.currentIndex = currentIndex;
            this.totalIndex = totalIndex;
            this.progress = progress;
        }

        public string label;
        public int currentIndex;
        public int totalIndex;
        public float progress; //Not necesseraly current
    }

    [System.Serializable]
    public struct Patch
    {
        public Patch(int srcX, int srcY, int dstX, int dstY, int w, int h)
        {
            srcPos = new Vector2Int(srcX, srcY);
            dstPos = new Vector2Int(dstX, dstY);
            size = new Vector2Int(w, h);
        }

        public Vector2Int srcPos; //Where to pick into the source texture (mask text, split bg files)
        public Vector2Int dstPos; //Where to place/pick into the assembled BG texture
        public Vector2Int size;

        public void Scale(float ratio)
        {
            srcPos.x = Mathf.RoundToInt(srcPos.x * ratio);
            srcPos.y = Mathf.RoundToInt(srcPos.y * ratio);

            dstPos.x = Mathf.RoundToInt(dstPos.x * ratio);
            dstPos.y = Mathf.RoundToInt(dstPos.y * ratio);

            size.x = Mathf.RoundToInt(size.x * ratio);
            size.y = Mathf.RoundToInt(size.y * ratio);
        }

        public void Move(Vector2Int translation)
        {
            srcPos += translation;
            dstPos += translation;
        }
    }

    [System.Serializable]
    public struct Mask
    {
        public Patch patch;
        public int groupIndex;
        public int[] opaqueIndices; //It seems studid to store the alpha in a file and as text.
        //TODO - a test to store the needed alpha from the top as png ? it can be processed to generate AA (keep two file)
		public bool ignoreAltMaskSource;
    }

    [System.Serializable]
    public struct BgInfo
    {
        //The Texture prefixes in the CR format. ROOM_XXX_YY. 
        //An array for different Camera Position (BG Info) sharing the same textures - Only When the BG AND the Mask textures are identical (Hash comparison).
        public string namePrefix;
        //public string[] rdtFilenames;

        public BgInfoStatus status;

        public string bgMd5;
        public Vector2Int bgTexSize;

        //Tex Dump Matches - Texture names for the formats/platforms other than Classic Rebirth.
        public DumpMatch[] texDumpMatches;

        //Mask
        public bool hasMask;
        public int camPosIndex;
        public int groupsCount;
        public string maskMd5;
        public Vector2Int maskTexSize;
        public bool useProcessedMaskTex;       //For tex generation, will throw an error (in the log) if name_mask.png is missing
        public Mask[] masks;

        public string GetFileName()
        {
            return "BGINFO_" + namePrefix;
        }

        public void ResetDumpMatches(string formatName)
        {
            for (int i = 0; i < texDumpMatches.Length; i++)
            {
                if(texDumpMatches[i].formatName == formatName)
                {
                    texDumpMatches[i].partIndices = new int[0];
                    texDumpMatches[i].texNames = new string[0];
                }
            }
        }

        public void AddDumpMatch(string formatName, string name, int partIndex)
        {
            for (int i = 0; i < texDumpMatches.Length; i++)
            {
                if (texDumpMatches[i].formatName == formatName)
                {
                    texDumpMatches[i].AddTexName(name, partIndex);
                    return;
                }
            }

            //This format is not listed in this bginfo, create a new one.
            texDumpMatches = texDumpMatches.Concat(new DumpMatch[1] { new DumpMatch(formatName, name, partIndex) }).ToArray();
        }

        public void SetCamPosIndex(int index)
        {
            camPosIndex = index;
        }

        public void Reset()
        {
            namePrefix = null;
            //rdtFilenames = null;

            status = 0;

            bgMd5 = null;
            bgTexSize.x = bgTexSize.y = 0;

            hasMask = false;
            maskMd5 = null;
            maskTexSize.x = maskTexSize.y = 0;
            useProcessedMaskTex = false;
            masks = null;

            texDumpMatches = null;
        }
    }

}
