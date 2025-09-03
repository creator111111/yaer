using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.UI.Component
{
    public class ComponentSystemUI: ComponentSystemMono
    {
        [SerializeField] private GameObject componentsRoot;
        
//         protected override void OnValidate()
//         {
//             // ui
// #if UNITY_EDITOR
//             if (componentsRoot == null)
//             {
//                 Log.Warning("componentsRoot引用丢失");
//                 return;
//             }
//             
//             root = transform.Find("Components");
//
//             if (root is null && !Application.isPlaying)
//             {
//                 root = Instantiate(componentsRoot, transform).transform;
//                 root.name = "Components";
//                 // 设置父对象
//                 root.SetParent(transform);
//                 // 重置位置（UI 对象可能还需要设置 AnchoredPosition、LocalScale 等）
//                 root.localPosition = Vector3.zero;
//                 root.localRotation = Quaternion.identity;
//                 root.localScale = Vector3.one;
//
//                 var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
//                 if (prefabStage != null)
//                 {
//                     EditorUtility.SetDirty(prefabStage.prefabContentsRoot);
//                 }
//             }
//
//             // 自动更新可视化组件
//             RefreshComponents();
// #endif
//         }
    }
}