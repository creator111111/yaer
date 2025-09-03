#if UNITY_EDITOR

using ParadoxNotion.Design;
using UnityEngine;
using NodeCanvas.DialogueTrees;
using UnityEditor;

namespace NodeCanvas.Editor
{

    ///<summary>A drawer for dialogue tree statements</summary>
    public class StatementDrawer : ObjectDrawer<Statement>
    {
        public override Statement OnGUI(GUIContent content, Statement instance) {
            if ( instance == null ) { instance = new Statement("..."); }
            UnityEditor.EditorGUILayout.LabelField("中文文本", EditorStyles.boldLabel);
            instance.text = UnityEditor.EditorGUILayout.TextArea(instance.text, Styles.wrapTextArea, GUILayout.Height(100));
            UnityEditor.EditorGUILayout.LabelField("英文文本", EditorStyles.boldLabel);
            instance.text_en = UnityEditor.EditorGUILayout.TextArea(instance.text_en, Styles.wrapTextArea, GUILayout.Height(100));
            UnityEditor.EditorGUILayout.LabelField("日文文本", EditorStyles.boldLabel);
            instance.text_jp = UnityEditor.EditorGUILayout.TextArea(instance.text_jp, Styles.wrapTextArea, GUILayout.Height(100));
            instance.audio = UnityEditor.EditorGUILayout.ObjectField("Audio File", instance.audio, typeof(AudioClip), false) as AudioClip;
            instance.meta = UnityEditor.EditorGUILayout.TextField("Metadata", instance.meta);
            return instance;
        }
    }
}

#endif