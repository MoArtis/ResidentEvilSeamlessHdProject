using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

namespace RE1
{
    public class RdtParser
    {
        private RdtRoom _room;

        public bool ParseRdtData(byte[] data, string filename, out RdtRoom room)
        {
            room = new RdtRoom();

            _room.stage = filename.Substring(4, 1);
            _room.room = filename.Substring(5, 2);
            _room.player = filename.Substring(7, 1);
            _room.name = string.Concat("ROOM_", _room.stage, _room.room, "_P", _room.player);

            var header = MarshalIntoStructure<RdtHeader>(data, 0);

            _room.header = header;

            ParseCameraPos(data, header.nCut);

            if (_room.header.nCut <= 0)
            {
                Debug.LogWarning($"{_room.name} has no Camera position... 🤔");
                return false;
            }
            
            //Return the room data
            room = _room;
            return true;
        }

        private void ParseCameraPos(byte[] data, int cameraPosCount)
        {
            var hasMasks = new bool[cameraPosCount];

            var cameraPos = new RdtCameraPos[cameraPosCount];
            var maskGroupHeaders = new RdtMaskGroupsHeader[cameraPosCount];
            var maskGroups = new RdtMaskGroup[cameraPosCount][];

            //3d Array... it might be better just creating handler/Container types...
            var masks = new RdtRectMask[cameraPosCount][][];
            _room.masks = masks;

            for (var i = 0; i < cameraPosCount; i++)
            {
                var cameraPosOffset = Marshal.SizeOf(typeof(RdtHeader)) + i * Marshal.SizeOf(typeof(RdtCameraPos));
                cameraPos[i] = MarshalIntoStructure<RdtCameraPos>(data, cameraPosOffset);

                if (cameraPos[i].masks_offset == uint.MaxValue) continue;

                var maskGroupsHeaderOffset = (int) cameraPos[i].masks_offset;
                maskGroupHeaders[i] = MarshalIntoStructure<RdtMaskGroupsHeader>(data, maskGroupsHeaderOffset);
                var maskGroupCount = maskGroupHeaders[i].count_Groups;

                //If there is mask for that camera position
                if (maskGroupCount <= 0 || maskGroupCount == ushort.MaxValue) continue;

                hasMasks[i] = true;

                var masksOffset = maskGroupsHeaderOffset + Marshal.SizeOf(typeof(RdtMaskGroupsHeader));
                masksOffset += maskGroupCount * Marshal.SizeOf(typeof(RdtMaskGroup));

                masks[i] = new RdtRectMask[maskGroupCount][];
                maskGroups[i] = new RdtMaskGroup[maskGroupCount];
                for (var j = 0; j < maskGroupCount; j++)
                {
                    var maskGroupOffset = maskGroupsHeaderOffset + Marshal.SizeOf(typeof(RdtMaskGroupsHeader));
                    maskGroupOffset += j * Marshal.SizeOf(typeof(RdtMaskGroup));
                    maskGroups[i][j] = MarshalIntoStructure<RdtMaskGroup>(data, maskGroupOffset);

                    //Process and parse the masks themselve - Return a new mask offset
                    masksOffset = ParseMaskGroup(data, masksOffset, maskGroups[i][j].count, i, j);
                }
            }

            _room.hasMasks = hasMasks;
            _room.cameraPos = cameraPos;
            _room.cameraMasks = maskGroupHeaders;
            _room.maskGroups = maskGroups;
        }

        private int ParseMaskGroup(byte[] data, int maskOffset, int masksCount, int camPosIndex, int groupIndex)
        {
            _room.masks[camPosIndex][groupIndex] = new RdtRectMask[masksCount];
            for (var i = 0; i < masksCount; i++)
            {
                var rectMask = new RdtRectMask();
                var sqrMask = MarshalIntoStructure<RdtSquareMask>(data, maskOffset);
                // var fourBitSize = (ushort) (sqrMask.size & 0x0000000000001111);
                // Debug.Log(fourBitSize);
                // if (fourBitSize == 0)
                if (sqrMask.size == 0)
                {
                    rectMask = MarshalIntoStructure<RdtRectMask>(data, maskOffset);
                    // rectMask.width *= 2;
                    // rectMask.height *= 2;
                    maskOffset += Marshal.SizeOf(typeof(RdtRectMask));
                }
                else
                {
                    rectMask.u = sqrMask.u;
                    rectMask.v = sqrMask.v;
                    rectMask.x = sqrMask.x;
                    rectMask.y = sqrMask.y;

                    rectMask.depth = sqrMask.depth;
                    // rectMask.width = rectMask.height = (ushort) (fourBitSize * 8);
                    rectMask.width = rectMask.height = (ushort)(sqrMask.size / 2);

                    maskOffset += Marshal.SizeOf(typeof(RdtSquareMask));
                }

                _room.masks[camPosIndex][groupIndex][i] = rectMask;
            }

            return maskOffset;
        }

        private T MarshalIntoStructure<T>(byte[] data, int offset)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            T structure;
            try
            {
                structure = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject() + offset, typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return structure;
        }
    }
}