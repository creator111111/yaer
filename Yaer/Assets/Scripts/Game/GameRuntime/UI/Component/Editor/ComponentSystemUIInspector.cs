using GameFramework.Editor.GFExtend.ComponentSystemEditor;
using UnityEditor;

namespace Game.GameRuntime.UI.Component.Editor
{
    [CustomEditor(typeof(ComponentSystemUI), true)]
    public class ComponentSystemUIInspector: ComponentSystemMonoInspector
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            selectedType = typeof(BaseGFComponentUI);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}