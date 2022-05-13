using UnityEngine;

namespace MarchingCube.Sciprts
{
    [CreateAssetMenu()]
    public class ColorSetting:UnityEngine.ScriptableObject
    {
        public Gradient gradient;
        public Material material;
        
    }
}