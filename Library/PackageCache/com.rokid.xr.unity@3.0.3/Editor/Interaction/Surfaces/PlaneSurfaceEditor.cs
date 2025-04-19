
using UnityEngine;
using UnityEditor;
using Rokid.UXR.Interaction;
namespace Rokid.UXR.Editor
{
    [CustomEditor(typeof(PlaneSurface))]
    public class PlaneSurfaceEditor : UnityEditor.Editor
    {
        private const int NUM_SEGMENTS = 40;
        private const float FADE_DISTANCE = 10f;

        private static readonly Color Color = EditorConstants.PRIMARY_COLOR_DISABLED;

        private static float Interval => FADE_DISTANCE / NUM_SEGMENTS;

        public void OnSceneGUI()
        {
            PlaneSurface plane = target as PlaneSurface;
            Draw(plane);
        }

        public static void Draw(PlaneSurface plane)
        {
            Vector3 origin = plane.transform.position;

            if (SceneView.lastActiveSceneView?.camera != null)
            {
                Transform camTransform = SceneView.lastActiveSceneView.camera.transform;
                if (plane.ClosestSurfacePoint(camTransform.position, out SurfaceHit hit, 0))
                {
                    Vector3 hitDelta = PoseUtils.Delta(plane.transform, new Pose(hit.Point, plane.transform.rotation)).position;
                    hitDelta.x = Mathf.RoundToInt(hitDelta.x / Interval) * Interval;
                    hitDelta.y = Mathf.RoundToInt(hitDelta.y / Interval) * Interval;
                    hitDelta.z = 0f;
                    origin = PoseUtils.Multiply(plane.transform.GetPose(), new Pose(hitDelta, Quaternion.identity)).position;
                }
            }

            DrawLines(origin, plane.Normal, plane.transform.up, Color);
            DrawLines(origin, plane.Normal, -plane.transform.up, Color);
            DrawLines(origin, plane.Normal, plane.transform.right, Color);
            DrawLines(origin, plane.Normal, -plane.transform.right, Color);
        }

        private static void DrawLines(in Vector3 origin,
                                      in Vector3 normal,
                                      in Vector3 direction,
                                      in Color baseColor)
        {
            Color prevColor = Handles.color;
            Color color = baseColor;
            Vector3 offsetOrigin = origin;

            for (int i = 0; i < NUM_SEGMENTS; ++i)
            {
                Handles.color = color;

                Vector3 cross = Vector3.Cross(normal, direction).normalized;
                float interval = Interval;

                for (int j = -NUM_SEGMENTS; j < NUM_SEGMENTS; ++j)
                {
                    float horizStart = interval * j;
                    float horizEnd = horizStart + interval;

                    Vector3 start = offsetOrigin + cross * horizStart;
                    Vector3 end = offsetOrigin + cross * horizEnd;

                    color.a = 1f - Mathf.Abs((float)j / NUM_SEGMENTS);
                    color.a *= 1f - ((float)i / NUM_SEGMENTS);
                    color.a *= color.a;

                    Handles.color = color;
                    Handles.DrawLine(start, end);
                }

                offsetOrigin += direction * interval;
                color = baseColor;
            }

            Handles.color = prevColor;
        }
    }

}
