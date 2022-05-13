using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class TerrainGenerator : MonoBehaviour
{
    public MarchingCubeMesh prefab;
    MarchingCubeMesh[] nineCubes = new MarchingCubeMesh[9];

    public Vector3 curCenterPos = Vector3.zero;

    private void Start()
    {
        prefab.gameObject.SetActive(false);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var item = Object.Instantiate(prefab);
                item.gameObject.SetActive(true);
                nineCubes[i * 3 + j] = item;
            }
        }

        UpdateTerrain(this.transform.position);
    }

    private void FixedUpdate()
    {
        UpdateCenterPos(this.transform.position);
    }

    void UpdateCenterPos(Vector3 pos)
    {
        var shape = prefab.shapeSetting;
        var dis = curCenterPos - pos;
        if (Mathf.Abs(dis.x) > shape.resolution.x ||Mathf.Abs( dis.z) > shape.resolution.z)
        {
            curCenterPos = pos;
            UpdateTerrain(pos);
        }
    }

    private void UpdateTerrain(Vector3 pos)
    {
        var shape = prefab.shapeSetting;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var gridPosOffsetX = (i - 1) * shape.resolution.x;
                var gridPosOffsetZ = (j - 1) * shape.resolution.z;
                var gridPos = new Vector3(pos.x,0,pos.z) + new Vector3(gridPosOffsetX, 0, gridPosOffsetZ);
                nineCubes[i * 3 + j].transform.position = (gridPos);
                nineCubes[i * 3 + j].OnPositionUpdated();
            }
        }

    }
}
