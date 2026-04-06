using EditorC.Tool.GameTool;
using UnityEditor;
using UnityEngine;

namespace GameDebug.Editor.Components
{
    public class AddDateEditorComponentGT : BaseGTEditorComponent
    {
        protected override void OnInit()
        {
            base.OnInit();
            name = "增加日期";
        }

        public override void OnGUI()
        {
            base.OnGUI();
            if (GUILayout.Button("增加日期"))
            {
                tool.GetComponent<AddDateComponentGT>().AddOneDay();
            }
        }
    }
}
