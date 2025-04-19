
using Rokid.UXR.Interaction;
using UnityEngine;
using Rokid.UXR.Native;
namespace Rokid.UXR.Utility
{
    public class HandUtils
    {
        public static Quaternion[] AdjustSkeletonsRot(Vector3[] skeletons, Quaternion[] rotations, Vector3 forward, Quaternion[] skeletonsRot)
        {
            skeletonsRot[0] = Quaternion.FromToRotation(rotations[0] * forward, skeletons[9] - skeletons[0]) * rotations[0];

            skeletonsRot[1] = Quaternion.FromToRotation(rotations[1] * forward, skeletons[2] - skeletons[1]) * rotations[1];
            skeletonsRot[2] = Quaternion.FromToRotation(rotations[2] * forward, skeletons[3] - skeletons[2]) * rotations[2];
            skeletonsRot[3] = Quaternion.FromToRotation(rotations[3] * forward, skeletons[4] - skeletons[3]) * rotations[3];
            skeletonsRot[4] = Quaternion.FromToRotation(rotations[4] * forward, skeletons[4] - skeletons[3]) * rotations[4];

            skeletonsRot[5] = Quaternion.FromToRotation(rotations[5] * forward, skeletons[6] - skeletons[5]) * rotations[5];
            skeletonsRot[6] = Quaternion.FromToRotation(rotations[6] * forward, skeletons[7] - skeletons[6]) * rotations[6];
            skeletonsRot[7] = Quaternion.FromToRotation(rotations[7] * forward, skeletons[8] - skeletons[7]) * rotations[7];
            skeletonsRot[8] = Quaternion.FromToRotation(rotations[8] * forward, skeletons[8] - skeletons[7]) * rotations[8];

            skeletonsRot[9] = Quaternion.FromToRotation(rotations[9] * forward, skeletons[10] - skeletons[9]) * rotations[9];
            skeletonsRot[10] = Quaternion.FromToRotation(rotations[10] * forward, skeletons[11] - skeletons[10]) * rotations[10];
            skeletonsRot[11] = Quaternion.FromToRotation(rotations[11] * forward, skeletons[12] - skeletons[11]) * rotations[11];
            skeletonsRot[12] = Quaternion.FromToRotation(rotations[12] * forward, skeletons[12] - skeletons[11]) * rotations[12];

            skeletonsRot[13] = Quaternion.FromToRotation(rotations[13] * forward, skeletons[14] - skeletons[13]) * rotations[13];
            skeletonsRot[14] = Quaternion.FromToRotation(rotations[14] * forward, skeletons[15] - skeletons[14]) * rotations[14];
            skeletonsRot[15] = Quaternion.FromToRotation(rotations[15] * forward, skeletons[16] - skeletons[15]) * rotations[15];
            skeletonsRot[16] = Quaternion.FromToRotation(rotations[16] * forward, skeletons[16] - skeletons[15]) * rotations[16];

            skeletonsRot[17] = Quaternion.FromToRotation(rotations[17] * forward, skeletons[18] - skeletons[17]) * rotations[17];
            skeletonsRot[18] = Quaternion.FromToRotation(rotations[18] * forward, skeletons[19] - skeletons[18]) * rotations[18];
            skeletonsRot[19] = Quaternion.FromToRotation(rotations[19] * forward, skeletons[20] - skeletons[19]) * rotations[19];
            skeletonsRot[20] = Quaternion.FromToRotation(rotations[20] * forward, skeletons[20] - skeletons[19]) * rotations[20];

            return skeletonsRot;
        }

        #region  AdjustSkeletonPosition
        static float wrist_middle_cmp_dis = 0.099f;

        static float thumb_mcp_pip_dis = 0.03646206f;
        static float index_mcp_pip_dis = 0.0302203f;
        static float middle_mcp_pip_dis = 0.03317486f;
        static float ring_mcp_pip_dis = 0.02867948f;
        static float pinky_mcp_pip_dis = 0.02232722f;

        static float thumb_dip_pip_dis = 0.02413473f;
        static float index_dip_pip_dis = 0.01864309f;
        static float middle_dip_pip_dis = 0.02395519f;
        static float ring_dip_pip_dis = 0.02499392f;
        static float pinky_dip_pip_dis = 0.02014625f;

