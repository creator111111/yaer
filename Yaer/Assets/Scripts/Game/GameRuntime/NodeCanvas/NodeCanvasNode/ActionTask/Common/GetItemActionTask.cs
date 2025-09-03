using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive;
using Game.GameMgr;
using NodeCanvas.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;

[Category("Common")]
public class GetItemActionTask : ActionTask
{
    public string ItemName;
    public int Num;

    protected override void OnExecute()
    {
        GameManager.GetGMComponent<ArchiveComponentGM>().GetData<PlayerBagData>().AddMainItem(ItemName, Num);
        EndAction();
    }
}
