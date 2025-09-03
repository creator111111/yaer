using System.Collections.Generic;
using System.Linq;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.CldController
{
    /// <summary>
    /// 整体控制碰撞盒组件
    /// </summary>
    public class CldControllerComponent : BaseGFComponentMono
    {
        [SerializeField] private Transform nodeParentTsf;
        [SerializeField] private List<CldControllerNode> nodes = new List<CldControllerNode>();
        private Dictionary<string, List<CldControllerNode>> nodeGroupMap = new Dictionary<string, List<CldControllerNode>>();

        private void OnValidate()
        {
            if (nodeParentTsf != null)
            {
                nodes = nodeParentTsf.GetComponentsInChildren<CldControllerNode>().ToList();
            }
        }

        protected override void OnInit()
        {
            foreach (var node in nodes)
            {
                if (nodeGroupMap.ContainsKey(node.groupName))
                {
                    nodeGroupMap[node.groupName].Add(node);
                }
                else
                {
                    nodeGroupMap.Add(node.groupName, new List<CldControllerNode>() { node });
                }
            }
        }

        public void SetIsTriggerGroup(string groupName, bool isTrigger)
        {
            if (nodeGroupMap.ContainsKey(groupName))
            {
                foreach (var node in nodeGroupMap[groupName])
                {
                    node.SetIsTrigger(isTrigger);
                }
            }
        }

        public void SetIsTriggerOne(string groupName, string nodeName, bool isTrigger)
        {
            if (nodeGroupMap.ContainsKey(groupName))
            {
                foreach (var node in nodeGroupMap[groupName])
                {
                    if (node.name == nodeName)
                    {
                        node.SetIsTrigger(isTrigger);
                    }
                }
            }
        }
        
        public void SetIsTriggerAll(bool isTrigger)
        {
            foreach (var node in nodes)
            {
                node.SetIsTrigger(isTrigger);
            }
        }
        
        public void SetActiveGroup(string groupName, bool isActive)
        {
            if (nodeGroupMap.ContainsKey(groupName))
            {
                foreach (var node in nodeGroupMap[groupName])
                {
                    node.SetCldActive(isActive);
                }
            }
        }
        
        public void SetActiveOne(string groupName, string nodeName, bool isActive)
        {
            if (nodeGroupMap.ContainsKey(groupName))
            {
                foreach (var node in nodeGroupMap[groupName])
                {
                    if (node.name == nodeName)
                    {
                        node.SetCldActive(isActive);
                    }
                }
            }
        }
        
        public void SetActiveAll(bool isActive)
        {
            foreach (var node in nodes)
            {
                node.SetCldActive(isActive);
            }
        }
    }
}