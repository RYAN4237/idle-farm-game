# 🌾⏱ 双独立 App 架构说明

## 🎯 你遇到的问题

**现状**：Timer 和 Farm 在同一个场景 (DesktopIdleGame.unity)  
**目标**：两个完全独立的 App (FarmApp.exe + TimerApp.exe)

---

## ✅ 解决方案：场景分离

### 两个独立场景

| 场景 | 用途 | 窗口 | 内容 |
|------|------|------|------|
| **DesktopIdleGame.unity** | Farm App | 底部全宽条 (120px高) | 农田、商店、资源系统 |
| **TimerScene.unity** | Timer App | 右下角卡片 (220x300) | 番茄钟、专注计时 |

### 通信方式

```
Timer App                     Farm App
    ↓                           ↓
FocusSystem                 FarmStateReader
    ↓                           ↓
TimerStateWriter  ─JSON─→   读取状态
    ↓                           ↓
SharedState.json            应用加速效果
(%AppData%/FocusFarm/)
```

---

## 🚀 实施步骤

### 步骤 1：创建 Timer 场景

在 Unity Editor 中：

```
菜单 → FocusFarm → 1. Create Timer Scene
```

这会创建 `Assets/Scenes/TimerScene.unity`，包含：
- ✅ 独立的 Canvas (220x300 固定尺寸)
- ✅ Timer UI (时钟、按钮、阶段标签)
- ✅ FocusSystem (番茄钟逻辑)
- ✅ TimerWindowManager (窗口定位)
- ✅ TimerStateWriter (写入状态文件)

### 步骤 2：验证两个场景

运行诊断：

```
菜单 → FocusFarm → 🔍 检查构建状态
```

应该看到：
```
✅ Farm App 场景存在: Assets/Scenes/DesktopIdleGame.unity
✅ Timer App 场景存在: Assets/Scenes/TimerScene.unity
```

### 步骤 3：构建两个 App

```
菜单 → FocusFarm → 🔨 仅构建 Apps
```

这会生成：
- `Build/FarmApp.exe` - 农场应用
- `Build/TimerApp.exe` - 计时器应用

### 步骤 4：启动双 App

```
菜单 → FocusFarm → ▶ 仅启动 Apps
```

或使用 PowerShell：
```powershell
.\launch.ps1
```

---

## 🖼️ 最终效果

运行后你会看到：

```
┌─────────────────────────────────────────────┐
│                  桌面                          │
│                                               │
│                                               │
│                                    ┌────────┐│
│                                    │ Timer  ││
│                                    │ 25:00  ││
│                                    │ ●专注  ││
│                                    │[开始]  ││
│                                    └────────┘│
└───────────────────────────────────────────────┘
   └─── Farm 农田条（全宽 120px 高）────┘
```

**两个独立窗口**：
- Farm：屏幕底部，全宽，120px 高
- Timer：右下角浮动，220x300px，可拖动

---

## 🔗 通信机制

### Timer 写入状态

```csharp
// TimerStateWriter.cs (每 0.5s 执行)
var state = new FocusFarmState
{
    isRunning = true,
    timeRemaining = 1500f, // 25 分钟
    completedCycles = 2,
    ...
};
SharedState.Write(state); // → %AppData%/FocusFarm/state.json
```

### Farm 读取状态

```csharp
// FarmStateReader.cs (每 1s 执行)
var state = SharedState.Read(); // ← 读取 JSON
if (state != null && state.isRunning)
{
    // Timer 正在运行，显示连接状态
}
```

### 事件联动

```
Timer 完成 25 分钟专注
    ↓
TimerStateWriter 写入 completedCycles++
    ↓
FarmStateReader 检测到新周期
    ↓
触发 FarmBoostReceiver.ApplyBoost(2x, 300s)
    ↓
农田生长速度 x2，持续 5 分钟
```

---

## ⚠️ 关键设计差异

### ❌ 之前（单场景）

```
DesktopIdleGame.unity
├── Farm 组件
├── Timer 组件  ← 在同一个窗口
└── 共享 GameObject
```

问题：
- Timer 和 Farm 混在一起
- 无法独立启动 Timer
- 耦合度高

### ✅ 现在（双场景）

```
DesktopIdleGame.unity      TimerScene.unity
├── Farm 组件               ├── Timer 组件
├── WindowManager           ├── TimerWindowManager
└── FarmStateReader         └── TimerStateWriter
         ↓                           ↓
         └──── SharedState.json ────┘
```

优势：
- ✅ 完全独立的两个 App
- ✅ 松耦合（JSON 文件通信）
- ✅ Timer 可单独运行
- ✅ 各自崩溃不影响对方

---

## 🎨 窗口行为

### Farm App (WindowManager.cs)

```csharp
public bool isFarmWindow = true;
public int farmHeight = 120;

// 自动定位到屏幕底部
int screenH = GetSystemMetrics(1);
SetWindowPos(hwnd, TOPMOST, 
    0, screenH - 120,  // 底部
    screenW, 120,      // 全宽 x 120px
    ...);
```

### Timer App (TimerWindowManager.cs)

```csharp
public int windowWidth = 220;
public int windowHeight = 300;
public int marginRight = 20;
public int marginBottom = 140; // 避开 Farm 条

// 定位到右下角
int x = screenW - 220 - 20;
int y = screenH - 300 - 140;
SetWindowPos(hwnd, TOPMOST, x, y, 220, 300, ...);
```

---

## 📊 验证清单

构建前确认：

- [ ] Timer 场景已创建
  ```
  FocusFarm → 1. Create Timer Scene
  ```
  
- [ ] 两个场景都存在
  ```
  Assets/Scenes/DesktopIdleGame.unity ✅
  Assets/Scenes/TimerScene.unity ✅
  ```

- [ ] 运行诊断无错误
  ```
  FocusFarm → 🔍 检查构建状态
  ```

- [ ] 构建成功
  ```
  Build/FarmApp.exe ✅
  Build/TimerApp.exe ✅
  ```

---

## 🚀 下一步

1. **创建 Timer 场景**
   ```
   菜单 → FocusFarm → 1. Create Timer Scene
   ```

2. **验证场景**
   - 在 Project 窗口查看 `Assets/Scenes/`
   - 应该看到两个 .unity 文件

3. **构建并测试**
   ```
   菜单 → FocusFarm → 🚀 一键构建全部
   ```

4. **验证效果**
   - Farm 在屏幕底部
   - Timer 在右下角浮动
   - 专注完成后 Farm 加速

---

📅 Last Updated: 2026-04-20  
🏷 Version: 2.0 - 真正的双独立 App 架构
