# PomodoroTimer 独立番茄钟组件

## 📌 概述
`PomodoroTimer` 是一个独立的、可复用的番茄钟UI组件。它从 `FocusSystem` 获取数据并展示，可以放置在场景中的任意位置。

## 🎯 特性
- ✅ **完全独立**：不包含业务逻辑，仅负责UI展示
- ✅ **事件驱动**：自动响应 FocusSystem 的状态变化
- ✅ **可配置**：支持自定义颜色和视觉效果
- ✅ **即插即用**：拖放到场景即可使用
- ✅ **多实例支持**：可以在不同位置放置多个番茄钟UI

## 🔧 使用方法

### 1. 在场景中创建番茄钟UI
1. 在Unity编辑器中创建一个新的GameObject
2. 添加 `PomodoroTimer` 组件
3. 创建或指定UI子元素

### 2. 设置UI引用
在Inspector中配置以下引用：

#### 必需引用：
- **Timer Text** (TextMeshProUGUI) - 显示倒计时（格式：MM:SS）
- **Phase Label Text** (TextMeshProUGUI) - 显示当前阶段（"专注"或"休息"）
- **Action Button** (Button) - 开始/暂停按钮
- **Action Button Text** (TextMeshProUGUI) - 按钮上的文字
- **Progress Ring** (Image) - 圆形进度条（Image Type设为Filled）

#### 可选引用：
- **Cycle Dots** (GameObject[]) - 最多4个点，表示完成的番茄钟数量

### 3. 配置视觉设置
在Inspector的 `Visual Settings` 区域：
- **Work Color** - 专注状态的进度环颜色（默认：绿色 #1D9E75）
- **Rest Color** - 休息状态的进度环颜色（默认：蓝色 #378ADD）
- **Enable Pulse Effect** - 完成时是否启用脉冲动画

## 📋 UI层级结构示例

```
PomodoroTimerUI (GameObject with PomodoroTimer.cs)
├── TimerDisplay (TextMeshProUGUI) → timerText
├── PhaseLabel (TextMeshProUGUI) → phaseLabelText
├── ProgressRing (Image, Filled) → progressRing
├── ActionButton (Button) → actionButton
│   └── ButtonText (TextMeshProUGUI) → actionButtonText
└── CycleDots (Parent)
    ├── Dot1 (GameObject) → cycleDots[0]
    ├── Dot2 (GameObject) → cycleDots[1]
    ├── Dot3 (GameObject) → cycleDots[2]
    └── Dot4 (GameObject) → cycleDots[3]
```

## 🎮 公共方法

### OnActionButtonPressed()
开始/暂停番茄钟
```csharp
// 在Button的OnClick事件中绑定此方法
```

### ResetTimer()
重置番茄钟到初始状态
```csharp
// 可以绑定到重置按钮
pomodoroTimer.ResetTimer();
```

## 🔗 依赖关系

### 必需系统：
- **FocusSystem** - 提供番茄钟的核心逻辑和状态

### 自动订阅的事件：
- `OnTimerTick` - 每帧更新计时
- `OnRunningChanged` - 运行状态改变
- `OnPhaseChanged` - 阶段切换（工作↔休息）
- `OnFocusCompleted` - 专注完成
- `OnRestCompleted` - 休息完成

## 🎨 自定义样式

### 修改颜色
```csharp
// 在Inspector中直接修改
Work Color = #1D9E75  // 专注时的绿色
Rest Color = #378ADD  // 休息时的蓝色
```

### 禁用脉冲效果
```csharp
// 在Inspector中取消勾选
Enable Pulse Effect = false
```

## 🚀 高级用法

### 创建多个番茄钟实例
你可以在场景中放置多个 `PomodoroTimer` 组件：
- 主UI面板上的大型番茄钟
- 迷你悬浮窗番茄钟
- 不同位置的指示器

所有实例会自动同步显示相同的番茄钟状态。

### 与其他系统集成
```csharp
// 其他脚本可以通过FocusSystem访问状态
if (FocusSystem.Instance.IsRunning && !FocusSystem.Instance.IsResting)
{
    // 当前正在专注工作
}
```

## ⚠️ 注意事项

1. **确保FocusSystem存在**
   - 场景中必须有 FocusSystem 组件
   - PomodoroTimer 会在没有FocusSystem时显示警告

2. **Progress Ring设置**
   - Image Type 必须设为 **Filled**
   - Fill Method 推荐设为 **Radial 360**
   - Fill Origin 设为 **Top**

3. **事件生命周期**
   - 组件启用时自动订阅事件
   - 组件禁用时自动取消订阅
   - 无需手动管理事件

## 📝 更新日志

### v2.0 (当前版本)
- 重构为纯UI组件
- 从FocusSystem获取数据（单一数据源）
- 支持多实例
- 改进了事件管理
- 添加了更好的错误提示

### v1.0 (旧版本)
- 独立管理状态
- 不与FocusSystem集成

## 🤝 与UIManager的区别

| 功能 | UIManager | PomodoroTimer |
|------|-----------|---------------|
| 定位 | 全局UI管理器 | 独立番茄钟组件 |
| 职责 | 管理所有UI元素 | 仅管理番茄钟UI |
| 复用性 | 单例，不可复用 | 可创建多个实例 |
| 数据源 | 直接订阅多个系统 | 仅订阅FocusSystem |

建议：
- 使用 `PomodoroTimer` 作为独立的番茄钟UI组件
- `UIManager` 可以专注于其他全局UI功能
