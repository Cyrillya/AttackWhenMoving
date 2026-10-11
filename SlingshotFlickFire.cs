using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;

namespace AttackWhenMoving;

/// <summary>Drives the slingshot with the right stick alone: pushing the stick to the edge draws and
/// aims a shot, and letting it snap back to the center fires it. Pulling the stick back by hand
/// instead ends the shot without firing, so the player is never left holding a drawn slingshot they
/// cannot put away.</summary>
internal static class SlingshotFlickFire
{
    /// <summary>Deflection below which the stick counts as back in the center, matching the threshold
    /// <see cref="Slingshot"/> itself uses when it decides the stick is no longer aiming.</summary>
    private const float CenterThreshold = 0.25f;

    /// <summary>How far the stick may move between two frames and still count as resting where it is,
    /// rather than passing through the center on its way somewhere else.</summary>
    private const float SettleDistance = 0.2f;

    /// <summary>How long the stick must stay away from the edge before the return is judged, so a
    /// momentary wobble while aiming is not mistaken for letting go.</summary>
    private const float ReleaseWindowSeconds = 0.05f;

    /// <summary>Return-to-center time (seconds) that the lowest sensitivity maps to: only a stick
    /// that returns on its own, within a couple of frames, counts as released.</summary>
    private const float FastestReturnSeconds = 0.05f;

    /// <summary>Return-to-center time (seconds) that the highest sensitivity maps to.</summary>
    private const float SlowestReturnSeconds = 1f;

    /// <summary>The direction a flicked shot aims in. Only set while that shot is being fired, and
    /// read back by <see cref="ModEntry.GetSlingshotAimStick"/>.</summary>
    internal static Vector2? AimOverride;

    /// <summary>Whether the stick reached the edge since the current shot was drawn.</summary>
    private static bool _armed;

    /// <summary>Whether the stick then came back fast enough to count as let go, and is on its way to
    /// the center.</summary>
    private static bool _released;

    /// <summary>When the stick was last at the edge.</summary>
    private static double _edgeTime;

    /// <summary>The deflection the stick had the last time it was at the edge.</summary>
    private static float _edgeDeflection;

    /// <summary>When the release was detected.</summary>
    private static double _releaseTime;

    /// <summary>The direction the stick was aiming in, kept for the shot that is fired.</summary>
    private static Vector2 _aimDirection;

    /// <summary>Where the stick was on the previous frame.</summary>
    private static Vector2 _lastStick;

    /// <summary>Track the right stick for the current slingshot shot.</summary>
    internal static void Update(Farmer who) {
        if (who.CurrentTool is not Slingshot slingshot || !IsEnabled()) {
            Reset();
            return;
        }

        Vector2 stick = Game1.input.GetGamePadState().ThumbSticks.Right;
        float deflection = stick.Length();
        float edgeThreshold = ModEntry.Config.ControllerAttackDeadZone;

        // The shot stays drawn while the stick is held out; it only resolves once the stick has been
        // pulled back well past the edge, so aiming with a comfortably held stick keeps the shot.
        float pullBackThreshold = (edgeThreshold + CenterThreshold) / 2f;

        if (!who.usingSlingshot) {
            Reset();
            // Pushing the stick to the edge draws the slingshot. Without this the feature would be
            // unusable: 'use tool' is bound to the X button, on the same side of the controller as
            // the stick that aims.
            if (deflection >= edgeThreshold && CanUseSlingshot(who)) who.BeginUsingTool();
            return;
        }

        if (!CanUseSlingshot(who)) {
            // Keep the shot drawn, but never fire one the player may no longer want (e.g. after a
            // menu was opened in the middle of the gesture).
            _released = false;
            return;
        }

        if (deflection >= edgeThreshold) {
            _armed = true;
            _released = false;
            _edgeTime = Now;
            _edgeDeflection = deflection;
            _aimDirection = Vector2.Normalize(stick);
            _lastStick = stick;
            return;
        }

        // A release that never settles in the center is not a shot the player is waiting for (e.g.
        // the stick was swept past the center to re-aim and then held there), so stop holding the
        // shot drawn for it.
        if (_released && Now - _releaseTime > AllowedReturnSeconds) {
            Reset();
            Cancel(slingshot, who);
            return;
        }

        if (deflection >= pullBackThreshold) {
            // Still held out, just not pressed all the way to the edge: the player is aiming.
            if (!_released) _aimDirection = Vector2.Normalize(stick);
            _lastStick = stick;
            return;
        }

        if (!_armed) return;

        if (!_released) {
            // The stick has been pulled back well past the edge. Let go and it comes back fast; ease
            // it back by hand and it comes back slowly, which ends the shot rather than leaving the
            // player stuck holding a drawn slingshot they cannot put away.
            double elapsed = Now - _edgeTime;
            if (elapsed < ReleaseWindowSeconds) return;

            if ((_edgeDeflection - deflection) / (float)elapsed < RequiredReturnSpeed) {
                Reset();
                Cancel(slingshot, who);
                return;
            }

            _released = true;
            _releaseTime = Now;
            _lastStick = stick;
            return;
        }

        // A released stick fires once it has come to rest in the center; one still travelling (e.g.
        // swept through the center to re-aim) must not fire.
        if (deflection > CenterThreshold) return;

        bool settled = (stick - _lastStick).Length() <= SettleDistance;
        _lastStick = stick;
        if (!settled) return;

        Reset();
        Fire(slingshot, who);
    }

