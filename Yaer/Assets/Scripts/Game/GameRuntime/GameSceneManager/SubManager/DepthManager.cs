using System.Collections.Generic;
using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Name.Settings;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.SubManager
{
    public class DepthManager : MonoBehaviour, IDepthManager
    {
        private List<IDepthObject> allObjs;
        private Dictionary<int, List<IDepthObject>> layerDic;

        private IPlayer player;
        private IGameSceneManager sceneManager;

        private void FixedUpdate()
        {
            // UpdateLayer();
        }

        public void Init(IGameSceneManager manager)
        {
            sceneManager = manager;
            allObjs = new List<IDepthObject>();
            layerDic = new Dictionary<int, List<IDepthObject>>();
            //
            // layerDic.Add(LayerMask.NameToLayer(LayerName.SceneObjectOther), new List<IDepthObject>());
            // layerDic.Add(LayerMask.NameToLayer(LayerName.SceneObjectDepth), new List<IDepthObject>());
            // layerDic.Add(LayerMask.NameToLayer(LayerName.SceneObjectPlayer), new List<IDepthObject>());
            // layerDic.Add(LayerMask.NameToLayer(LayerName.SceneObjectPhy1), new List<IDepthObject>());
            // layerDic.Add(LayerMask.NameToLayer(LayerName.SceneObjectPhy2), new List<IDepthObject>());
            // layerDic.Add(LayerMask.NameToLayer(LayerName.SceneObjectPhy3), new List<IDepthObject>());
            // layerDic.Add(LayerMask.NameToLayer(LayerName.SceneObjectPhy4), new List<IDepthObject>());
            // layerDic.Add(LayerMask.NameToLayer(LayerName.SceneObjectPhy5), new List<IDepthObject>());
            // layerDic.Add(LayerMask.NameToLayer(LayerName.SceneObjectPhy6), new List<IDepthObject>());
        }

        public void RegisterPlayerAndObjs(List<ISceneObject> objs, IPlayer player)
        {
            // this.player = player;
            //
            // // 获取含有深度组件的对象
            // foreach (var obj in objs)
            // {
            //     if (obj is IDepthObject depth)
            //     {
            //         Add(depth);
            //     }
            // }
        }

        public void ExitScene()
        {
            allObjs.Clear();
            layerDic.Clear();
            player = null;
            sceneManager = null;
        }

        private void Add(IDepthObject obj)
        {
            allObjs.Add(obj);

            // 首次分配层级
            if (obj.GameObject.CompareTag("Player"))
            {
                obj.DepthComponent.LayerID = LayerMask.NameToLayer(LayerName.SceneObjectPlayer);
                layerDic[obj.DepthComponent.LayerID].Add(obj);
                return;
            }

            // obj.DepthComponent.LayerID = sceneManager.GetLayerIndex(obj.DepthComponent.FootCld.bounds.center.y);
            layerDic[obj.DepthComponent.LayerID].Add(obj);
        }

        public void Remove(IDepthObject obj)
        {
            allObjs.Remove(obj);
        }

        private void UpdateLayer()
        {
            foreach (var o in allObjs)
            {
                if (o.GameObject.CompareTag("Player")) continue;

                // 判断是否在玩家层
                if (player.IsInSameDepth(o))
                {
                    var index = LayerMask.NameToLayer(LayerName.SceneObjectPlayer);
                    if (o.DepthComponent.LayerID != index)
                    {
                        layerDic[o.DepthComponent.LayerID].Remove(o);
                        o.DepthComponent.LayerID = index;
                        layerDic[index].Add(o);
                    }

                    continue;
                }

                // var newId = sceneManager.GetLayerIndex(o.DepthComponent.FootCld.bounds.center.y);
                // if (o.DepthComponent.LayerID != newId)
                // {
                //     layerDic[o.DepthComponent.LayerID].Remove(o);
                //     o.DepthComponent.LayerID = newId;
                //     layerDic[newId].Add(o);
                // }
            }
        }
    }
}