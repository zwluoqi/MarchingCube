using System;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCube.Sciprts
{
    public class MeshData
    {
        public TriangleXXOO[] vertices;
        public  Vector2[] uvs;//x,高度（0,1）,y,深度(-1,1)
        public  int[] triangles;
        
        public struct TriangleXXOO
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;

            public override string ToString()
            {
                return a.ToString() + "\t" + b.ToString() + "\t" + c.ToString();
            }
        };

        public void UpdateLength(int length)
        {
            vertices = new TriangleXXOO[length];
        }
    }
    
    public class MeshDataComputerBuffer
    {
        public ComputeBuffer _bufferVertices;
        public  ComputeBuffer _bufferNormals;
        public  ComputeBuffer _bufferTangents;
        
        public  ComputeBuffer _bufferUVs;
        public  ComputeBuffer _bufferTriangles;


        public void SetData(MeshData meshData)
        {
            _bufferVertices.SetData(meshData.vertices);
            _bufferTriangles.SetData(meshData.triangles);
        }

        public void GetData(MeshData meshData)
        {
            
            _bufferVertices.GetData(meshData.vertices);
            // _bufferNormals.GetData(meshData.normals);
            // _bufferTangents.GetData(meshData.tangents);
            //
            // _bufferUVs.GetData(meshData.uvs);
            // _bufferTriangles.GetData(meshData.triangles);
        }

        public void CreateShapeBuffer()
        {

            ResizeComputerBuffer(ref _bufferVertices,3*4*3);
            // ResizeComputerBuffer(ref _bufferNormals, ref meshData.normals,3*4);
            // ResizeComputerBuffer(ref _bufferTangents, ref meshData.tangents,4*4);
            // ResizeComputerBuffer(ref _bufferTriangles, ref meshData.triangles,4);
            // ResizeComputerBuffer(ref _bufferUVs, ref meshData.uvs,2*4);
            
        }

        private void ResizeComputerBuffer(ref ComputeBuffer bufferVertices, int stride)
        {
                        
            // if (bufferVertices != null&&bufferVertices.count != meshDataVertices.Length)
            // {
            //     bufferVertices.Release();
            //     bufferVertices.Dispose();
            //     bufferVertices = null;
            // }
            if(bufferVertices == null)
            {
                bufferVertices = new ComputeBuffer(1024*1024,stride, ComputeBufferType.Append);
            }
            bufferVertices.SetCounterValue( 0 );
        }

        public void Dispose()
        {
            _bufferVertices?.Dispose();
            _bufferNormals?.Dispose();
            _bufferTangents?.Dispose();
            
            _bufferUVs?.Dispose();
            _bufferTriangles?.Dispose();
        }
    }
    
    public class MarchCubeGPUGenerator:System.IDisposable
    {
        private MeshDataComputerBuffer _meshDataComputerBuffer = new MeshDataComputerBuffer();
        
        private readonly int ResolutionID = Shader.PropertyToID("Resolution");
        private readonly int roughnessID = Shader.PropertyToID("roughness");
        private readonly int strengthID = Shader.PropertyToID("strength");
        
        private readonly int cubeSizeID = Shader.PropertyToID("cubeSize");
        private readonly int offsetID = Shader.PropertyToID("offsets");
        private readonly int objPosID = Shader.PropertyToID("objPos");
        
        private readonly int shapetypeID = Shader.PropertyToID("shapetype");
        private readonly int densityID = Shader.PropertyToID("density");
        
        
        private readonly int verticesID = Shader.PropertyToID("vertices");
        
        private readonly int edgeDatasID = Shader.PropertyToID("edgeDatas");

        private void CreateShapeBuffer(ShapeSetting shapeSettting)
        {
            _meshDataComputerBuffer.CreateShapeBuffer();
        }
        
        public MeshData UpdateShape(ShapeSetting shapeSetting,DebugSetting debugSetting,Vector3 pos)
        {
            // if (shapeSetting.resolution < 8)
            // {
            //     throw new Exception("分辨率低于8不允许使用GPU");
            // }
            CreateShapeBuffer(shapeSetting);
            
            var computeShader = shapeSetting.computeShader;

            computeShader.SetVector(ResolutionID, new float4(shapeSetting.resolution.xyzx));
            computeShader.SetFloat(roughnessID, shapeSetting.roughness);
            computeShader.SetFloat(strengthID, shapeSetting.strength);
            
            computeShader.SetVector(objPosID, pos);
            computeShader.SetInt(shapetypeID, (int)shapeSetting.type);
            computeShader.SetFloat(cubeSizeID, shapeSetting.cubeSize);
            computeShader.SetVector("floorOffset", shapeSetting.floorOffset);
            computeShader.SetVector("sharpenParams",shapeSetting.sharpenParams);
            
            computeShader.SetFloat("weightMultiplier", shapeSetting.weightMultiplier);
            computeShader.SetFloat("persistence", shapeSetting.persistence);
            computeShader.SetFloat("lacunarity", shapeSetting.lacunarity);
            computeShader.SetInt("octaves", shapeSetting.octaves.Length);
            computeShader.SetInt(densityID, debugSetting.density);
            computeShader.SetVector("weightNoiseXYZ",shapeSetting.weightNoise);
            
            

            //获取内核函数的索引
            var kernelVertices = computeShader.FindKernel("CSMainVertices");
            SetLookupTableData(computeShader,kernelVertices);
            

            var offsetComputeBuffer = new ComputeBuffer(shapeSetting.octaves.Length, 3*4, ComputeBufferType.Constant);
            offsetComputeBuffer.SetData(shapeSetting.octaves);
            computeShader.SetBuffer(kernelVertices,offsetID, offsetComputeBuffer);
            
            computeShader.SetBuffer(kernelVertices,verticesID,_meshDataComputerBuffer._bufferVertices);
            
            // var edgeDataBuffers = new ComputeBuffer(1024*128,3*4, ComputeBufferType.Append);
            // computeShader.SetBuffer(kernelVertices,edgeDatasID,edgeDataBuffers);
            //
            computeShader.Dispatch(kernelVertices, ((shapeSetting.resolution.x-1)/8+1), ((shapeSetting.resolution.y-1)/8+1), ((shapeSetting.resolution.z-1)/8+1));

            var count = GetComputerBufferWriteCount(_meshDataComputerBuffer._bufferVertices);

            Debug.Log( " _bufferTriangles: " +count);
            
            
            var _meshData = new MeshData();
            _meshData.UpdateLength(count);
            _meshDataComputerBuffer.GetData(_meshData);
            // Debug.Log("vertex:"+_meshData.vertices[0].ToString());

            
            // var edgeDataCount = GetComputerBufferWriteCount(edgeDataBuffers);
            // if (edgeDataCount > 0)
            // {
            //     Debug.Log(" edgeDataBuffers: " + edgeDataCount);
            //     float3[] edgeData = new float3[edgeDataCount];
            //     edgeDataBuffers.GetData(edgeData);
            // }
            //
            // // for (int i = 0; i < edgeData.Length&&i<16; i++)
            // // {
            // //     Debug.Log($"edgeData{i}:"+edgeData[i].ToString());
            // // }
            // edgeDataBuffers.Release();
            // edgeDataBuffers.Dispose();
            
            offsetComputeBuffer.Dispose();
            offsetComputeBuffer.Release();
            
            return _meshData;
        }

        private int GetComputerBufferWriteCount(ComputeBuffer bufferVertices)
        {
            var countBuffer = new ComputeBuffer(1, sizeof(int ), ComputeBufferType.IndirectArguments);
            ComputeBuffer.CopyCount(bufferVertices, countBuffer, 0 );
            
            int[] counter = new  int[1] { 0 };
            countBuffer.GetData(counter);
            int count = counter[0 ];
            countBuffer.Release();
            countBuffer.Dispose();
            
            return count;
        }

        private ComputeBuffer triTablesComputeBuffer;
        private ComputeBuffer edgeMaskTableComputeBuffer;
        private ComputeBuffer edgeTableComputeBuffer;
        private ComputeBuffer vertexMappingComputeBuffer;
        private ComputeBuffer pointTableComputeBuffer;

        private void SetLookupTableData(ComputeShader computeShader,int kenrel)
        {
            
            if (triTablesComputeBuffer == null)
            {
                var triTables = MarchingCubeLookupTable.triTable.ToInts();
                triTablesComputeBuffer = new ComputeBuffer(triTables.Length, 4, ComputeBufferType.Constant);
                triTablesComputeBuffer.SetData(triTables);
            }
            
            if (edgeMaskTableComputeBuffer == null)
            {
                var edgeMaskTables = MarchingCubeLookupTable.edgeMaskTable;
                edgeMaskTableComputeBuffer = new ComputeBuffer(edgeMaskTables.Length, 4, ComputeBufferType.Constant);
                edgeMaskTableComputeBuffer.SetData(edgeMaskTables);
            }
            if (edgeTableComputeBuffer == null)
            {
                var edgeTables = MarchingCubeLookupTable.edgeTable.ToInts();
                edgeTableComputeBuffer = new ComputeBuffer(edgeTables.Length, 4, ComputeBufferType.Constant);
                edgeTableComputeBuffer.SetData(edgeTables);
            }
            if (vertexMappingComputeBuffer == null)
            {
                var vertexMappings = MarchingCubeLookupTable.vertexMapping;
                vertexMappingComputeBuffer = new ComputeBuffer(vertexMappings.Length, 4, ComputeBufferType.Constant);
                vertexMappingComputeBuffer.SetData(vertexMappings);
            }
            if (pointTableComputeBuffer == null)
            {
                var pointTables = MarchingCubeLookupTable.pointTable.ToV4Array();
                pointTableComputeBuffer = new ComputeBuffer(pointTables.Length, 4*4, ComputeBufferType.Constant);
                pointTableComputeBuffer.SetData(pointTables);
            }
            
            computeShader.SetBuffer(kenrel,"triTable",triTablesComputeBuffer);
            computeShader.SetBuffer(kenrel,"edgeMaskTable",edgeMaskTableComputeBuffer);
            computeShader.SetBuffer(kenrel,"edgeTable",edgeTableComputeBuffer);
            computeShader.SetBuffer(kenrel,"vertexMapping",vertexMappingComputeBuffer);
            computeShader.SetBuffer(kenrel,"pointTable",pointTableComputeBuffer);
            
        }

        public void Dispose()
        {
            triTablesComputeBuffer?.Dispose();
            _meshDataComputerBuffer.Dispose();
        }
    }
}