using UnityEngine;
using System.Collections.Generic;
using Rokid.UXR.Utility;

namespace Rokid.UXR.Interaction
{
    public class HandVisual : MonoBehaviour
    {
        /// <summary>
        /// HandType
        /// </summary>
        [SerializeField]
        private HandType hand;
        /// <summary>
        /// Bone Coordinate Axis Transformation"
        /// </summary>
        [SerializeField]
        private Vector3 axisRot = Vector3.zero;
        /// <summary>
        /// The root joint of the hand is generally the "Wrist."
        /// </summary>
        [SerializeField]
        private Transform handRootSkeleton;
        /// <summary>
        /// Hand MeshRender
        /// </summary>
        [SerializeField]
        private SkinnedMeshRenderer handMesh;
        /// <summary>
        /// Hand Skeleton
        /// </summary>
        /// <typeparam name="Transform"></typeparam>
        /// <returns></returns>
        [SerializeField]
        private List<Transform> handSkeletons = new List<Transform>();
        private bool pokeSelect = false;
        private Vector3 pokeClosePoint = Vector3.zero;

        private void Start()
        {
            if (!Utils.IsAndroidPlatform())
            {
                Destroy(GetComponent<InputModuleSwitchActive>());
                this.gameObject.SetActive(false);
            }
            InteractorStateChange.OnPokeSelectUpdate += OnPokeSelectUpdate;
            InteractorStateChange.OnPokeUnSelectUpdate += OnPokeUnSelectUpdate;
            if (Utils.IsAndroidPlatform())
                GesEventInput.OnRenderHand += OnRenderHand;
        }
        private void OnDestroy()
        {
            InteractorStateChange.OnPokeSelectUpdate -= OnPokeSelectUpdate;
            InteractorStateChange.OnPokeUnSelectUpdate -= OnPokeUnSelectUpdate;
            if (Utils.IsAndroidPlatform())
                GesEventInput.OnRenderHand -= OnRenderHand;
        }

        private void OnPokeUnSelectUpdate(HandType handType)
        {
            if (handType == this.hand)
            {
                pokeSelect = false;
            }
        }

        private void OnPokeSelectUpdate(HandType handType, Vector3 pokeClosePoint)
        {
            if (handType == this.hand)
            {
                this.pokeClosePoint = pokeClosePoint;
                pokeSelect = true;
            }
        }

        private void OnRenderHand(HandType hand, GestureBean data)
        {
            handRootSkeleton.localScale = GesEventInput.Instance.GetHandScale() * Vector3.one;
            Quaternion[] rotations = data.skeletonsRot;
            if (hand == this.hand)
            {
                for (int i = 0; i < 21; i++)
                {
                    handSkeletons[i].rotation = rotations[i] * Quaternion.Euler(axisRot);
                    if (pokeSelect)
                    {
                        Vector3 offset = pokeClosePoint - GesEventInput.Instance.GetSkeletonPose(SkeletonIndexFlag.INDEX_FINGER_TIP, hand).position;
                        handSkeletons[i].position = data.skeletons[i] + offset;
                    }
                    else
                    {
                        handSkeletons[i].position = data.skeletons[i];
                    }
                }
            }
        }

        /// <summary>
        /// Update hand mesh materials
        /// </summary>
        /// <param name="materials"></param>
        public void UpdateHandMeshMaterials(Material[] materials)
        {
            handMesh.materials = materials;
        }

        /// <summary>
        ///  Set hand mesh active
        /// </summary>
        /// <param name="active"></param>
        public void SetHandMeshActive(bool active)
        {
            handMesh.gameObject.SetActive(active);
        }
    }
}

