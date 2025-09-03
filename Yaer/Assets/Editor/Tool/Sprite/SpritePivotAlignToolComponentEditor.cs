using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SpritePivotAlignToolComponent))]
public class SpritePivotAlignToolComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SpritePivotAlignToolComponent cpt = (SpritePivotAlignToolComponent)target;
        if (GUILayout.Button("设置标准Sprites数组"))
        {
            cpt.SetStandardSprites();
        }
        if (GUILayout.Button("设置需校准的Sprites数组"))
        {
            cpt.SetNeedAlignSprites();
        }
        if (GUILayout.Button("重置索引"))
        {
            cpt.ResetIndex();
        }
        if (GUILayout.Button("下一张"))
        {
            cpt.NextSprite();
        }
        if (GUILayout.Button("设置相同的锚点"))
        {
            cpt.SetSamePivot();
        }
        if (GUILayout.Button("所有Sprites设置相同的锚点"))
        {
            cpt.AllSetSamePivot();
        }
        if (GUILayout.Button("切换启用需校准的Sprite渲染"))
        {
            cpt.ToggleNeedAlignSpriteRendererActive();
        }
    }
}
