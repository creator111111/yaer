using Game.GameRuntime.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEffects
{
    public class CharacterEnvironmentShadow : MonoBehaviour
    {
        private EnvironmentShadowCamera shadowCamera;
        private SpriteRenderer spriteRenderer;
        // Start is called before the first frame update
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (shadowCamera == null)
            {
                shadowCamera = FindObjectOfType<EnvironmentShadowCamera>();
            }
            if (shadowCamera != null)
            {
                spriteRenderer.material.SetTexture("_ShadowTex", shadowCamera.myCamera.targetTexture);
            }
        }
    }
}