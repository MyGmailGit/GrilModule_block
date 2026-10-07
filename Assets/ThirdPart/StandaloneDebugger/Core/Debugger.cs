#if TEST_MODE
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StandaloneDebugger
{
    /// <summary>
    /// 独立调试器
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class Debugger : MonoBehaviour
    {
        private static Debugger _instance;
        public static Debugger Instance => _instance;

        /// <summary>
        /// 默认调试器漂浮框大小。
        /// </summary>
        internal static readonly Rect DefaultIconRect = new Rect(10f, 10f, 60f, 60f);

        /// <summary>
        /// 默认调试器窗口大小。
        /// </summary>
        internal static readonly Rect DefaultWindowRect = new Rect(10f, 10f, 640f, 480f);

        /// <summary>
        /// 默认调试器窗口缩放比例。
        /// </summary>
        internal static readonly float DefaultWindowScale = 1.5f;

        private static TextEditor s_TextEditor = null;
        private DebuggerWindowGroup _debuggerWindowRoot = null;
        private readonly Rect _dragRect = new Rect(0f, 0f, float.MaxValue, 25f);
        private Rect _iconRect = DefaultIconRect;
        private Rect _windowRect = DefaultWindowRect;
        private float _windowScale = DefaultWindowScale;

        [SerializeField]
        private GUISkin skin = null;

        [SerializeField]
        private DebuggerActiveWindowType activeWindow = DebuggerActiveWindowType.AlwaysOpen;

        public DebuggerActiveWindowType ActiveWindowType => activeWindow;

        [SerializeField]
        private bool _showFullWindow = false;

        [SerializeField]
        private ConsoleWindow _consoleWindow = new ConsoleWindow();

        private SystemInformationWindow _systemInformationWindow = new SystemInformationWindow();
        private EnvironmentInformationWindow _environmentInformationWindow = new EnvironmentInformationWindow();
        private ScreenInformationWindow _screenInformationWindow = new ScreenInformationWindow();
        private GraphicsInformationWindow _graphicsInformationWindow = new GraphicsInformationWindow();
        private InputSummaryInformationWindow _inputSummaryInformationWindow = new InputSummaryInformationWindow();
        private InputTouchInformationWindow _inputTouchInformationWindow = new InputTouchInformationWindow();
        // private InputLocationInformationWindow _inputLocationInformationWindow = new InputLocationInformationWindow();
        private InputAccelerationInformationWindow _inputAccelerationInformationWindow = new InputAccelerationInformationWindow();
        private InputGyroscopeInformationWindow _inputGyroscopeInformationWindow = new InputGyroscopeInformationWindow();
        private InputCompassInformationWindow _inputCompassInformationWindow = new InputCompassInformationWindow();
        private PathInformationWindow _pathInformationWindow = new PathInformationWindow();
        private SceneInformationWindow _sceneInformationWindow = new SceneInformationWindow();
        private TimeInformationWindow _timeInformationWindow = new TimeInformationWindow();
        private QualityInformationWindow _qualityInformationWindow = new QualityInformationWindow();
        private ProfilerInformationWindow _profilerInformationWindow = new ProfilerInformationWindow();
        // private RuntimeMemorySummaryWindow _runtimeMemorySummaryWindow = new RuntimeMemorySummaryWindow();
        // private RuntimeMemoryInformationWindow<Object> _runtimeMemoryAllInformationWindow = new RuntimeMemoryInformationWindow<Object>();
        // private RuntimeMemoryInformationWindow<Texture> _runtimeMemoryTextureInformationWindow = new RuntimeMemoryInformationWindow<Texture>();
        // private RuntimeMemoryInformationWindow<Mesh> _runtimeMemoryMeshInformationWindow = new RuntimeMemoryInformationWindow<Mesh>();
        // private RuntimeMemoryInformationWindow<Material> _runtimeMemoryMaterialInformationWindow = new RuntimeMemoryInformationWindow<Material>();
        // private RuntimeMemoryInformationWindow<Shader> _runtimeMemoryShaderInformationWindow = new RuntimeMemoryInformationWindow<Shader>();
        // private RuntimeMemoryInformationWindow<AnimationClip> _runtimeMemoryAnimationClipInformationWindow = new RuntimeMemoryInformationWindow<AnimationClip>();
        // private RuntimeMemoryInformationWindow<AudioClip> _runtimeMemoryAudioClipInformationWindow = new RuntimeMemoryInformationWindow<AudioClip>();
        // private RuntimeMemoryInformationWindow<Font> _runtimeMemoryFontInformationWindow = new RuntimeMemoryInformationWindow<Font>();
        // private RuntimeMemoryInformationWindow<TextAsset> _runtimeMemoryTextAssetInformationWindow = new RuntimeMemoryInformationWindow<TextAsset>();
        // private RuntimeMemoryInformationWindow<ScriptableObject> _runtimeMemoryScriptableObjectInformationWindow = new RuntimeMemoryInformationWindow<ScriptableObject>();
        private SettingsWindow _settingsWindow = new SettingsWindow();
        private GMWindow _gmWindow = new GMWindow();

        private FpsCounter _fpsCounter = null;
        private bool _activeWindowFlag = true;

        /// <summary>
        /// 获取或设置调试器窗口是否激活。
        /// </summary>
        public bool ActiveWindow
        {
            get => _activeWindowFlag;
            set
            {
                _activeWindowFlag = value;
                enabled = value;
            }
        }

        /// <summary>
        /// 获取或设置是否显示完整调试器界面。
        /// </summary>
        public bool ShowFullWindow
        {
            get => _showFullWindow;
            set
            {
                _showFullWindow = value;
            }
        }

        /// <summary>
        /// 获取或设置调试器漂浮框大小。
        /// </summary>
        public Rect IconRect
        {
            get => _iconRect;
            set => _iconRect = value;
        }

        /// <summary>
        /// 获取或设置调试器窗口大小。
        /// </summary>
        public Rect WindowRect
        {
            get => _windowRect;
            set => _windowRect = value;
        }

        /// <summary>
        /// 获取或设置调试器窗口缩放比例。
        /// </summary>
        public float WindowScale
        {
            get => _windowScale;
            set => _windowScale = value;
        }

        /// <summary>
        /// 游戏框架模块初始化。
        /// </summary>
        void Awake()
        {
            _instance = this;
            s_TextEditor = new TextEditor();
            _instance.gameObject.name = $"[{nameof(Debugger)}]";

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            PlayerPrefs.Save();
        }

        private void Initialize()
        {
            _debuggerWindowRoot = new DebuggerWindowGroup();
            _fpsCounter = new FpsCounter(0.5f);

            var lastIconX = PlayerPrefs.GetFloat("Debugger.Icon.X", DefaultIconRect.x);
            var lastIconY = PlayerPrefs.GetFloat("Debugger.Icon.Y", DefaultIconRect.y);
            var lastWindowX = PlayerPrefs.GetFloat("Debugger.Window.X", DefaultWindowRect.x);
            var lastWindowY = PlayerPrefs.GetFloat("Debugger.Window.Y", DefaultWindowRect.y);
            var lastWindowWidth = PlayerPrefs.GetFloat("Debugger.Window.Width", DefaultWindowRect.width);
            var lastWindowHeight = PlayerPrefs.GetFloat("Debugger.Window.Height", DefaultWindowRect.height);
            _windowScale = PlayerPrefs.GetFloat("Debugger.Window.Scale", DefaultWindowScale);
            _iconRect = new Rect(lastIconX, lastIconY, DefaultIconRect.width, DefaultIconRect.height);
            _windowRect = new Rect(lastWindowX, lastWindowY, lastWindowWidth, lastWindowHeight);
        }

        private void Start()
        {
            Initialize();
            RegisterDebuggerWindow("Console", _consoleWindow);
            RegisterDebuggerWindow("Information/System", _systemInformationWindow);
            RegisterDebuggerWindow("Information/Environment", _environmentInformationWindow);
            RegisterDebuggerWindow("Information/Screen", _screenInformationWindow);
            RegisterDebuggerWindow("Information/Graphics", _graphicsInformationWindow);
            RegisterDebuggerWindow("Information/Input/Summary", _inputSummaryInformationWindow);
            RegisterDebuggerWindow("Information/Input/Touch", _inputTouchInformationWindow);
            // RegisterDebuggerWindow("Information/Input/Location", _inputLocationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Acceleration", _inputAccelerationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Gyroscope", _inputGyroscopeInformationWindow);
            RegisterDebuggerWindow("Information/Input/Compass", _inputCompassInformationWindow);
            RegisterDebuggerWindow("Information/Other/Scene", _sceneInformationWindow);
            RegisterDebuggerWindow("Information/Other/Path", _pathInformationWindow);
            RegisterDebuggerWindow("Information/Other/Time", _timeInformationWindow);
            RegisterDebuggerWindow("Information/Other/Quality", _qualityInformationWindow);
            RegisterDebuggerWindow("Profiler/Summary", _profilerInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/Summary", _runtimeMemorySummaryWindow);
            // RegisterDebuggerWindow("Profiler/Memory/All", _runtimeMemoryAllInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/Texture", _runtimeMemoryTextureInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/Mesh", _runtimeMemoryMeshInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/Material", _runtimeMemoryMaterialInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/Shader", _runtimeMemoryShaderInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/AnimationClip", _runtimeMemoryAnimationClipInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/AudioClip", _runtimeMemoryAudioClipInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/Font", _runtimeMemoryFontInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/TextAsset", _runtimeMemoryTextAssetInformationWindow);
            // RegisterDebuggerWindow("Profiler/Memory/ScriptableObject", _runtimeMemoryScriptableObjectInformationWindow);
            // RegisterDebuggerWindow("Other/Settings", _settingsWindow);
            RegisterDebuggerWindow("GM", _gmWindow);

            switch (activeWindow)
            {
                case DebuggerActiveWindowType.AlwaysOpen:
                    ActiveWindow = true;
                    break;
                case DebuggerActiveWindowType.OnlyOpenWhenDevelopment:
                    // ActiveWindow = Debug.isDebugBuild;
#if TEST_MODE
                    ActiveWindow = true;//Debug.isDebugBuild;
#else
                    ActiveWindow = false;
#endif
                    break;
                case DebuggerActiveWindowType.OnlyOpenInEditor:
                    ActiveWindow = Application.isEditor;
                    break;
                default:
                    ActiveWindow = false;
                    break;
            }
        }

        private void Update()
        {
            _fpsCounter?.Update(Time.deltaTime, Time.unscaledDeltaTime);
            _debuggerWindowRoot?.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnGUI()
        {
            if (!_activeWindowFlag || _debuggerWindowRoot == null)
            {
                return;
            }

            GUISkin cachedGuiSkin = GUI.skin;
            Matrix4x4 cachedMatrix = GUI.matrix;

            GUI.skin = skin;
            GUI.matrix = Matrix4x4.Scale(new Vector3(_windowScale, _windowScale, 1f));

            if (_showFullWindow)
            {
                _windowRect = GUILayout.Window(0, _windowRect, DrawWindow, "<b>DEBUGGER</b>");
            }
            else
            {
                _iconRect = GUILayout.Window(0, _iconRect, DrawDebuggerWindowIcon, "<b>DEBUGGER</b>");
            }

            GUI.matrix = cachedMatrix;
            GUI.skin = cachedGuiSkin;
        }

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
        {
            if (_debuggerWindowRoot == null)
                return;

            _debuggerWindowRoot.RegisterDebuggerWindow(path, debuggerWindow);
            debuggerWindow.Initialize(args);
        }

        /// <summary>
        /// 解除注册调试器窗口。
        /// </summary>
        public bool UnregisterDebuggerWindow(string path)
        {
            return _debuggerWindowRoot?.UnregisterDebuggerWindow(path) ?? false;
        }

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            return _debuggerWindowRoot?.GetDebuggerWindow(path);
        }

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        public bool SelectDebuggerWindow(string path)
        {
            return _debuggerWindowRoot?.SelectDebuggerWindow(path) ?? false;
        }

        /// <summary>
        /// 还原调试器窗口布局。
        /// </summary>
        public void ResetLayout()
        {
            IconRect = DefaultIconRect;
            WindowRect = DefaultWindowRect;
            WindowScale = DefaultWindowScale;
        }

        /// <summary>
        /// 获取记录的所有日志。
        /// </summary>
        public void GetRecentLogs(List<LogNode> results)
        {
            _consoleWindow?.GetRecentLogs(results);
        }

        /// <summary>
        /// 获取记录的最近日志。
        /// </summary>
        public void GetRecentLogs(List<LogNode> results, int count)
        {
            _consoleWindow?.GetRecentLogs(results, count);
        }

        private void DrawWindow(int windowId)
        {
            GUI.DragWindow(_dragRect);
            DrawDebuggerWindowGroup(_debuggerWindowRoot);
        }

        private void DrawDebuggerWindowGroup(IDebuggerWindowGroup debuggerWindowGroup)
        {
            if (debuggerWindowGroup == null)
            {
                return;
            }

            List<string> names = new List<string>();
            string[] debuggerWindowNames = debuggerWindowGroup.GetDebuggerWindowNames();
            for (int i = 0; i < debuggerWindowNames.Length; i++)
            {
                names.Add($"<b>{debuggerWindowNames[i]}</b>");
            }

            if (debuggerWindowGroup == _debuggerWindowRoot)
            {
                names.Add("<b>Close</b>");
            }

            int toolbarIndex = GUILayout.Toolbar(debuggerWindowGroup.SelectedIndex, names.ToArray(), GUILayout.Height(30f), GUILayout.MaxWidth(Screen.width));
            if (toolbarIndex >= debuggerWindowGroup.DebuggerWindowCount)
            {
                ShowFullWindow = false;
                return;
            }

            if (debuggerWindowGroup.SelectedWindow == null)
            {
                return;
            }

            if (debuggerWindowGroup.SelectedIndex != toolbarIndex)
            {
                debuggerWindowGroup.SelectedWindow.OnLeave();
                debuggerWindowGroup.SelectedIndex = toolbarIndex;
                debuggerWindowGroup.SelectedWindow.OnEnter();
            }

            IDebuggerWindowGroup subDebuggerWindowGroup = debuggerWindowGroup.SelectedWindow as IDebuggerWindowGroup;
            if (subDebuggerWindowGroup != null)
            {
                DrawDebuggerWindowGroup(subDebuggerWindowGroup);
            }

            debuggerWindowGroup?.SelectedWindow?.OnDraw();
        }

        private void DrawDebuggerWindowIcon(int windowId)
        {
            GUI.DragWindow(_dragRect);
            GUILayout.Space(5);
            Color32 color = Color.white;
            _consoleWindow?.RefreshCount();
            if (_consoleWindow != null && _consoleWindow.FatalCount > 0)
            {
                color = _consoleWindow.GetLogStringColor(LogType.Exception);
            }
            else if (_consoleWindow != null && _consoleWindow.ErrorCount > 0)
            {
                color = _consoleWindow.GetLogStringColor(LogType.Error);
            }
            else if (_consoleWindow != null && _consoleWindow.WarningCount > 0)
            {
                color = _consoleWindow.GetLogStringColor(LogType.Warning);
            }
            else if (_consoleWindow != null)
            {
                color = _consoleWindow.GetLogStringColor(LogType.Log);
            }

            string title = $"<color=#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}><b>FPS: {_fpsCounter?.CurrentFps ?? 0:F2}</b></color>";
            if (GUILayout.Button(title, GUILayout.Width(100f), GUILayout.Height(40f)))
            {
                ShowFullWindow = true;
            }
        }

        internal static void CopyToClipboard(string content)
        {
            s_TextEditor.text = content;
            s_TextEditor.OnFocus();
            s_TextEditor.Copy();
            s_TextEditor.text = string.Empty;
        }
    }
}
#endif