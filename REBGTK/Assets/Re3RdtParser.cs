using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

namespace RE3
{
    public class RdtParser
    {
        RdtRoom room;

        public bool ParseRdtData(byte[] data, string filename, out RdtRoom outRoom)
        {
            outRoom = new RdtRoom();

            room.stage = filename.Substring(1, 1);
            room.room = filename.Substring(2, 2);
            room.player = "0";//filename.Substring(7, 1); //No player in RE3?
            room.name = string.Concat("R", room.stage, room.room, "_P", room.player);

            RdtHeader header = MarshalIntoStructure<RdtHeader>(data, 0);

            room.header = header;

            ParseCameraPos(data, (int)header.ptr[(int)RdtObject.ROB_CAMERA], header.nCut);

            //Return the room data
            outRoom = room;
            return true;
        }

        private void ParseCameraPos(byte[] data, int segment, int cameraPosCount)
        {
            bool[] hasMasks = new bool[cameraPosCount];

            RdtCameraPos[] cameraPos = new RdtCameraPos[cameraPosCount];
            RdtCameraMask[] cameraMasks = new RdtCameraMask[cameraPosCount];
            RdtMaskGroup[][] maskGroups = new RdtMaskGroup[cameraPosCount][];

            //3d Array... it might be better just creating handler/Container types...
            RdtMask[][][] masks = new RdtMask[cameraPosCount][][];
            room.masks = masks;

            for (int i = 0; i < cameraPosCount; i++)
            {
                int cameraPosOffset = segment + i * Marshal.SizeOf(typeof(RdtCameraPos));
                cameraPos[i] = MarshalIntoStructure<RdtCameraPos>(data, cameraPosOffset);
                if (cameraPos[i].masks_offset != uint.MaxValue)
                {
                    int camPosMasksOffset = (int)cameraPos[i].masks_offset;
                    cameraMasks[i] = MarshalIntoStructure<RdtCameraMask>(data, camPosMasksOffset);
                    ushort groupCount = cameraMasks[i].count_Groups;
                    ushort masksCount = cameraMasks[i].count_masks;

                    //If there is mask for that camera position
                    if (groupCount != ushort.MaxValue && masksCount != ushort.MaxValue)
                    {
                        hasMasks[i] = true;

                        int masksOffset = camPosMasksOffset + Marshal.SizeOf(typeof(RdtCameraMask));
                        masksOffset += groupCount * Marshal.SizeOf(typeof(RdtMaskGroup));

                        masks[i] = new RdtMask[groupCount][];
                        maskGroups[i] = new RdtMaskGroup[groupCount];
                        for (int j = 0; j < groupCount; j++)
                        {
                            int maskGroupOffset = camPosMasksOffset + Marshal.SizeOf(typeof(RdtCameraMask));
                            maskGroupOffset += j * Marshal.SizeOf(typeof(RdtMaskGroup));
                            maskGroups[i][j] = MarshalIntoStructure<RdtMaskGroup>(data, maskGroupOffset);

                            //Process and parse the masks themselve - Return a new mask offset
                            masksOffset = ParseMaskGroup(data, masksOffset, maskGroups[i][j].count, i, j);
                        }
                    }
                }
            }

            room.hasMasks = hasMasks;
            room.cameraPos = cameraPos;
            room.cameraMasks = cameraMasks;
            room.maskGroups = maskGroups;
        }

        private int ParseMaskGroup(byte[] data, int maskOffset, int masksCount, int camPosIndex, int groupIndex)
        {
            room.masks[camPosIndex][groupIndex] = new RdtMask[masksCount];
            for (int i = 0; i < masksCount; i++)
            {
                RdtMask mask = MarshalIntoStructure<RdtMask>(data, maskOffset);
                if (mask.size == 0)
                {
                    maskOffset += Marshal.SizeOf(typeof(RdtMask));
                }
                else
                {
                    mask.width = mask.height = mask.size;
                    maskOffset += Marshal.SizeOf(typeof(RdtSquareMask));
                }
                room.masks[camPosIndex][groupIndex][i] = mask;
            }
            return maskOffset;
        }

        private T MarshalIntoStructure<T>(byte[] data, int offset)
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            T structure;
            try
            {
                structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject() + offset, typeof(T));
            }
            finally
            {
                handle.Free();
            }
            return structure;
        }
    }
}