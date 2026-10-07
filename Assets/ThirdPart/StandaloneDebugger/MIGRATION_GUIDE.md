# TEngine Debugger 解耦迁移指南

## 项目概述

已成功将TEngine中的Debugger工具从框架中完全解耦，创建了一个独立的`StandaloneDebugger`工具包。

## 主要变化

### ✅ 保留的功能
所有与TEngine框架无关的调试功能都已保留：

1. **Console窗口** - 完整的日志管理和查看
2. **FPS计数器** - 性能监控
3. **系统信息** - 设备和硬件信息
4. **环境信息** - 应用配置和版本
5. **屏幕信息** - 分辨率和显示设置
6. **图形信息** - GPU和渲染统计
7. **输入信息** - 所有传感器数据（触摸、加速度、陀螺仪、位置等）
8. **路径信息** - 应用数据路径
9. **场景信息** - 场景和对象统计
10. **时间信息** - 游戏时间统计
11. **质量设置** - 性能配置
12. **分析器** - 内存和资源使用
13. **设置** - 窗口布局持久化

### ❌ 移除的功能

#### ObjectPoolInformationWindow (对象池信息窗口)
- **原因**: 依赖于 `IObjectPoolModule` - TEngine特定模块
- **替代方案**: 可以创建自定义窗口来显示项目中的对象池统计

#### MemoryPoolPoolInformationWindow (内存池信息窗口)
- **原因**: 依赖于 `IMemoryPoolModule` - TEngine特定模块
- **替代方案**: 可以通过自定义窗口集成项目的内存管理

### 移除的依赖

#### 1. ModuleSystem 框架依赖
**原文件：** `Debugger.cs` Initialize()
```csharp
// 原代码
_debuggerModule = ModuleSystem.GetModule<IDebuggerModule>();
```

**新代码：** 直接创建WindowGroup
```csharp
// 新代码
_debuggerWindowRoot = new DebuggerWindowGroup();
```

#### 2. TEngine Log 类
**原文件：** 多个窗口文件
```csharp
// 原代码
Log.Fatal("Debugger component is invalid.");
```

**新代码：** 使用Unity原生Debug
```csharp
// 新代码
Debug.LogError("Debugger component is invalid.");
```

#### 3. TEngine Utility 类
创建了简化的 `Utility` 类来处理：
- `Utility.Text.Format()` - 字符串格式化
- `Utility.Path.GetRegularPath()` - 路径标准化
- `Utility.Converter.*` - 单位转换
- `Utility.Marshal.CachedHGlobalSize` - 内存统计

## 文件映射

### 核心文件
| TEngine 原文件 | StandaloneDebugger 新位置 | 状态 |
|---|---|---|
| `Debugger.cs` | `Core/Debugger.cs` | ✅ 修改 |
| `IDebuggerModule.cs` | 已删除 (不需要) | ❌ 移除 |
| `IDebuggerWindow.cs` | `Core/IDebuggerWindow.cs` | ✅ 保留 |
| `IDebuggerWindowGroup.cs` | `Core/IDebuggerWindowGroup.cs` | ✅ 保留 |
| `DebuggerManager.DebuggerWindowGroup.cs` | `Core/DebuggerWindowGroup.cs` | ✅ 修改 |
| `DebuggerActiveWindowType.cs` | `Core/DebuggerActiveWindowType.cs` | ✅ 保留 |

### 窗口组件文件
| 功能 | 文件名 | 依赖关系 |
|---|---|---|
| Console | `DebuggerComponent.ConsoleWindow.cs` | 无TEngine依赖 ✅ |
| FPS计数器 | `DebuggerComponent.FpsCounter.cs` | 无TEngine依赖 ✅ |
| 系统信息 | `DebuggerModule.SystemInformationWindow.cs` | 无TEngine依赖 ✅ |
| 环境信息 | `DebuggerModule.EnvironmentInformationWindow.cs` | 无TEngine依赖 ✅ |
| 屏幕信息 | `DebuggerModule.ScreenInformationWindow.cs` | 无TEngine依赖 ✅ |
| 图形信息 | `DebuggerModule.GraphicsInformationWindow.cs` | 无TEngine依赖 ✅ |
| 输入信息 (所有) | `DebuggerModule.Input*InformationWindow.cs` | 无TEngine依赖 ✅ |
| 路径信息 | `DebuggerModule.PathInformationWindow.cs` | 无TEngine依赖 ✅ |
| 场景信息 | `DebuggerModule.SceneInformationWindow.cs` | 无TEngine依赖 ✅ |
| 时间信息 | `DebuggerModule.TimeInformationWindow.cs` | 无TEngine依赖 ✅ |
| 质量信息 | `DebuggerModule.QualityInformationWindow.cs` | 无TEngine依赖 ✅ |
| 分析器 | `DebuggerModule.ProfilerInformationWindow.cs` | 无TEngine依赖 ✅ |
| 内存信息 | `DebuggerModule.RuntimeMemory*.cs` | 无TEngine依赖 ✅ |
| 对象池 | `DebuggerModule.ObjectPoolInformationWindow.cs` | ❌ 已移除 |
| 内存池 | `DebuggerModule.MemoryPoolInformationWindow.cs` | ❌ 已移除 |
| 设置 | `DebuggerModule.SettingsWindow.cs` | 无TEngine依赖 ✅ |

