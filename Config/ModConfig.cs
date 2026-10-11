using Microsoft.Xna.Framework;

using StardewValley.BellsAndWhistles;

namespace AttackWhenMoving.Config;

internal sealed class ModConfig
{
    public bool FaceMouseWhenAttack { get; set; } = true;
    public bool WeaponAutoswing { get; set; } = true;
    public bool SpecialAttackCancellable { get; set; } = true;
    public bool KeyboardDontAim { get; set; } = true;
    public bool EnableForWeapons { get; set; } = true;
    public bool EnableForSlingshot { get; set; } = true;
    public bool EnableForTools { get; set; } = true;

    /// <summary>
    /// Options: mouse, direction, edge
    /// </summary>
    public string ControllerAttackMode { get; set; } = "edge";
    public float ControllerAimDeadZone { get; set; } = 0f;
    public float ControllerAttackDeadZone { get; set; } = 0.9f;
    public bool ControllerSlingshot { get; set; } = true;

    /// <summary>
    /// Whether pushing the right stick to the edge draws the slingshot and letting it snap back to
    /// the center fires it. Only used when <see cref="ControllerSlingshot"/> is enabled.
    /// </summary>
    public bool ControllerSlingshotFlickFire { get; set; } = true;

    /// <summary>
    /// How slow a return of the right stick to the center may be and still count as a release that
    /// fires the slingshot, from 0 (only a real snap-back counts) to 1 (even a very slow push back
    /// counts). Only used when <see cref="ControllerSlingshot"/> and
    /// <see cref="ControllerSlingshotFlickFire"/> are enabled.
    /// </summary>
    public float ControllerSlingshotFlickSensitivity { get; set; } = 0.4f;
}
