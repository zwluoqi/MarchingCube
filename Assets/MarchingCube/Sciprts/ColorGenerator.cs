using UnityEngine;

namespace MarchingCube.Sciprts
{
    public class ColorGenerator
    {

        private Material _material;
        private Texture2D _texture2D;

        public void UpdateSetting(ColorSetting colorSetting)
        {
            if (colorSetting == null)
            {
                return;
            }
            if (_texture2D != null)
            {
                Object.DestroyImmediate(_texture2D); 
            }

            _texture2D = new Texture2D(64, 1) {wrapMode = TextureWrapMode.Clamp};
            UpdateTexture2D(colorSetting);
        }

        void UpdateTexture2D(ColorSetting colorSetting)
        {
            Color[] colors = new Color[_texture2D.width];
            for (int i = 0; i < _texture2D.width; i++)
            {
                var color = colorSetting.gradient.Evaluate(i * 1.0f / _texture2D.width);
                colors[i] = color;
            }
            _texture2D.SetPixels(colors);
            _texture2D.Apply();
            if (_material == null)
            {
                _material = Object.Instantiate(colorSetting.material);
            }

            if (_material.name != colorSetting.material.name)
            {
                Object.DestroyImmediate(_material);
                _material = Object.Instantiate(colorSetting.material);
            }

            _material.mainTexture = _texture2D;
            // _material.SetTexture("ramp", _texture2D);
        }


        public void SetMeshFilter(float min, float max,MeshRenderer meshRenderer)
        {
            if (_material == null)
            {
                return;
            }
            meshRenderer.sharedMaterial = _material;
            var startY = 0;
            _material.SetVector("minMax", new Vector4(min,
                max
                ,0,0));
        }

    }
}