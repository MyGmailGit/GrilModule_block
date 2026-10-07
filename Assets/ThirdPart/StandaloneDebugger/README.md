# StandaloneDebugger - 独立调试工具

一个完全独立于TEngine框架的Unity调试器工具，可以直接集成到任何Unity项目中。

## 功能特性

### ✅ 包含的功能
- **Console** - 实时日志查看和过滤
- **FPS计数器** - 实时性能监控
- **系统信息** - 设备和系统信息
- **环境信息** - 应用配置和版本信息
- **屏幕信息** - 屏幕分辨率和显示设置
- **图形信息** - GPU和图形渲染信息
- **输入信息** - 触摸、加速度、陀螺仪、位置等传感器信息
- **路径信息** - 应用路径和数据存储位置
- **场景信息** - 当前场景和对象统计
- **时间信息** - 游戏时间和帧数统计
- **质量设置** - 渲染质量和性能配置
- **分析器信息** - 内存和资源使用统计
- **设置** - 调试器窗口位置和尺寸持久化

### ❌ 移除的功能
- **对象池信息** (ObjectPoolInformationWindow) - 依赖TEngine对象池模块
- **内存池信息** (MemoryPoolInformationWindow) - 依赖TEngine内存池模块

## 使用方法

### 1. 集成到项目
将`Assets/StandaloneDebugger`文件夹复制到你的Unity项目中。

### 2. 添加到场景
在你的游戏场景中添加一个GameObject，并为其添加`Debugger`组件：

```csharp
using UnityEngine;
using StandaloneDebugger;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Debugger会自动初始化
        // 可以通过Debugger.Instance来访问
        if (Debugger.Instance != null)
        {
            Debugger.Instance.ActiveWindow = true;
        }
    }
}
```

### 3. 控制调试器

```csharp
// 获取调试器实例
Debugger debugger = Debugger.Instance;

// 显示/隐藏完整窗口
debugger.ShowFullWindow = true;

// 启用/禁用调试器
debugger.ActiveWindow = true;

// 选中特定窗口
debugger.SelectDebuggerWindow("Console");

// 获取日志
List<LogNode> logs = new List<LogNode>();
debugger.GetRecentLogs(logs, 10); // 获取最近10条日志

// 重置窗口布局
debugger.ResetLayout();
```

## 配置选项

### 在Inspector中配置

1. **Active Window** - 选择调试器激活模式：
   - `AlwaysOpen` - 始终显示
   - `OnlyOpenWhenDevelopment` - 仅在Debug Build中显示
   - `OnlyOpenInEditor` - 仅在编辑器中显示

2. **GUISkin** - 指定UI外观（可选）

3. **Console Window** - 控制台窗口配置：
   - Max Line - 最大日志行数（默认100）
   - Lock Scroll - 是否锁定滚动
   - Filter - 按日志等级过滤

## 文件结构

```
Assets/StandaloneDebugger/
├── Core/
│   ├── Debugger.cs                    # 主调试器类
│   ├── IDebuggerWindow.cs             # 窗口接口
│   ├── IDebuggerWindowGroup.cs        # 窗口组接口
│   ├── DebuggerWindowGroup.cs         # 窗口组实现
│   ├── DebuggerActiveWindowType.cs    # 枚举定义
│   └── Utility.cs                     # 工具类
├── Components/
│   ├── LogNode.cs                     # 日志数据结构
│   ├── Debugger.FpsCounter.cs         # FPS计数器
│   ├── Debugger.ConsoleWindow.cs      # 控制台窗口
│   ├── Debugger.ScrollableDebuggerWindowBase.cs  # 基础窗口类
│   ├── Debugger.SystemInformationWindow.cs
│   ├── Debugger.EnvironmentInformationWindow.cs
│   ├── Debugger.ScreenInformationWindow.cs
│   ├── Debugger.GraphicsInformationWindow.cs
│   ├── Debugger.InputSummaryInformationWindow.cs
│   ├── Debugger.InputTouchInformationWindow.cs
│   ├── Debugger.InputLocationInformationWindow.cs
│   ├── Debugger.InputAccelerationInformationWindow.cs
│   ├── Debugger.InputGyroscopeInformationWindow.cs
│   ├── Debugger.InputCompassInformationWindow.cs
│   ├── Debugger.PathInformationWindow.cs
│   ├── Debugger.SceneInformationWindow.cs
│   ├── Debugger.TimeInformationWindow.cs
│   ├── Debugger.QualityInformationWindow.cs
│   ├── Debugger.ProfilerInformationWindow.cs
│   ├── Debugger.RuntimeMemoryInformationWindow.cs
│   ├── Debugger.RuntimeMemorySummaryWindow.cs
│   └── Debugger.SettingsWindow.cs
```

## 扩展自定义窗口

你可以轻松添加自己的调试窗口：

```csharp
using StandaloneDebugger;
using UnityEngine;

public class MyDebugWindow : IDebuggerWindow
{
    public void Initialize(params object[] args)
    {
        // 初始化
    }

    public void Shutdown()
    {
        // 清理资源
    }

    public void OnEnter()
    {
        // 窗口获得焦点
    }

    public void OnLeave()
    {
        // 窗口失去焦点
    }

    public void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        // 每帧更新逻辑
    }

    public void OnDraw()
    {
        // 绘制GUI
        GUILayout.Label("My Custom Debug Window");
        if (GUILayout.Button("Do Something"))
        {
            // 处理按钮点击
        }
    }
}

// 在Debugger中注册
Debugger.Instance.RegisterDebuggerWindow("Custom/MyWindow", new MyDebugWindow());
```

## 依赖关系

- **Unity 2019.4+** （经过测试的最低版本）
- **无第三方依赖** - 完全独立，不依赖TEngine或其他框架

## 移除TEngine依赖的改动

与原始TEngine版本相比，以下改动已进行：

1. ✅ 移除了`ModuleSystem`依赖
2. ✅ 移除了`IObjectPoolModule`和对象池统计
3. ✅ 移除了`IMemoryPoolModule`和内存池统计
4. ✅ 用标准Unity API替换了`Log`类
5. ✅ 创建了简化的`Utility`类来处理文本、路径等操作
6. ✅ 所有窗口类都继承自`ScrollableDebuggerWindowBase`
7. ✅ 保留了所有与性能、系统信息相关的窗口

## 常见问题

### Q: 为什么移除了对象池窗口？
A: 对象池功能是TEngine特定的功能，与游戏框架紧密耦合。移除它可以让调试器完全独立，无需任何框架依赖。

### Q: 可以添加对象池支持吗？
A: 可以！你可以创建自己的窗口来显示对象池信息。只需实现`IDebuggerWindow`接口即可。

### Q: 调试器支持多语言吗？
A: 当前版本使用英文和中文混合。可以通过自定义来支持其他语言。

### Q: 可以在发布版本中使用吗？
A: 可以。建议通过条件编译来控制在生产环境中禁用：
```csharp
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    debugger.ActiveWindow = true;
#endif
```

## 许可证

根据原TEngine项目许可证分发。

## 更新日志

### v1.0.0 (2024)
- 初始版本发布
- 完全移除TEngine依赖
- 保留所有主要调试功能
