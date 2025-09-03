using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Static.Name.Clothes;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.GameRuntime.UI.FormLogic.Fighting
{
    public class FightingBroken : MonoBehaviour
    {
        [SerializeField] private Image BImage;
        [SerializeField] private Animator animator;

        [SerializeField] private List<BrokenPose> BrokenImage = new List<BrokenPose>();

        private int Part;
        private int Pose;

        private void RsndomPose()
        {
            Pose = Random.Range(0, 2);
            BImage.sprite = BrokenImage[Pose].GetPartFirst(Part);
            BImage.SetNativeSize();
            Debug.Log($"随机立绘1 {BImage.sprite.name}");
        }

        public void ChangeImage()
        {
            BImage.sprite = BrokenImage[Pose].GetPartSecond(Part);
            BImage.SetNativeSize();
            Debug.Log($"随机立绘2 {BImage.sprite.name}");
        }

        public void PlayBrokenAnimation()
        {
            RsndomPose();
            animator.SetTrigger("Show");
        }


        /// <summary>
        ///     part 0:无，1:王冠，2:护头
        /// </summary>
        /// <param name="part"></param>
        public void Initialize(string headWear)
        {
            switch (headWear)
            {
                case ClothesName.HeadWear.NoHeadWear:
                    Part = 0;
                    break;
                case ClothesName.HeadWear.Crown:
                    Part = 1;
                    break;
                case ClothesName.HeadWear.ArmorHead:
                    Part = 2;
                    break;
            }
        }
    }

    [Serializable]
    internal struct BrokenPose
    {
        public Sprite[] Sprite;

        public Sprite GetPartFirst(int part)
        {
            return Sprite[part * 2];
        }

        public Sprite GetPartSecond(int part)
        {
            return Sprite[part * 2 + 1];
        }
    }
}