# 🔧 构建问题快速修复指南

## 问题：Build completed with a result of 'Unknown'

这个错误通常是因为场景文件问题或构建配置问题。

---

## ✅ 解决步骤

### 1️⃣ 运行诊断检查

在 Unity Editor 中：

```
菜单 → FocusFarm → 🔍 检查构建状态
```

这会检查：
- ✅ 场景文件是否存在
- ✅ 关键脚本是否完整
- ✅ TextMeshPro 是否安装
- ✅ 构建目录状态

查看 Console 输出，找出缺失的文件。

---

### 2️⃣ 常见问题修复

#### 问题 A：TimerScene.unity 不存在

**解决**：
```
菜单 → FocusFarm → 📋 仅创建场景
```

或者单独运行：
```
菜单 → FocusFarm → 1. Create Timer Scene
```

#### 问题 B：DesktopIdleGame.unity 不存在

**解决**：检查场景是否在其他位置

```powershell
# PowerShell 中查找场景
Get-ChildItem -Path "Assets" -Filter "*.unity" -Recurse
```

如果场景在其他位置，修改 `Assets/Editor/AppLauncher.cs`：

```csharp
// 找到这一行：
string scenePath = "Assets/Scenes/DesktopIdleGame.unity";

// 改为实际路径：
string scenePath = "Assets/DesktopIdleGame.unity"; // 或实际路径
```

#### 问题 C：TextMeshPro 未安装

**解决**：
```
Window → TextMeshPro → Import TMP Essential Resources
```

---

### 3️⃣ 手动验证场景

在 Unity Editor 中：

1. 双击打开 `Assets/Scenes/DesktopIdleGame.unity`
2. 按 `Ctrl+S` 保存
3. 双击打开 `Assets/Scenes/TimerScene.unity`（如果存在）
4. 按 `Ctrl+S` 保存

然后重新构建。

---

### 4️⃣ 清理并重建

如果上述方法都不行：

```
菜单 → File → Build Settings
  → Delete All（删除所有场景）
  → Add Open Scenes（添加当前打开的场景）
  → Close

然后：
菜单 → FocusFarm → 🚀 一键构建全部
```

---

## 🔍 详细诊断

### 检查场景路径

在 PowerShell 中运行：

```powershell
# 检查场景文件
Test-Path "Assets\Scenes\DesktopIdleGame.unity"
Test-Path "Assets\Scenes\TimerScene.unity"
```

应该都返回 `True`。

### 检查场景内容

场景文件不应该是空的或损坏的。检查文件大小：

```powershell
Get-ChildItem "Assets\Scenes\*.unity" | Select-Object Name, Length
```

如果文件 < 1KB，可能是损坏的。

---

## 🚀 推荐工作流

### 首次设置（按顺序执行）

1. **诊断检查**
   ```
   FocusFarm → 🔍 检查构建状态
   ```

2. **创建 Timer 场景**
   ```
   FocusFarm → 1. Create Timer Scene
   ```

3. **再次诊断**
   ```
   FocusFarm → 🔍 检查构建状态
   ```
   确认两个场景都存在

4. **构建**
   ```
   FocusFarm → 🔨 仅构建 Apps
   ```

5. **启动**
   ```
   FocusFarm → ▶ 仅启动 Apps
   ```

---

## 📋 检查清单

构建前确保：

- [ ] Unity 版本 >= 2021.3
- [ ] TextMeshPro 已导入
- [ ] `Assets/Scenes/DesktopIdleGame.unity` 存在
- [ ] `Assets/Scenes/TimerScene.unity` 存在
- [ ] 所有脚本编译无错误
- [ ] Build 目录可写

---

## 🆘 还是不行？

### 导出详细日志

在 Unity Console 中：

1. 右上角点击 `☰` 菜单
2. 选择 `Open Editor Log`
3. 搜索 "Build"
4. 复制错误信息

### 手动构建测试

```
菜单 → File → Build Settings
  → Add Open Scenes（添加当前场景）
  → Platform: PC, Mac & Linux Standalone
  → Target Platform: Windows
  → Architecture: x86_64
  → Build（选择输出路径）
```

如果这个能成功，说明是脚本问题。
如果这个也失败，说明是项目配置问题。

---

## 🎯 快速测试

最简单的测试方法：

1. 打开 `DesktopIdleGame.unity`
2. 按 `Ctrl+Shift+B`（打开 Build Settings）
3. 点击 `Build`
4. 选择输出路径（如 `Build/Test.exe`）
5. 等待构建完成

如果成功，说明场景本身没问题，是构建脚本的问题。

---

📅 Last Updated: 2026-04-20
