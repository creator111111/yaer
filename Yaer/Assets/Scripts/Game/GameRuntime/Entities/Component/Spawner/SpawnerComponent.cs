using System.Collections;
using System.Collections.Generic;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Spawner
{
    public class SpawnerComponent : BaseGFComponentMono
    {
        [SerializeField] private float spawnTime;
        [SerializeField] private int maxCount;
        [SerializeField] private List<GameObject> clonePrefabs = new List<GameObject>();

        private bool start;
        private bool pause;
     
        private Coroutine coroutine;
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private Dictionary<string, GameObject> clonePrefabDic = new Dictionary<string, GameObject>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (clonePrefabs.Count == 0) Debug.LogError(GetType().Name + "的ClonePrefab未绑定");
        }
#endif
        protected override void OnInit()
        {
            foreach (var prefab in clonePrefabs)
            {
                if (clonePrefabDic.ContainsKey(prefab.name))
                {
                    Debug.LogError(GetType().Name + "ClonePrefab名称重复");
                    continue;
                }

                clonePrefabDic.Add(prefab.name, prefab);
            }
        }

        public void StartSpawn(string prefabName)
        {
            start = true;
            StartCoroutine(SpawnCoroutine(prefabName));
        }

        public void PauseSpawn(string key)
        {
            start = false;
            StopCoroutine(coroutine);
        }

        public void StopSpawn(string key)
        {
            start = false;
            StopCoroutine(coroutine);
        }

        public void Spawn(string key)
        {
            if (clonePrefabDic.TryGetValue(key, out var prefab))
            {
                var obj = Instantiate(prefab, transform.position, transform.rotation, transform.parent);
                spawnedObjects.Add(obj);
            }
        }

        private IEnumerator SpawnCoroutine(string prefabName)
        {
            while (start)
            {
                if (maxCount <= 0)
                {
                    break;
                }

                Spawn(prefabName);
                maxCount--;
                yield return new WaitForSeconds(spawnTime);
            }
        }
    }
}