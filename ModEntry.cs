using System.Reflection;
using System.Reflection.Emit;
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
    internal static Harmony HarmonyEntry;
    internal static ModConfig Config;
    internal static ModEntry Instance;

    private static FieldInfo _meleeWeaponAnotherClick;
    private static bool _oldCanMove;
    private static bool _oldUseTool;

    public override void Entry(IModHelper helper) {
        Instance = this;
        Config = Helper.ReadConfig<ModConfig>();
        Localization.Init(helper.Translation);

        _meleeWeaponAnotherClick =
            typeof(MeleeWeapon).GetField("anotherClick", BindingFlags.NonPublic | BindingFlags.Instance);

        helper.Events.GameLoop.GameLaunched += (sender, args) => { GenericModConfigMenuIntegration.Init(); };

        helper.Events.GameLoop.UpdateTicking += (sender, args) => {
            if (TryPerformAttackNow(out var weapon)) {
                if (Config.SpecialAttackCancellable) Utils.CancelSpecialAttack(Game1.player);
                Utils.UseWeapon(weapon);
            }
        };

        HarmonyEntry = new Harmony(ModManifest.UniqueID);

        Hook(AccessTools.Method(typeof(Farmer), nameof(Farmer.MovePosition)),
            nameof(HookFarmerMovePositionPrefix), nameof(HookFarmerMovePositionPostfix));
        Hook(AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.setFarmerAnimating)),
            nameof(HookSetFarmerAnimatingMethod));
        Hook(AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.animateSpecialMove)),
            nameof(HookAnimateSpecialMoveMethod));
        Hook(AccessTools.Method(typeof(Farmer), nameof(Farmer.warpFarmer), new[] { typeof(Warp), typeof(int) }),
            nameof(HookWarpFarmerMethod));
        Hook(GetUpdateControlInputMethod(),
            transpilerName: nameof(UpdateControlInputTranspiler));
    }

    private static MethodInfo? GetUpdateControlInputMethod() {
        var displayClass = typeof(Game1).GetNestedType("<>c__DisplayClass978_0", BindingFlags.NonPublic);
        return displayClass?.GetMethod("<UpdateControlInput>b__0", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private static MethodInfo? Hook(MethodInfo? method, string? prefixName = null, string? postfixName = null,
        string? transpilerName = null) {
        if (method is null) return null;
        return HarmonyEntry.Patch(
            original: method,
            prefix: prefixName is null ? null : new HarmonyMethod(typeof(ModEntry), prefixName),
            postfix: postfixName is null ? null : new HarmonyMethod(typeof(ModEntry), postfixName),
            transpiler: transpilerName is null ? null : new HarmonyMethod(typeof(ModEntry), transpilerName)
        );
    }

    public static void HookFarmerMovePositionPrefix(Farmer __instance, GameTime time,
        xTile.Dimensions.Rectangle viewport,
        GameLocation currentLocation) {
        var farmer = __instance;
        _oldCanMove = farmer.CanMove;
        _oldUseTool = farmer.UsingTool;
        if (Game1.panMode ||
            !Context.IsPlayerFree ||
            !farmer.UsingTool ||
            Game1.isWarping ||
            farmer.canStrafeForToolUse() ||
            !farmer.CurrentTool.AllowMoving())
            return;

        farmer.CanMove = true;
        farmer.UsingTool = false;
        HandleMovementInput();
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

    private static bool FaceMouse(Farmer who) {
        if (!Config.FaceMouseWhenAttack) return false;
        if (!who.IsLocalPlayer) return false;

        var plrPosition = who.getLocalPosition(Game1.viewport);
        double theta;
        if (Game1.options.gamepadControls) {
            var currentPadState = Game1.input.GetGamePadState();
            var stick = currentPadState.ThumbSticks.Right;
            if (stick.Length() <= Config.ControllerAimDeadZone) return false;
            theta = Math.Atan2(stick.Y, stick.X);
        }
        else {
            var mousePosition = Game1.getMousePosition().ToVector2();
            var direPosition = mousePosition - plrPosition;
            theta = Math.Atan2(-direPosition.Y, direPosition.X);
        }

        who.FacingDirection = theta switch {
            > MathF.PI / 4 and < 3 * MathF.PI / 4 => 0, // Up
            < -MathF.PI / 4 and > -3 * MathF.PI / 4 => 2, // Down
            >= -MathF.PI / 4 and <= MathF.PI / 4 => 1, // Right
            _ => 3 // Left
        };
        return true;
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
        return _oldCanMove == __instance.CanMove || _oldUseTool == __instance.UsingTool;
    }

    private static bool SuitableAttackTiming() {
        if (Utils.MouseOnMenu() ||
            !Context.IsPlayerFree ||
            Game1.isWarping ||
            Game1.player is null)
            return false;
        var who = Game1.player;
        if (who.CurrentTool is not MeleeWeapon ||
            who.swimming.Value || who.bathingClothes.Value || who.onBridge.Value)
            return false;
        return true;
    }

    private static bool TryPerformAttackNow(out MeleeWeapon? weapon) {
        weapon = null;
        if (!SuitableAttackTiming() ||
            !Utils.DidPlayerJustLeftHold())
            return false;
        if (!Config.WeaponAutoswing && !Game1.didPlayerJustLeftClick(true))
            return false;
        weapon = Game1.player.CurrentTool as MeleeWeapon;
        return true;
    }

    internal static bool ShouldStickMoveMouse() {
        if (!Game1.options.gamepadControls) return false;
        if (Config.ControllerAttackMode is "mouse") return true;
        if (!SuitableAttackTiming()) return true;

        Utils.ForceHideMouseInGamepadMode();
        return false;
    }

    private static IEnumerable<CodeInstruction> UpdateControlInputTranspiler(
        IEnumerable<CodeInstruction> instructions) {
        // Match pattern: call Game1::get_options() + ldfld Options::gamepadControls + brfalse/brfalse_s
        // Replace with:  call ModEntry::OverrideGamepadControls() + brfalse/brfalse_s (same target)
        var getOptions = typeof(Game1).GetMethod("get_options", BindingFlags.Public | BindingFlags.Static);
        var gamepadControlsField =
            typeof(Options).GetField("gamepadControls", BindingFlags.Public | BindingFlags.Instance);
        var overrideMethod = typeof(ModEntry).GetMethod(nameof(ShouldStickMoveMouse),
            BindingFlags.NonPublic | BindingFlags.Static);

        if (getOptions is null || gamepadControlsField is null || overrideMethod is null) {
            Instance.Monitor.Log(
                $"UpdateControlInputTranspiler: reflection failed — getOptions={getOptions is not null}, " +
                $"gamepadControlsField={gamepadControlsField is not null}, overrideMethod={overrideMethod is not null}. " +
                "The transpiler will be skipped; gamepad-controls logic remains unmodified.",
                LogLevel.Error);
            return instructions;
        }

        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count - 2; i++) {
            if (codes[i].opcode != OpCodes.Call || !Equals(codes[i].operand, getOptions))
                continue;
            if (codes[i + 1].opcode != OpCodes.Ldfld || !Equals(codes[i + 1].operand, gamepadControlsField))
                continue;
            if (codes[i + 2].opcode != OpCodes.Brfalse && codes[i + 2].opcode != OpCodes.Brfalse_S)
                continue;

            // Save the branch target label
            var branchTarget = codes[i + 2].operand;

            // Replace the 3 instructions with 2: call OverrideGamepadControls + brfalse
            codes[i].opcode = OpCodes.Call;
            codes[i].operand = overrideMethod;
            codes.RemoveRange(i + 1, 2);
            codes.Insert(i + 1, new CodeInstruction(OpCodes.Brfalse, branchTarget));

            break; // Only replace the first occurrence
        }

        return codes;
    }
}