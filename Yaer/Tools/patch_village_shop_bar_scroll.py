#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
0704 Shop_Bar 列表滚动 · Village_Shop 场景 YAML 补丁（SC-0～SC-4）。
依据：Assets/Doc/执行文档/0704/Shop_Bar列表滚动_架构溯源与施工执行说明.md

用法（Unity 关闭时）：
  python Tools/patch_village_shop_bar_scroll.py

若 Unity 已打开同场景，请先保存/关闭场景再运行，或在 Unity 内执行：
  Tools / Shop / Setup Bar List Scroll (SC-0~SC-4)
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

SCENE = Path(__file__).resolve().parents[1] / "Assets/GameRes/Scenes/Village_Shop.unity"

# 常量与文档 §4.2 一致
ROW_HEIGHT = 88
ROW_SPACING = 16
VISIBLE_ROWS = 6
VIEWPORT_HEIGHT = ROW_HEIGHT * VISIBLE_ROWS + ROW_SPACING * (VISIBLE_ROWS - 1)  # 568
TEST_ROW_COUNT = 8

BAR_RT = 223743912
BAR_BG_GO = 955175014
BAR_BG_RT = 955175015
BAR_BG_IMAGE = 955175016
EXISTING_SHOP_BAR_PI = 3823689269437328575
EXISTING_SHOP_BAR_STRIPPED_RT = 1639222911

# 新节点 fileID（避免与场景现有 ID 冲突）
IDS = {
    "scroll_go": 770070040001,
    "scroll_rt": 770070040002,
    "scroll_cr": 770070040003,
    "scroll_img": 770070040004,
    "scroll_sr": 770070040005,
    "viewport_go": 770070040010,
    "viewport_rt": 770070040011,
    "viewport_cr": 770070040012,
    "viewport_img": 770070040013,
    "viewport_mask": 770070040014,
    "viewport_rmask": 770070040015,
    "content_go": 770070040020,
    "content_rt": 770070040021,
    "content_vlg": 770070040022,
    "content_csf": 770070040023,
    "sb_go": 770070040030,
    "sb_rt": 770070040031,
    "sb_cr": 770070040032,
    "sb_img": 770070040033,
    "sb_comp": 770070040034,
    "sb_area_go": 770070040035,
    "sb_area_rt": 770070040036,
    "sb_handle_go": 770070040037,
    "sb_handle_rt": 770070040038,
    "sb_handle_cr": 770070040039,
    "sb_handle_img": 770070040040,
}

SHOP_BAR_GUID = "fbb1826d55e82e24d90e90c345f43328"
SHOP_BAR_ROOT = "3823689270002737856"
SHOP_BAR_NAME = "3823689270002737857"


def shop_bar_prefab_block(pi_id: int, stripped_rt: int, root_order: int, name: str) -> str:
    """生成 Shop_Bar 预制体实例块（Content 下，Layout 接管位置）。"""
    return f"""--- !u!1001 &{pi_id}
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    m_TransformParent: {{fileID: {IDS['content_rt']}}}
    m_Modifications:
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_Pivot.x
      value: 0.5
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_Pivot.y
      value: 0.5
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_RootOrder
      value: {root_order}
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_AnchorMax.x
      value: 0.5
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_AnchorMax.y
      value: 0.5
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_AnchorMin.x
      value: 0.5
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_AnchorMin.y
      value: 0.5
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_SizeDelta.x
      value: 482
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_SizeDelta.y
      value: {ROW_HEIGHT}
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_AnchoredPosition.x
      value: 0
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_AnchoredPosition.y
      value: 0
      objectReference: {{fileID: 0}}
    - target: {{fileID: {SHOP_BAR_NAME}, guid: {SHOP_BAR_GUID}, type: 3}}
      propertyPath: m_Name
      value: {name}
      objectReference: {{fileID: 0}}
    m_RemovedComponents: []
  m_SourcePrefab: {{fileID: 100100000, guid: {SHOP_BAR_GUID}, type: 3}}
--- !u!224 &{stripped_rt} stripped
RectTransform:
  m_CorrespondingSourceObject: {{fileID: {SHOP_BAR_ROOT}, guid: {SHOP_BAR_GUID}, type: 3}}
  m_PrefabInstance: {{fileID: {pi_id}}}
  m_PrefabAsset: {{fileID: 0}}
"""


