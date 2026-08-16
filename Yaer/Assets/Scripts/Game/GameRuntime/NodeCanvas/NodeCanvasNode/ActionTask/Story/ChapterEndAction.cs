using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.Static.Path;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace Game.GameRuntime.Story.Node
{
    [Category("Story")]
    [Name("�½ڽ���")]
    // ���ھ���Ի�ϵͳ�е��¼�����
    public class ChapterEndAction : ActionTask
    {
        public BBParameter<int> chapterId; // �½�ID

        protected override string OnInit()
        {
            
            return base.OnInit();
        }

        protected override string info { 
            get
            {
                return chapterId.value == 0 ? "���½���" : "�½�" + chapterId.value + "����";
            }
        }

        protected override void OnExecute()
        {
            // ���½ڽ������棨����� �� ��ͼ �� ����Ļ�� ChapterEndFormLogic ���֣�
            // 0722�����ߵ���Ե�жԻ��Զ���λ��ʼ���޴���⣬���� Console��ChapterEnd�����Ƿ�ȱ����־
            UnityEngine.Debug.Log(
                $"[ChapterEnd] ChapterEndAction.OnExecute �� ChapterEndPanel��chapterId={chapterId.value}");

            string uiPrefabPath = UIPrefabPath.GetUIPrefabPath("ChapterEndPanel");
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(uiPrefabPath, EUIGroup.Top, new OpenFormArgs()
            {

            });

            EndAction();
        }
    }
}