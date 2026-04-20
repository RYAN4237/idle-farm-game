# 🚀 FocusFarm 快速启动指南

## 方法 1：Unity Editor 内执行（推荐 ⭐）

### 一键构建全部

1. 打开 Unity Editor
2. 等待脚本编译完成
3. 菜单栏 → **FocusFarm → 🚀 一键构建全部**
4. 等待 3-5 分钟（会自动创建场景、构建、启动）
5. 完成！

### 分步执行

如果你想分步执行：

```
菜单 → FocusFarm → 📋 仅创建场景    (创建 TimerScene.unity)
菜单 → FocusFarm → 🔨 仅构建 Apps   (构建两个 exe)
菜单 → FocusFarm → ▶ 仅启动 Apps   (启动已构建的 App)
```

---

## 方法 2：PowerShell 脚本执行

### 完整构建 + 启动

```powershell
.\build.ps1
```

这会：
1. 自动查找 Unity.exe
2. 创建 Timer 场景
3. 构建 Farm App
4. 构建 Timer App
5. 启动两个应用

### 仅启动（已构建）

```powershell
.\launch.ps1
```

### 停止所有应用

```powershell
.\stop.ps1
```

---

## 方法 3：命令行批处理（高级）

### 手动执行单个步骤

```powershell
# 查找 Unity 路径
$unity = Get-ChildItem -Path "C:\Program Files\Unity\Hub\Editor" -Filter "Unity.exe" -Recurse | Select-Object -First 1

# 1. 创建 Timer 场景
& $unity.FullName -quit -batchmode -projectPath "C:\Users\Administrator\Idle Game" -executeMethod CreateTimerScene.Execute

# 2. 构建 Farm App
& $unity.FullName -quit -batchmode -projectPath "C:\Users\Administrator\Idle Game" -executeMethod AppLauncher.BuildFarmApp

# 3. 构建 Timer App
& $unity.FullName -quit -batchmode -projectPath "C:\Users\Administrator\Idle Game" -executeMethod AppLauncher.BuildTimerApp

# 4. 启动
Start-Process "Build\FarmApp.exe"
Start-Sleep 1
Start-Process "Build\TimerApp.exe"
```

---

## 📂 构建输出

构建成功后会生成：

```
Build/
├── FarmApp.exe              ← Farm 应用（底部条）
├── FarmApp_Data/            ← Farm 数据文件
├── TimerApp.exe             ← Timer 应用（右下角）
├── TimerApp_Data/           ← Timer 数据文件
└── ...
```

---

## 🔧 故障排除

### 问题 1：Unity.exe 找不到

**解决**：编辑 `build.ps1`，手动指定 Unity 路径：

```powershell
$unityExe = "C:\Program Files\Unity\Hub\Editor\2022.3.10f1\Editor\Unity.exe"
```

### 问题 2：构建失败

**检查**：
1. Unity 版本是否兼容（推荐 2021.3+）
2. 项目是否打开
3. 查看日志：`Logs\BuildFarm.log` 或 `Logs\BuildTimer.log`

### 问题 3：场景创建失败

**检查**：
1. TextMeshPro 是否已导入
   - `Window → TextMeshPro → Import TMP Essential Resources`
2. 场景是否已存在（会覆盖）

### 问题 4：应用启动失败

**检查**：
1. exe 文件是否存在：`Build\FarmApp.exe` 和 `Build\TimerApp.exe`
2. Windows Defender 是否阻止
3. 运行权限是否足够

---

## 📊 构建时间参考

| 步骤 | 时间 |
|------|------|
| 创建场景 | ~5 秒 |
| 构建 Farm App | ~2 分钟 |
| 构建 Timer App | ~1 分钟 |
| 启动应用 | ~2 秒 |
| **总计** | **~3-4 分钟** |

---

## 🎯 推荐工作流

### 开发阶段

```
Unity Editor 内测试 → 修改代码 → Play 模式测试
```

### 发布测试

```
菜单 → FocusFarm → 🚀 一键构建全部 → 测试独立 exe
```

### 日常使用

```
.\launch.ps1  ← 快速启动
.\stop.ps1    ← 快速停止
```

---

## 📝 提示

- ✅ **首次构建**：使用 Unity Editor 方法（更直观）
- ✅ **自动化构建**：使用 `build.ps1`（CI/CD）
- ✅ **快速迭代**：在 Editor 中 Play，不需要每次构建
- ✅ **发布前**：使用 PowerShell 脚本验证独立 exe

---

📅 Last Updated: 2026-04-20  
🏷 Version: 1.0
