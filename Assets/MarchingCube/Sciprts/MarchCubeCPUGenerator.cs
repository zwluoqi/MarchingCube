using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCube.Sciprts
{
    public class MarchCubeCPUGenerator
    {


        public List<Vector3> GetVertex(ShapeSetting shapeSetting)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            List<Vector3> vector3S = new List<Vector3>();
            // var cubePos = GetCubePos(pos,scale);

                        
            for (int y = 0; y < shapeSetting.resolution.y; y++)
            {
                for (int x = 0; x < shapeSetting.resolution.x; x++)
                {
                    {
                        for (int z = 0; z < shapeSetting.resolution.z; z++)
                        {
                            var offset = new Vector3(x, y, z);
                            GenerateCube(ref vector3S, offset, shapeSetting);
                        }
                    }
                }
            }
            
            // File.WriteAllText(shapeSetting.resolution+"_result.txt",stringBuilder.ToString());
            return vector3S;
        }

        float KeyFun(Vector3 pos)
        {
            return (pos - Vector3.one).magnitude - 0.5f;
        }

         float KeyFun2(Vector3 pos)
         {
             return pos.y - Unity.Mathematics.noise.snoise(pos);
         }

         float KeyFun3(Vector3 pos)
         {
             var x = pos.x - 1.0f;
             var y = pos.y - 1.0f;
             var z = pos.z - 1.0f;
             
             return math.sin(x*y + x*z + y*z) + math.sin(x*y) + math.sin(y*z) + math.sin(x*z) - 0.5f;
         }

         public StringBuilder stringBuilder = new StringBuilder();
        
        void GenerateCube(ref List<Vector3> vertexs,Vector3 offset,ShapeSetting shapeSetting)
        {
            //TODO
            var vertexMapping = MarchingCubeLookupTable.vertexMapping;
            var edgeMaskTable = MarchingCubeLookupTable.edgeMaskTable;
            var pointTable = MarchingCubeLookupTable.pointTable;
            int density = 0;
            int isoVal = 0;
            Vector3[] interpVertices = new Vector3[12];
            int[] values = new int[8];

            
            for (int i = 0; i < 8; i++)
            {
                var vertex = vertexMapping[i];
                float noiseValue = 0;
                var x = shapeSetting.roughness * (pointTable[vertex] + offset) + shapeSetting.offset;
                if (shapeSetting.type == ShapeType.Noise)
                {
                    noiseValue = KeyFun2(x);
                }
                else if(shapeSetting.type == ShapeType.Circle)
                {
                    x *= shapeSetting.cubeSize;
                    noiseValue = KeyFun(x);
                }
                else if(shapeSetting.type == ShapeType.SinSurface)
                {
                    noiseValue = KeyFun3(x);
                }
                
                values[vertex] = (int)(noiseValue*255);
                if (values[vertex] < isoVal)
                {
                    density += (1 << vertex);
                }
            }
            stringBuilder.AppendLine("density:"+density+" offset："+offset);

            if (edgeMaskTable[density] == 0)
            {
                return;
            }



            var triTable = MarchingCubeLookupTable.triTable;
            // var edgeTable = MarchingCubeLookupTable.edgeTable;
            
            /* Find the vertices where the surface intersects the cube */
            if ((edgeMaskTable[density] & 1) != 0)
                interpVertices[0] = VertexInterp(pointTable[0], pointTable[1], values[0], values[1], isoVal);

            if ((edgeMaskTable[density] & 2) != 0)
                interpVertices[1] = VertexInterp(pointTable[1], pointTable[2], values[1], values[2], isoVal);

            if ((edgeMaskTable[density] & 4) != 0)
                interpVertices[2] =
                    VertexInterp(pointTable[2], pointTable[3], values[2], values[3], isoVal);

            if ((edgeMaskTable[density] & 8) != 0)
                interpVertices[3] =
                    VertexInterp(pointTable[3], pointTable[0], values[3], values[0], isoVal);

            if ((edgeMaskTable[density] & 16) != 0)
                interpVertices[4] =
                    VertexInterp(pointTable[4], pointTable[5], values[4], values[5], isoVal);

            if ((edgeMaskTable[density] & 32) != 0)
                interpVertices[5] = VertexInterp(pointTable[5], pointTable[6], values[5], values[6], isoVal);

            if ((edgeMaskTable[density] & 64) != 0)
                interpVertices[6] = VertexInterp(pointTable[6], pointTable[7], values[6], values[7], isoVal);

            if ((edgeMaskTable[density] & 128) != 0)
                interpVertices[7] = VertexInterp(pointTable[7], pointTable[4], values[7], values[4], isoVal);

            if ((edgeMaskTable[density] & 256) != 0)
                interpVertices[8] = VertexInterp(pointTable[0], pointTable[4], values[0], values[4], isoVal);

            if ((edgeMaskTable[density] & 512) != 0)
                interpVertices[9] = VertexInterp(pointTable[1], pointTable[5], values[1], values[5], isoVal);

            if ((edgeMaskTable[density] & 1024) != 0)
                interpVertices[10] = VertexInterp(pointTable[2], pointTable[6], values[2], values[6], isoVal);

            if ((edgeMaskTable[density] & 2048) != 0)
                interpVertices[11] = VertexInterp(pointTable[3], pointTable[7], values[3], values[7], isoVal);

            List<Vector3> triangles = new List<Vector3>();
            for (int i = 0; i < 16; i++)
            {
                var edgeIndex = triTable[density, i];
                if (edgeIndex == -1)
                {
                    break;
                }

                triangles.Add((interpVertices[edgeIndex] + offset)*shapeSetting.cubeSize);
            }

            triangles.Reverse();
            vertexs.AddRange(triangles);
        }
        

         private Vector3 VertexInterp(Vector3 p1, Vector3 p2, int v1, int v2, int isoVal)
         {
             if (isoVal == v1)
             {
                 return p1;
             }
             if (isoVal == v2)
             {
                 return p2;
             }

             if (v1 == v2)
             {
                 return p2;
             }

             float t = (isoVal - v1)*1.0f / (v2 - v1);
             return Vector3.Lerp(p1, p2, t);
         }

         
         #region Test
         public List<Vector3> GenerateCubeByDensity(int density)
         {
             List<Vector3> vector3s = new List<Vector3>();
             var triTable = MarchingCubeLookupTable.triTable;
             var edgeTable = MarchingCubeLookupTable.edgeTable;
             var pointTable = MarchingCubeLookupTable.pointTable;
             for (int i = 0; i < 16; i++)
             {
                 var edgeIndex = triTable[density, i];
                 if (edgeIndex == -1)
                 {
                     break;
                 }

                 var point0 = edgeTable[edgeIndex,0];
                 var point1 = edgeTable[edgeIndex,1];
                 var pos = pointTable[point0] + pointTable[point1];
                 vector3s.Add(pos * 0.5f);
             }
             //vector3s.Reverse();
             return vector3s;
         }

         #endregion
         
         
         
    }
}