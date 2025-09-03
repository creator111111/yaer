using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.UnityRuntimeExtend.Pool
{
    public class PoolContainer
    {
        // 该容器的父对象
        public GameObject father;

        // 储存的所有游戏对象list
        public List<GameObject> objectList;

        public PoolContainer(string objectName, GameObject poolObject)
        {
            objectList = new List<GameObject>();
            // 用对象名字创建父对象储存list
            father = new GameObject(objectName);
            // 设为pool对象的子物体
            father.transform.parent = poolObject.transform;
        }

        public GameObject Get()
        {
            // 获取末尾的对象
            var targetObject = objectList[0];
            // 断开父子关系
            targetObject.transform.parent = null;
            // 从list移除
            objectList.RemoveAt(0);

            targetObject.SetActive(true);
            // 调用OnGet方法
            if (targetObject.TryGetComponent<IPoolObject>(out var poolObjectComponent)) poolObjectComponent.OnGet();

            return targetObject;
        }

        public void Push(GameObject gameObject)
        {
            if (objectList.Contains(gameObject)) return;

            objectList.Add(gameObject);
            // 设置为list的子对象
            gameObject.transform.SetParent(father.transform);
            // 调用OnPush方法
            gameObject.GetComponent<IPoolObject>()?.OnPush();
            // 失活
            gameObject.SetActive(false);
        }
    }
}