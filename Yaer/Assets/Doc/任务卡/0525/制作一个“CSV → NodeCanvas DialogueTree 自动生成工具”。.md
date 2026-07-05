你现在是Unity工具开发工程师。

目标：
制作一个“CSV → NodeCanvas DialogueTree 自动生成工具”。

当前情况：
- Unity项目已经存在
- 已经导入NodeCanvas
- CSV文件已经放入Unity
- 需要制作Editor工具自动生成DialogueTree
- 使用C#
- 目标是减少手动创建NodeCanvas节点的工作量

请先分析当前项目中的：
1. NodeCanvas版本
2. DialogueTree类型
3. StatementNode类型
4. Choice节点类型
5. NodeCanvas的节点创建API
6. 节点连线API
7. DialogueTree资源创建方式

先不要直接开始乱写代码。

第一步：
请先扫描项目结构，找到：
- NodeCanvas Dialogue相关类
- DialogueTree定义
- StatementNode定义
- Graph API
- AddNode API
- Connect API

然后输出：
# NodeCanvas架构分析
包括：
- 实际使用的类名
- 命名空间
- 节点创建方式
- 连线方式
- 文本字段名称
- Speaker字段名称
- Choice字段名称

确认完毕后，再开始正式实现。

------------------------------------------------

CSV格式如下：

ID,Type,Speaker,Text,Next,Extra

示例：

1,Dialogue,Dog,你好,2,
2,Dialogue,NPC,欢迎来到村庄,3,
3,Choice,,你要去哪？,4|5,商店|离开
4,Dialogue,NPC,这里是商店,END,
5,Dialogue,Dog,再见,END,

字段说明：
- ID：节点ID
- Type：节点类型（Dialogue / Choice）
- Speaker：说话人
- Text：对白内容
- Next：下一节点ID，多分支使用 |
- Extra：Choice显示文本，多选项使用 |

------------------------------------------------

工具目标：

制作一个：
Tools/Dialogue/Import CSV

Editor工具。

功能要求：

# 第一阶段（必须完成）

实现：

1. 读取CSV
2. 解析CSV数据
3. 创建DialogueTree
4. 自动生成StatementNode
5. 自动生成Choice节点
6. 自动连线
7. 保存.asset资源

------------------------------------------------

# 技术要求

必须：

- 使用EditorWindow或MenuItem
- 放到Editor目录
- 代码添加中文注释
- 不允许硬编码路径
- 使用Unity标准AssetDatabase
- 使用Undo.RecordObject
- 使用SerializedObject（如果NodeCanvas需要）

------------------------------------------------

# 架构要求

必须先建立：

DialogueRow 数据结构

例如：

class DialogueRow
{
    public int id;
    public string type;
    public string speaker;
    public string text;
    public string next;
    public string extra;
}

------------------------------------------------

# 导入流程要求

实现流程：

Step1：
读取CSV

Step2：
解析所有行为 DialogueRow

Step3：
创建 DialogueTree

Step4：
第一轮遍历：
创建全部节点
建立：
Dictionary<int, Node>

Step5：
第二轮遍历：
根据Next字段自动连线

Step6：
保存.asset

------------------------------------------------

# 重要要求

不要假设NodeCanvas API。

必须：
先分析项目实际NodeCanvas类结构，
再编写兼容当前项目的代码。

如果发现：
- StatementNode不存在
- DialogueTree API不同
- 连线方式不同

必须根据项目实际情况适配。

------------------------------------------------

# 输出要求

先输出：

1. NodeCanvas架构分析
2. 实现方案
3. 会新增哪些文件
4. 每个文件职责

确认后再开始生成代码。

不要直接一次性输出巨大代码。
按步骤进行。