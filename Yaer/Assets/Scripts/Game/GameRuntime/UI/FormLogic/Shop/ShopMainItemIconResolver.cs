using System;
using Game.DataTable.MainItem;
using Game.Static.Enum.Goods;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Shop
{
    /// <summary>
    /// 已收敛至 <see cref="MainItemDefProvider.ResolveIcon"/>；保留此类避免 ShopBarRowView 调用点大面积改动。
    /// </summary>
    [Obsolete("Use MainItemDefProvider.ResolveIcon instead.")]
    public static class ShopMainItemIconResolver
    {
        /// <summary>解析列表小图标，逻辑与 MainItemDefProvider 一致。</summary>
        public static Sprite ResolveIcon(EMainItemName itemId)
        {
            return MainItemDefProvider.ResolveIcon(itemId);
        }
    }
}
