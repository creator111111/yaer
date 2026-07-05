/**
 * 0704 Shop_Bar 列表滚动 · Village_Shop 场景 YAML 补丁（SC-0～SC-4）
 */
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const SCENE = path.join(__dirname, '../Assets/GameRes/Scenes/Village_Shop.unity');

const ROW_HEIGHT = 88;
const ROW_SPACING = 16;
const BAR_BG_HEIGHT = 559.043;
const VIEWPORT_HEIGHT = BAR_BG_HEIGHT; // 与 Bar_BG 对齐，不再用 6 行公式撑高
const TEST_ROW_COUNT = 8;

const BAR_RT = 223743912;
const BAR_BG_GO = 955175014;
const BAR_BG_RT = 955175015;
const BAR_BG_IMAGE = 955175016;
const EXISTING_SHOP_BAR_PI = 3823689269437328575;
const EXISTING_SHOP_BAR_STRIPPED_RT = 1639222911;

const IDS = {
  scroll_go: 770070040001,
  scroll_rt: 770070040002,
  scroll_cr: 770070040003,
  scroll_img: 770070040004,
  scroll_sr: 770070040005,
  viewport_go: 770070040010,
  viewport_rt: 770070040011,
  viewport_cr: 770070040012,
  viewport_img: 770070040013,
  viewport_mask: 770070040014,
  viewport_rmask: 770070040015,
  content_go: 770070040020,
  content_rt: 770070040021,
  content_vlg: 770070040022,
  content_csf: 770070040023,
  sb_go: 770070040030,
  sb_rt: 770070040031,
  sb_cr: 770070040032,
  sb_img: 770070040033,
  sb_comp: 770070040034,
  sb_area_go: 770070040035,
  sb_area_rt: 770070040036,
  sb_handle_go: 770070040037,
  sb_handle_rt: 770070040038,
  sb_handle_cr: 770070040039,
  sb_handle_img: 770070040040,
};

const SHOP_BAR_GUID = 'fbb1826d55e82e24d90e90c345f43328';
const SHOP_BAR_ROOT = '3823689270002737856';
const SHOP_BAR_NAME = '3823689270002737857';

function shopBarBlock(piId, strippedRt, rootOrder, name) {
  return `--- !u!1001 &${piId}
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    m_TransformParent: {fileID: ${IDS.content_rt}}
    m_Modifications:
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_Pivot.x
      value: 0.5
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_Pivot.y
      value: 0.5
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_RootOrder
      value: ${rootOrder}
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_AnchorMax.x
      value: 0.5
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_AnchorMax.y
      value: 0.5
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_AnchorMin.x
      value: 0.5
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_AnchorMin.y
      value: 0.5
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_SizeDelta.x
      value: 482
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_SizeDelta.y
      value: ${ROW_HEIGHT}
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_AnchoredPosition.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_AnchoredPosition.y
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: ${SHOP_BAR_NAME}, guid: ${SHOP_BAR_GUID}, type: 3}
      propertyPath: m_Name
      value: ${name}
      objectReference: {fileID: 0}
    m_RemovedComponents: []
  m_SourcePrefab: {fileID: 100100000, guid: ${SHOP_BAR_GUID}, type: 3}
--- !u!224 &${strippedRt} stripped
RectTransform:
  m_CorrespondingSourceObject: {fileID: ${SHOP_BAR_ROOT}, guid: ${SHOP_BAR_GUID}, type: 3}
  m_PrefabInstance: {fileID: ${piId}}
  m_PrefabAsset: {fileID: 0}
`;
}

