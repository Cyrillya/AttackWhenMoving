using AttackWhenMoving.Config;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace AttackWhenMoving;

public class ModEntry : Mod
{
    internal static ModConfig Config;
    internal static ModEntry Instance;

    private static bool _oldCanMove;
    private static bool _oldUseTool;

    public override void Entry(IModHelper helper) {
        Instance = this;
        Config = Helper.ReadConfig<ModConfig>();
        Localization.Init(helper.Translation);

        helper.Events.GameLoop.GameLaunched += (sender, args) => { GenericModConfigMenuIntegration.Init(); };

        helper.Events.GameLoop.UpdateTicking += (sender, args) => {
            if (Game1.player is null) return;
            var who = Game1.player;
            if (!Utils.DidPlayerJustLeftHold()) return;
            if (!who.IsLocalPlayer) return;
            if (who.CurrentTool == null) return;
            if (!Config.WeaponAutoswing) return;
            if (who.CurrentTool is not MeleeWeapon weapon) return;

            if (weapon.isScythe() && !who.UsingTool)
                who.BeginUsingTool();
            else
                who.FireTool();
        };

        var harmony = new Harmony(ModManifest.UniqueID);

        var farmerMoveMethod = AccessTools.Method(typeof(Farmer), nameof(Farmer.MovePosition));
        if (farmerMoveMethod != null) {
            harmony.Patch(
                original: farmerMoveMethod,
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(HookFarmerMovePositionPrefix)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(HookFarmerMovePositionPostfix))
            );
        }