        static float thumb_dip_tip_dis = 0.03569441f;
        static float index_dip_tip_dis = 0.02287725f;
        static float middle_dip_tip_dis = 0.02539373f;
        static float ring_dip_tip_dis = 0.02466991f;
        static float pinky_dip_tip_dis = 0.01812316f;

        static float[] boneDis = new float[] {
            wrist_middle_cmp_dis,

            thumb_mcp_pip_dis,
            index_mcp_pip_dis,
            middle_mcp_pip_dis,
            ring_mcp_pip_dis,
            pinky_mcp_pip_dis,

            thumb_dip_pip_dis,
            index_dip_pip_dis,
            middle_dip_pip_dis,
            ring_dip_pip_dis,
            pinky_dip_pip_dis,

            thumb_dip_tip_dis,
            index_dip_tip_dis,
            middle_dip_tip_dis,
            ring_dip_tip_dis,
            pinky_dip_tip_dis
         };

        public static Vector3[] AdjustSkeletonsPos(Vector3[] skeletons, Vector3[] skeletonsPos)
        {
            int fingerMcpDisIndex = 1, fingerPipDisIndex = 6, fingerTipDisIndex = 11;
            for (int i = 0; i < 21; i++)
            {
                skeletonsPos[i] = skeletons[i];
                //调整MCP
                if (i == 1 || i == 5 || i == 9 || i == 13 || i == 17)
                {
                    Vector3 direct = Vector3.Normalize(skeletons[i + 1] - skeletons[i]);
                    skeletonsPos[i] = skeletons[i + 1] - direct * boneDis[fingerMcpDisIndex];
                    fingerMcpDisIndex++;
                }

                //调整Pip
                if (i == 3 || i == 7 || i == 11 || i == 15 || i == 19)
                {
                    Vector3 direct = Vector3.Normalize(skeletons[i] - skeletons[i - 1]);
                    skeletonsPos[i] = skeletons[i - 1] + direct * boneDis[fingerPipDisIndex];
                    fingerPipDisIndex++;
                }


                //调整Tip
                if (i == 4 || i == 8 || i == 12 || i == 16 || i == 20)
                {
                    Vector3 direct = Vector3.Normalize(skeletons[i] - skeletons[i - 1]);
                    skeletonsPos[i] = skeletons[i - 1] + direct * boneDis[fingerTipDisIndex];
                    fingerTipDisIndex++;
                }

                //调整Wrist
                if (i == 0)
                {
                    Vector3 direct = Vector3.Normalize(skeletons[9] - skeletons[0]);
                    skeletonsPos[0] = skeletons[9] - direct * boneDis[0];
                }
            }

            return skeletonsPos;
        }

        #endregion

        private static Quaternion rotation = Quaternion.identity;

        private static SkeletonIndexFlag[] IndexFingerFlags = new SkeletonIndexFlag[]{
            SkeletonIndexFlag.INDEX_FINGER_MCP,
            SkeletonIndexFlag.INDEX_FINGER_PIP,
            SkeletonIndexFlag.INDEX_FINGER_DIP,
            SkeletonIndexFlag.INDEX_FINGER_TIP
        };

        private static Vector3[] LeftIndexStraightEulers = new Vector3[] {
            new Vector3(15,-4.354f,0),
            new Vector3(10,0,0),
            new Vector3(0,0,0)
        };

        private static Vector3[] LeftIndexStraightPositions = new Vector3[] {
            new Vector3(0,0.033f,0),
            new Vector3(0,0.022f,0),
            new Vector3(0,0.0256f,0)
        };

        private static Vector3[] RightIndexStraightEulers = new Vector3[] {
            new Vector3(15,4.238f,0.518f),
            new Vector3(10,0,0),
            new Vector3(0,0,0)
        };


        private static Vector3[] RightIndexStraightPositions = new Vector3[] {
            new Vector3(0,0.033f,0),
            new Vector3(0,0.022f,0),
            new Vector3(0,0.0256f,0)
        };