def scroll_hierarchy_yaml() -> str:
    i = IDS
    content_children = "\n".join(
        f"  - {{fileID: {770070041000 + n}}}" for n in range(TEST_ROW_COUNT)
    )
    shop_bars = ""
    for n in range(TEST_ROW_COUNT):
        pi = 770070042000 + n
        stripped = 770070041000 + n
        name = "Shop_Bar" if n == 0 else f"Shop_Bar ({n})"
        shop_bars += shop_bar_prefab_block(pi, stripped, n, name)

    return f"""
--- !u!1 &{i['scroll_go']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {i['scroll_rt']}}}
  - component: {{fileID: {i['scroll_cr']}}}
  - component: {{fileID: {i['scroll_img']}}}
  - component: {{fileID: {i['scroll_sr']}}}
  m_Layer: 5
  m_Name: Bar_ListScroll
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{i['scroll_rt']}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['scroll_go']}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children:
  - {{fileID: {i['viewport_rt']}}}
  - {{fileID: {i['sb_rt']}}}
  m_Father: {{fileID: {BAR_RT}}}
  m_RootOrder: 1
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0.5, y: 0.5}}
  m_AnchorMax: {{x: 0.5, y: 0.5}}
  m_AnchoredPosition: {{x: -449, y: -28.521484}}
  m_SizeDelta: {{x: 482, y: {VIEWPORT_HEIGHT}}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{i['scroll_cr']}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['scroll_go']}}}
  m_CullTransparentMesh: 1
--- !u!114 &{i['scroll_img']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['scroll_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 0}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 0}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{i['scroll_sr']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['scroll_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 1aa08ab6e0800fa44ae55d278d1423e3, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Content: {{fileID: {i['content_rt']}}}
  m_Horizontal: 0
  m_Vertical: 1
  m_MovementType: 1
  m_Elasticity: 0.1
  m_Inertia: 1
  m_DecelerationRate: 0.135
  m_ScrollSensitivity: 40
  m_Viewport: {{fileID: {i['viewport_rt']}}}
  m_HorizontalScrollbar: {{fileID: 0}}
  m_VerticalScrollbar: {{fileID: {i['sb_comp']}}}
  m_HorizontalScrollbarVisibility: 2
  m_VerticalScrollbarVisibility: 2
  m_HorizontalScrollbarSpacing: -3
  m_VerticalScrollbarSpacing: -3
  m_OnValueChanged:
    m_PersistentCalls:
      m_Calls: []
--- !u!1 &{i['viewport_go']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {i['viewport_rt']}}}
  - component: {{fileID: {i['viewport_cr']}}}
  - component: {{fileID: {i['viewport_img']}}}
  - component: {{fileID: {i['viewport_mask']}}}
  - component: {{fileID: {i['viewport_rmask']}}}
  m_Layer: 5
  m_Name: Viewport
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{i['viewport_rt']}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['viewport_go']}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children:
  - {{fileID: {i['content_rt']}}}
  m_Father: {{fileID: {i['scroll_rt']}}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 1}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: {VIEWPORT_HEIGHT}}}
  m_Pivot: {{x: 0.5, y: 1}}
--- !u!222 &{i['viewport_cr']}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['viewport_go']}}}
  m_CullTransparentMesh: 1
--- !u!114 &{i['viewport_img']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['viewport_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 10917, guid: 0000000000000000f000000000000000, type: 0}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{i['viewport_mask']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['viewport_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 31a19414c41e5ae4aae2af33fee712f6, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_ShowMaskGraphic: 0
--- !u!114 &{i['viewport_rmask']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['viewport_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 3312d7739989d2b4e91e6319e9a96d76, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Padding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Softness: {{x: 0, y: 0}}
--- !u!1 &{i['content_go']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {i['content_rt']}}}
  - component: {{fileID: {i['content_vlg']}}}
  - component: {{fileID: {i['content_csf']}}}
  m_Layer: 5
  m_Name: Content
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{i['content_rt']}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['content_go']}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children:
{content_children}
  m_Father: {{fileID: {i['viewport_rt']}}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 1}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 1}}
--- !u!114 &{i['content_vlg']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['content_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 59f8146938fff824cb5fd77236b75775, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Padding:
    m_Left: 0
    m_Right: 0
    m_Top: 0
    m_Bottom: 0
  m_ChildAlignment: 1
  m_Spacing: {ROW_SPACING}
  m_ChildForceExpandWidth: 1
  m_ChildForceExpandHeight: 0
  m_ChildControlWidth: 1
  m_ChildControlHeight: 1
  m_ChildScaleWidth: 0
  m_ChildScaleHeight: 0
  m_ReverseArrangement: 0
--- !u!114 &{i['content_csf']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['content_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 3245ec927659c4140ac4f8d17403cc18, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_HorizontalFit: 0
  m_VerticalFit: 2
--- !u!1 &{i['sb_go']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {i['sb_rt']}}}
  - component: {{fileID: {i['sb_cr']}}}
  - component: {{fileID: {i['sb_img']}}}
  - component: {{fileID: {i['sb_comp']}}}
  m_Layer: 5
  m_Name: Scrollbar Vertical
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{i['sb_rt']}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['sb_go']}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children:
  - {{fileID: {i['sb_area_rt']}}}
  m_Father: {{fileID: {i['scroll_rt']}}}
  m_RootOrder: 1
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 1, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 20, y: 0}}
  m_Pivot: {{x: 1, y: 1}}
--- !u!222 &{i['sb_cr']}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['sb_go']}}}
  m_CullTransparentMesh: 1
--- !u!114 &{i['sb_img']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['sb_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 10907, guid: 0000000000000000f000000000000000, type: 0}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &{i['sb_comp']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['sb_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 2a4db7a114972834c8e4117be1d82ba3, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {{fileID: 0}}
    m_SelectOnDown: {{fileID: 0}}
    m_SelectOnLeft: {{fileID: 0}}
    m_SelectOnRight: {{fileID: 0}}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {{r: 1, g: 1, b: 1, a: 1}}
    m_HighlightedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_PressedColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}}
    m_SelectedColor: {{r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}}
    m_DisabledColor: {{r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5019608}}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {{fileID: 0}}
    m_PressedSprite: {{fileID: 0}}
    m_SelectedSprite: {{fileID: 0}}
    m_DisabledSprite: {{fileID: 0}}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {{fileID: {i['sb_handle_img']}}}
  m_HandleRect: {{fileID: {i['sb_handle_rt']}}}
  m_Direction: 2
  m_Value: 0
  m_Size: 1
  m_NumberOfSteps: 0
  m_OnValueChanged:
    m_PersistentCalls:
      m_Calls: []
--- !u!1 &{i['sb_area_go']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {i['sb_area_rt']}}}
  m_Layer: 5
  m_Name: Sliding Area
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{i['sb_area_rt']}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['sb_area_go']}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children:
  - {{fileID: {i['sb_handle_rt']}}}
  m_Father: {{fileID: {i['sb_rt']}}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: -20, y: -20}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!1 &{i['sb_handle_go']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {i['sb_handle_rt']}}}
  - component: {{fileID: {i['sb_handle_cr']}}}
  - component: {{fileID: {i['sb_handle_img']}}}
  m_Layer: 5
  m_Name: Handle
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &{i['sb_handle_rt']}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['sb_handle_go']}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_Children: []
  m_Father: {{fileID: {i['sb_area_rt']}}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
  m_AnchorMin: {{x: 0, y: 0}}
  m_AnchorMax: {{x: 1, y: 1}}
  m_AnchoredPosition: {{x: 0, y: 0}}
  m_SizeDelta: {{x: 0, y: 0}}
  m_Pivot: {{x: 0.5, y: 0.5}}
--- !u!222 &{i['sb_handle_cr']}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['sb_handle_go']}}}
  m_CullTransparentMesh: 1
--- !u!114 &{i['sb_handle_img']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i['sb_handle_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_RaycastTarget: 1
  m_RaycastPadding: {{x: 0, y: 0, z: 0, w: 0}}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {{fileID: 10905, guid: 0000000000000000f000000000000000, type: 0}}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
{shop_bars}"""


