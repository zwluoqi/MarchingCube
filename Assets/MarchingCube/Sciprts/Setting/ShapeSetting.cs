﻿using UnityEngine;

namespace MarchingCube.Sciprts
{
    [CreateAssetMenu()]
    public class ShapeSetting:UnityEngine.ScriptableObject
    {
        [Range(1,128)]
        public int resolution = 2;
        public float roughness = 1;
        public Vector3 offset;
        public ShapeType type;
        public float cubeSize = 1;
    }

    public enum ShapeType
    {
        Noise,
        Circle,
        SinSurface,
    }
}