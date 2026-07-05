using Game.GameMgr.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ???????ForestEastScene???????????????????? <see cref="SoundToggleComponent"/> + <see cref="SoundComponentGM"/>?
/// ???? <c>SceneSfxNode</c> ????? <c>BGM</c> ????????????????
/// </summary>
public class ForestEastSceneSfxNode : MonoBehaviour
{
    /// <summary>?????????SFX?????????? PlaySound??</summary>
    public SoundToggleComponent soundSfxCpn_1;

    /// <summary>????????????????????? 0.1s ?????? GM ????</summary>
    public SoundToggleComponent soundSfxCpn_2;

    float timeCount_1 = 0;
    float timeCount_2 = 0;

    [Header("????????")]
    public float timeDistance_1 = 15f;

    [Header("????????")]
    public float timeDistance_2 = 11f;

    [Header("?????")]
    [SerializeField]
    [Tooltip("???????????1~5.mp3 ?????")]
    private bool enableBugAmbientSfx = false;

    void Start()
    {
        // ???????????????????
        timeCount_1 = timeDistance_1 - 1;
        timeCount_2 = timeDistance_2 - 1;
    }

    void Update()
    {
        timeCount_1 += Time.deltaTime;
        if (timeCount_1 > timeDistance_1)
        {
            timeCount_1 = 0;
            PlayBirdAudio();
        }
        if (enableBugAmbientSfx)
        {
            timeCount_2 += Time.deltaTime;
            if (timeCount_2 > timeDistance_2)
            {
                timeCount_2 = 0;
                PlayBugAudio();
            }
        }
    }

    /// <summary>???? Assets/GameRes/Audio/SFX/ ? ??1~3.mp3?</summary>
    void PlayBirdAudio()
    {
        var baseName = "??{0}.mp3";
        var randomIndex = GameTools.getRandomIntNum(1, 3);
        var realName = string.Format(baseName, randomIndex);
        soundSfxCpn_1.ChangeSoundRes(realName);
        soundSfxCpn_1.PlaySound();
    }

    /// <summary>???? ??1~5.mp3????? SoundToggle??????????????????????</summary>
    void PlayBugAudio()
    {
        var baseName = "??{0}.mp3";
        var randomIndex = GameTools.getRandomIntNum(1, 5);
        var realName = string.Format(baseName, randomIndex);
        soundSfxCpn_2.ChangeSoundRes(realName);
        soundSfxCpn_2.PlaySound();
    }
}
