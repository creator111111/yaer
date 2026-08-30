using System;
using System.Collections.Generic;
using UnityEngine;

namespace EditorC.Tool.Dialogue
{
    /// <summary>
    /// 策划 CSV 简称 → NodeCanvas 图内 Actor 参数名的映射表。
    /// 未命中映射时导入器应报错中止，避免生成红色未定义 Actor 节点。
    /// </summary>
    [CreateAssetMenu(
        fileName = "DialogueSpeakerMapping",
        menuName = "Yaer/Dialogue/Speaker Mapping",
        order = 0)]
    public class DialogueSpeakerMapping : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("CSV Speaker 列中的策划简称，如「雅」")]
            public string csvSpeaker;

            [Tooltip("DialogueTree.actorParameters 中的参数名，如「雅尔」")]
            public string actorParameterName;
        }

        [SerializeField]
        private List<Entry> entries = new List<Entry>();

        /// <summary>
        /// 将 CSV 简称解析为图内 Actor 参数名。
        /// </summary>
        /// <returns>命中返回 true；未命中返回 false 且 actorParameterName 为空。</returns>
        public bool TryResolve(string csvSpeaker, out string actorParameterName)
        {
            actorParameterName = null;
            if (string.IsNullOrWhiteSpace(csvSpeaker))
            {
                return false;
            }

            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.csvSpeaker))
                {
                    continue;
                }

                if (string.Equals(entry.csvSpeaker.Trim(), csvSpeaker.Trim(), StringComparison.Ordinal))
                {
                    actorParameterName = entry.actorParameterName?.Trim();
                    return !string.IsNullOrEmpty(actorParameterName);
                }
            }

            return false;
        }

        /// <summary>
        /// 创建带项目默认映射的内存实例，供未指定 SO 时使用。
        /// 内置十三条：雅→雅尔、古→古莎、艾米→艾米、艾莉→艾莉、村→村长、埃吉尔→埃吉尔、—→旁白、
        /// 1→NPC1、2→NPC2、3→NPC3、4→NPC4、5→NPC5、店→老板娘、老人→老人。
        /// 替代方案：也可强制要求窗口必须拖入 SO，但默认映射可加速样例 CSV 验收。
        /// </summary>
        public static DialogueSpeakerMapping CreateDefaultInstance()
        {
            var mapping = CreateInstance<DialogueSpeakerMapping>();
            mapping.entries = new List<Entry>
            {
                // 村内开场等旧台本沿用
                new Entry { csvSpeaker = "雅", actorParameterName = "雅尔" },
                new Entry { csvSpeaker = "古", actorParameterName = "古莎" },
                // 晚宴等台本：CSV 简称与 Prefab Actor 名一致（恒等映射）
                new Entry { csvSpeaker = "艾米", actorParameterName = "艾米" },
                new Entry { csvSpeaker = "艾莉", actorParameterName = "艾莉" },
                // 台本 Speaker 列用单字「村」，图内 Actor 统一为「村长」（与 Village_Leader 占位及立绘命名一致）
                // 未命中映射时 TrySetupActorParameters 会中止导入，避免 NodeCanvas 图出现红色未定义 Actor
                new Entry { csvSpeaker = "村", actorParameterName = "村长" },
                // 埃吉尔台本：CSV 简称与 Prefab Actor 名一致（恒等映射）
                new Entry { csvSpeaker = "埃吉尔", actorParameterName = "埃吉尔" },
                // 旁白行：Speaker 列填 em dash「—」，图内 Actor 统一为「旁白」（仅字幕，不绑立绘）
                new Entry { csvSpeaker = "—", actorParameterName = "旁白" },
                // HomeScene23 屋内数字 Speaker：对齐 2→NPC2 / 3→NPC3；图内名与 HomeScene1Npc1/4 及 0601 台本一致
                // 不选恒等 1/4/5（图内名难看）也不改 CSV；NPC5 尚无独立 Prefab，命名与 1～4 同一约定预留
                new Entry { csvSpeaker = "1", actorParameterName = "NPC1" },
                new Entry { csvSpeaker = "2", actorParameterName = "NPC2" },
                new Entry { csvSpeaker = "3", actorParameterName = "NPC3" },
                new Entry { csvSpeaker = "4", actorParameterName = "NPC4" },
                new Entry { csvSpeaker = "5", actorParameterName = "NPC5" },
                // 商店老板娘：CSV Speaker「店」→ 图内 Actor「老板娘」
                new Entry { csvSpeaker = "店", actorParameterName = "老板娘" },
                // 0830 老农台本：CSV「老人」→ 图内 Actor「老人」（恒等；无立绘资产本期仅字幕）
                new Entry { csvSpeaker = "老人", actorParameterName = "老人" },
            };
            return mapping;
        }
    }
}
