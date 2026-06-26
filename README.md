<h1 align="center">Combat QoL</h1>

<div align="center">

English | [简体中文](README.zh.md)

A mod that improves combat experience in Stardew Valley by increasing player mobility while using weapons or tools

Inspired by Terraria

</div>

## ❤️ Key Features

- Allow movement while using weapons or tools (can be enabled/disabled separately for weapons and tools)
- Face the mouse cursor while attacking (also supports gamepad right-stick aiming)
- Weapon autoswing support (when enabled, weapons will continue swinging while the use key is held)
- **Controller right-stick support** — three modes: Mouse Aim, Directional Attack, Edge Trigger
- **Cancel special attacks** (e.g. sword special moves) when movement is triggered

<img width="286" height="302" alt="Image" src="https://github.com/user-attachments/assets/c63e95ee-ef36-48a2-9a85-9d1a17838778" />
<img width="322" height="300" alt="Image" src="https://github.com/user-attachments/assets/6f145345-1f6b-4b50-8b96-45a6796c25cf" />

## 💻 Installation

1. Extract the downloaded zip into the Stardew Valley `Mods` folder
2. Start the game and ensure SMAPI loads the mod

## ⚙️ Configuration

The config is located at `AttackWhenMoving/config.json`. It is recommended to use Generic Mod Config Menu (GMCM) for in-game configuration. Available options:

### Common Settings

| Option                     | Type | Default | Description                                                                       |
|----------------------------|------|---------|-----------------------------------------------------------------------------------|
| `FaceMouseWhenAttack`      | bool | true    | Face the mouse or right-stick direction when attacking                            |
| `WeaponAutoswing`          | bool | true    | Enable weapon autoswing (hold use key to keep swinging)                           |
| `EnableForWeapons`         | bool | true    | Allow movement while using weapons                                                |
| `EnableForTools`           | bool | true    | Allow movement while using tools                                                  |
| `SpecialAttackCancellable` | bool | true    | Cancel special attacks (e.g. sword special moves) when normal attack is triggered |

### Controller Settings

| Option                     | Type   | Default | Description                                                                                                                                 |
|----------------------------|--------|---------|---------------------------------------------------------------------------------------------------------------------------------------------|
| `ControllerAttackMode`     | string | `edge`  | Right stick control logic: `mouse` (simulate mouse), `direction` (independent aim, press use to attack), `edge` (auto-attack at stick edge) |
| `ControllerAimDeadZone`    | float  | 0.0     | Right-stick dead zone for facing direction (prevents drift from constant re-aiming)                                                         |
| `ControllerAttackDeadZone` | float  | 0.9     | Right-stick threshold for triggering edge-trigger attack                                                                                    |

## 📖 License
This project is licensed under the MIT License. See the `LICENSE` file for details
