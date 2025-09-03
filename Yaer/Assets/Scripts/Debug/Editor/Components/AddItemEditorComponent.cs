using GameDebug;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.GameTool
{
    public class AddItemEditorComponent : BaseGTEditorComponent
    {
        private string itemName;
        private int count;
        protected override void OnInit()
        {
            base.OnInit();
            
            name = "道具";
        }

        public override void OnGUI()
        {
            base.OnGUI();
            
            itemName = EditorGUILayout.TextField("道具名称", itemName);
            count = EditorGUILayout.IntField("数量", count);

            if (GUILayout.Button("添加道具"))
            {
                tool.GetComponent<AddItemComponentGT>().AddItem(itemName, count);
            }
        }
    }
}