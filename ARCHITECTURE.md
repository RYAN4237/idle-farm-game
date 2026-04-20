# 🌾⏱ FocusFarm - 双独立 App 架构文档

## 📖 核心理念

**不是两个窗口，而是桌面生态系统里的两个独立微应用**

参考：Rusty's Retirement

---

## 🎯 设计目标

| 目标 | 实现方式 |
|------|----------|
| ✅ 非打扰 (Non-intrusive) | Always-on-top + 透明背景 + 不抢焦点 |
| ✅ 持续运行 (Ambient) | 用户不用"打开游戏"，它一直存在 |
| ✅ 弱交互 (Low Interaction) | 偶尔点一下，大部分时间自动运行 |
| ✅ 可扩展 | 未来可加音乐播放器、To-do、股票等 |

---

## 🏗 架构设计

### 核心结构：多进程 + 松耦合

```
Desktop Ecosystem
 ├── Farm App（农田）     → FarmApp.exe
 ├── Timer App（番茄钟）  → TimerApp.exe
 └── (未来更多模块)
```

### 关键原则

❗ **每个模块都是"独立产品"**

- ❌ Timer 是 Farm 的子功能
- ✅ Timer 是可以单独运行的 App

### 模块关系

**弱连接 (Loose Coupling)**

- ✅ 各自运行
- ✅ 可选通信
- ❌ 不强依赖
- ❌ Farm 崩了 Timer 也挂

---

## 📐 窗口设计

### 🌾 Farm App

| 属性 | 值 |
|------|------|
| 位置 | 屏幕底部 |
| 宽度 | 全宽 |
| 高度 | 120px |
| 特性 | Always-on-top + 不抢焦点 + 透明背景 |

**实现**：`WindowManager.cs`

```csharp
public bool  alwaysOnTop   = true;
public bool  transparent   = true;
public bool  isFarmWindow  = true;
public int   farmHeight    = 120;
```

### ⏱ Timer App

| 属性 | 值 |
|------|------|
| 位置 | 右下角 |
| 尺寸 | 220x300px |
| 特性 | 可拖动 + Always-on-top + 透明背景 |

**实现**：`TimerWindowManager.cs`

```csharp
public int  windowWidth   = 220;
public int  windowHeight  = 300;
public int  marginRight   = 20;
public int  marginBottom  = 140; // 避开 Farm 条
public bool allowDrag     = true;
```

---

## 🔗 通信设计

### 原则

- 异步
- 可失败
- 不影响主流程

### 实现：JSON 文件

**位置**：`%AppData%/FocusFarm/state.json`

**结构**：

```csharp
public class FocusFarmState
{
    public bool  isRunning;
    public bool  isResting;
    public float timeRemaining;
    public int   completedCycles;
    public float focusPoints;
    public bool  boostActive;
    public float boostMultiplier;
    public long  timestamp; // 检测 Timer 是否还在运行
}
```

**流程**：

```
Timer App (写)                Farm App (读)
    ↓                             ↓
TimerStateWriter.cs ──→ state.json ──→ FarmStateReader.cs
(每 0.5s 写一次)                     (每 1s 读一次)
```

---

## 🎁 奖励闭环

**核心卖点**："你专注 → 游戏变好"

### 行为 → 奖励映射

| 事件 | 行为 | 效果 |
|------|------|------|
| Timer 完成 | 专注 25 分钟 | Farm 加速 2x，持续 5 分钟 |
| Timer 开始 | 用户开始专注 | 特效增强 |
| Timer 暂停 | 提前放弃 | 降低效率 |

**实现**：

```
FocusSystem.OnFocusCompleted
    ↓
FocusEventBridge.HandleFocusComplete()
    ↓
GameEventBus.PublishBoost(2x, 300s)
    ↓
FarmBoostReceiver.ApplyBoost()
    ↓
农田生长速度 x2
```

---

## 🚀 构建流程

### 1. 创建 Timer 场景

```
菜单 → FocusFarm → 1. Create Timer Scene
```

