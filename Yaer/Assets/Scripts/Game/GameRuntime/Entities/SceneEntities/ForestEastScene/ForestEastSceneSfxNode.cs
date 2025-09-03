using Game.GameMgr.Component;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ¶«³Ç½¼Î÷²¿³¡¾°±³¾°ÒôÐ§
public class ForestEastSceneSfxNode : MonoBehaviour
{
    public SoundToggleComponent soundSfxCpn_1; // Äñ½ÐÉùÓÃÒôÐ§×é¼þ
    public SoundToggleComponent soundSfxCpn_2; // ³æ½Ð

    float timeCount_1 = 0;
    float timeCount_2 = 0;
    [Header("Äñ½ÐÉù¼ä¸ô")]
    public float timeDistance_1 = 15f;
    [Header("³æ½ÐÉù¼ä¸ô")]
    public float timeDistance_2 = 11f;
    // Start is called before the first frame update
    void Start()
    {
        timeCount_1 = timeDistance_1 - 1;
        timeCount_2 = timeDistance_2 - 1;
    }

    // Update is called once per frame
    void Update()
    {
        timeCount_1 += Time.deltaTime;
        if (timeCount_1 > timeDistance_1)
        {
            timeCount_1 = 0;
            PlayBirdAudio();
        }
        timeCount_2 += Time.deltaTime;
        if ( timeCount_2 > timeDistance_2)
        {
            timeCount_2 = 0;
            PlayBugAudio();
        }
    }

    // ²¥·ÅÄñ½Ð
    void PlayBirdAudio()
    {
        var baseName = "Äñ½Ð{0}.mp3";
        var randomIndex = GameTools.getRandomIntNum(1, 3);
        var realName = string.Format(baseName, randomIndex);
        soundSfxCpn_1.ChangeSoundRes(realName);
        soundSfxCpn_1.PlaySound();
    }

    // ²¥·Å³æ½Ð
    void PlayBugAudio()
    {
        var baseName = "À¥³æ{0}.mp3";
        var randomIndex = GameTools.getRandomIntNum(1, 5);
        var realName = string.Format(baseName, randomIndex);
        soundSfxCpn_1.ChangeSoundRes(realName);
        soundSfxCpn_1.PlaySound();
    }
}
