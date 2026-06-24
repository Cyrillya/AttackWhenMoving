using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Tools;

namespace AttackWhenMoving;

public static class Utils
{
    public static bool IsTool(this Tool item) => item is not MeleeWeapon weapon || weapon.isScythe();

    public static bool DidPlayerJustLeftHold()
    {
        if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
        {
            return true;
        }
        if (Game1.input.GetGamePadState().Buttons.X == ButtonState.Pressed)
        {
            return true;
        }
        if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.useToolButton))
        {
            return true;
        }
        return false;
    }
    
    public static int SpecialCooldown(MeleeWeapon who)
    {
        return who.type.Value switch
        {
            3 => MeleeWeapon.defenseCooldown, 
            1 => MeleeWeapon.daggerCooldown, 
            2 => MeleeWeapon.clubCooldown, 
            0 => MeleeWeapon.attackSwordCooldown, 
            _ => 0, 
        };
    }
}