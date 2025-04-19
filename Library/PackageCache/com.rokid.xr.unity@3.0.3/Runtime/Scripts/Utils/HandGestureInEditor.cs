using UnityEngine;
using Rokid.UXR.Interaction;

namespace Rokid.UXR.Utility
{
    /// <summary>
    /// 编辑的模拟手势
    /// </summary>
    public class HandGestureInEditor : AutoInjectBehaviour
    {

        [SerializeField, Autowrited]
        private HandType hand;
        [SerializeField, Autowrited]
        private GameObject grip;

        [SerializeField, Autowrited]
        private GameObject poke;
        [SerializeField, Autowrited]
        private GameObject palm;

        [SerializeField, Autowrited]
        private Transform content;

        public float fallbackMaxDistanceNoItem = 10.0f;
        public float fallbackMaxDistanceWithItem = 0.5f;
        private float fallbackInteractorDistance = -1.0f;

        protected override void Awake()
        {
            base.Awake();
            InteractorStateChange.OnPokeInteractorHover += OnPokeInteractorHover;
            InteractorStateChange.OnPokeInteractorUnHover += OnPokeInteractorUnHover;
        }
        private void Start()
        {
#if !UNITY_EDITOR
        DestroyImmediate(this.gameObject);
#endif
            poke.gameObject.SetActive(true);
            grip.gameObject.SetActive(false);
            palm.gameObject.SetActive(false);
        }

        private void OnPokeInteractorHover(HandType hand)
        {
            if (hand == this.hand)
                this.gameObject.SetActive(true);
        }

        private void OnPokeInteractorUnHover(HandType hand)
        {
            if (hand == this.hand)
                this.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            InteractorStateChange.OnPokeInteractorHover -= OnPokeInteractorHover;
            InteractorStateChange.OnPokeInteractorUnHover -= OnPokeInteractorUnHover;
        }


        private void Update()
        {
            UpdateInEditor();

            if (Input.GetKey(KeyCode.X))
            {
                poke.gameObject.SetActive(false);
                if (Input.GetMouseButton(0))
                {
                    grip.gameObject.SetActive(true);
                    palm.gameObject.SetActive(false);
                }
                else
                {
                    grip.gameObject.SetActive(false);
                    palm.gameObject.SetActive(true);
                }
            }

            if (Input.GetKeyUp(KeyCode.X))
            {
                poke.gameObject.SetActive(true);
                grip.gameObject.SetActive(false);
                palm.gameObject.SetActive(false);
            }
        }

        protected virtual void UpdateInEditor()
        {
            Ray ray = MainCameraCache.mainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 oldPosition = transform.position;
            transform.position = MainCameraCache.mainCamera.transform.forward * (-1000.0f);

            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit, fallbackMaxDistanceNoItem))
            {
                if (Input.GetMouseButton(0))
                {
                    transform.position = raycastHit.point;
                }
                else
                {
                    transform.position = raycastHit.point + raycastHit.normal * 0.02f;
                }
                fallbackInteractorDistance = Mathf.Min(fallbackMaxDistanceNoItem, raycastHit.distance);
            }
            else if (fallbackInteractorDistance > 0.0f)
            {
                transform.position = ray.origin + Mathf.Min(fallbackMaxDistanceNoItem, fallbackInteractorDistance) * ray.direction;
            }
            else
            {
                transform.position = oldPosition;
            }
        }
    }
}