    /// <summary>Forget the gesture in progress.</summary>
    private static void Reset() {
        _armed = false;
        _released = false;
    }

    /// <summary>Get whether the right stick is allowed to control the slingshot.</summary>
    private static bool IsEnabled() {
        var config = ModEntry.Config;
        return config.ControllerSlingshot && config.ControllerSlingshotFlickFire && Game1.options.gamepadControls;
    }

    /// <summary>How long the stick may take to return to the center and still count as released.</summary>
    private static float AllowedReturnSeconds {
        get {
            float sensitivity = Math.Clamp(ModEntry.Config.ControllerSlingshotFlickSensitivity, 0f, 1f);

            // Squaring the slider keeps the useful range in the lower half: a real snap-back takes
            // tens of milliseconds, while the top end still reaches "even a very slow push back
            // counts". The default of 0.4 lands on 0.2s, which is the window the game itself treats
            // as a gamepad slingshot release (Game1.controllerSlingshotSafeTime, and the 0.3s
            // Slingshot.GetRequiredChargeTime next to it): several times longer than any stick takes
            // to snap back, and shorter than a thumb takes to ease one back to the center.
            return MathHelper.Lerp(FastestReturnSeconds, SlowestReturnSeconds, sensitivity * sensitivity);
        }
    }

    /// <summary>How fast the stick has to come back for the gesture to count as released rather than
    /// pulled back, expressed as the speed that would carry it from the edge to the center within
    /// <see cref="AllowedReturnSeconds"/>.</summary>
    private static float RequiredReturnSpeed {
        get {
            float edgeThreshold = ModEntry.Config.ControllerAttackDeadZone;
            return (edgeThreshold - CenterThreshold) / AllowedReturnSeconds;
        }
    }

    private static double Now => Game1.currentGameTime.TotalGameTime.TotalSeconds;

    /// <summary>Get whether the game currently allows the player to use the slingshot, mirroring the
    /// states <c>Game1.pressUseToolButton</c> refuses to use a tool in.</summary>
    private static bool CanUseSlingshot(Farmer who) {
        return Game1.activeClickableMenu is null
               && Game1.currentMinigame is null
               && !Game1.eventUp
               && Game1.CurrentEvent is null
               && Game1.farmEvent is null
               && !Game1.dialogueUp
               && !Game1.isWarping
               && !Game1.fadeToBlack
               && Game1.pauseTime <= 0f
               && !who.IsSitting()
               && !who.isRidingHorse()
               && !who.isEating
               && !who.onBridge.Value
               && !who.swimming.Value
               && !who.bathingClothes.Value
               && !who.canOnlyWalk;
    }

    /// <summary>Put the slingshot away without firing, so the player can move and switch tools again.</summary>
    private static void Cancel(Slingshot slingshot, Farmer who) {
        // The next draw has to charge again, exactly as after any other draw that ends without a shot.
        slingshot.canPlaySound = true;
        who.completelyStopAnimatingOrDoingAction();
        // Putting the shot away is not a shot, so the game's post-shot movement lock does not apply.
        Game1.game1.controllerSlingshotSafeTime = 0f;
    }

    /// <summary>Fire the drawn slingshot along the direction the stick was flicked in.</summary>
    private static void Fire(Slingshot slingshot, Farmer who) {
        // Slingshot.PerformFire reads the aim from the current input, which no longer points where
        // the player flicked once the stick is back in the center, so pin the aim for this shot.
        bool mouseWasAiming = Game1.lastCursorMotionWasMouse;
        AimOverride = _aimDirection;
        Game1.lastCursorMotionWasMouse = false;
        try {
            slingshot.onRelease(Game1.currentLocation, (int)slingshot.aimPos.X, (int)slingshot.aimPos.Y, who);
        }
        finally {
            AimOverride = null;
            Game1.lastCursorMotionWasMouse = mouseWasAiming;
        }
    }
}
