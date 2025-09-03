//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using GameFramework.UnityRuntime.Base;
using UnityEngine;

namespace GameFramework.UnityRuntime.Debugger
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed class PathInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Path Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Current Directory", GameFramework.Utility.Path.GetRegularPath(Environment.CurrentDirectory));
                    DrawItem("Data Path", GameFramework.Utility.Path.GetRegularPath(Application.dataPath));
                    DrawItem("Persistent Data Path", GameFramework.Utility.Path.GetRegularPath(Application.persistentDataPath));
                    DrawItem("Streaming Assets Path", GameFramework.Utility.Path.GetRegularPath(Application.streamingAssetsPath));
                    DrawItem("Temporary Cache Path", GameFramework.Utility.Path.GetRegularPath(Application.temporaryCachePath));
#if UNITY_2018_3_OR_NEWER
                    DrawItem("Console Log Path", GameFramework.Utility.Path.GetRegularPath(Application.consoleLogPath));
#endif
                }
                GUILayout.EndVertical();
            }
        }
    }
}
