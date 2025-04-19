using UnityEngine;
using UnityEditor;
using Rokid.UXR.Interaction;
namespace Rokid.UXR.Editor
{
    [CustomEditor(typeof(CylinderSurface))]
    public class CylinderSurfaceEditor : UnityEditor.Editor
    {
        private const int NUM_SEGMENTS = 30;

        private static readonly Color ValidColor = Color.green * 0.8f;

        private static readonly Color InvalidColor = Color.red * 0.8f;

        public void OnSceneGUI()
        {
            CylinderSurface cylinder = target as CylinderSurface;

            if (cylinder.Cylinder != null)
            {
                Draw(cylinder);
            }
        }

        public static void Draw(CylinderSurface cylinderSurface)
        {
            Color prevColor = Handles.color;
            Handles.color = cylinderSurface.IsValid ? ValidColor : InvalidColor;

            float gizmoHeight = cylinderSurface.Height;
            float camYOffset = 0;
            bool infiniteHeight = cylinderSurface.Height <= 0;

            if (infiniteHeight && SceneView.lastActiveSceneView?.camera != null)
            {
                gizmoHeight = 1000f;
                Vector3 sceneCamPos = SceneView.lastActiveSceneView.camera.transform.position;
                camYOffset = cylinderSurface.Cylinder.transform.InverseTransformPoint(sceneCamPos).y;
            }

            for (int i = 0; i < 2; ++i)
            {
                bool isTop = i == 1;
                float y = isTop ? gizmoHeight / 2 : -gizmoHeight / 2;
                int numSegments = (int)(NUM_SEGMENTS * Mathf.Max(cylinderSurface.Radius / 2, 1));
                Vector3 prevSegmentWorld = Vector3.zero;

                for (int seg = 0; seg <= numSegments; ++seg)
                {
                    float ratio = (float)seg / numSegments * Mathf.PI * 2;
                    float x = Mathf.Cos(ratio) * cylinderSurface.Radius;
                    float z = Mathf.Sin(ratio) * cylinderSurface.Radius;
                    Vector3 curSegmentLocal = new Vector3(x, y + camYOffset, z);
                    Vector3 curSegmentWorld = cylinderSurface.Cylinder.transform.TransformPoint(curSegmentLocal);

                    if (isTop) // Draw connecting lines from top circle
                    {
                        Vector3 bottomVert = new Vector3(curSegmentLocal.x,
                                                         curSegmentLocal.y - gizmoHeight,
                                                         curSegmentLocal.z);
                        bottomVert = cylinderSurface.Cylinder.transform.TransformPoint(bottomVert);
                        Handles.DrawLine(curSegmentWorld, bottomVert);
                    }

                    if (seg > 0 && !infiniteHeight)
                    {
                        Handles.DrawLine(curSegmentWorld, prevSegmentWorld);
                    }

                    prevSegmentWorld = curSegmentWorld;
                }
            }

            Handles.color = prevColor;
        }
    }

}
