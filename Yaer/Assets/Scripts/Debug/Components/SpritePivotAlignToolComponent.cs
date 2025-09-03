
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
public class SpritePivotAlignToolComponent : MonoBehaviour
{
    [SerializeField]
    private Sprite[] StandardSprites;
    [SerializeField]
    private Sprite[] NeedAlignSprites;

    [SerializeField]
    private SpriteRenderer StandardSpriteRenderer;
    [SerializeField]
    private SpriteRenderer NeedAlignSpriteRenderer;

    private int CurrentIndex;

    public void SetStandardSprites()
    {
        Object[] selection = Selection.objects;
        StandardSprites = new Sprite[selection.Length];
        for (int i = 0; i < selection.Length; i++) 
        {
            var obj = selection[i];
            if (obj is Texture2D t2d)
            {
                string assetPath = AssetDatabase.GetAssetPath(t2d);
                StandardSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
        }
    }

    public void SetNeedAlignSprites() 
    {
        Object[] selection = Selection.objects;
        NeedAlignSprites = new Sprite[selection.Length];
        for (int i = 0; i < selection.Length; i++)
        {
            var obj = selection[i];
            if (obj is Texture2D t2d)
            {
                string assetPath = AssetDatabase.GetAssetPath(t2d);
                NeedAlignSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
        }
    }

    public void ResetIndex()
    {
        CurrentIndex = 0;
        Refresh();
    }

    public void NextSprite()
    {
        CurrentIndex++;
        Refresh();
    }

    public void Refresh()
    {
        if (!CheckValid()) return;
        int length = StandardSprites.Length;
        CurrentIndex = Mathf.Clamp(CurrentIndex, 0, length - 1);
        StandardSpriteRenderer.sprite = StandardSprites[CurrentIndex];
        NeedAlignSpriteRenderer.sprite = NeedAlignSprites[CurrentIndex];
    }

    public void SetSamePivot()
    {
        string assetPath1 = AssetDatabase.GetAssetPath(StandardSprites[CurrentIndex]);
        string assetPath2 = AssetDatabase.GetAssetPath(NeedAlignSprites[CurrentIndex]);
        TextureImporter textureImporter1 = AssetImporter.GetAtPath(assetPath1) as TextureImporter;
        TextureImporter textureImporter2 = AssetImporter.GetAtPath(assetPath2) as TextureImporter;

        textureImporter2.spritePivot = textureImporter1.spritePivot;

        textureImporter2.SaveAndReimport();
        AssetDatabase.Refresh();
    }

    public void AllSetSamePivot()
    {
        if (!CheckValid()) return;
        for (int i=0; i< StandardSprites.Length; i++)
        {
            string assetPath1 = AssetDatabase.GetAssetPath(StandardSprites[i]);
            string assetPath2 = AssetDatabase.GetAssetPath(NeedAlignSprites[i]);
            TextureImporter textureImporter1 = AssetImporter.GetAtPath(assetPath1) as TextureImporter;
            TextureImporter textureImporter2 = AssetImporter.GetAtPath(assetPath2) as TextureImporter;

            textureImporter2.spritePivot = textureImporter1.spritePivot;

            textureImporter2.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    private bool CheckValid()
    {
        if (StandardSprites.Length != NeedAlignSprites.Length)
        {
            Debug.LogError("需校准的Sprite数组不等长");
            return false;
        }
        if (StandardSprites.Length == 0)
        {
            Debug.LogError("未设置Sprite数组");
            return false;
        }
        return true;
    }

    public void ToggleNeedAlignSpriteRendererActive()
    {
        bool f = NeedAlignSpriteRenderer.gameObject.activeSelf;
        NeedAlignSpriteRenderer.gameObject.SetActive(!f);
    }
}
#endif