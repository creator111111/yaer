using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr;
using Game.GameRuntime.Story.NodeCanvasExtend;
using Game.GameRuntime.UI.FormLogic.Story.Base;
using Game.Static.Name.Clothes;
using UnityEngine;
using System.Collections.Generic;
using Game.Static.Enum.Dialogue;

namespace Game.GameRuntime.UI.FormLogic.Story.Painting
{
    // 古莎的对话立绘管理脚本
    public class GuShaPainting : StoryFormPainting
    {
        public GameObject clothes_normal;
        public GameObject clothes_other;

        List<DialogueFaceType> spcFaces = new List<DialogueFaceType>()
        {
            DialogueFaceType.Awkward, DialogueFaceType.Cry, DialogueFaceType.Daze, DialogueFaceType.Sad
        };

        public override void UpdateFace(string faceName)
        {
            base.UpdateFace(faceName);
            bool isSpcFace = false;
            foreach (var face in spcFaces)
            {
                if (face.ToString() == faceName)
                {
                    isSpcFace = true;
                    break;
                }
            }
            clothes_normal.SetActive(!isSpcFace);
            clothes_other.SetActive(isSpcFace);
        }
    }
}