function scrollHierarchy() {
  const i = IDS;
  const contentChildren = Array.from({ length: TEST_ROW_COUNT }, (_, n) => `  - {fileID: ${770070041000 + n}}`).join('\n');
  const shopBars = Array.from({ length: TEST_ROW_COUNT }, (_, n) => {
    const name = n === 0 ? 'Shop_Bar' : `Shop_Bar (${n})`;
    return shopBarBlock(770070042000 + n, 770070041000 + n, n, name);
  }).join('');

  return `
--- !u!1 &${i.scroll_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: ${i.scroll_rt}}
  - component: {fileID: ${i.scroll_cr}}
  - component: {fileID: ${i.scroll_img}}
  - component: {fileID: ${i.scroll_sr}}
  m_Layer: 5
  m_Name: Bar_ListScroll
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &${i.scroll_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.scroll_go}}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children:
  - {fileID: ${i.viewport_rt}}
  - {fileID: ${i.sb_rt}}
  m_Father: {fileID: ${BAR_RT}}
  m_RootOrder: 1
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0.5, y: 0.5}
  m_AnchorMax: {x: 0.5, y: 0.5}
  m_AnchoredPosition: {x: -449, y: -28.521484}
  m_SizeDelta: {x: 482, y: ${BAR_BG_HEIGHT}}
  m_Pivot: {x: 0.5, y: 0.5}
--- !u!222 &${i.scroll_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.scroll_go}}
  m_CullTransparentMesh: 1
--- !u!114 &${i.scroll_img}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.scroll_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {fileID: 0}
  m_Color: {r: 1, g: 1, b: 1, a: 0}
  m_RaycastTarget: 1
  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {fileID: 0}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &${i.scroll_sr}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.scroll_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 1aa08ab6e0800fa44ae55d278d1423e3, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Content: {fileID: ${i.content_rt}}
  m_Horizontal: 0
  m_Vertical: 1
  m_MovementType: 1
  m_Elasticity: 0.1
  m_Inertia: 1
  m_DecelerationRate: 0.135
  m_ScrollSensitivity: 40
  m_Viewport: {fileID: ${i.viewport_rt}}
  m_HorizontalScrollbar: {fileID: 0}
  m_VerticalScrollbar: {fileID: 0}
  m_HorizontalScrollbarVisibility: 2
  m_VerticalScrollbarVisibility: 2
  m_HorizontalScrollbarSpacing: -3
  m_VerticalScrollbarSpacing: -3
  m_OnValueChanged:
    m_PersistentCalls:
      m_Calls: []
--- !u!1 &${i.viewport_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: ${i.viewport_rt}}
  - component: {fileID: ${i.viewport_cr}}
  - component: {fileID: ${i.viewport_img}}
  - component: {fileID: ${i.viewport_mask}}
  - component: {fileID: ${i.viewport_rmask}}
  m_Layer: 5
  m_Name: Viewport
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &${i.viewport_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.viewport_go}}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children:
  - {fileID: ${i.content_rt}}
  m_Father: {fileID: ${i.scroll_rt}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0, y: 1}
  m_AnchorMax: {x: 1, y: 1}
  m_AnchoredPosition: {x: 0, y: 0}
  m_SizeDelta: {x: 0, y: ${VIEWPORT_HEIGHT}}
  m_Pivot: {x: 0.5, y: 1}
--- !u!222 &${i.viewport_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.viewport_go}}
  m_CullTransparentMesh: 1
--- !u!114 &${i.viewport_img}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.viewport_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {fileID: 0}
  m_Color: {r: 1, g: 1, b: 1, a: 1}
  m_RaycastTarget: 1
  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {fileID: 10917, guid: 0000000000000000f000000000000000, type: 0}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &${i.viewport_mask}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.viewport_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 31a19414c41e5ae4aae2af33fee712f6, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_ShowMaskGraphic: 0
--- !u!114 &${i.viewport_rmask}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.viewport_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 3312d7739989d2b4e91e6319e9a96d76, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Padding: {x: 0, y: 0, z: 0, w: 0}
  m_Softness: {x: 0, y: 0}
--- !u!1 &${i.content_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: ${i.content_rt}}
  - component: {fileID: ${i.content_vlg}}
  - component: {fileID: ${i.content_csf}}
  m_Layer: 5
  m_Name: Content
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &${i.content_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.content_go}}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children:
${contentChildren}
  m_Father: {fileID: ${i.viewport_rt}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0, y: 1}
  m_AnchorMax: {x: 1, y: 1}
  m_AnchoredPosition: {x: 0, y: 0}
  m_SizeDelta: {x: 0, y: 0}
  m_Pivot: {x: 0.5, y: 1}
--- !u!114 &${i.content_vlg}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.content_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 59f8146938fff824cb5fd77236b75775, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Padding:
    m_Left: 0
    m_Right: 0
    m_Top: 0
    m_Bottom: 0
  m_ChildAlignment: 1
  m_Spacing: ${ROW_SPACING}
  m_ChildForceExpandWidth: 1
  m_ChildForceExpandHeight: 0
  m_ChildControlWidth: 1
  m_ChildControlHeight: 1
  m_ChildScaleWidth: 0
  m_ChildScaleHeight: 0
  m_ReverseArrangement: 0
--- !u!114 &${i.content_csf}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.content_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 3245ec927659c4140ac4f8d17403cc18, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_HorizontalFit: 0
  m_VerticalFit: 2
--- !u!1 &${i.sb_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: ${i.sb_rt}}
  - component: {fileID: ${i.sb_cr}}
  - component: {fileID: ${i.sb_img}}
  - component: {fileID: ${i.sb_comp}}
  m_Layer: 5
  m_Name: Scrollbar Vertical
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 0
--- !u!224 &${i.sb_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.sb_go}}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children:
  - {fileID: ${i.sb_area_rt}}
  m_Father: {fileID: ${i.scroll_rt}}
  m_RootOrder: 1
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 1, y: 0}
  m_AnchorMax: {x: 1, y: 1}
  m_AnchoredPosition: {x: 0, y: 0}
  m_SizeDelta: {x: 20, y: 0}
  m_Pivot: {x: 1, y: 1}
--- !u!222 &${i.sb_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.sb_go}}
  m_CullTransparentMesh: 1
--- !u!114 &${i.sb_img}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.sb_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {fileID: 0}
  m_Color: {r: 1, g: 1, b: 1, a: 1}
  m_RaycastTarget: 1
  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {fileID: 10907, guid: 0000000000000000f000000000000000, type: 0}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
--- !u!114 &${i.sb_comp}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.sb_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: 2a4db7a114972834c8e4117be1d82ba3, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Navigation:
    m_Mode: 3
    m_WrapAround: 0
    m_SelectOnUp: {fileID: 0}
    m_SelectOnDown: {fileID: 0}
    m_SelectOnLeft: {fileID: 0}
    m_SelectOnRight: {fileID: 0}
  m_Transition: 1
  m_Colors:
    m_NormalColor: {r: 1, g: 1, b: 1, a: 1}
    m_HighlightedColor: {r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}
    m_PressedColor: {r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 1}
    m_SelectedColor: {r: 0.9607843, g: 0.9607843, b: 0.9607843, a: 1}
    m_DisabledColor: {r: 0.78431374, g: 0.78431374, b: 0.78431374, a: 0.5019608}
    m_ColorMultiplier: 1
    m_FadeDuration: 0.1
  m_SpriteState:
    m_HighlightedSprite: {fileID: 0}
    m_PressedSprite: {fileID: 0}
    m_SelectedSprite: {fileID: 0}
    m_DisabledSprite: {fileID: 0}
  m_AnimationTriggers:
    m_NormalTrigger: Normal
    m_HighlightedTrigger: Highlighted
    m_PressedTrigger: Pressed
    m_SelectedTrigger: Selected
    m_DisabledTrigger: Disabled
  m_Interactable: 1
  m_TargetGraphic: {fileID: ${i.sb_handle_img}}
  m_HandleRect: {fileID: ${i.sb_handle_rt}}
  m_Direction: 2
  m_Value: 0
  m_Size: 1
  m_NumberOfSteps: 0
  m_OnValueChanged:
    m_PersistentCalls:
      m_Calls: []
--- !u!1 &${i.sb_area_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: ${i.sb_area_rt}}
  m_Layer: 5
  m_Name: Sliding Area
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &${i.sb_area_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.sb_area_go}}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children:
  - {fileID: ${i.sb_handle_rt}}
  m_Father: {fileID: ${i.sb_rt}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0, y: 0}
  m_AnchorMax: {x: 1, y: 1}
  m_AnchoredPosition: {x: 0, y: 0}
  m_SizeDelta: {x: -20, y: -20}
  m_Pivot: {x: 0.5, y: 0.5}
--- !u!1 &${i.sb_handle_go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: ${i.sb_handle_rt}}
  - component: {fileID: ${i.sb_handle_cr}}
  - component: {fileID: ${i.sb_handle_img}}
  m_Layer: 5
  m_Name: Handle
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!224 &${i.sb_handle_rt}
RectTransform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.sb_handle_go}}
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children: []
  m_Father: {fileID: ${i.sb_area_rt}}
  m_RootOrder: 0
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
  m_AnchorMin: {x: 0, y: 0}
  m_AnchorMax: {x: 1, y: 1}
  m_AnchoredPosition: {x: 0, y: 0}
  m_SizeDelta: {x: 0, y: 0}
  m_Pivot: {x: 0.5, y: 0.5}
--- !u!222 &${i.sb_handle_cr}
CanvasRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.sb_handle_go}}
  m_CullTransparentMesh: 1
--- !u!114 &${i.sb_handle_img}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: ${i.sb_handle_go}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  m_Material: {fileID: 0}
  m_Color: {r: 1, g: 1, b: 1, a: 1}
  m_RaycastTarget: 1
  m_RaycastPadding: {x: 0, y: 0, z: 0, w: 0}
  m_Maskable: 1
  m_OnCullStateChanged:
    m_PersistentCalls:
      m_Calls: []
  m_Sprite: {fileID: 10905, guid: 0000000000000000f000000000000000, type: 0}
  m_Type: 1
  m_PreserveAspect: 0
  m_FillCenter: 1
  m_FillMethod: 4
  m_FillAmount: 1
  m_FillClockwise: 1
  m_FillOrigin: 0
  m_UseSpriteMesh: 0
  m_PixelsPerUnitMultiplier: 1
${shopBars}`;
}

