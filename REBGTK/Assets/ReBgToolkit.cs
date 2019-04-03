//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using RE2;

//public class ReBgToolkit : MonoBehaviour
//{
//    [SerializeField] protected string filename;

//    FileManager fileManager = new FileManager();
//    RdtParser rdtParser = new RdtParser();

//    void Start()
//    {
//        byte[] data = fileManager.GetBytesFromFile(filename);

//        rdtParser.ParseRdtData(data, out RdtRoom room);

//        Debug.Log(room.header.nCut);

//        for (int i = 0; i < room.cameraPos.Length; i++)
//        {
//            Debug.Log("<color=red> - Camera Pos - " + i + "</color>");
//            Debug.Log("Flag:" + room.cameraPos[i].end_flg);
//            Debug.Log("Group Count:" + room.cameraMasks[i].count_Groups);
//            Debug.Log("Mask Count:" + room.cameraMasks[i].count_masks);
//            if (room.maskGroups[i] != null)
//            {
//                for (int j = 0; j < room.maskGroups[i].Length; j++)
//                {
//                    Debug.Log("GROUP " + j);
//                    Debug.Log("Count:" + room.maskGroups[i][j].count);
//                    Debug.Log("clut:" + room.maskGroups[i][j].clut);
//                    Debug.Log("x:" + room.maskGroups[i][j].x);
//                    Debug.Log("y:" + room.maskGroups[i][j].y);

//                    Debug.Log("- MASKS - " + room.masks[i][j].Length);
//                    for (int k = 0; k < room.masks[i][j].Length; k++)
//                    {
//                        Debug.Log("<color=green> Mask " + k + "</color>");
//                        Debug.Log(string.Concat("Src: ", room.masks[i][j][k].u, " - ", room.masks[i][j][k].v));
//                        Debug.Log(string.Concat("Dest: ", room.masks[i][j][k].x, " - ", room.masks[i][j][k].y));
//                        Debug.Log(string.Concat("Depth: ", room.masks[i][j][k].depth));
//                        Debug.Log(string.Concat("Size: ", room.masks[i][j][k].size));
//                        Debug.Log(string.Concat("WH: ", room.masks[i][j][k].width, " - ", room.masks[i][j][k].height));
//                    }
//                }

//            }
//            else
//            {
//                Debug.Log("No mask");
//            }
//        }
//    }

//}
