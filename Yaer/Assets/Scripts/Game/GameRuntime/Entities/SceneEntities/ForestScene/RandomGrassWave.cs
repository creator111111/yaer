using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.ForestScene
{
    public class RandomGrassWave : MonoBehaviour
    {
        [SerializeField] private Material[] WaveMaterials;
        [SerializeField] private bool useChildren;
        [SerializeField] private SpriteRenderer[] Grases;

        private void Start()
        {
            if (useChildren) Grases = transform.GetComponentsInChildren<SpriteRenderer>();
            SetRandomMaterials();
        }

        private void SetRandomMaterials()
        {
            var matCount = WaveMaterials.Length;
            var grassCount = Grases.Length;
            if (matCount <= 0)
            {
                Debug.LogError("草丛摆动材质丢失，请检查");
                return;
            }

            for (var i = 0; i < grassCount; i++) Grases[i].material = WaveMaterials[Random.Range(0, matCount)];
            // Debug.Log(Grases[i].material);
        }
    }
}