using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RESHDP_PackConv
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/dxgiformat/ne-dxgiformat-dxgi_format
    /// </summary>
    public enum DxgiFormat
    {
        BC1_UNORM,
        BC1_UNORM_SRGB,
        BC3_UNORM,
        BC3_UNORM_SRGB,
        BC6H_UF16,
        BC6H_SF16,
        BC7_UNORM,
        BC7_UNORM_SRGB
    }

    public enum AlphaOptions
    {
        None = 0,
        Straight = 1,
        Premultiplied = 2,
    }

    public struct FolderStructure
    {
        public string name;
        public TextureFolder[] TexFolders { get; set; }
    }

    public class TextureFolder
    {
        public string FolderPath { get; set; }
        public DxgiFormat Format { get; set; }
    }
}
