//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;

namespace GameFramework.UnityRuntime.Entity
{
    internal sealed class ShowEntityInfo : IReference
    {
        private Type m_EntityLogicType;
        private object m_UserData;
        private int m_EntityId;

        public ShowEntityInfo()
        {
            m_EntityLogicType = null;
            m_UserData = null;
        }

        public Type EntityLogicType
        {
            get
            {
                return m_EntityLogicType;
            }
        }

        public object UserData
        {
            get
            {
                return m_UserData;
            }
        }

        public int EntityID => m_EntityId;

        public static ShowEntityInfo Create(Type entityLogicType, int entityID, object userData)
        {
            ShowEntityInfo showEntityInfo = GameFramework.ReferencePool.Acquire<ShowEntityInfo>();
            showEntityInfo.m_EntityLogicType = entityLogicType;
            showEntityInfo.m_UserData = userData;
            showEntityInfo.m_EntityId = entityID;
            return showEntityInfo;
        }

        public void Clear()
        {
            m_EntityLogicType = null;
            m_UserData = null;
        }
    }
}
