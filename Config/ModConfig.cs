using Microsoft.Xna.Framework;

using StardewValley.BellsAndWhistles;

namespace AttackWhenMoving.Config;

internal sealed class ModConfig
{
    public bool FaceMouseWhenAttack { get; set; } = true;
    public bool WeaponAutoswing { get; set; } = true;
    public bool EnableForWeapons { get; set; } = true;
    public bool EnableForTools { get; set; } = true;
}
