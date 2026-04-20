# ✅ FocusFarm 实施清单

## 🎯 Phase 1: 基础架构（已完成 ✅）

- [x] 双窗口系统
  - [x] `WindowManager.cs` (Farm 底部条)
  - [x] `TimerWindowManager.cs` (Timer 右下角)
- [x] 跨进程通信
  - [x] `SharedState.cs` (JSON 文件)
  - [x] `TimerStateWriter.cs` (Timer 写)
  - [x] `FarmStateReader.cs` (Farm 读)
- [x] 事件系统
  - [x] `FocusSystem.cs` (番茄钟核心)
  - [x] `GameEventBus.cs` (事件总线)
  - [x] `FocusEventBridge.cs` (桥接器)
- [x] 奖励系统
  - [x] `FarmBoostReceiver.cs` (农场加速)

---

## 🚀 Phase 2: 场景分离（进行中 ⏳）

### 2.1 创建 Timer 场景

```
✅ 已创建：Assets/Editor/CreateTimerScene.cs

执行步骤：
1. Unity 菜单 → FocusFarm → 1. Create Timer Scene
2. 检查生成的场景：Assets/Scenes/TimerScene.unity
3. 验证组件：
   - Main Camera (transparent bg)
   - TimerCanvas
   - Systems (FocusSystem + TimerWindowManager)
```

### 2.2 构建独立 App

```
✅ 已创建：Assets/Editor/AppLauncher.cs

执行步骤：
1. Unity 菜单 → FocusFarm → 2. Build Farm App
   → 生成：Build/FarmApp.exe
   
2. Unity 菜单 → FocusFarm → 3. Build Timer App
   → 生成：Build/TimerApp.exe
```

### 2.3 测试双进程

```
执行步骤：
1. Unity 菜单 → FocusFarm → ▶ Launch Both Apps
2. 检查：
   - Farm 在屏幕底部（全宽条）
   - Timer 在右下角（220x300 卡片）
   - Timer 可拖动
   - 专注完成后 Farm 显示加速效果
```

---

## 🎨 Phase 3: 视觉优化（待做 📋）

### 3.1 Timer UI 美化

- [ ] 更换背景图（治愈系风格）
- [ ] 添加进度环动画
- [ ] 完成时粒子特效
- [ ] 声音反馈（可选）

**实施文件**：
- `Assets/Scripts/UI/PomodoroTimer.cs`
- 添加 UI 资源到 `Assets/Textures/`

### 3.2 Farm UI 治愈化

- [ ] 柔和色彩
- [ ] 平滑动画（生长/收获）
- [ ] 加速特效（金色光晕）
- [ ] 稀有植物（专注奖励）

**实施文件**：
- `Assets/Scripts/Systems/FarmPlot.cs`
- `Assets/Scripts/Systems/FarmBoostReceiver.cs`

### 3.3 字体 + 图标

- [ ] 选择治愈系字体（推荐：思源黑体 / Noto Sans）
- [ ] 设计极简图标
- [ ] 统一色板（参考 INS 配色）

---

## ⚡ Phase 4: 性能优化（待做 📋）

### 4.1 帧率限制

```csharp
// Assets/Scripts/Systems/AmbientPerformance.cs (已存在？)

void Awake()
{
    Application.targetFrameRate = 15; // 桌面挂件不需要 60 FPS
    QualitySettings.vSyncCount = 0;
}
```

### 4.2 减少 Update 调用

- [ ] Timer 轮询改为事件驱动
- [ ] Farm 读取状态降低频率（1s → 2s）
- [ ] UI 更新仅在数值改变时触发

**优化文件**：
- `Assets/Scripts/Systems/FarmStateReader.cs`
- `Assets/Scripts/UI/PomodoroTimer.cs`

### 4.3 内存优化

- [ ] 对象池（粒子特效）
- [ ] 图集合并（减少 Draw Call）
- [ ] 卸载未使用资源

---

## 🔗 Phase 5: 联动增强（待做 📋）

### 5.1 丰富奖励闭环

| Timer 事件 | Farm 反馈 |
|-----------|----------|
| 开始专注 | 农田微光闪烁 |
| 完成专注 | x2 加速 + 金色特效 |
| 连续 3 次 | 稀有植物出现 |
| 暂停 | 生长速度 -50% |

**实施文件**：
- `Assets/Scripts/Systems/FocusEventBridge.cs`（添加事件）
- `Assets/Scripts/Systems/FarmBoostReceiver.cs`（添加特效）

### 5.2 数据持久化

- [ ] 保存专注历史
- [ ] 成就系统（连续 7 天、总时长等）
- [ ] 统计面板（可选）

**实施文件**：
- `Assets/Scripts/Systems/SaveSystem.cs`（已存在）
- 扩展 `SharedState.cs`

---

## 🌟 Phase 6: 未来扩展（规划 🗺️）

### 6.1 新模块

- [ ] 音乐播放器（放松音乐 + 白噪音）
- [ ] To-do List（任务管理）
- [ ] 桌面宠物（互动元素）

### 6.2 社交功能

- [ ] 好友排行榜
- [ ] 专注挑战赛
- [ ] 分享成就到社交媒体

---

## 📊 当前进度

```
[████████████░░░░░░░░] 60% 完成

✅ Phase 1: 基础架构 (100%)
⏳ Phase 2: 场景分离 (50%)
📋 Phase 3: 视觉优化 (0%)
📋 Phase 4: 性能优化 (0%)
📋 Phase 5: 联动增强 (20%)
🗺️ Phase 6: 未来扩展 (0%)
```

---

## 🎯 下一步行动

### 立即执行（今天）

1. ✅ Unity → FocusFarm → **1. Create Timer Scene**
2. ✅ Unity → FocusFarm → **2. Build Farm App**
3. ✅ Unity → FocusFarm → **3. Build Timer App**
4. ✅ Unity → FocusFarm → **▶ Launch Both Apps**
5. ✅ 测试专注完成 → 农场加速

### 本周计划

- [ ] 优化 Timer UI（更美观）
- [ ] 添加完成特效（粒子 + 声音）
- [ ] 测试长时间运行稳定性
- [ ] 记录 bug 和优化点

### 下周计划

- [ ] 视觉风格统一
- [ ] 性能优化（降低 CPU 占用）
- [ ] 丰富奖励机制
- [ ] 准备测试版本

---

## 🔧 常见问题

### Q1: Timer 场景创建失败？

**A**: 检查是否已安装 TextMeshPro
```
Window → TextMeshPro → Import TMP Essential Resources
```

### Q2: 构建后窗口位置不对？

**A**: 检查屏幕分辨率设置
```csharp
// TimerWindowManager.cs
public int marginBottom = 140; // 根据实际 Farm 高度调整
```

### Q3: 双进程通信失败？

**A**: 检查 SharedState 文件路径
```
位置：%AppData%/FocusFarm/state.json
权限：确保可读写
```

### Q4: Farm 没有加速效果？

**A**: 检查事件连接
```
1. FocusEventBridge 是否挂载到场景
2. GameEventBus 事件是否正常触发
3. FarmBoostReceiver.Instance 是否存在
```

---

📅 Last Updated: 2026-04-20  
🏷 Version: 1.0
