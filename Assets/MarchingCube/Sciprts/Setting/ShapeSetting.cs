using Unity.Mathematics;
using UnityEngine;

namespace MarchingCube.Sciprts
{
    [CreateAssetMenu()]
    public class ShapeSetting:UnityEngine.ScriptableObject
    {
        public bool gpu = true;
        public ComputeShader computeShader;
        // [Range(1,128)]
        [SerializeField]
        public int3 resolution = new int3(8,8,8);
        public float roughness = 1;
        public float strength = 1;
        public Vector3[] octaves = new Vector3[1];
        public float weightMultiplier =1;
        public float persistence =1;
        public float lacunarity =1;
        
        // public Vector3 offset;
        public ShapeType type;
        public float cubeSize = 1;
        public Vector3 floorOffset;
        public Vector2 sharpenParams;
    }

    public struct ShapeSettingBuffer
    {
        
    }

    public enum ShapeType
    {
        Noise,
        Circle,
        SinSurface,
        Noise2,
    }
}