using EditorC.Tool.GameTool;
using UnityEditor;
using UnityEngine;

namespace GameDebug.Editor.Components
{
    public class ChangeClothesEditorComponentGT : BaseGTEditorComponent
    {
        private int times;
        protected override void OnInit()
        {
            base.OnInit();

            name = "换装";
        }

        public override void OnGUI()
        {
            base.OnGUI();

            EditorGUILayout.BeginHorizontal();
            {
                times = EditorGUILayout.IntField("进入换装场景次数", times);
                
                if (GUILayout.Button("应用"))
                {
                    tool.GetComponent<ChangeClothesComponentGT>().SetTimesChangeClothesScene(times);
                }
            }
            EditorGUILayout.EndHorizontal();
           
        }
    }
}