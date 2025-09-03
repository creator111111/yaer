using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Manager
{
    public class ComponentManager : MonoBehaviour
    {
        private Transform root;
        public List<GameComponent> Components { get; } = new List<GameComponent>();

        private void Awake()
        {
            Components.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            foreach (var component in Components)
            {
                component.Init(gameObject);
                component.AwakeComponent();
            }
        }

        private void Start()
        {
            Components.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            foreach (var component in Components) component.StartComponent();
        }

        private void Update()
        {
            // 按优先级排序并调用 UpdateComponent
            Components.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            foreach (var component in Components) component.UpdateComponent();
        }

        private void OnDestroy()
        {
            Components.Clear();
            DestroyImmediate(root.gameObject);
            root = null;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            root = transform.Find("Components");

            if (root is null)
            {
                root = new GameObject("Components").transform;
                root.parent = transform;
            }
#endif
        }

        public void RefreshComponents()
        {
            Components.Clear();
            foreach (Transform child in root)
            {
                var component = child.GetComponent<GameComponent>();
                if (component != null) Components.Add(component);
            }

            SortComponentObj();
        }

        public void AddComponent<T>(T component) where T : GameComponent
        {
            component.transform.parent = root;
            Components.Add(component);

            SortComponentObj();
        }

        public void AddComponent(Type type)
        {
            var obj = new GameObject(type.Name, type);
            obj.transform.parent = root;
            SortComponentObj();
            Debug.Log($"Added component: {type.Name}");
        }

        public void RemoveComponent<T>(T component) where T : GameComponent
        {
            Components.Remove(component);
            DestroyImmediate(component.gameObject);

            SortComponentObj();
        }

        public void RemoveComponent(Type type)
        {
            var obj = root.Find(type.Name);
            if (obj != null) Destroy(obj.gameObject);

            SortComponentObj();
        }

        public void RemoveComponent(string name)
        {
            var obj = root.Find(name);
            if (obj != null) Destroy(obj.gameObject);

            SortComponentObj();
        }

        public void RemoveAllComponents()
        {
            foreach (Transform child in root) Destroy(child.gameObject);

            SortComponentObj();
        }

        public void SortComponentObj()
        {
            // 获取所有子对象及其优先级
            var children = new List<(Transform transform, int priority)>();
            foreach (Transform child in root)
            {
                var component = child.GetComponent<GameComponent>();
                if (component != null) children.Add((child, component.Priority));
            }

            // 按优先级排序
            children.Sort((a, b) => b.priority.CompareTo(a.priority));

            // 设置新的层级顺序
            for (var i = 0; i < children.Count; i++) children[i].transform.SetSiblingIndex(i);
        }
    }
}