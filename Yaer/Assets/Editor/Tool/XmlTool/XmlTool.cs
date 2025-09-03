using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.XmlTool
{
    /// <summary>
    /// auto generate archive data class from excel
    /// </summary>
    public static class XmlTool
    {
        public static string XML_PATH = Application.dataPath + "/Congif/ArchiveData/";
        
        private static XmlGenerateCsharp xmlGenerateCsharp = new XmlGenerateCsharp();

        [MenuItem("Editor/XmlTool/GenerateCsharp/GenerateArchiveClass")]
        public static void GenerateArchiveClass()
        {
            xmlGenerateCsharp.GenerateArchiveClass();
        }

    }
}