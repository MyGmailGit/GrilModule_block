#if TEST_MODE
using System;
using System.Collections.Generic;

namespace StandaloneDebugger
{
    /// <summary>
    /// 调试器窗口组实现。
    /// </summary>
    internal sealed class DebuggerWindowGroup : IDebuggerWindowGroup
    {
        private readonly List<KeyValuePair<string, IDebuggerWindow>> _debuggerWindows = new();
        private int _selectedIndex = 0;
        private string[] _debuggerWindowNames = null;

        /// <summary>
        /// 获取调试器窗口数量。
        /// </summary>
        public int DebuggerWindowCount => _debuggerWindows.Count;

        /// <summary>
        /// 获取或设置当前选中的调试器窗口索引。
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => _selectedIndex = value;
        }

        /// <summary>
        /// 获取当前选中的调试器窗口。
        /// </summary>
        public IDebuggerWindow SelectedWindow
        {
            get
            {
                if (_selectedIndex >= _debuggerWindows.Count)
                {
                    return null;
                }
                return _debuggerWindows[_selectedIndex].Value;
            }
        }

        /// <summary>
        /// 初始化调试组。
        /// </summary>
        public void Initialize(params object[] args) { }

        /// <summary>
        /// 关闭调试组。
        /// </summary>
        public void Shutdown()
        {
            foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in _debuggerWindows)
            {
                debuggerWindow.Value.Shutdown();
            }
            _debuggerWindows.Clear();
        }

        /// <summary>
        /// 进入调试器窗口。
        /// </summary>
        public void OnEnter()
        {
            SelectedWindow?.OnEnter();
        }

        /// <summary>
        /// 离开调试器窗口。
        /// </summary>
        public void OnLeave()
        {
            SelectedWindow?.OnLeave();
        }

        /// <summary>
        /// 调试组轮询。
        /// </summary>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            SelectedWindow?.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 调试器窗口绘制。
        /// </summary>
        public void OnDraw() { }

        private void RefreshDebuggerWindowNames()
        {
            int index = 0;
            _debuggerWindowNames = new string[_debuggerWindows.Count];
            foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in _debuggerWindows)
            {
                _debuggerWindowNames[index++] = debuggerWindow.Key;
            }
        }

        /// <summary>
        /// 获取调试组的调试器窗口名称集合。
        /// </summary>
        public string[] GetDebuggerWindowNames()
        {
            return _debuggerWindowNames;
        }

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            int pos = path.IndexOf('/');
            if (pos < 0 || pos >= path.Length - 1)
            {
                return InternalGetDebuggerWindow(path);
            }

            string debuggerWindowGroupName = path.Substring(0, pos);
            string leftPath = path.Substring(pos + 1);
            DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
            if (debuggerWindowGroup == null)
            {
                return null;
            }

            return debuggerWindowGroup.GetDebuggerWindow(leftPath);
        }

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        public bool SelectDebuggerWindow(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            int pos = path.IndexOf('/');
            if (pos < 0 || pos >= path.Length - 1)
            {
                return InternalSelectDebuggerWindow(path);
            }

            string debuggerWindowGroupName = path.Substring(0, pos);
            string leftPath = path.Substring(pos + 1);
            DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
            if (debuggerWindowGroup == null || !InternalSelectDebuggerWindow(debuggerWindowGroupName))
            {
                return false;
            }

            return debuggerWindowGroup.SelectDebuggerWindow(leftPath);
        }

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path is invalid.");
            }

            int pos = path.IndexOf('/');
            if (pos > 0 && pos < path.Length - 1)
            {
                string debuggerWindowGroupName = path.Substring(0, pos);
                string leftPath = path.Substring(pos + 1);
                DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
                if (debuggerWindowGroup == null)
                {
                    debuggerWindowGroup = new DebuggerWindowGroup();
                    InternalRegisterDebuggerWindow(debuggerWindowGroupName, debuggerWindowGroup);
                }
                debuggerWindowGroup.RegisterDebuggerWindow(leftPath, debuggerWindow);
            }
            else
            {
                InternalRegisterDebuggerWindow(path, debuggerWindow);
            }
        }

        /// <summary>
        /// 解除注册调试器窗口。
        /// </summary>
        public bool UnregisterDebuggerWindow(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            int pos = path.IndexOf('/');
            if (pos > 0 && pos < path.Length - 1)
            {
                string debuggerWindowGroupName = path.Substring(0, pos);
                string leftPath = path.Substring(pos + 1);
                DebuggerWindowGroup debuggerWindowGroup = (DebuggerWindowGroup)InternalGetDebuggerWindow(debuggerWindowGroupName);
                if (debuggerWindowGroup == null)
                {
                    return false;
                }
                return debuggerWindowGroup.UnregisterDebuggerWindow(leftPath);
            }
            else
            {
                int index = -1;
                for (int i = 0; i < _debuggerWindows.Count; i++)
                {
                    if (_debuggerWindows[i].Key == path)
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0)
                {
                    return false;
                }
                _debuggerWindows[index].Value.Shutdown();
                _debuggerWindows.RemoveAt(index);
                RefreshDebuggerWindowNames();
                if (_selectedIndex >= _debuggerWindows.Count)
                {
                    _selectedIndex = _debuggerWindows.Count - 1;
                }
                return true;
            }
        }

        private IDebuggerWindow InternalGetDebuggerWindow(string name)
        {
            foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in _debuggerWindows)
            {
                if (debuggerWindow.Key == name)
                {
                    return debuggerWindow.Value;
                }
            }
            return null;
        }

        private bool InternalSelectDebuggerWindow(string name)
        {
            int index = 0;
            foreach (KeyValuePair<string, IDebuggerWindow> debuggerWindow in _debuggerWindows)
            {
                if (debuggerWindow.Key == name)
                {
                    SelectedWindow?.OnLeave();
                    _selectedIndex = index;
                    SelectedWindow?.OnEnter();
                    return true;
                }
                index++;
            }
            return false;
        }

        private void InternalRegisterDebuggerWindow(string name, IDebuggerWindow debuggerWindow)
        {
            if (debuggerWindow == null)
            {
                throw new ArgumentException("Debugger window is invalid.");
            }

            _debuggerWindows.Add(new KeyValuePair<string, IDebuggerWindow>(name, debuggerWindow));
            RefreshDebuggerWindowNames();
        }
    }
}
#endif