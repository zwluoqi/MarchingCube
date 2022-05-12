using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarchingCube.Sciprts;
using UnityEngine;
using UnityEngine.Rendering;

public class MarchingCubeMesh : MonoBehaviour
{

    public MeshFilter meshFilter;

    
    public ShapeSetting shapeSetting;
    public DebugSetting debugSetting;
    
    MarchCubeCPUGenerator _marchCubeCPUGenerator = new MarchCubeCPUGenerator();
    MarchCubeGPUGenerator _marchCubeGPUGenerator = new MarchCubeGPUGenerator();

    [NonSerialized]
    public bool shapeSetttingsFoldOut;
    [NonSerialized]
    public bool debugSetttingsFoldOut;


    private void OnDestroy()
    {
        _marchCubeGPUGenerator.Dispose();
    }

    public void GenerateMesh()
    {
        System.DateTime start = System.DateTime.Now;
        List<Vector3> vector3s = null;
        if (debugSetting.test)
        {
            vector3s = _marchCubeCPUGenerator.GenerateCubeByDensity(debugSetting.density);
            Debug.Log("vertex:"+vector3s[0].ToString()+" "+vector3s[1].ToString()+" "+vector3s[2].ToString());

        }
        else
        {
            if (shapeSetting.computeShader == null || !shapeSetting.gpu)
            {
                vector3s = _marchCubeCPUGenerator.GetVertex(shapeSetting);
            }
            else
            {
                vector3s = new List<Vector3>();
                
                var _meshData = _marchCubeGPUGenerator.UpdateShape(shapeSetting,debugSetting,this.transform.position);
                System.DateTime end = System.DateTime.Now;
                Debug.LogWarning((end-start).TotalMilliseconds+"ms");
                for (int i = 0; i < _meshData.vertices.Length; i++)
                {
                    vector3s.Add(_meshData.vertices[i].a);
                    vector3s.Add(_meshData.vertices[i].b);
                    vector3s.Add(_meshData.vertices[i].c);
                }
            }

            // vector3s = ;
        }

        // var 
        if (meshFilter == null)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(this.transform);
            meshFilter = go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
        }

        if (meshFilter.sharedMesh == null)
        {
            meshFilter.sharedMesh = new Mesh {hideFlags = HideFlags.DontSave};
        }

        Mesh sharedMesh;
        (sharedMesh = meshFilter.sharedMesh).Clear();
        if (vector3s.Count > UInt16.MaxValue)
        {
            sharedMesh.indexFormat = IndexFormat.UInt32;
        }
        else
        {
            sharedMesh.indexFormat = IndexFormat.UInt16;
        }
        sharedMesh.vertices = vector3s.ToArray();

        sharedMesh.triangles = vector3s.Select(((vector3, i) => i)).ToArray();
        sharedMesh.RecalculateNormals();
        System.DateTime end2 = System.DateTime.Now;
        Debug.LogWarning((end2-start).TotalMilliseconds+"ms");
    }


    private void OnValidate()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        GenerateMesh();
    }

    public void OnShapeSetttingUpdated()
    {
        GenerateMesh();
    }


    private void OnDrawGizmos()
    {
        // var cubePos = cubeTangentUtil.GetCubePos(Vector3.one, scale);
        if (debugSetting.showCube)
        {
            Vector3 startPos = Vector3.one * shapeSetting.cubeSize * 0.5f + this.transform.position;
            Gizmos.color = Color.red;

            for (int x = 0; x < shapeSetting.resolution.x; x++)
            {
                for (int y = 0; y < shapeSetting.resolution.y; y++)
                {
                    for (int z = 0; z < shapeSetting.resolution.z; z++)
                    {
                        var offset = new Vector3(x, y, z) * shapeSetting.cubeSize;
                        // DrawCube(cubePos, offset);
                        Gizmos.DrawWireCube(startPos + offset, Vector3.one * shapeSetting.cubeSize);
                        if (z >= debugSetting.showCubeResolution)
                        {
                            break;
                        }
                    }

                    if (y >=  debugSetting.showCubeResolution)
                    {
                        break;
                    }
                }

                if (x >=  debugSetting.showCubeResolution)
                {
                    break;
                }
            }
        }
    }


    public void OnDebugSetttingUpdated()
    {
        if (debugSetting.showCube)
        {
            GenerateMesh();
        }
    }

    public void OnPositionUpdated()
    {
        // GenerateMesh();
    }
}