function patch(text) {
  if (text.includes('Bar_ListScroll')) {
    console.log('场景已含 Bar_ListScroll，跳过。');
    return text;
  }

  text = text.replace(
    `  m_GameObject: {fileID: ${BAR_BG_GO}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: \n  m_Material: {fileID: 0}\n  m_Color: {r: 1, g: 1, b: 1, a: 1}\n  m_RaycastTarget: 1`,
    `  m_GameObject: {fileID: ${BAR_BG_GO}}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: \n  m_Material: {fileID: 0}\n  m_Color: {r: 1, g: 1, b: 1, a: 1}\n  m_RaycastTarget: 0`
  );

  text = text.replace(
    `  m_Component:\n  - component: {fileID: ${BAR_BG_RT}}\n  - component: {fileID: ${BAR_BG_IMAGE + 1}}\n  - component: {fileID: ${BAR_BG_IMAGE}}\n  m_Layer: 5\n  m_Name: BG`,
    `  m_Component:\n  - component: {fileID: ${BAR_BG_RT}}\n  - component: {fileID: ${BAR_BG_IMAGE + 1}}\n  - component: {fileID: ${BAR_BG_IMAGE}}\n  m_Layer: 5\n  m_Name: Bar_BG`
  );

  text = text.replace(
    `  m_Children:\n  - {fileID: ${BAR_BG_RT}}\n  - {fileID: ${EXISTING_SHOP_BAR_STRIPPED_RT}}\n  m_Father: {fileID: 356795715}`,
    `  m_Children:\n  - {fileID: ${BAR_BG_RT}}\n  - {fileID: ${IDS.scroll_rt}}\n  m_Father: {fileID: 356795715}`
  );

  const oldShopBarRe = new RegExp(
    `(?:--- !u!224 &${EXISTING_SHOP_BAR_STRIPPED_RT} stripped[\\s\\S]*?m_PrefabAsset: \\{fileID: 0\\}\\n)?--- !u!1001 &${EXISTING_SHOP_BAR_PI}[\\s\\S]*?m_SourcePrefab: \\{fileID: 100100000, guid: ${SHOP_BAR_GUID}, type: 3\\}\\n`
  );
  text = text.replace(oldShopBarRe, '');
  text = text.replace(
    new RegExp(`--- !u!224 &${EXISTING_SHOP_BAR_STRIPPED_RT} stripped[\\s\\S]*?m_PrefabAsset: \\{fileID: 0\\}\\n`),
    ''
  );

  const insertMarker = `--- !u!222 &${BAR_BG_IMAGE + 1}\nCanvasRenderer:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: ${BAR_BG_GO}}\n  m_CullTransparentMesh: 1\n`;
  if (!text.includes(insertMarker)) throw new Error('未找到 Bar_BG CanvasRenderer 插入点');
  text = text.replace(insertMarker, insertMarker + scrollHierarchy());

  return text;
}

const original = fs.readFileSync(SCENE, 'utf8');
const patched = patch(original);
if (patched !== original) {
  fs.writeFileSync(SCENE, patched, 'utf8');
  console.log(`已补丁: ${SCENE}`);
  console.log(`  Viewport=${VIEWPORT_HEIGHT}px spacing=${ROW_SPACING}px rows=${TEST_ROW_COUNT}`);
}
