using UnityEngine;

namespace MarchingCube.Sciprts
{
    [CreateAssetMenu()]
    public class DebugSetting:UnityEngine.ScriptableObject
    {
        public bool test = false;
        public bool showCube = false;
        [Range(1,128)]
        public int showCubeResolution;
    
        [Range(0,255)]
        public int density = 2;
    }
}