        var setFarmerAnimatingMethod = AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.setFarmerAnimating));

        if (setFarmerAnimatingMethod != null) {
            harmony.Patch(
                original: setFarmerAnimatingMethod,
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(HookSetFarmerAnimatingMethod))
            );
        }

        var animateSpecialMoveMethod = AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.animateSpecialMove));

        if (animateSpecialMoveMethod != null) {
            harmony.Patch(
                original: animateSpecialMoveMethod,
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(HookAnimateSpecialMoveMethod))
            );
        }

        var warpFarmerMethod =
            AccessTools.Method(typeof(Farmer), nameof(Farmer.warpFarmer), new[] { typeof(Warp), typeof(int) });

        if (warpFarmerMethod != null) {
            harmony.Patch(
                original: warpFarmerMethod,
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(HookWarpFarmerMethod))
            );
        }
    }

    public static void HookFarmerMovePositionPrefix(Farmer __instance, GameTime time,
        xTile.Dimensions.Rectangle viewport,
        GameLocation currentLocation) {
        var farmer = __instance;
        _oldCanMove = farmer.CanMove;
        _oldUseTool = farmer.UsingTool;
        if (!farmer.UsingTool) return;
        if (farmer.canStrafeForToolUse()) return;
        if (Game1.panMode) return;

        bool ok = (!farmer.CurrentTool.IsTool() && Config.EnableForWeapons) ||
                  (farmer.CurrentTool.IsTool() && Config.EnableForTools);
        if (!ok) return;

        farmer.CanMove = true;
        farmer.UsingTool = false;
        HandleMovementInput();

        // GameUpdateControlInputMethod?.Invoke(Game1.game1, new object[] { time });
        // Console.WriteLine($"[AttackWhenMoving]: {farmer.Name} Tool CanMove = {farmer.CanMove}");
        // Console.WriteLine($"[AttackWhenMoving]: Move Speed = {farmer.getMovementSpeed()}");
        // Console.WriteLine($"[AttackWhenMoving]: Movement Directions = {farmer.movementDirections.Count}");
    }

    private static void HandleMovementInput() {
        KeyboardState currentKBState = Game1.GetKeyboardState();
        GamePadState currentPadState = Game1.input.GetGamePadState();
        bool moveUpReleased = false;
        bool moveRightReleased = false;
        bool moveDownReleased = false;
        bool moveLeftReleased = false;
        bool moveUpHeld = false;
        bool moveRightHeld = false;
        bool moveDownHeld = false;
        bool moveLeftHeld = false;
        if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveUpButton) &&
            Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveUpButton))
            moveUpReleased = true;
        if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveRightButton) &&
            Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveRightButton))
            moveRightReleased = true;
        if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveDownButton) &&
            Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveDownButton))
            moveDownReleased = true;
        if (Game1.areAllOfTheseKeysUp(currentKBState, Game1.options.moveLeftButton) &&
            Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.moveLeftButton))
            moveLeftReleased = true;
        if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveUpButton))
            moveUpHeld = true;
        if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveRightButton))
            moveRightHeld = true;
        if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveDownButton))
            moveDownHeld = true;
        if (Game1.isOneOfTheseKeysDown(currentKBState, Game1.options.moveLeftButton))
            moveLeftHeld = true;
        if (Game1.options.gamepadControls) {
            if (!currentPadState.IsButtonDown(Buttons.DPadUp) && Game1.oldPadState.IsButtonDown(Buttons.DPadUp))
                moveUpReleased = true;
            if (!currentPadState.IsButtonDown(Buttons.DPadRight) && Game1.oldPadState.IsButtonDown(Buttons.DPadRight))
                moveRightReleased = true;
            if (!currentPadState.IsButtonDown(Buttons.DPadDown) && Game1.oldPadState.IsButtonDown(Buttons.DPadDown))
                moveDownReleased = true;
            if (!currentPadState.IsButtonDown(Buttons.DPadLeft) && Game1.oldPadState.IsButtonDown(Buttons.DPadLeft))
                moveLeftReleased = true;
            if (currentPadState.IsButtonDown(Buttons.DPadUp))
                moveUpHeld = true;
            if (currentPadState.IsButtonDown(Buttons.DPadRight))
                moveRightHeld = true;
            if (currentPadState.IsButtonDown(Buttons.DPadDown))
                moveDownHeld = true;
            if (currentPadState.IsButtonDown(Buttons.DPadLeft))
                moveLeftHeld = true;
            if ((double)currentPadState.ThumbSticks.Left.X < -0.2)
                moveLeftHeld = true;
            else if ((double)currentPadState.ThumbSticks.Left.X > 0.2)
                moveRightHeld = true;
            if ((double)currentPadState.ThumbSticks.Left.Y < -0.2)
                moveDownHeld = true;
            else if ((double)currentPadState.ThumbSticks.Left.Y > 0.2)
                moveUpHeld = true;
            if (Game1.oldPadState.ThumbSticks.Left.X < -0.2 && !moveLeftHeld)
                moveLeftReleased = true;
            if (Game1.oldPadState.ThumbSticks.Left.X > 0.2 && !moveRightHeld)
                moveRightReleased = true;
            if (Game1.oldPadState.ThumbSticks.Left.Y < -0.2 && !moveDownHeld)
                moveDownReleased = true;
            if (Game1.oldPadState.ThumbSticks.Left.Y > 0.2 && !moveUpHeld)
                moveUpReleased = true;
        }

        if (Game1.pauseTime <= 0f && Game1.locationRequest == null &&
            (!Game1.player.UsingTool || Game1.player.canStrafeForToolUse()) &&
            ((!Game1.eventUp && Game1.farmEvent == null) ||
             (Game1.CurrentEvent != null && Game1.CurrentEvent.playerControlSequence))) {
            if (Game1.player.movementDirections.Count < 2) {
                if (moveUpHeld) Game1.player.setMoving(1);
                if (moveRightHeld) Game1.player.setMoving(2);
                if (moveDownHeld) Game1.player.setMoving(4);
                if (moveLeftHeld) Game1.player.setMoving(8);
            }

            if (moveUpReleased || (Game1.player.movementDirections.Contains(0) && !moveUpHeld)) {
                Game1.player.setMoving(33);
                if (Game1.player.movementDirections.Count == 0) Game1.player.setMoving(64);
            }

            if (moveRightReleased || (Game1.player.movementDirections.Contains(1) && !moveRightHeld)) {
                Game1.player.setMoving(34);
                if (Game1.player.movementDirections.Count == 0) Game1.player.setMoving(64);
            }

            if (moveDownReleased || (Game1.player.movementDirections.Contains(2) && !moveDownHeld)) {
                Game1.player.setMoving(36);
                if (Game1.player.movementDirections.Count == 0) Game1.player.setMoving(64);
            }

            if (moveLeftReleased || (Game1.player.movementDirections.Contains(3) && !moveLeftHeld)) {
                Game1.player.setMoving(40);
                if (Game1.player.movementDirections.Count == 0) Game1.player.setMoving(64);
            }

            if ((!moveUpHeld && !moveRightHeld && !moveDownHeld && !moveLeftHeld && !Game1.player.UsingTool) ||
                Game1.activeClickableMenu != null) {
                Game1.player.Halt();
            }
        }
    }

    public static void HookFarmerMovePositionPostfix(Farmer __instance, GameTime time,
        xTile.Dimensions.Rectangle viewport,
        GameLocation currentLocation) {
        var farmer = __instance;
        farmer.CanMove = _oldCanMove;
        farmer.UsingTool = _oldUseTool;
    }

    private static void FaceMouse(Farmer who) {
        if (!Config.FaceMouseWhenAttack) return;

        var plrPosition = who.getLocalPosition(Game1.viewport);
        var mousePosition = Game1.getMousePosition().ToVector2();
        var direPosition = mousePosition - plrPosition;
        var theta = Math.Atan2(direPosition.Y, direPosition.X);
        who.FacingDirection = theta switch {
            > MathF.PI / 4 and < 3 * MathF.PI / 4 => 2, // Up
            < -MathF.PI / 4 and > -3 * MathF.PI / 4 => 0, // Down
            >= -MathF.PI / 4 and <= MathF.PI / 4 => 1, // Right
            _ => 3 // Left
        };
    }

    public static void HookSetFarmerAnimatingMethod(MeleeWeapon __instance, Farmer who) {
        FaceMouse(who);
    }

    public static void HookAnimateSpecialMoveMethod(MeleeWeapon __instance, Farmer who) {
        if ((__instance.type.Value != 3 || (!__instance.Name.Contains("Scythe") && !__instance.isScythe())) &&
            !Game1.fadeToBlack && Utils.SpecialCooldown(__instance) <= 0) {
            FaceMouse(who);
        }
    }

    public static bool HookWarpFarmerMethod(Farmer __instance, Warp w, int warp_collide_direction) {
        if (_oldCanMove != __instance.CanMove && _oldUseTool != __instance.UsingTool) {
            return false;
        }

        return true;
    }
}