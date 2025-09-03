using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EditorC.Tool.UITool
{
    public class BindUI
    {
        private string[] controlType = new[]
        {
            "Image",
            "Button",
            "Text",
            "InputField",
            "Dropdown",
            "Slider",
            "Scrollbar",
            "Toggle",
            "ToggleGroup",
            "Scroll View",
            "CanvasGroup",
            "RawImage",
        };

        private bool isAdd;
        private string filePath;
        private GameObject selected;

        public void Generate(GameObject obj)
        {
            selected = obj;
            Debug.LogWarning("控件命名中含有'_ref'才能被识别, 且以名称开头类型为主要功能进行绑定");
            // 获取所有子物体的组件
            var components = obj.GetComponentsInChildren<RectTransform>(true);


            StringBuilder classBuilder = new StringBuilder();
            classBuilder.AppendLine("using System;");
            classBuilder.AppendLine("using UnityEngine;");
            classBuilder.AppendLine("using UnityEngine.UI;");
            classBuilder.AppendLine();
            classBuilder.AppendLine($"public class {obj.name}Bind : MonoBehaviour");
            classBuilder.AppendLine("{");

            StringBuilder onActionFieldBuilder = new StringBuilder();
            // 
            onActionFieldBuilder.AppendLine();

            StringBuilder findFunctionBuilder = new StringBuilder();
            // 添加 Find 方法并查找控件
            findFunctionBuilder.AppendLine();
            findFunctionBuilder.AppendLine("\tpublic void Bind()");
            findFunctionBuilder.AppendLine("\t{");

            StringBuilder bindActionBuilder = new StringBuilder();
            bindActionBuilder.AppendLine();

            foreach (var component in components)
            {
                string componentName = component.gameObject.name;

                // 筛选符合命名规则的 UI 控件
                if (controlType.Any(type => componentName.StartsWith(type)) && componentName.EndsWith("_ref"))
                {
                    string fieldName;
                    string fieldType;

                    // 根据组件类型确定变量类型（例如 Image 或 Button）
                    if (component.GetComponent<Image>() && componentName.StartsWith("Image"))
                    {
                        fieldType = "Image";
                        fieldName = RemoveEndWith(componentName.Replace("Image", "img"));
                    }
                    else if (component.GetComponent<Button>() && componentName.StartsWith("Button"))
                    {
                        fieldType = "Button";
                        fieldName = RemoveEndWith(componentName.Replace("Button", "btn"));

                        onActionFieldBuilder.AppendLine($"\tpublic Action {fieldName}OnClickAction;");
                        bindActionBuilder.AppendLine(
                            $"\t\t{fieldName}.onClick.AddListener(() => {fieldName}OnClickAction());");
                    }
                    else if (component.GetComponent<Text>() && componentName.StartsWith("Text"))
                    {
                        fieldType = "Text";
                        fieldName = RemoveEndWith(componentName.Replace("Text", "tx"));
                    }
                    else if (component.GetComponent<RawImage>() && componentName.StartsWith("RawImage"))
                    {
                        fieldType = "RawImage";
                        fieldName = RemoveEndWith(componentName.Replace("RawImage", "rImg"));
                    }
                    else if (component.GetComponent<ScrollRect>() && componentName.StartsWith("Scroll View"))
                    {
                        fieldType = "ScrollRect";
                        fieldName = RemoveEndWith(componentName.Replace("Scroll View", "sv"));

                        onActionFieldBuilder.AppendLine(
                            "\tpublic Action<float> " + fieldName + "OnValueChangedAction;");
                    }
                    else if (component.GetComponent<Slider>() && componentName.StartsWith("Slider"))
                    {
                        fieldType = "Slider";
                        fieldName = RemoveEndWith(componentName.Replace("Slider", "sld"));

                        onActionFieldBuilder.AppendLine(
                            "\tpublic Action<float> " + fieldName + "OnValueChangedAction;");
                    }
                    else if (component.GetComponent<Toggle>() && componentName.StartsWith("Toggle"))
                    {
                        fieldType = "Toggle";
                        fieldName = RemoveEndWith(componentName.Replace("Toggle", "tg"));

                        onActionFieldBuilder.AppendLine("\tpublic Action<bool> " + fieldName +
                                                        "OnValueChangedAction;");
                        bindActionBuilder.AppendLine(
                            $"\t\t{fieldName}.onValueChanged.AddListener((b) => {fieldName}OnValueChangedAction(b));");
                    }
                    else if (component.GetComponent<ToggleGroup>() && componentName.StartsWith("ToggleGroup"))
                    {
                        fieldType = "ToggleGroup";
                        fieldName = RemoveEndWith(componentName.Replace("ToggleGroup", "tgg"));

                        onActionFieldBuilder.AppendLine("\tpublic Action<bool> " + fieldName +
                                                        "OnActiveToggleChangedAction;");
                    }
                    else if (component.GetComponent<Dropdown>() && componentName.StartsWith("Dropdown"))
                    {
                        fieldType = "Dropdown";
                        fieldName = RemoveEndWith(componentName.Replace("Dropdown", "dd"));

                        onActionFieldBuilder.AppendLine("\tpublic Action<int> " + fieldName + "OnValueChangedAction;");
                    }
                    else if (component.GetComponent<InputField>() && componentName.StartsWith("InputField"))
                    {
                        fieldType = "InputField";

                        fieldName = RemoveEndWith(componentName.Replace("InputField", "ipf"));


                        onActionFieldBuilder.AppendLine("\tpublic Action<string> " + fieldName +
                                                        "OnValueChangedAction;");
                    }
                    else if (component.GetComponent<Scrollbar>() && componentName.StartsWith("Scrollbar"))
                    {
                        fieldType = "Scrollbar";
                        fieldName = RemoveEndWith(componentName.Replace("Scrollbar", "sb"));

                        onActionFieldBuilder.AppendLine(
                            "\tpublic Action<float> " + fieldName + "OnValueChangedAction;");
                    }
                    else
                    {
                        continue;
                    }

                    // 生成代码字段
                    classBuilder.AppendLine($"\tpublic {fieldType} {fieldName};");


                    // 添加 Find 方法
                    findFunctionBuilder.AppendLine(
                        $"\t\t{fieldName} = transform.Find(\"{GetFullPath(component.gameObject)}\").GetComponent<{fieldType}>();");
                }
            }

            // 添加事件
            classBuilder.AppendLine(onActionFieldBuilder.ToString());

            // 添加绑定文本
            findFunctionBuilder.AppendLine(bindActionBuilder.ToString());

            // find 方法结束
            findFunctionBuilder.AppendLine("\t}");

            // 添加 Find 方法
            classBuilder.Append(findFunctionBuilder);
            // 结束类定义
            classBuilder.AppendLine("}");


            // 打开文件浏览器
            filePath = EditorUtility.SaveFilePanel("保存绑定脚本", "Assets/Scripts/", obj.name + "Bind", "cs");

            File.WriteAllText(filePath, classBuilder.ToString());
            
            // 将绝对路径转换为相对路径
            string relativePath = "Assets" + filePath.Replace(Application.dataPath, "");
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
            // 将脚本类型添加为组件
            selected.AddComponent(script.GetClass());
            // 刷新编辑器以显示名称更改
            EditorUtility.SetDirty(selected);
            
            AssetDatabase.Refresh();
            
            Debug.Log($"UI 绑定脚本生成成功");
        }

        private static string RemoveEndWith(string fieldName)
        {
            // 去除_ref
            if (fieldName.EndsWith("_ref"))
            {
                fieldName = fieldName.Substring(0, fieldName.Length - 4);
            }

            return fieldName;
        }

        // 获取从根对象到目标控件的绝对路径
        private string GetFullPath(GameObject obj)
        {
            string path = obj.name; // 从当前对象开始
            Transform current = obj.transform;

            // 一直向上遍历，直到没有父对象
            while (current.parent.name != selected.name)
            {
                current = current.parent;
                path = current.name + "/" + path; // 构建路径
            }

            return path;
        }
    }
}