using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Component
{
    public class EnvironmentShadowCamera : MonoBehaviour
    {
        private Camera mainCamera;

        public Camera myCamera { get; private set; }

        private void OnEnable()
        {
            mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            myCamera = GetComponent<Camera>();
            Debug.Log($"[EnvironmentShadowCamera] {myCamera.targetTexture.name}");
        }

        private void Update()
        {
            if (mainCamera == null) 
            {
                mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            }
            else
            {
                transform.position = mainCamera.transform.position;
            }
        }
    }
}

