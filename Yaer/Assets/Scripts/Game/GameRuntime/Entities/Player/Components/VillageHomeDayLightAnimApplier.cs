using System.Collections.Generic;
using Game.Static.Name.Res;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.Entities.Player.Components
{
    /// <summary>
    /// 方案 B′：仅在「村民家室内」把 Home 控制器的 Idle/Walk/Bink 槽临时换成白天片子，Animator 状态名仍是 Idle/Walk/Bink。
    /// </summary>
    /// <remarks>
    /// 为何不用 <c>TerrainType.IndoorType</c> / <c>!isFightingScene</c>：龙宫也是室内 + Home，会误伤。
    /// 为何必须 <c>new AnimatorOverrideController</c>：改磁盘 Override 等于方案 E，龙宫会一起变白天。
    /// 替代方案 C：复制 Dress+三套白天控制器，由 PathHelper 按白名单换路径；B 验收失败再走 C，仍禁止 E。
    /// </remarks>
    public static class VillageHomeDayLightAnimApplier
    {
        /// <summary>底图状态 / Override 原 Clip 名，必须精确相等，禁止用 Contains（Idle 会误伤 Idle_DayLight）。</summary>
        private const string ClipIdle = "Idle";
        private const string ClipWalk = "Walk";
        private const string ClipBink = "Bink";
        private const string ClipIdleDayLight = "Idle_DayLight";
        private const string ClipWalkDayLight = "Walk_DayLight";
        /// <summary>底图孤岛 Clip 名；Applier 从 Override 表取生效片后 remap 到 <see cref="ClipBink"/> 原槽。</summary>
        private const string ClipBinkDayLight = "Bink_DayLight";

        private const string LogTag = "[VillageHomeDayLight]";

        /// <summary>
        /// Unity 场景文件名白名单。House4 现网场景文件可能缺失，仍暂留占位；45 已接通进屋。
        /// 判断只用 <see cref="SceneManager.GetActiveScene"/>.name，不用 GSM 的 nowSceneName。
        /// </summary>
        private static readonly HashSet<string> VillageHomeSceneNames = new HashSet<string>
        {
            SceneName.Village_HomeScene1,
            SceneName.Village_HomeScene2,
            SceneName.Village_HomeScene23,
            SceneName.Village_House4,
            SceneName.Village_HomeScene45,
        };

        /// <summary>
        /// 若当前是村民家，返回「换了 Idle/Walk/Bink 白天片」的运行时 Override；否则原样返回 <paramref name="loaded"/>。
        /// 调用方必须先按 <paramref name="loaded"/>.name 判断 Home/Combat，再调本方法（Clone 默认名不含 Home）。
        /// </summary>
        public static RuntimeAnimatorController ApplyIfVillageHome(RuntimeAnimatorController loaded)
        {
            if (loaded == null)
            {
                return loaded;
            }

            string sceneName = SceneManager.GetActiveScene().name;
            if (!VillageHomeSceneNames.Contains(sceneName))
            {
                // 龙宫、村街道、Combat、商店：静默原样返回，避免误伤日志。
                return loaded;
            }

            AnimatorOverrideController runtime = CloneAsRuntimeOverride(loaded);
            if (runtime == null)
            {
                Debug.LogWarning(LogTag + " Clone Override 失败，保持原控制器。scene=" + sceneName + " asset=" + loaded.name);
                return loaded;
            }

            var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>(32);
            runtime.GetOverrides(pairs);

            AnimationClip idleDay = FindEffectiveClip(pairs, ClipIdleDayLight);
            AnimationClip walkDay = FindEffectiveClip(pairs, ClipWalkDayLight);
            AnimationClip binkDay = FindEffectiveClip(pairs, ClipBinkDayLight);
            if (idleDay == null || walkDay == null || binkDay == null)
            {
                // 缺任一白天行时不强行换片，避免部分槽 Missing 导致 IsName 卡死。
                Debug.LogWarning(LogTag + " 找不到白天 Clip，保持原 Idle/Walk/Bink。scene=" + sceneName
                    + " Idle_DayLight=" + (idleDay != null) + " Walk_DayLight=" + (walkDay != null)
                    + " Bink_DayLight=" + (binkDay != null) + " asset=" + loaded.name);
                return loaded;
            }

            bool idleOk = RemapOriginalSlot(pairs, ClipIdle, idleDay);
            bool walkOk = RemapOriginalSlot(pairs, ClipWalk, walkDay);
            bool binkOk = RemapOriginalSlot(pairs, ClipBink, binkDay);
            if (!idleOk || !walkOk || !binkOk)
            {
                Debug.LogWarning(LogTag + " 找不到 Idle/Walk/Bink 原槽，保持原控制器。scene=" + sceneName
                    + " Idle槽=" + idleOk + " Walk槽=" + walkOk + " Bink槽=" + binkOk);
                return loaded;
            }

            runtime.ApplyOverrides(pairs);
            Debug.Log(LogTag + " 已换白天 Idle/Walk/Bink。scene=" + sceneName + " asset=" + loaded.name
                + " idle=" + idleDay.name + " walk=" + walkDay.name + " bink=" + binkDay.name);
            return runtime;
        }

        /// <summary>
        /// 复制一份运行时 Override，禁止写回磁盘资产。
        /// 铠甲 loaded 本身已是 Override：必须挂同一张底图再拷贝映射，避免把 Override 再套一层导致原 Clip 名对不上。
        /// </summary>
        private static AnimatorOverrideController CloneAsRuntimeOverride(RuntimeAnimatorController loaded)
        {
            var existing = loaded as AnimatorOverrideController;
            AnimatorOverrideController runtime;
            if (existing != null)
            {
                RuntimeAnimatorController baseController = existing.runtimeAnimatorController;
                if (baseController == null)
                {
                    return null;
                }

                runtime = new AnimatorOverrideController(baseController);
                var copied = new List<KeyValuePair<AnimationClip, AnimationClip>>(32);
                existing.GetOverrides(copied);
                runtime.ApplyOverrides(copied);
            }
            else
            {
                // 裙子 Dress 底图 .controller：直接包一层，GetOverrides 原 Clip 名即 Idle/Walk/Bink 及 *_DayLight 孤岛。
                runtime = new AnimatorOverrideController(loaded);
            }

            // 保底：若后续仍有 Contains("Home") 判断，Clone 默认名不含 Home 会误走 Combat。
            runtime.name = loaded.name;
            return runtime;
        }

        /// <summary>取 Override 生效片子：Value 为空则用原 Clip（裙子未覆盖时）。</summary>
        private static AnimationClip FindEffectiveClip(
            List<KeyValuePair<AnimationClip, AnimationClip>> pairs,
            string originalClipName)
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                AnimationClip original = pairs[i].Key;
                if (original == null || original.name != originalClipName)
                {
                    continue;
                }

                return pairs[i].Value != null ? pairs[i].Value : original;
            }

            return null;
        }

        /// <summary>只改指定原槽的 Value，Key 必须仍是底图那条 Clip，否则 Animator 对不上状态。</summary>
        private static bool RemapOriginalSlot(
            List<KeyValuePair<AnimationClip, AnimationClip>> pairs,
            string originalClipName,
            AnimationClip replacement)
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                AnimationClip original = pairs[i].Key;
                if (original == null || original.name != originalClipName)
                {
                    continue;
                }

                pairs[i] = new KeyValuePair<AnimationClip, AnimationClip>(original, replacement);
                return true;
            }

            return false;
        }
    }
}