## 使用指南

### 快速开始

1. **在场景中添加Debugger**
   - 创建一个空GameObject
   - 添加 `Debugger` 组件

2. **配置Inspector设置**
   - Active Window Type: 选择何时显示调试器
   - GUISkin: 可选，用于自定义UI外观

3. **在代码中控制**
   ```csharp
   using StandaloneDebugger;
   
   Debugger debugger = Debugger.Instance;
   debugger.ActiveWindow = true;
   ```

### 完整项目迁移步骤

如果你正在使用TEngine中的Debugger，按照以下步骤迁移：

#### 第1步: 移除旧的Debugger
```csharp
// 删除或注释掉原来的TEngine Debugger初始化代码
// GameFramework.GetModule<IDebuggerModule>().xxx
```

#### 第2步: 导入StandaloneDebugger
```csharp
// 添加使用声明
using StandaloneDebugger;
```

#### 第3步: 更新代码引用
```csharp
// 原代码
var debugger = GameFramework.GetModule<IDebuggerModule>();

// 新代码
var debugger = Debugger.Instance;
```

#### 第4步: 自定义对象池窗口（可选）
如果你需要对象池统计，创建自定义窗口：

```csharp
public class ProjectObjectPoolWindow : IDebuggerWindow
{
    public void OnDraw()
    {
        // 显示你项目中的对象池统计
    }
    // 实现其他接口方法...
}

// 注册窗口
Debugger.Instance.RegisterDebuggerWindow("Profiler/Object Pool", 
    new ProjectObjectPoolWindow());
```

## 代码示例

### 示例1: 基本使用
```csharp
public class GameManager : MonoBehaviour
{
    void Start()
    {
        var debugger = Debugger.Instance;
        
        // 启用调试器
        debugger.ActiveWindow = true;
        
        // 选择特定窗口
        debugger.SelectDebuggerWindow("Console");
    }
}
```

### 示例2: 访问日志
```csharp
var debugger = Debugger.Instance;
var logs = new List<LogNode>();
debugger.GetRecentLogs(logs, 10);

foreach (var log in logs)
{
    Debug.Log($"[{log.Type}] {log.Message}");
}
```

### 示例3: 创建自定义窗口
```csharp
public class MyCustomDebugWindow : IDebuggerWindow
{
    public void Initialize(params object[] args) { }
    public void Shutdown() { }
    public void OnEnter() { }
    public void OnLeave() { }
    public void OnUpdate(float dt, float realDt) { }
    
    public void OnDraw()
    {
        GUILayout.Label("My Custom Debug Info");
        // 你的调试UI代码
    }
}

// 使用
var window = new MyCustomDebugWindow();
Debugger.Instance.RegisterDebuggerWindow("Custom/MyWindow", window);
```

## 性能影响

- **内存**: 完全相同（移除对象池窗口后反而减少）
- **CPU**: 不依赖ModuleSystem查询，略有性能提升
- **启动时间**: 略快（无需初始化TEngine模块）

## 常见问题

### Q: 原项目中已使用TEngine Debugger，如何升级？
A: 只需将StandaloneDebugger文件夹添加到项目，然后：
1. 删除或注释掉TEngine的ModuleSystem Debugger初始化
2. 在需要的地方使用 `Debugger.Instance`
3. 如果需要对象池统计，创建自定义窗口

### Q: 可以同时使用两个版本吗？
A: 不建议。会产生命名冲突。建议选择一个版本使用。

### Q: 如何添加对象池监控？
A: 创建实现IDebuggerWindow的自定义窗口，在OnDraw()中显示你项目的对象池数据。

### Q: StandaloneDebugger支持哪些Unity版本？
A: 2019.4+ (理论上2018+也支持，但未测试)

## 文件统计

| 类别 | 数量 | 状态 |
|---|---|---|
| 核心接口和类 | 6 | ✅ 完成 |
| 窗口组件 | 21 | ✅ 完成 |
| 工具类 | 1 | ✅ 完成 |
| 文档 | 2 | ✅ 完成 |
| 示例代码 | 1 | ✅ 完成 |
| **总计** | **31** | ✅ **完成** |

## 总结

✅ **成功完成解耦**
- 完全移除所有TEngine框架依赖
- 保留所有与框架无关的调试功能
- 创建了完整的独立工具包
- 提供了详细的迁移指南和使用示例

🎯 **项目位置**
```
Assets/StandaloneDebugger/
├── Core/          # 核心类和接口
├── Components/    # 调试窗口组件
├── README.md      # 使用说明
└── DebuggerExample.cs  # 使用示例
```

✨ **特点**
- 零外部依赖
- 易于集成
- 可随意扩展
- 完整的文档和示例
