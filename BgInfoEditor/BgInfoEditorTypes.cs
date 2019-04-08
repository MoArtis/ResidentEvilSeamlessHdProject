using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Media;

namespace BgInfoEditor
{
    public struct Vector2
    {
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x { get; set; }
        public int y { get; set; }
    }

    public struct Patch
    {
        public Patch(int srcX, int srcY, int dstX, int dstY, int w, int h)
        {
            srcPos = new Vector2(srcX, srcY);
            dstPos = new Vector2(dstX, dstY);
            size = new Vector2(w, h);
        }

        public Vector2 srcPos; //Where to pick into the source texture (mask text, split bg files)
        public Vector2 dstPos; //Where to place/pick into the assembled BG texture
        public Vector2 size;

        public void Scale(float ratio)
        {
            srcPos.x = (int)Math.Round(srcPos.x * ratio);
            srcPos.y = (int)Math.Round(srcPos.y * ratio);

            dstPos.x = (int)Math.Round(dstPos.x * ratio);
            dstPos.y = (int)Math.Round(dstPos.y * ratio);

            size.x = (int)Math.Round(size.x * ratio);
            size.y = (int)Math.Round(size.y * ratio);
        }

        public void Move(Vector2 translation)
        {
            srcPos += translation;
            dstPos += translation;
        }
    }

    public struct Mask
    {
        public Patch patch;
        public int groupIndex;
        public int[] opaqueIndices;
        public bool ignoreAltMaskSource;
    }

    public struct EditableMask
    {
        public EditableMask(int test)
        {
            index = 0;
            Margin = new Thickness(
                200.0 * MainWindow.random.NextDouble(),
                0.0,
                0.0,
                200.0 * MainWindow.random.NextDouble());

            Row = MainWindow.random.Next(0, 4);
            Column = MainWindow.random.Next(0, 4);

            width = 0.0;
            height = 0.0;
            ignoreAltMaskSource = false;
        }

        public int index;
        public Thickness Margin { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public double width;
        public double height;
        public bool ignoreAltMaskSource;
    }

    public enum BgInfoStatus
    {
        Todo,
        Done,
        Tested
    }

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


    public struct BgInfo
    {
        //The Texture prefixes in the CR format. ROOM_XXX_YY. 
        //An array for different Camera Position (BG Info) sharing the same textures - Only When the BG AND the Mask textures are identical (Hash comparison).
        public string namePrefix;
        //public string[] rdtFilenames;

        public BgInfoStatus status;

        public string bgMd5;
        public Vector2 bgTexSize;

        //Tex Dump Matches - Texture names for the formats/platforms other than Classic Rebirth.
        public DumpMatch[] texDumpMatches;

        //Mask
        public bool hasMask;
        public int camPosIndex;
        public int groupsCount;
        public string maskMd5;
        public Vector2 maskTexSize;
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
                if (texDumpMatches[i].formatName == formatName)
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
            bgTexSize.x = bgTexSize.x = 0;

            hasMask = false;
            maskMd5 = null;
            maskTexSize.x = maskTexSize.y = 0;
            useProcessedMaskTex = false;
            masks = null;

            texDumpMatches = null;
        }
    }

    public class CurrentBgInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string arg)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(arg));
        }

        private BgFileInfo bgFileInfo;
        public BgFileInfo BgFileInfo
        {
            get { return bgFileInfo; }
            set
            {
                bgFileInfo = value;
                OnPropertyChanged("BgFileInfo");
                OnPropertyChanged("OMaskImage");
            }
        }

        public BitmapImage RMaskImage { get; set; }
        public BitmapImage OMaskImage { get; set; }

        public BgInfo BgInfo { get; set; }
    }

    public struct BgFileInfo
    {
        public string DisplayName { get; set; }
        public FileInfo Fi { get; set; }
        public int Index { get; set; }
    }
}
