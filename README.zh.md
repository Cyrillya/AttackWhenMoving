<h1 align="center">战斗优化</h1>

<div align="center">

[English](README.md) | 简体中文

通过提升玩家的机动性来改善星露谷战斗体验

灵感来自 Terraria

</div>

## ❤️ 主要功能

- 在使用武器、弹弓或工具时**允许角色移动**（可分别为武器、弹弓和工具启用/禁用）
- 在攻击时让角色**面向鼠标方向**（也支持手柄**右摇杆瞄准**）
- 支持武器**自动挥动**（如启用，会在按住左键/使用键期间持续挥动）
- **手柄右摇杆支持** —— 三种模式：模拟鼠标、定向攻击、边缘触发
- 武器特殊攻击可被普攻**取消**

<img width="286" height="302" alt="Image" src="https://github.com/user-attachments/assets/c63e95ee-ef36-48a2-9a85-9d1a17838778" />
<img width="322" height="300" alt="Image" src="https://github.com/user-attachments/assets/6f145345-1f6b-4b50-8b96-45a6796c25cf" />

## 💻 安装

1. 将下载的 zip 解压到 Stardew Valley 的 Mods 目录
2. 启动游戏，确保 SMAPI 已加载 Mod

## ⚙️ 配置
配置项位于 `AttackWhenMoving/config.json`，建议搭配 Generic Mod Config Menu (GMCM) (通用模组配置菜单) 模组修改，支持以下选项：

### 通用设置

| 选项                         | 类型   | 默认值  | 说明                   |
|----------------------------|------|------|----------------------|
| `FaceMouseWhenAttack`      | bool | true | 在攻击时让角色面向鼠标/摇杆方向     |
| `WeaponAutoswing`          | bool | true | 启用武器自动挥动，按住左键持续攻击    |
| `EnableForWeapons`         | bool | true | 使用武器时允许移动            |
| `EnableForSlingshot`       | bool | true | 使用弹弓时允许移动            |
| `EnableForTools`           | bool | true | 使用工具时允许移动            |
| `SpecialAttackCancellable` | bool | true | 普攻时自动中断特殊攻击（如剑的特殊技能） |
| `KeyboardDontAim`          | bool | true | 使用键盘控制攻击时角色不会转向      |

### 手柄设置

| 选项                         | 类型     | 默认值    | 说明                                                                |
|----------------------------|--------|--------|-------------------------------------------------------------------|
| `ControllerAttackMode`     | string | `edge` | 右摇杆攻击控制逻辑：`mouse`（模拟鼠标）、`direction`（独立瞄准，按使用键攻击）、`edge`（推至边缘自动攻击） |
| `ControllerAimDeadZone`    | float  | 0.0    | 右摇杆朝向死区，防止轻微偏移导致频繁转向                                              |
| `ControllerAttackDeadZone` | float  | 0.9    | 右摇杆攻击触发阈值，防止边缘触发模式误触                                              |
| `ControllerSlingshot`      | bool   | true   | 弹弓用右摇杆瞄准                                                          |

## 📖 许可证
本项目采用 MIT 许可证，详见 `LICENSE` 文件
