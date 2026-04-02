using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Date;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.Component
{
    /// <summary>
    /// 菜单面板中日历 DayNum 的数字图片显示（十位 + 个位）。
    /// 可从存档刷新，也可直接通过 SetDay 传入天数。
    /// </summary>
    public class MenuCalendarDayNumDisplay : MonoBehaviour
    {
        [Header("0-9 数字图片，索引即数字本身")]
        [SerializeField] private Sprite[] digitSprites = Array.Empty<Sprite>();

        [Header("十位与个位图片")]
        [SerializeField] private Image tensImage;
        [SerializeField] private Image onesImage;

        [Header("个位数时处理策略")]
        [SerializeField] private bool hideTensWhenSingleDigit = true;

        [Header("天数范围（包含）")]
        [SerializeField] private int minDay = 1;
        [SerializeField] private int maxDay = 31;

        /// <summary>
        /// 从存档中读取当前日期并刷新图片。
        /// 供 Unity Tools「增加日期」后调用。
        /// </summary>
        public void RefreshFromArchive()
        {
            var archive = GameManager.GetGMComponent<ArchiveComponentGM>();
            if (archive == null)
            {
                return;
            }

            var dateData = archive.GetData<DateData>();
            if (dateData == null)
            {
                return;
            }

            // 直接使用 DateData.Day，避免解析字符串 Date。
            SetDay(dateData.Day);
        }

        /// <summary>
        /// 直接设置当前天数，刷新 DayNum 图片。
        /// </summary>
        /// <param name="day">1-31 的日期</param>
        public void SetDay(int day)
        {
            if (digitSprites == null || digitSprites.Length < 10)
            {
                return;
            }

            if (tensImage == null || onesImage == null)
            {
                return;
            }

            day = Mathf.Clamp(day, minDay, maxDay);

            int tens = day / 10;
            int ones = day % 10;

            // 个位
            if (ones >= 0 && ones < digitSprites.Length)
            {
                onesImage.sprite = digitSprites[ones];
                onesImage.enabled = digitSprites[ones] != null;
            }

            // 十位
            if (tens <= 0 && hideTensWhenSingleDigit)
            {
                tensImage.enabled = false;
            }
            else
            {
                if (tens >= 0 && tens < digitSprites.Length)
                {
                    tensImage.sprite = digitSprites[tens];
                    tensImage.enabled = digitSprites[tens] != null;
                }
                else
                {
                    tensImage.enabled = false;
                }
            }
        }
    }
}

