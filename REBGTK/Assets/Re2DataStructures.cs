using System.Runtime.InteropServices;

namespace RE2
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
        public RdtCameraMask[] cameraMasks;
        public RdtMaskGroup[][] maskGroups;
        public bool[][] isRectMaskFlags;
        public RdtMask[][][] masks;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtCameraPos
    {
        public ushort end_flg;        // seems like the last entry is set to 1 while the rest to 0
        public ushort view_r;         // distance to screen, 0x683c or 0x73b7
        public int camera_from_x;  // View_p[3] - Position of the camera
        public int camera_from_y;
        public int camera_from_z;
        public int camera_to_x;    // View_r[3] - Where the camera is looking to
        public int camera_to_y;
        public int camera_to_z;
        public uint masks_offset;   // [pSp] Offset to background image masks in the file, see below
                                    // 0xffffffff if no masking
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtCameraMask
    {
        public ushort count_Groups;  /* Number of global offsets, or 0xffff for none */
        public ushort count_masks;    /* Number of masks, or 0xffff for none */
    }

    public enum RdtObject
    {
        ROB_EDT0,           // [08] pEdt0: Sound Table
        ROB_VH,             // [0C] pVh0: embedded VAB file
        ROB_VB,             // [10] pVb0: ---
        ROB_EDT1,           // [14] pEdt1: Sound table
        ROB_VHX,            // [18] pVh1: embedded VAB file
        ROB_VBX,            // [1C] pVb1: ---
        ROB_BOUNDARY,       // [20] Room boundaries (.SCA)
        ROB_CAMERA,         // [24] pRcut: camera positions for each image, background image masks (.RID)
        ROB_CAMERA_SWITCH,  // [28] pVcut: camera switches (.RVD)
        ROB_CAMERA_LIGHT,   // [2C] pLight: lights (.LIT)
        ROB_MD1_OBJECT,     // [30] 3D Object model pointers [ptr0:tim, ptr1:md1]
        ROB_FLOOR,          // [34] Floor attributes (.FLR)
        ROB_BLOCK,          // [38] Block attributes (.BLK)
        ROB_TEXT,           // [3C] encoded text for stuff you can examine
        ROB_DOOR_WORK,      // [40] data about how doors work and connect other rooms (.SCD?)
        ROB_EVENT_INIT,     // [44] initialization script (.SCD)
        ROB_EVENT,          // [48] execution script (.SCD)
        ROB_PREGUNTA,
        ROB_SPRT_DESC,      // [4C] Effect sprite ID table & pointers (.ESP)
        ROB_EFFECT_PTR,     // [50] Pointers to .EFF data, subtract -0x20 (.ESP)
        ROB_TIM_EFFECT,     // [54] Effect sprite textures (.EFF) / POINTER - EFFECT (TIM)
        ROB_TIM_OBJECT,     // [58] 3D Object model textures (.TIM) / POINTER - MODEL (TIM)
        ROB_3D_ANIM,        // [5C] Player/room unique animations (.RBJ)
                            /* ----- */
        ROB_COUNT
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtHeader
    {
        public byte nSprite;
        public byte nCut;          // Number of objects at offset 7, seems to be number of cameras of the room
        public byte nOmodel;       // Number of objects at offset 10
        public byte nItem;         // unused?
        public byte nDoor;         // unused?
        public byte nRoom_at;      // unused?
        public byte Reverb_lv;     // unused?
        public byte nSprite_max;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)RdtObject.ROB_COUNT)]
        public uint[] ptr; // pointers to each section
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtMaskGroup
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
        public ushort size;         /* Width and height of mask */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RdtMask
    {
        public byte u;           /* Source position in common/objspr/rsRXXP.adt image file */
        public byte v;
        public byte x;           /* Destination position on background image/screen */
        public byte y;
        public ushort depth;         /* Distance/32 from camera */
        public ushort size;
        public ushort width, height; /* Dimensions of mask */
    }
}
