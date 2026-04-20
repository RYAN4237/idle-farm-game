# 🌾⏱ FocusFarm - 快速开始

## ⚡ 最快方式（1 步完成）

### 在 Unity Editor 中：

打开项目后，菜单栏点击：

```
FocusFarm → 🚀 一键构建全部
```

等待 3-4 分钟，完成后会自动启动两个应用！

---

## 📋 你创建的所有工具

### Unity Editor 菜单

| 菜单项 | 功能 |
|--------|------|
| 🚀 一键构建全部 | 创建场景 + 构建 + 启动（全自动） |
| 📋 仅创建场景 | 创建 TimerScene.unity |
| 🔨 仅构建 Apps | 构建两个 exe |
| ▶ 仅启动 Apps | 启动已构建的应用 |

### PowerShell 脚本

| 脚本 | 功能 |
|------|------|
| `build.ps1` | 完整构建流程（命令行模式） |
| `launch.ps1` | 快速启动已构建的 App |
| `stop.ps1` | 停止所有运行中的 App |

---

## 🎯 使用场景

### 场景 1：第一次使用

```
Unity Editor → FocusFarm → 🚀 一键构建全部
```

### 场景 2：修改代码后重新构建

```
Unity Editor → FocusFarm → 🔨 仅构建 Apps
```

### 场景 3：快速启动（已构建）

```
PowerShell → .\launch.ps1
```

### 场景 4：关闭应用

```
PowerShell → .\stop.ps1
```

或直接关闭窗口

---

## 📦 构建产物

```
Build/
├── FarmApp.exe       ← 农场应用（屏幕底部全宽条）
├── TimerApp.exe      ← 计时器应用（右下角 220x300px）
└── ...数据文件
```

---

## ✅ 验证成功

运行后你应该看到：

1. **Farm App**：屏幕底部的全宽条
2. **Timer App**：右下角的小卡片（可拖动）
3. 专注 25 分钟完成后，Farm 会加速 x2

---

## 🔧 系统信息

- Unity 版本：`6000.4.1f1`
- Unity 路径：`C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe`
- 项目路径：`C:\Users\Administrator\Idle Game`

---

## 📚 更多文档

- `ARCHITECTURE.md` - 完整架构设计文档
- `TODO.md` - 详细实施清单
- `BUILD_GUIDE.md` - 构建指南（故障排除）

---

**开始体验吧！🚀**
