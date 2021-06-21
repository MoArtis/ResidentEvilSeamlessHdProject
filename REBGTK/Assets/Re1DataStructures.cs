using System.Linq;
using System.Runtime.InteropServices;

namespace RE1
{
    //Custom container type
    public struct RdtRoom
    {
        //Helper fields (Not from the RDT files themselve)
        public string name; // ROOM_StageRoom_Player

        public string stage;
        public string room;
        public string player;

        public bool[] hasMasks;     //One index per Camera Pos

        //Game Data
        public RdtHeader header;

        public RdtCameraPos[] cameraPos;
        public RdtMaskGroupsHeader[] cameraMasks;
        public RdtMaskGroup[][] maskGroups;
        public bool[][] isRectMaskFlags;
        public RdtRectMask[][][] masks;
        
        public override string ToString()
        {
	        return $"{name}: {stage} - {room} - {header.nCut} - {maskGroups.Sum(x=>x.Sum(y=>y.count))}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtCameraPos
    {
        public uint masks_offset;   // offset to RID data
        public uint pTim;        // seems like the last entry is set to 1 while the rest to 0
        public int camera_from_x;  // View_p[3] - Position of the camera
        public int camera_from_y;
        public int camera_from_z;
        public int camera_to_x;    // View_r[3] - Where the camera is looking to
        public int camera_to_y;
        public int camera_to_z;
        public uint zero_0, zero_1;
        public uint view_r;         // distance to screen, 0x683c or 0x73b7
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtMaskGroupsHeader //tagPspHeader
    {
        public ushort count_Groups;  /* Number of global offsets, or 0xffff for none */
        public ushort count_masks_unused;    /* unused in RE1 */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtHeader
    {
        public byte nSprite;
        public byte nCut;          // Number of objects at offset 7, seems to be number of cameras of the room
        public byte nItem;         // unused?
        public byte nOmodel;       // Number of objects at offset 10
        public byte nDoor;         // unused?
        public sbyte nRoom_at;      // unused?
		public ushort ambient_x, ambient_y, ambient_z;
		public LightData Light_x, ligh_y, light_z;
		public uint pVcut;			// 0x48 [DONE]
		public uint pSca;			// 0x4C [DONE]
		public uint pObj_0, pObj_1;		// 0x50-54
		public uint pBlk;			// 0x58
		public uint pFlr;			// 0x5C
		public uint pScrl;			// 0x60 SCD initializer
		public uint pScdx;			// 0x64 SCD threading
		public uint pScd;			// 0x68 useless entry, keep for legacy
		public uint pEmr;			// 0x6C [DONE]
		public uint pEdd;			// 0x70 [DONE]
		public uint pMessage;		// 0x74
		public uint pRaw;			// 0x78
		public uint pEsp;			// 0x7C
		public uint pEff;			// 0x80
		public uint pTim;			// 0x84
		public uint pEdt, pVh, pVb;	// 0x88-8C-90
		
		// public RdtCameraPos[] Cut;		// 0x94 x[nCut] number of elements
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LightData
    {
	    public int x, y, z;
	    public byte r, g, b;
	    public byte dummy01;		// unused
	    public ushort mode;
	    public ushort luminosity;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtMaskGroup //tagPspGroup
    {
        public ushort count;        /* Number of masks, with which to use this structure */
        public ushort clut;         //Color lookup table?
        public short x;             /* Destination position on background image/screen, to be added */
        public short y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtSquareMask
    {
        public byte u;          /* Source position in common/objspr/rsRXXP.adt image file */
        public byte v;
        public byte x;          /* Destination position on background image/screen */
        public byte y;
        public ushort depth;        /* Distance/32 from camera */
        
        public byte tpage;        
        public byte size;         /* Width and height of mask */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtRectMask
    {
        public byte u;           /* Source position in common/objspr/rsRXXP.adt image file */
        public byte v;
        public byte x;           /* Destination position on background image/screen */
        public byte y;
        public ushort depth;         /* Distance/32 from camera */
        
        public ushort size; //tpage ? zero ?
        public ushort width, height; /* Dimensions of mask */
    }
}
