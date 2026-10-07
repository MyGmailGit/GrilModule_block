#if TEST_MODE
using System.Collections.Generic;
using UnityEngine;

namespace StandaloneDebugger
{
    public class GMWindow : Debugger.ScrollableDebuggerWindowBase
    {
        private readonly List<string> _history = new List<string>();
        private string _input = string.Empty;

        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>GM Console</b>");

            GUILayout.BeginHorizontal();
            _input = GUILayout.TextField(_input, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Send", GUILayout.Width(80f)))
            {
                SendCommand(_input);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            // GUILayout.BeginHorizontal();
            // if (GUILayout.Button("Clear", GUILayout.Height(24f)))
            // {
            //     _history.Clear();
            // }
            // if (GUILayout.Button("Copy", GUILayout.Height(24f)))
            // {
            //     string joined = string.Join("\n", _history);
            //     Debugger.CopyToClipboard(joined);
            // }
            // GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("开启广告Debugger", GUILayout.Width(200f), GUILayout.Height(60)))
            {
#if MODULE_APPLOVIN
                MaxSdk.ShowMediationDebugger();
#endif
            }
            if (GUILayout.Button("重置UMP", GUILayout.Width(200f), GUILayout.Height(60)))
            {
            }
            if (GUILayout.Button("显示GDPRForm", GUILayout.Width(200f), GUILayout.Height(60)))
            {
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("设置Coin", GUILayout.MaxWidth(200), GUILayout.Height(60)))
            {
                Watermelon.CurrencyController.Set(Watermelon.CurrencyType.Coins, 200000);
            }
            if (GUILayout.Button("设置Diamond", GUILayout.MaxWidth(200), GUILayout.Height(60)))
            {
                Watermelon.CurrencyController.Set(Watermelon.CurrencyType.Diamond, 200000);
            }

            GUILayout.Space(10);

            if (GUILayout.Button($"Force To Video\n{(Watermelon.DevPanelEnabler.IsDevForceToVideo ? "force video" : "organic")}", GUILayout.MaxWidth(200), GUILayout.Height(60)))
            {
                Watermelon.DevPanelEnabler.SetForceToVideo(!Watermelon.DevPanelEnabler.IsDevForceToVideo);
                // if (DevPanelEnabler.IsDevForceToVideo)
                {
                    Watermelon.GameBackgroundVideoPreview.RefreshPlayNowVideo();
                }
            }
            GUILayout.EndHorizontal();

            // GUILayout.BeginHorizontal();
            // returnNum = GUILayout.TextField(returnNum, GUILayout.MaxWidth(250), GUILayout.Height(60));
            // if (GUILayout.Button("添加回退", GUILayout.MaxWidth(250), GUILayout.Height(60)))
            // {
            //     if (int.TryParse(returnNum, out var num))
            //     {
            //         // var data = ArchiveMgr.Instance.GetArchive<PlayerDataArchive>();
            //         // data.returnToolNum.Value += num;
            //     }
            // }

            // GUILayout.Space(10);

            // frozeNum = GUILayout.TextField(frozeNum, GUILayout.MaxWidth(250), GUILayout.Height(60));
            // if (GUILayout.Button("添加冰冻", GUILayout.MaxWidth(250), GUILayout.Height(60)))
            // {
            //     if (int.TryParse(frozeNum, out var key))
            //     {
            //         // var data = ArchiveMgr.Instance.GetArchive<PlayerDataArchive>();
            //         // data.frozeToolNum.Value += key;
            //     }
            // }
            // GUILayout.EndHorizontal();

            // GUILayout.BeginHorizontal();
            // addTargetFinishNum = GUILayout.TextField(addTargetFinishNum, GUILayout.MaxWidth(250), GUILayout.Height(60));
            // if (GUILayout.Button("添加目标Finish", GUILayout.MaxWidth(250), GUILayout.Height(60)))
            // {
            //     if (int.TryParse(addTargetFinishNum, out var num))
            //     {
            //         // var data = ArchiveMgr.Instance.GetArchive<PlayerDataArchive>();
            //         // data.findMasterTask_FindNum.Value += num;
            //         // FindMaskTaskCtrl.Instance.AddFindMasterTask_TargetNum(num);
            //     }
            // }

            // GUILayout.Space(10);
            // GUILayout.EndHorizontal();

            GUILayout.EndVertical();


            // _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            // for (int i = 0; i < _history.Count; i++)
            // {
            //     GUILayout.Label(_history[i]);
            // }
            // GUILayout.EndScrollView();
        }

        private void SendCommand(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                return;
            }

            // _history.Add($"> {cmd}");
            // Basic local command handling examples
            // if (cmd.Equals("help", System.StringComparison.OrdinalIgnoreCase))
            // {
            //     _history.Add("Available commands: help, ping, gc");
            // }
            // else if (cmd.Equals("ping", System.StringComparison.OrdinalIgnoreCase))
            // {
            //     _history.Add("pong");
            // }
            // else
            if (cmd.Equals("gc", System.StringComparison.OrdinalIgnoreCase))
            {
                System.GC.Collect();
                // _history.Add("Garbage collection triggered.");
            }
            else
            {
                // _history.Add($"Unknown command: {cmd}");
            }

            _input = string.Empty;
        }
    }
}
#endif