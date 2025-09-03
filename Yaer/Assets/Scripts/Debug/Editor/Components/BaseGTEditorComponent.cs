using GameFramework.CoreExtend.Component.GameFramework.CoreExtend.Systems.Component;

namespace EditorC.Tool.GameTool
{
    public class BaseGTEditorComponent: BaseGFEComponent
    {
        protected GameDebug.GameTool tool;
        
        public string name;
        protected override void OnInit()
        {
            
        }

        public override void OnUpdate()
        {
        }

        public virtual void OnGUI()
        {
            
        }
        
        public void SetGameTool(GameDebug.GameTool tool) => this.tool = tool;
    }
}