using System;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace EditorC.Tool.XmlTool
{
    public class XmlGenerateCsharp
    {
        private string XML_PATH = Application.dataPath + "/Config/ArchiveData/";
        private string CLASS_PATH = Application.dataPath + "/Scripts/App/DataClass/";


        public void GenerateArchiveClass()
        {
            /*
                 <class name="HomeScene1Data" version="1" namespace="" extend="BaseSceneData">
                    <field type="int" name="version" value="1" />
                    <field type="bool" name="xiaerDialogue" value=""/>
                    <field type="bool" name="getMap" value="" />
                    <field type="float" name="testFloat" value="0.123f" />
                    <field type="string" name="testString" value="test" />
                    <field type="enum" enumType="EPlace" name="testEnum" value="EPlace.Home"/>
                    <field type="List<string>" name="testlist"/>
                    <field type="Dictionary<int, string>" name="testMap"/>
                    <field type="PlayerData" name="playerData"/>
                </class>
             */
            if (!Directory.Exists(XML_PATH))
            {
                Debug.Log("xml directory dont exist");
                return;
            }

            // check class folder
            if (!Directory.Exists(CLASS_PATH))
            {
                Directory.CreateDirectory(CLASS_PATH);
            }

            string[] files = Directory.GetFiles(XML_PATH, "*.xml");


            foreach (string file in files)
            {
                try
                {
                    // parse protocol.xml
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);

                    XmlNode root = doc.DocumentElement;
                    // clear folder
                    if (Directory.Exists(CLASS_PATH + root.Name))
                    {
                        Directory.Delete(CLASS_PATH + root.Name, true);
                    }

                    Directory.CreateDirectory(CLASS_PATH + root.Name);
                    // generate data class

                    for (int i = 0; i < root.SelectNodes("class").Count; i++)
                    {
                        XmlNode classNode = root.SelectNodes("class")[i];

                        string usingText = "using System;\n" +
                                           "using System.Collections;\n" +
                                           "using System.Collections.Generic;\n" +
                                           "\n" +
                                           "[Serializable]";

                        string classNameText;

                        // is end class
                        if (i == root.SelectNodes("class").Count - 1)
                        {
                            // not version
                            classNameText = classNode.Attributes["name"].Value;
                        }
                        else
                        {
                            classNameText = $"{classNode.Attributes["name"].Value}_{i}";
                        }

                        string extendText = $"{classNode.Attributes["extend"].Value}";
                        string classFieldsText = GetFieldsText(classNode.SelectNodes("field"));

                        string classFunText = "";
                        if (root.Name == "ArchiveInfo")
                        {
                            extendText += ", IComparable<ArchiveInfo>";
                            classFunText += "\n\tpublic int CompareTo(ArchiveInfo other)\n" +
                                            "\t{\n" +
                                            "\t\treturn other.id > id ? -1 : 1;\n" +
                                            "\t}\n";
                        }

                        string text = $"{usingText}\n" +
                                      $"public class {classNameText} : {extendText}" +
                                      "\n{\n" +
                                      $"{classFieldsText}" +
                                      $"{classFunText}" +
                                      "}";

                        File.WriteAllText(CLASS_PATH + root.Name + "/" + classNameText + ".cs", text);
                    }

                    // foreach (XmlNode classNode in root.SelectNodes("class"))
                    // {
                    //     string usingText = "using System;\n" +
                    //                        "using System.Collections;\n" +
                    //                        "using System.Collections.Generic;\n" +
                    //                        "\n" +
                    //                        "[Serializable]";
                    //
                    //     string classNameText;
                    //     if (classNode.Attributes["version"].Value == "")
                    //     {
                    //         classNameText = classNode.Attributes["name"].Value;
                    //     }
                    //     else
                    //     {
                    //         classNameText = $"{classNode.Attributes["name"].Value}_{classNode.Attributes["version"].Value}";
                    //     }
                    //
                    //     string extendText = $"{classNode.Attributes["extend"].Value}";
                    //     string classFieldsText = GetFieldsText(classNode.SelectNodes("field"));
                    //
                    //     string classFunText = "";
                    //     if (root.Name == "ArchiveInfo")
                    //     {
                    //         extendText += ", IComparable<ArchiveInfo>";
                    //         classFunText += "\n\tpublic int CompareTo(ArchiveInfo other)\n" +
                    //                         "\t{\n" +
                    //                         "\t\treturn other.id > id ? -1 : 1;\n" +
                    //                         "\t}\n";
                    //     }
                    //
                    //     string text = $"{usingText}\n" +
                    //                   $"public class {classNameText} : {extendText}" +
                    //                   "\n{\n" +
                    //                   $"{classFieldsText}" +
                    //                   $"{classFunText}" +
                    //                   "}";
                    //
                    //     File.WriteAllText(CLASS_PATH + root.Name + "/" + classNameText + ".cs", text);
                    // }
                }
                catch (Exception e)
                {
                    Debug.LogError("xml file error: " + file);
                    Debug.LogError(e);
                    throw;
                }
            }


            Debug.Log("Generate Csharp Success!");
            AssetDatabase.Refresh();

            // ==========================================================
        }

        private string GetFieldsText(XmlNodeList fieldNodes)
        {
            string text = "";
            foreach (XmlNode field in fieldNodes)
            {
                switch (field.Attributes["type"].Value)
                {
                    case "int":
                        if (field.Attributes["value"].Value == "")
                            text += $"\tpublic int {field.Attributes["name"].Value};\n";
                        else
                            text += $"\tpublic int {field.Attributes["name"].Value} = {field.Attributes["value"].Value};\n";
                        break;
                    case "float":

                        if (field.Attributes["value"].Value == "")
                            text += $"\tpublic float {field.Attributes["name"].Value};\n";
                        else
                            text += $"\tpublic float {field.Attributes["name"].Value} = {field.Attributes["value"].Value};\n";
                        break;
                    case "bool":

                        if (field.Attributes["value"].Value == "")
                            text += $"\tpublic bool {field.Attributes["name"].Value};\n";
                        else
                            text += $"\tpublic bool {field.Attributes["name"].Value} = {field.Attributes["value"].Value};\n";

                        break;
                    case "string":

                        if (field.Attributes["value"].Value == "")
                        {
                            text += $"\tpublic string {field.Attributes["name"].Value};\n";
                        }
                        else
                        {
                            text += $"\tpublic string {field.Attributes["name"].Value} = \"{field.Attributes["value"].Value}\";\n";
                        }

                        break;
                    case "list":
                        text +=
                            $"\tpublic List<{field.Attributes["valueType"].Value}> {field.Attributes["name"].Value} " +
                            $"= new List<{field.Attributes["valueType"].Value}>();\n";
                        break;
                    case "dic":

                        text +=
                            $"\tpublic Dictionary<{field.Attributes["keyType"].Value}, {field.Attributes["valueType"].Value}> {field.Attributes["name"].Value}" +
                            $" = new Dictionary<{field.Attributes["keyType"].Value}, {field.Attributes["valueType"].Value}>();\n";
                        break;
                    case "enum":
                        if (field.Attributes["value"].Value == "")
                        {
                            text += $"\tpublic {field.Attributes["enumType"].Value} {field.Attributes["name"].Value};\n";
                        }
                        else
                        {
                            text +=
                                $"\tpublic {field.Attributes["enumType"].Value} {field.Attributes["name"].Value} = {field.Attributes["value"].Value};\n";
                        }

                        break;
                    default:
                        text +=
                            $"\tpublic {field.Attributes["type"].Value} {field.Attributes["name"].Value} = new {field.Attributes["type"].Value}();\n";
                        break;
                }
            }

            return text;
        }
    }
}