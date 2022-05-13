using System;
using UnityEditor;
using UnityEngine;

namespace MarchingCube.Sciprts
{
    [CustomEditor(typeof(MarchingCubeMesh))]
    public class ShapeSettingInspector : Editor
    {

        private SettingEditor<MarchingCubeMesh> shapeEdirot;

        private void OnEnable()
        {
            shapeEdirot = new SettingEditor<MarchingCubeMesh>();
            shapeEdirot.OnEnable(this);
        }

        private Vector3 pos;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            shapeEdirot.OnInspectorGUI(this);
            var dis = Vector3.Distance(pos, ((MarchingCubeMesh) target).transform.position);
            if (dis > 0.01f)
            {
                pos = ((MarchingCubeMesh) target).transform.position;
                ((MarchingCubeMesh) target).OnPositionUpdated();
            }
        }

    }
}