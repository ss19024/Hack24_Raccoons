using System;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rokid.UXR.Interaction
{
	public class ProximityLight : MonoBehaviour
	{
		[SerializeField]
		private PokeInteractor pokeInteractor;
		[SerializeField]
		private Material lightMat;

		[SerializeField]
		private GameObject lightPoint;

		[SerializeField]
		private float baseDistance = 0.1f;

		[SerializeField]
		private float minScale = 0.2f;

		[SerializeField]
		private float maxScale = 2f;

		private void Start()
		{
			if (pokeInteractor == null)
			{
				pokeInteractor = GetComponentInParent<PokeInteractor>();
			}
			if (lightMat == null)
			{
				lightMat = GetComponentInChildren<MeshRenderer>().material;
			}
			if (lightPoint == null)
			{
				lightPoint = transform.GetChild(0).gameObject;
			}
		}

		private void Update()
		{
			bool light_active = false;
			if (pokeInteractor.HasInteractable)
			{
				// pointer pokeInteractor.Interactable 
				Vector3 touchPoint = pokeInteractor.TouchPoint;
				Vector3 touchNormal = pokeInteractor.TouchNormal;
				if (touchPoint != Vector3.zero)
				{
					if (pokeInteractor.InteractorButtonUpPosition != Vector3.zero)
					{
						transform.position = new Vector3(touchPoint.x, touchPoint.y, pokeInteractor.InteractorButtonUpPosition.z);
					}
					else
					{
						transform.position = touchPoint;
					}
					transform.rotation = Quaternion.FromToRotation(Vector3.back, pokeInteractor.TouchNormal);
					float distance = Vector3.Distance(pokeInteractor.transform.position, transform.position);
					distance *= Mathf.Sign(Vector3.Dot(pokeInteractor.TouchNormal, pokeInteractor.transform.position - transform.position));
					float scale = Mathf.Clamp(distance / baseDistance, minScale, maxScale);
					lightPoint.transform.localScale = Vector3.one * scale;

					lightMat.SetFloat("_Opacity", Mathf.Clamp(0.4f / (scale * 0.8f), 0, 1f));
					light_active = distance > 0.001f;
				}
			}
			else
			{
				light_active = false;
				//lightPoint.gameObject.SetActive(false);
			}

			if (light_active != lightPoint.activeSelf)
			{
				lightPoint.SetActive(light_active);
			}
		}
	}
}
