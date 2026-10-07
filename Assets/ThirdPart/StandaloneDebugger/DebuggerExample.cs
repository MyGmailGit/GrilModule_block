#if TEST_MODE
using UnityEngine;
using System.Collections.Generic;
using StandaloneDebugger;

/// <summary>
/// StandaloneDebugger 使用示例
/// 将此脚本添加到你的场景中以演示如何使用独立调试器
/// </summary>
public class DebuggerExample : MonoBehaviour
{
    private Debugger _debugger;

    void Start()
    {
        // 获取Debugger实例
        _debugger = Debugger.Instance;

        if (_debugger == null)
        {
            Debug.LogError("Debugger not found in scene!");
            return;
        }

        Debug.Log("StandaloneDebugger initialized successfully!");

        // 示例1: 基本控制
        ExampleBasicControl();

        // 示例2: 访问日志
        ExampleAccessLogs();

        // 示例3: 添加自定义窗口
        ExampleAddCustomWindow();
    }

    /// <summary>
    /// 示例1: 基本的Debugger控制
    /// </summary>
    void ExampleBasicControl()
    {
        // 启用调试器
        _debugger.ActiveWindow = true;

        // 控制是否显示完整窗口
        // _debugger.ShowFullWindow = true;

        // 调整窗口大小和位置
        _debugger.WindowScale = 1.2f;

        // 重置窗口布局到默认值
        // _debugger.ResetLayout();

        Debug.Log($"Debugger Window Type: {_debugger.ActiveWindowType}");
    }

    /// <summary>
    /// 示例2: 访问Console窗口的日志
    /// </summary>
    void ExampleAccessLogs()
    {
        // 获取所有日志
        List<LogNode> allLogs = new List<LogNode>();
        _debugger.GetRecentLogs(allLogs);

        Debug.Log($"Total logs: {allLogs.Count}");

        // 获取最近的5条日志
        List<LogNode> recentLogs = new List<LogNode>();
        _debugger.GetRecentLogs(recentLogs, 5);

        foreach (var log in recentLogs)
        {
            Debug.Log($"[{log.Type}] {log.Message}");
        }
    }

    /// <summary>
    /// 示例3: 添加自定义调试窗口
    /// </summary>
    void ExampleAddCustomWindow()
    {
        // 创建并注册自定义窗口
        var customWindow = new CustomDebugWindow();
        _debugger.RegisterDebuggerWindow("Custom/Example", customWindow);

        Debug.Log("Custom debug window registered!");
    }

    void Update()
    {
        // 示例: 按下D键来切换调试器显示
        if (Input.GetKeyDown(KeyCode.D))
        {
            _debugger.ActiveWindow = !_debugger.ActiveWindow;
        }

        // 示例: 按下F键来切换完整窗口
        if (Input.GetKeyDown(KeyCode.F))
        {
            _debugger.ShowFullWindow = !_debugger.ShowFullWindow;
        }
    }
}

/// <summary>
/// 自定义调试窗口示例
/// </summary>
public class CustomDebugWindow : IDebuggerWindow
{
    private float _deltaTime = 0f;
    private int _frameCount = 0;

    public void Initialize(params object[] args)
    {
        Debug.Log("CustomDebugWindow initialized");
    }

    public void Shutdown()
    {
        Debug.Log("CustomDebugWindow shutdown");
    }

    public void OnEnter()
    {
        Debug.Log("CustomDebugWindow entered");
    }

    public void OnLeave()
    {
        Debug.Log("CustomDebugWindow left");
    }

    public void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        _deltaTime = realElapseSeconds;
        _frameCount++;
    }

    public void OnDraw()
    {
        GUILayout.Label("<b>Custom Debug Window Example</b>", new GUIStyle(GUI.skin.label) { fontSize = 16 });
        GUILayout.Label($"Frame Count: {_frameCount}");
        GUILayout.Label($"Delta Time: {_deltaTime:F3}s");

        GUILayout.Space(10);

        GUILayout.Label("<b>System Information</b>");
        GUILayout.Label($"Device: {SystemInfo.deviceName}");
        GUILayout.Label($"Memory: {SystemInfo.systemMemorySize} MB");
        GUILayout.Label($"Processor: {SystemInfo.processorType}");

        GUILayout.Space(10);

        if (GUILayout.Button("Log Message", GUILayout.Height(30)))
        {
            Debug.Log("Custom button clicked!");
        }

        if (GUILayout.Button("Test Performance", GUILayout.Height(30)))
        {
            TestPerformance();
        }
    }

    private void TestPerformance()
    {
        // 性能测试示例
        var startTime = System.DateTime.Now;

        int sum = 0;
        for (int i = 0; i < 1000000; i++)
        {
            sum += i;
        }

        var elapsed = System.DateTime.Now - startTime;
        Debug.Log($"Performance test completed in {elapsed.TotalMilliseconds:F2}ms. Sum: {sum}");
    }
}
#endif