这会生成：
- `Assets/Scenes/TimerScene.unity`
- 独立的 Timer UI（220x300px 卡片）
- 必要的系统组件

### 2. 构建两个 App

```
菜单 → FocusFarm → 2. Build Farm App    → Build/FarmApp.exe
菜单 → FocusFarm → 3. Build Timer App   → Build/TimerApp.exe
```

### 3. 启动生态系统

```
菜单 → FocusFarm → ▶ Launch Both Apps
```

或者：手动启动两个 exe

---

## ⚡ 性能策略

**核心理念**：你不是在做游戏，而是做"系统插件"

| 优化项 | 实现 |
|--------|------|
| CPU 极低 | 减少 Update 调用 |
| 帧率低 | 15 FPS 足够（桌面挂件不需要 60 FPS） |
| I/O 节流 | Timer 0.5s 写一次，Farm 1s 读一次 |

**实现**：`AmbientPerformance.cs`

```csharp
Application.targetFrameRate = 15;
QualitySettings.vSyncCount = 0;
```

---

## 🎨 视觉风格

**方向**：INS 风 / 治愈系

| 元素 | 设计 |
|------|------|
| 色彩 | 低对比、柔和 |
| 动画 | 缓慢、平滑 |
| 背景 | 半透明 (0.95 alpha) |
| 图标 | 极简、扁平 |

**原因**：如果太"游戏化"，会干扰用户，不适合长期挂着

---

## 📂 文件结构

```
Assets/
├── Scenes/
│   ├── DesktopIdleGame.unity  ← Farm 场景
│   └── TimerScene.unity        ← Timer 场景
├── Scripts/
│   ├── Systems/
│   │   ├── FocusSystem.cs            ← 番茄钟核心逻辑
│   │   ├── WindowManager.cs          ← Farm 窗口管理
│   │   ├── TimerWindowManager.cs     ← Timer 窗口管理
│   │   ├── SharedState.cs            ← 跨进程通信
│   │   ├── TimerStateWriter.cs       ← Timer 写状态
│   │   ├── FarmStateReader.cs        ← Farm 读状态
│   │   ├── FocusEventBridge.cs       ← 事件桥接
│   │   ├── FarmBoostReceiver.cs      ← 农场加速接收器
│   │   └── GameEventBus.cs           ← 事件总线
│   └── UI/
│       ├── PomodoroTimer.cs          ← Timer UI 控制器
│       └── ...
└── Editor/
    ├── CreateTimerScene.cs     ← 自动生成 Timer 场景
    └── AppLauncher.cs          ← 一键启动双 App
```

---

## ⚠️ 常见坑

| 坑 | 为什么错 | 正确做法 |
|----|----------|----------|
| ❌ 想用一个 Unity 做多窗口 | 会卡死你 | 两个独立 exe |
| ❌ 做成"全屏游戏" | 违背产品定位 | 桌面挂件 |
| ❌ 过度交互 | 用户会关掉 | 弱交互 + 自动化 |
| ❌ 强耦合 | 后期扩展困难 | 松耦合 + 事件驱动 |

---

## 🧾 一句话总结

👉 **你要做的不是：两个 Unity 窗口**

👉 **而是：一个桌面常驻的"轻量游戏生态"，用多进程 + 弱耦合实现**

---

## 📝 下一步

### 立即可做：

1. ✅ 运行 `CreateTimerScene` 生成独立场景
2. ✅ 构建两个 App
3. ✅ 测试双进程通信
4. ✅ 验证奖励闭环

### 未来扩展：

- 🎵 音乐播放器模块
- 📝 To-do List 模块
- 📊 数据统计面板
- 🐱 桌面宠物

---

## 🔥 产品级思考

**核心卖点**：生产力游戏化

类似：
- Forest（专注种树）
- Rusty's Retirement（挂机农田）

**你的优势**：
- 多模块生态（不只是单一功能）
- 弱交互设计（适合长期挂着）
- 视觉治愈（INS 风格）

---

📅 Created: 2026-04-20  
🏷 Version: 1.0  
👨‍💻 Author: AI Architecture Review
