# 🚀 快速开始指南（手动操作最可靠）

## ⚡ 推荐方式：在 Unity Editor 中操作

由于 Unity 批处理模式可能遇到编译问题，**最可靠的方式是在 Unity Editor 中直接点击菜单**。

---

## 📋 步骤（3 分钟完成）

### 步骤 1：打开 Unity Editor

1. 打开 Unity Hub
2. 打开项目 "Idle Game"
3. 等待编译完成（查看右下角进度条）

---

### 步骤 2：创建 Timer 场景

在 Unity 菜单栏点击：

```
FocusFarm → 1. Create Timer Scene
```

**你会看到**：
- Console 窗口显示创建进度
- 场景自动切换到 TimerScene
- Project 窗口出现 `Assets/Scenes/TimerScene.unity`

**如果提示场景已存在**：
- 点击 "Yes, 覆盖" 重新创建

---

### 步骤 3：检查构建状态

菜单栏点击：

```
FocusFarm → 🔍 检查构建状态
```

**应该看到**：
```
✅ Farm App 场景存在: Assets/Scenes/DesktopIdleGame.unity
✅ Timer App 场景存在: Assets/Scenes/TimerScene.unity
✅ TextMeshPro 已安装
```

---

### 步骤 4A：一键构建全部（推荐）

菜单栏点击：

```
FocusFarm → 🚀 一键构建全部
```

这会：
1. 创建场景（如果不存在）
2. 构建 Farm App
3. 构建 Timer App
4. 自动启动两个应用

**等待时间**：3-5 分钟

---

### 步骤 4B：分步执行（可选）

如果你想分步执行：

```
1. FocusFarm → 🔨 仅构建 Apps
   （等待构建完成，约 3-4 分钟）

2. FocusFarm → ▶ 仅启动 Apps
   （启动已构建的应用）
```

---

## ✅ 验证成功

运行后你应该看到：

### Farm App
- 位置：**屏幕底部**
- 样式：全宽条，约 120px 高
- 内容：农田、商店、资源显示

### Timer App
- 位置：**右下角**
- 样式：220x300px 卡片，可拖动
- 内容：25:00 计时器 + "开始专注" 按钮

### 测试联动
1. 在 Timer App 点击 "开始专注"
2. 等待 25 分钟（或修改时长）
3. 完成后 Farm App 应该显示加速效果（x2）

---

## 🔧 故障排除

### Q1: 菜单中没有 "FocusFarm"？

**A**: 脚本编译失败

1. 查看 Console 窗口是否有红色错误
2. 检查 `Assets/Editor/` 文件夹是否存在
3. 重新导入项目：`Assets → Reimport All`

---

### Q2: "Create Timer Scene" 点击没反应？

**A**: 查看 Console 窗口的错误信息

1. 按 `Ctrl+Shift+C` 打开 Console
2. 查看红色错误
3. 常见问题：
   - TextMeshPro 未安装 → `Window → TextMeshPro → Import TMP Essential Resources`
   - 编译错误 → 修复代码后重试

---

### Q3: 构建时间太长？

**A**: 正常，首次构建需要 3-5 分钟

- Farm App: ~2 分钟
- Timer App: ~1 分钟
- 后续构建会更快（增量构建）

---

### Q4: 应用启动后看不到窗口？

**A**: 检查窗口位置

- Farm: 检查屏幕**最底部**（可能被任务栏挡住）
- Timer: 检查右下角（可能在屏幕外）
- 尝试按 `Alt+Tab` 切换窗口

---

## 🎨 自定义设置

### 修改 Timer 窗口位置

编辑 `Assets/Scripts/Systems/TimerWindowManager.cs`：

```csharp
public int marginRight = 20;   // 距离右边缘
public int marginBottom = 140; // 距离底部
```

### 修改专注时长

编辑 `Assets/Scripts/Systems/FocusSystem.cs`：

```csharp
public float focusDurationMinutes = 25f; // 专注时长
public float restDurationMinutes = 5f;   // 休息时长
```

或者在 Unity Inspector 中修改 `TimerSystems` 对象的参数。

---

## 📚 更多文档

- `ARCHITECTURE.md` - 完整架构设计
- `DUAL_APP_ARCHITECTURE.md` - 双 App 架构说明
- `BUILD_TROUBLESHOOTING.md` - 构建问题排查
- `TODO.md` - 开发计划

---

## 💡 提示

- ✅ **首次使用**：按步骤 1→2→3→4A 操作
- ✅ **日常使用**：直接运行 `.\launch.ps1` 启动
- ✅ **修改代码后**：运行 "🔨 仅构建 Apps" 重新构建
- ✅ **调试 UI**：在 Unity Editor 中 Play 模式测试

---

📅 Last Updated: 2026-04-20  
🏷 Version: 1.0
