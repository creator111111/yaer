using System;
using System.Collections.Generic;
using Game.GameRuntime.Entities.Generic;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Name.Settings;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Map
{
    public class Map : MonoBehaviour
    {
        [Header("出生点")] public Transform defaultBornTsf; // 地图默认出生位置

        [Header("场景地图设置")] public Transform mapLeftTsf;

        public Transform mapRightTsf;
        public Transform leftWall;
        public Transform rightWall;
        [SerializeField] private SceneChangeDoor leftDoor;
        [SerializeField] private SceneChangeDoor rightDoor;
        public Transform leftBornTsf;
        public Transform rightBornTsf;

        // layer
        [SerializeField] private int layerCount = 1;
        [SerializeField] private float layerWidth = 1;
        [SerializeField] private Transform layerTsf;
        private readonly Dictionary<string, int> layerIndex = new Dictionary<string, int>();
        private readonly List<LayerArea> layerList = new List<LayerArea>();

        public SceneChangeDoor LeftDoor => leftDoor;
        public SceneChangeDoor RightDoor => rightDoor;

        [SerializeField] private SceneEntityComponentGSM sceneEntityComponentGSM;

        private void Start()
        {
            if (mapLeftTsf == false) Debug.LogWarning(GetType().Name + "没有设置地图左边界");
            if (mapRightTsf == false) Debug.LogWarning(GetType().Name + "没有设置地图右边界");

            if (leftWall == false) Debug.LogWarning(GetType().Name + "没有设置地图左墙");
            if (rightWall == false) Debug.LogWarning(GetType().Name + "没有设置地图右墙");

            if (leftDoor == false) Debug.LogWarning(GetType().Name + "没有设置左门");
            if (rightDoor == false) Debug.LogWarning(GetType().Name + "没有设置右门");

            if (leftBornTsf == false) Debug.LogWarning(GetType().Name + "没有设置左出生点");
            if (rightBornTsf == false) Debug.LogWarning(GetType().Name + "没有设置右出生点");

            if (defaultBornTsf == false) Debug.LogWarning(GetType().Name + "没有设置默认出生点");
        }

        private void OnValidate()
        {
            FindObject();

            if (layerCount > 6)
            {
                Debug.LogError("地图最多设置6层");
                layerCount = 6;
            }
        }


        protected virtual void FindObject()
        {
            mapLeftTsf = transform.Find("MapLeft");
            mapRightTsf = transform.Find("MapRight");
            defaultBornTsf = transform.Find("DefaultBornPos");
            leftWall = mapLeftTsf.transform.Find("LeftWall");
            rightWall = mapRightTsf.transform.Find("RightWall");
            leftDoor = mapLeftTsf.transform.Find("LeftDoor").GetComponent<SceneChangeDoor>();
            rightDoor = mapRightTsf.transform.Find("RightDoor").GetComponent<SceneChangeDoor>();

            leftBornTsf = transform.Find("LeftBorn");
            rightBornTsf = transform.Find("RightBorn");
        }

        public void OnInit()
        {
            FindObject();

            if (sceneEntityComponentGSM != null)
            {
                leftDoor.OnInit(sceneEntityComponentGSM);
                rightDoor.OnInit(sceneEntityComponentGSM);
            }
            // if (depthOpen)
            // {
            //     for (var i = 0; i < layerCount; i++) layerList.Add(new LayerArea(layerTsf.position.y + (i + 1) * layerWidth, layerTsf.position.y + i * layerWidth));
            //
            //     layerIndex.Add(LayerName.SceneObjectOther, LayerMask.NameToLayer(LayerName.SceneObjectOther));
            //     layerIndex.Add(LayerName.SceneObjectPhy1, LayerMask.NameToLayer(LayerName.SceneObjectPhy1));
            //     layerIndex.Add(LayerName.SceneObjectPhy2, LayerMask.NameToLayer(LayerName.SceneObjectPhy2));
            //     layerIndex.Add(LayerName.SceneObjectPhy3, LayerMask.NameToLayer(LayerName.SceneObjectPhy3));
            //     layerIndex.Add(LayerName.SceneObjectPhy4, LayerMask.NameToLayer(LayerName.SceneObjectPhy4));
            //     layerIndex.Add(LayerName.SceneObjectPhy5, LayerMask.NameToLayer(LayerName.SceneObjectPhy5));
            //     layerIndex.Add(LayerName.SceneObjectPhy6, LayerMask.NameToLayer(LayerName.SceneObjectPhy6));
            // }
        }

        public int GetLayerIndex(float y)
        {
            foreach (var a in layerList)
                if (a.yMax > y && a.yMin < y)
                {
                    var index = layerList.IndexOf(a);
                    switch (index + 1)
                    {
                        case 1:
                            return layerIndex[LayerName.SceneObjectPhy1];
                        case 2:
                            return layerIndex[LayerName.SceneObjectPhy2];
                        case 3:
                            return layerIndex[LayerName.SceneObjectPhy3];
                        case 4:
                            return layerIndex[LayerName.SceneObjectPhy4];
                        case 5:
                            return layerIndex[LayerName.SceneObjectPhy5];
                        case 6:
                            return layerIndex[LayerName.SceneObjectPhy6];
                    }
                }

            return layerIndex[LayerName.SceneObjectOther];
        }

        // gizmos
#if UNITY_EDITOR
        [Header("Gizmos")] [SerializeField] private bool showGizmos = true;

        [SerializeField] private bool showLayerArea = true;

        protected virtual void OnDrawGizmos()
        {
            if (!showGizmos) return;

            if (mapLeftTsf)
            {
                Gizmos.color = Color.red;
                var position = mapLeftTsf.position;
                Gizmos.DrawLine(new Vector3(position.x, position.y - 10.8f), new Vector3(position.x, position.y + 10.8f));
            }

            if (mapRightTsf)
            {
                Gizmos.color = Color.red;
                var position = mapRightTsf.position;
                Gizmos.DrawLine(new Vector3(position.x, position.y - 10.8f), new Vector3(position.x, position.y + 10.8f));
            }

            if (layerTsf && showLayerArea)
            {
                Gizmos.color = Color.yellow;
                var p = layerTsf.position;
                for (var i = 0; i < layerCount; i++)
                {
                    var y1 = layerTsf.position.y + i * layerWidth;
                    var y2 = layerTsf.position.y + (i + 1) * layerWidth;
                    Gizmos.DrawLine(new Vector3(mapLeftTsf.position.x, y1), new Vector3(mapRightTsf.position.x, y1));
                    Gizmos.DrawLine(new Vector3(mapLeftTsf.position.x, y2), new Vector3(mapRightTsf.position.x, y2));
                }
            }
        }
#endif
    }
}