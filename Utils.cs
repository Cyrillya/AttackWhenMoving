using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace AttackWhenMoving;

public static class Utils
{
    public static bool MouseOnMenu() {
        Game1.PushUIMode();
        if (Game1.onScreenMenus
            .Where(menu => Game1.IsHudDrawn || menu == Game1.chatBox)
            .Any(menu =>
                (!Game1.IsChatting || menu == Game1.chatBox) &&
                menu is not LevelUpMenu { informationUp: false } &&
                menu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) ||
                menu == Game1.chatBox && Game1.options.gamepadControls && Game1.IsChatting)) {
            Game1.PopUIMode();
            return true;
        }

        Game1.PopUIMode();
        return false;
    }

    public static bool AllowMoving(this Tool item) {
        if (item is FishingRod) return false;
        if (!IsTool(item) && ModEntry.Config.EnableForWeapons) return true;
        if (IsTool(item) && ModEntry.Config.EnableForTools) return true;
        return false;
    }

    public static bool IsTool(this Tool item) => item is not MeleeWeapon weapon || weapon.isScythe();

    public static bool DidPlayerJustLeftHold() {
        if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed) {
            return true;
        }

        if (Game1.input.GetGamePadState().Buttons.X == ButtonState.Pressed) {
            return true;
        }

        if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.useToolButton)) {
            return true;
        }

        return false;
    }

    public static int SpecialCooldown(MeleeWeapon who) {
        return who.type.Value switch {
            3 => MeleeWeapon.defenseCooldown,
            1 => MeleeWeapon.daggerCooldown,
            2 => MeleeWeapon.clubCooldown,
            0 => MeleeWeapon.attackSwordCooldown,
            _ => 0,
        };
    }
}