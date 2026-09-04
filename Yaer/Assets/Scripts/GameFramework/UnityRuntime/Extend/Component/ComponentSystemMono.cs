using System;
using System.Collections.Generic;
using GameFramework.CoreExtend.Component;
using GameFramework.CoreExtend.Component.interf;
using GameFramework.UnityRuntime.Utility;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
using UnityEngine.UI;
#endif

namespace GameFramework.UnityRuntimeExtend.Component
{
    /// <summary>
    /// 仅仅使用Mono的 可视化
    /// </summary>
    public class ComponentSystemMono : MonoBehaviour//组件系统的“运行时载体”，负责把可视化组件同步到系统
    {
        [SerializeField] private List<BaseGFComponentMono> componentsList = new List<BaseGFComponentMono>();

        protected Transform root;
        private IComponentSystem componentSystem;

        public List<BaseGFComponentMono> GetAddComponents() => componentsList;

        public Action onInitBeforeAction;

        #region Unity回调

#if UNITY_EDITOR
        public void CreateComponentRoot()
        {
            root = transform.Find("Components");

            if (root is null)
            {
                root = new GameObject("Components").transform;
                root.parent = transform;
                root.localPosition = Vector3.zero;
            }
            
            // 自动更新可视化组件
            RefreshComponents();
        }
#endif

        public void OnInit()
        {
            root = transform.Find("Components");
            componentSystem = new ComponentSystem();
            // 序列化 List 常含 Inspector「None」空槽；不先清掉会 Sync 进运行时，InitComponents 对 null 调 Init → NRE 黑屏
            // （Village_HomeScene1 半套施工案例）。RefreshComponents 也会 RemoveAll(null)，但 OnInit 原先不调用它。
            componentsList.RemoveAll(item => item == null);
            SyncComponentsToSystem();
            onInitBeforeAction?.Invoke();
            componentSystem.InitComponents();
        }

        public void CheckComponents()
        {
            componentSystem.CheckComponents();
        }


        public void OnUpdate()
        {
            componentSystem?.UpdateComponents();
        }

        public void OnFixedUpdate()
        {
            foreach (var baseGFComponentMono in componentsList)
            {
                baseGFComponentMono.OnFixedUpdate();
            }
        }

        #endregion


        public void RefreshComponents()
        {
            // 删除所有空引用
            componentsList.RemoveAll(item => item == null);
            var components = root.GetComponentsInChildren<BaseGFComponentMono>();
            foreach (var cpn in components)
            {
                if (cpn.gameObject == gameObject)
                {
                    continue;
                }

                if (componentsList.Contains(cpn) == false)
                {
                    componentsList.Add(cpn);
                }
            }
        }

        public void SortComponent()
        {
            componentsList.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            // 获取所有子对象及其优先级
            var children = new List<(Transform transform, int priority)>();
            foreach (Transform child in root)
            {
                var componentMono = child.GetComponent<BaseGFComponentMono>();
                if (componentMono != null) children.Add((child, componentMono.Priority));
            }

            // 按优先级排序
            children.Sort((a, b) => b.priority.CompareTo(a.priority));

            // 设置新的层级顺序
            for (var i = 0; i < children.Count; i++) children[i].transform.SetSiblingIndex(i);
        }

        // --------------------------------------------------------------------------------
        
        public bool HasComponent<T>() where T : class, IGFEComponent => componentSystem.HasComponent<T>();
        
        public T TryGetComponent<T>() where T : class, IGFEComponent => componentSystem.TryGetComponent<T>();

        public new T GetComponent<T>() where T : class, IGFEComponent
        {
            var component = componentSystem.GetComponent<T>();
            if (component != null) return component;
            Log.Warning("ComponentSystem没有找到组件" + typeof(T).Name);
            return null;
        }

        //-----------------------------------------------------------------------------------

        public void AddComponent(Type componentMonoType)
        {
            var c = componentsList.Find(x => x.GetType() == componentMonoType);
            if (c != null) return;

            var obj = new GameObject(componentMonoType.Name);
            obj.transform.parent = root.transform;
            obj.transform.localPosition = Vector3.zero;
            BaseGFComponentMono monoComponent = obj.AddComponent(componentMonoType) as BaseGFComponentMono;
#if UNITY_EDITOR

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                EditorUtility.SetDirty(prefabStage.prefabContentsRoot);
            }

#endif
            if (monoComponent != null)
            {
                // system添加
                componentSystem?.AddComponent(monoComponent);

                // mono也添加
                componentsList.Add(monoComponent);
                SortComponent();
            }
            else
            {
                DestroyImmediate(obj);
                Debug.LogError("创建可视化mono组件失败");
            }
        }

        public void AddComponent(IGFEComponent componentMono) => AddComponent(componentMono.GetType());
        public void AddComponent<T>() where T : class, IGFEComponent => AddComponent(typeof(T));

        //-----------------------------------------------------------------------------------

        public void RemoveComponent(Type type)
        {
            componentsList.RemoveAll(x => x.GetType() == type);
            componentSystem?.RemoveComponent(type);
            var obj = root.transform.Find(type.Name);
            if (obj != null) DestroyImmediate(obj.gameObject);

            SortComponent();
        }

        public void RemoveComponent(IGFEComponent componentMono) => RemoveComponent(componentMono.GetType());

        public void RemoveComponent<T>() where T : class, IGFEComponent => RemoveComponent(typeof(T));

        private void SyncComponentsToSystem()
        {
            foreach (var component in componentsList)
            {
                // 跳过空槽，避免 AddComponent(null) 后 Init 炸场景
                if (component == null)
                {
                    continue;
                }

                if (componentSystem != null && !componentSystem.HasComponent(component))
                {
                    componentSystem.AddComponent(component);
                }
            }
        }
    }
}