        public static void AdjustIndexToStraight(Quaternion[] skeletonsRot, Vector3[] skeletons, HandType handType)
        {
            if (handType == HandType.LeftHand)
            {
                for (int i = 1; i < 4; i++)
                {
                    skeletonsRot[(int)IndexFingerFlags[i]] = skeletonsRot[(int)IndexFingerFlags[i - 1]] * Quaternion.Euler(LeftIndexStraightEulers[i - 1]);
                    skeletons[(int)IndexFingerFlags[i]] = skeletonsRot[(int)IndexFingerFlags[i - 1]] * LeftIndexStraightPositions[i - 1] + skeletons[(int)IndexFingerFlags[i - 1]];
                }
            }

            if (handType == HandType.RightHand)
            {
                for (int i = 1; i < 4; i++)
                {
                    skeletonsRot[(int)IndexFingerFlags[i]] = skeletonsRot[(int)IndexFingerFlags[i - 1]] * Quaternion.Euler(RightIndexStraightEulers[i - 1]);
                    skeletons[(int)IndexFingerFlags[i]] = skeletonsRot[(int)IndexFingerFlags[i - 1]] * RightIndexStraightPositions[i - 1] + skeletons[(int)IndexFingerFlags[i - 1]];
                }
            }
        }

        public static Quaternion GetQuaternion(float[] data, bool isRight, Pose cameraPose)
        {
            rotation[0] = -data[0];
            rotation[1] = -data[1];
            rotation[2] = data[2];
            rotation[3] = data[3];
            rotation = cameraPose.rotation * rotation * Quaternion.Euler(90, 0, 0);
            return rotation;
        }

        public static Quaternion GetQuaternion(float[] data)
        {
            return new Quaternion(data[0], data[1], data[2], data[3]);
        }

        public static Vector3 GetVector3(float[] data)
        {
            return new Vector3(data[0], data[1], data[2]);
        }


        public static Vector3[] GetVector3Arr(float[] data, Pose cameraPose)
        {
            Vector3[] vertices = new Vector3[data.Length / 3];
            Vector3 vert = Vector3.zero;
            for (int i = 0; i < vertices.Length; i++)
            {
                vert[0] = data[3 * i] / 1000.0f;
                vert[1] = -data[3 * i + 1] / 1000.0f;
                vert[2] = data[3 * i + 2] / 1000.0f;
                vertices[i] = cameraPose.rotation * vert;
                vertices[i] += cameraPose.position;
            }
            return vertices;
        }

        public static Vector3[] GetVector3Arr(float[][] data, Pose cameraPose)
        {
            Vector3[] vertices = new Vector3[data.Length];
            Vector3 vert = Vector3.zero;
            for (int i = 0; i < data.Length; i++)
            {
                vert[0] = data[i][0] / 1000.0f;
                vert[1] = -data[i][1] / 1000.0f;
                vert[2] = data[i][2] / 1000.0f;
                vertices[i] = cameraPose.rotation * vert;
                vertices[i] += cameraPose.position;
            }
            return vertices;
        }

        private static Matrix4x4 rightHandMatrix = Matrix4x4.identity;
        private static Quaternion leftHandRotation = Quaternion.identity;
        public static Quaternion[] GetSkeletonsQuaternion(float[] data, bool isRight, Quaternion[] rotations, Pose cameraPose)
        {
            for (int i = 0; i < 26; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    rightHandMatrix.SetRow(j, new Vector3(data[9 * i + 3 * j], data[9 * i + 3 * j + 1], data[9 * i + 3 * j + 2]));
                }
                //右->左
                leftHandRotation[0] = -rightHandMatrix.rotation[0];
                leftHandRotation[1] = -rightHandMatrix.rotation[1];
                leftHandRotation[2] = rightHandMatrix.rotation[2];
                leftHandRotation[3] = rightHandMatrix.rotation[3];
                leftHandRotation = cameraPose.rotation * leftHandRotation * Quaternion.Euler(90, 0, 0);
                rotations[i] = leftHandRotation;
            }
            return rotations;
        }

        public static bool UnPinchForDistance(Vector3 indexTip, Vector3 thumbTip, float unPinchDistance)
        {
            return Vector3.Distance(indexTip, thumbTip) > unPinchDistance;
        }

        public static bool CanReleaseHandDrag(HandType handType, float releaseDistance = 0.02f)
        {
            if (Utils.IsAndroidPlatform())
            {
                return GesEventInput.Instance.GetPinchDistance(handType) > releaseDistance && GesEventInput.Instance.GetGestureType(handType) != GestureType.Grip;
            }
            else
            {
                return Input.GetMouseButtonUp(0);
            }
        }

        public static Vector3 InverseTransformPoint(Pose pose, Vector3 point)
        {
            return Quaternion.Inverse(pose.rotation) * (point - pose.position);
        }

        public static Vector3 TransformPoint(Pose pose, Vector3 point)
        {
            return pose.rotation * point + pose.position;
        }
    }
}