def patch_scene(text: str) -> str:
    if "Bar_ListScroll" in text:
        print("场景已含 Bar_ListScroll，跳过。")
        return text

    # SC-0：Bar/BG → Bar_BG，关闭 Raycast
    text = text.replace(
        f"  m_GameObject: {{fileID: {BAR_BG_GO}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}\n  m_Name: \n  m_EditorClassIdentifier: \n  m_Material: {{fileID: 0}}\n  m_Color: {{r: 1, g: 1, b: 1, a: 1}}\n  m_RaycastTarget: 1",
        f"  m_GameObject: {{fileID: {BAR_BG_GO}}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}}\n  m_Name: \n  m_EditorClassIdentifier: \n  m_Material: {{fileID: 0}}\n  m_Color: {{r: 1, g: 1, b: 1, a: 1}}\n  m_RaycastTarget: 0",
        1,
    )

    # 仅改 Bar 下 BG 节点名（father = Bar RT）
    text = re.sub(
        rf"(m_Father: {{fileID: {BAR_RT}}}\n  m_RootOrder: 0\n  m_LocalEulerAnglesHint[^\n]+\n  m_AnchorMin[^\n]+\n  m_AnchorMax[^\n]+\n  m_AnchoredPosition[^\n]+\n  m_SizeDelta[^\n]+\n  m_Pivot[^\n]+\n--- !u!114 &{BAR_BG_IMAGE})",
        lambda m: m.group(0),
        text,
    )
    text = text.replace(
        f"  m_Component:\n  - component: {{fileID: {BAR_BG_RT}}}\n  - component: {{fileID: {BAR_BG_IMAGE + 1}}}\n  - component: {{fileID: {BAR_BG_IMAGE}}}\n  m_Layer: 5\n  m_Name: BG",
        f"  m_Component:\n  - component: {{fileID: {BAR_BG_RT}}}\n  - component: {{fileID: {BAR_BG_IMAGE + 1}}}\n  - component: {{fileID: {BAR_BG_IMAGE}}}\n  m_Layer: 5\n  m_Name: Bar_BG",
        1,
    )

    # Bar 子节点：Bar_BG + Bar_ListScroll（移除旧 Shop_Bar 直接挂载）
    text = text.replace(
        f"  m_Children:\n  - {{fileID: {BAR_BG_RT}}}\n  - {{fileID: {EXISTING_SHOP_BAR_STRIPPED_RT}}}\n  m_Father: {{fileID: 356795715}}",
        f"  m_Children:\n  - {{fileID: {BAR_BG_RT}}}\n  - {{fileID: {IDS['scroll_rt']}}}\n  m_Father: {{fileID: 356795715}}",
        1,
    )

    # 删除旧 Shop_Bar PrefabInstance 与 stripped RectTransform
    text = re.sub(
        rf"--- !u!1001 &{EXISTING_SHOP_BAR_PI}[\s\S]*?--- !u!224 &{EXISTING_SHOP_BAR_STRIPPED_RT} stripped[\s\S]*?m_PrefabAsset: {{fileID: 0}}\n",
        "",
        text,
        count=1,
    )

    # 在 Bar_BG CanvasRenderer 之后插入 Scroll 层级
    insert_marker = f"--- !u!222 &{BAR_BG_IMAGE + 1}\nCanvasRenderer:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {{fileID: 0}}\n  m_PrefabInstance: {{fileID: 0}}\n  m_PrefabAsset: {{fileID: 0}}\n  m_GameObject: {{fileID: {BAR_BG_GO}}}\n  m_CullTransparentMesh: 1\n"
    if insert_marker not in text:
        raise RuntimeError("未找到 Bar_BG CanvasRenderer 插入点")
    text = text.replace(insert_marker, insert_marker + scroll_hierarchy_yaml(), 1)

    return text


def main() -> int:
    if not SCENE.exists():
        print(f"场景不存在: {SCENE}", file=sys.stderr)
        return 1

    original = SCENE.read_text(encoding="utf-8")
    patched = patch_scene(original)
    if patched == original:
        return 0

    SCENE.write_text(patched, encoding="utf-8", newline="\n")
    print(f"已补丁: {SCENE}")
    print(f"  Bar_BG + Bar_ListScroll, Viewport={VIEWPORT_HEIGHT}px, spacing={ROW_SPACING}px, rows={TEST_ROW_COUNT}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
