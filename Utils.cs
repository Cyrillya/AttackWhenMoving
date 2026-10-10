using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace AttackWhenMoving;

public static class Utils
{
    public static void CancelSpecialAttack(Farmer who) {
        if (who.CurrentTool is not MeleeWeapon { isOnSpecial: true } weapon) return;
        
        weapon.isOnSpecial = false;
        who.UsingTool = false;
        who.CanMove = true;
        who.FarmerSprite.PauseForSingleAnimation = false;
        who.usingSlingshot = false;
        who.canReleaseTool = false;
        
        who.Halt();
        who.Sprite.StopAnimation();
    }

    public static void ForceHideMouseInGamepadMode() {
        if (!Game1.options.gamepadControls)
            return;
        Game1.mouseCursorTransparency = 0f;
        Game1.timerUntilMouseFade = 0;
        Game1.lastCursorMotionWasMouse = false;
    }

    public static void UseWeapon(MeleeWeapon? weapon) {
        var who = Game1.player;
        if (weapon != null && weapon.isScythe() && !who.UsingTool)
            who.BeginUsingTool();
        else
            who.FireTool();

        // 特判，因为我没招了！
        var currentPadState = Game1.input.GetGamePadState();
        var stick = currentPadState.ThumbSticks.Right;
        var config = ModEntry.Config;
        if (config.ControllerAttackMode is "edge" && stick.Length() > config.ControllerAttackDeadZone &&
            !who.UsingTool && !weapon.isScythe()) {
            who.completelyStopAnimatingOrDoingAction();
            who.CanMove = false;
            who.UsingTool = true;
            who.canReleaseTool = true;
            weapon.setFarmerAnimating(who);
        }
    }

    /// <summary>Cancel a swing the player attempted while sitting on furniture, and stand them up
    /// instead.</summary>
    /// <remarks>
    /// The game never uses a tool while the player is seated: <c>Game1.pressUseToolButton</c> only
    /// checks <c>Game1.player.IsSitting()</c> while <c>Farmer.UsingTool</c> is false, and makes the
    /// player stand up instead of using the tool. A swing started from here sets UsingTool, so that
    /// check never runs again: the player can't get up, keeps re-swinging through the autoswing
    /// path, and stays at <c>CanMove == false</c> — which also blocks the pause menu
    /// (<c>Game1.CanShowPauseMenu</c>), leaving force-quitting the game as the only way out.
    /// Cancel the swing and get up so a chair behaves the way it does in vanilla.
    /// </remarks>
    public static void CancelSwingWhileSitting(Farmer who) {
        if (who.UsingTool) who.completelyStopAnimatingOrDoingAction();
        // Don't restart the stand-up animation while it is already playing; the player only
        // finishes getting up once the lerp is done (Farmer.Update clears isStopSitting then).
        if (who.IsSitting() && !who.isStopSitting) who.StopSitting();
    }

    public static bool MouseOnMenu() {
        if (Game1.activeClickableMenu != null) return true;

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
        switch (item) {
            case FishingRod:
                return false;
            case Slingshot:
                return ModEntry.Config.EnableForSlingshot;
        }

        if (!IsTool(item)) return ModEntry.Config.EnableForWeapons;
        else return ModEntry.Config.EnableForTools;
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

        var currentPadState = Game1.input.GetGamePadState();
        var stick = currentPadState.ThumbSticks.Right;
        var config = ModEntry.Config;
        if (config.ControllerAttackMode is "edge" && stick.Length() > config.ControllerAttackDeadZone) {
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