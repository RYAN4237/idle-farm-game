// PomodoroTimer 组件使用示例
// 这是一个简化的配置指南，帮助你快速设置番茄钟UI

/*
═══════════════════════════════════════════════════════════════
  快速设置指南
═══════════════════════════════════════════════════════════════

1. 创建GameObject层级结构：

PomodoroTimerUI (Canvas或父物体下的GameObject)
├── Background (Image - 可选背景)
├── TimerText (TextMeshProUGUI)
│   └── 设置字体大小: 48
│   └── 对齐方式: Center
│   └── 文本: "25:00"
│
├── PhaseLabel (TextMeshProUGUI)  
│   └── 设置字体大小: 24
│   └── 对齐方式: Center
│   └── 文本: "专注"
│
├── ProgressRing (Image)
│   └── Image Type: Filled
│   └── Fill Method: Radial 360
│   └── Fill Origin: Top
│   └── Fill Amount: 1
│   └── Clockwise: ✓
│
├── ActionButton (Button)
│   ├── 添加过渡效果 (Transition: Color Tint)
│   └── ButtonText (TextMeshProUGUI子物体)
│       └── 文本: "开始专注"
│
└── CycleDotsContainer (GameObject)
    ├── Dot1 (Image - 圆形图标)
    ├── Dot2 (Image - 圆形图标)
    ├── Dot3 (Image - 圆形图标)
    └── Dot4 (Image - 圆形图标)

═══════════════════════════════════════════════════════════════
  2. 添加 PomodoroTimer 组件
═══════════════════════════════════════════════════════════════

在根GameObject (PomodoroTimerUI) 上：
1. Add Component → PomodoroTimer

2. 在Inspector中拖拽引用：
   ┌─────────────────────────────────────────┐
   │ PomodoroTimer (Script)                  │
   ├─────────────────────────────────────────┤
   │ UI References                           │
   │   Timer Text        → TimerText         │
   │   Phase Label Text  → PhaseLabel        │
   │   Action Button     → ActionButton      │
   │   Action Button Text→ ButtonText        │
   │   Progress Ring     → ProgressRing      │
   │   Cycle Dots                            │
   │     Size: 4                             │
   │     Element 0       → Dot1              │
   │     Element 1       → Dot2              │
   │     Element 2       → Dot3              │
   │     Element 3       → Dot4              │
   ├─────────────────────────────────────────┤
   │ Visual Settings                         │
   │   Work Color        → #1D9E75 (绿色)   │
   │   Rest Color        → #378ADD (蓝色)   │
   │   Enable Pulse Effect → ✓              │
   └─────────────────────────────────────────┘

3. 设置按钮事件：
   选中 ActionButton
   在 Button 组件的 OnClick() 事件中：
   ┌─────────────────────────────────────────┐
   │ OnClick()                               │
   │   ┌───────────────────────────────────┐ │
   │   │ PomodoroTimerUI                   │ │
   │   │ PomodoroTimer                     │ │
   │   │ OnActionButtonPressed()           │ │
   │   └───────────────────────────────────┘ │
   └─────────────────────────────────────────┘

═══════════════════════════════════════════════════════════════
  3. 推荐的UI布局 (RectTransform设置)
═══════════════════════════════════════════════════════════════

PomodoroTimerUI:
  - Anchor: Center
  - Size: 300 x 400
  - Position: (0, 0, 0)

TimerText:
  - Anchor: Top Center
  - Position: (0, -100, 0)
  - Size: 250 x 80

PhaseLabel:
  - Anchor: Top Center  
  - Position: (0, -50, 0)
  - Size: 150 x 40

ProgressRing:
  - Anchor: Center
  - Position: (0, 0, 0)
  - Size: 200 x 200

ActionButton:
  - Anchor: Bottom Center
  - Position: (0, 60, 0)
  - Size: 200 x 50

CycleDotsContainer:
  - Anchor: Bottom Center
  - Position: (0, 20, 0)
  - Horizontal Layout Group (Spacing: 10)
  
Dots (每个):
  - Size: 20 x 20
  - Image: 圆形Sprite

═══════════════════════════════════════════════════════════════
  4. 确保场景中有必需的系统
═══════════════════════════════════════════════════════════════

必需的GameObject:
├── GameManager (或类似名称)
│   └── FocusSystem.cs      ← 必需！
└── ResourceManager (或类似名称)
    └── ResourceSystem.cs   ← FocusSystem依赖

如果缺少这些系统，番茄钟将无法工作！

═══════════════════════════════════════════════════════════════
  5. 测试
═══════════════════════════════════════════════════════════════

运行游戏后：
1. 点击"开始专注"按钮 → 应该开始倒计时
2. 观察进度环 → 应该逐渐减少
3. 观察颜色 → 专注时为绿色
4. 等待计时结束 → 应该播放提示音并切换到休息状态
5. 休息状态 → 进度环应该变为蓝色
6. 完成一次专注 → 应该点亮第一个Dot

═══════════════════════════════════════════════════════════════
  6. 常见问题
═══════════════════════════════════════════════════════════════

Q: 按钮点击没反应？
A: 检查按钮的OnClick事件是否正确绑定到OnActionButtonPressed()

Q: 计时器不显示？
A: 检查TimerText是否正确分配，并且TextMeshPro导入是否正常

Q: 进度环不动？
A: 确保Image Type设为Filled，Fill Method为Radial 360

Q: 颜色不变？
A: 检查progressRing的Image组件是否分配，Color设置是否正确

Q: Console有警告？
A: 根据警告提示检查对应的UI引用是否分配

═══════════════════════════════════════════════════════════════
  7. 扩展建议
═══════════════════════════════════════════════════════════════

可选的额外功能：
- 添加重置按钮，调用 pomodoroTimer.ResetTimer()
- 添加设置按钮，打开番茄钟配置面板
- 添加音效按钮，切换音效开关
- 添加动画效果，使用Animator控制过渡
- 添加通知提示，在完成时显示Toast消息

*/