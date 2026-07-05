using System;
using HarmonyLib;
using UnityEngine;

namespace TarantulaTiltSensitivity.Patches
{
    /// <summary>
    /// Applies a fine-control sensitivity curve to the Tarantula's manual rotor tilt near 0°.
    ///
    /// <para>
    /// Manual tilt is driven by the <c>customAxis1</c> input, which
    /// <c>PilotPlayerState.PlayerThrottleAxis1Controls</c> integrates linearly (≈110°/s) — so a brief
    /// tap near level flight overshoots badly (e.g. +5° → −2°). This patch measures how far that
    /// method moved <c>customAxis1</c> this frame and, while the wing is inside the fine-control zone,
    /// scales the movement down by <see cref="SensitivityCurve"/>. Everything downstream (auto-tilt,
    /// yaw/pitch vectoring, networking) is untouched.
    /// </para>
    ///
    /// <para>
    /// Scope: only aircraft with a <see cref="TiltWingController"/> (the Tarantula) and, optionally,
    /// a matching unit name. When flight assist / auto-tilt is on, the game overwrites
    /// <c>customAxis1</c> itself, so this scaling is naturally inert outside manual control.
    /// </para>
    /// </summary>
    [HarmonyPatch(typeof(PilotPlayerState), MethodName)]
    internal static class TiltSensitivityPatch
    {
        private const string MethodName = "PlayerThrottleAxis1Controls";
        private const string PilotField = "pilot";

        // Cached accessor for PilotPlayerState's private 'pilot' field (resolved in Prepare).
        private static AccessTools.FieldRef<PilotPlayerState, Pilot> _pilot;

        private static bool _loggedEngage;

        /// <summary>
        /// Harmony pre-patch hook. Verifies the target method and backing field exist and caches the
        /// field accessor. Returning <c>false</c> skips the patch so a game update degrades gracefully.
        /// </summary>
        private static bool Prepare()
        {
            bool hasMethod = AccessTools.Method(typeof(PilotPlayerState), MethodName) != null;
            bool hasField = AccessTools.Field(typeof(PilotPlayerState), PilotField) != null;

            if (!hasMethod || !hasField)
            {
                Plugin.Log?.LogError(
                    $"Incompatible game build: PilotPlayerState.{MethodName} found={hasMethod}, "
                    + $"field '{PilotField}' found={hasField}. Patch skipped.");
                return false;
            }

            _pilot = AccessTools.FieldRefAccess<PilotPlayerState, Pilot>(PilotField);
            return true;
        }

        /// <summary>Captures <c>customAxis1</c> before the original runs. NaN = "skip this call".</summary>
        private static void Prefix(PilotPlayerState __instance, out float __state)
        {
            __state = float.NaN;

            if (!ModConfig.Enabled.Value)
            {
                return;
            }

            ControlInputs inputs = GetInputs(__instance);
            if (inputs != null)
            {
                __state = inputs.customAxis1;
            }
        }

        /// <summary>Rescales the frame's <c>customAxis1</c> movement while inside the fine-control zone.</summary>
        private static void Postfix(PilotPlayerState __instance, float __state)
        {
            if (float.IsNaN(__state))
            {
                return;
            }

            Aircraft aircraft = _pilot(__instance)?.aircraft;
            if (aircraft == null)
            {
                return;
            }

            // Tarantula gate: must be a tilt-wing aircraft (and optionally match the configured name).
            TiltWingController tilt = TiltWingLookup.Get(aircraft);
            if (tilt == null || !MatchesUnitFilter(aircraft))
            {
                return;
            }

            ControlInputs inputs = aircraft.GetInputs();
            if (inputs == null)
            {
                return;
            }

            float delta = inputs.customAxis1 - __state;
            if (delta == 0f)
            {
                return; // player didn't move the tilt axis this frame
            }

            float divisor = SensitivityCurve.ComputeDivisor(
                tilt.GetWingAngle(),
                ModConfig.ZoneHalfWidthDegrees.Value,
                ModConfig.CenterSensitivityDivisor.Value);

            if (divisor <= 1f)
            {
                return; // outside the zone → default sensitivity
            }

            inputs.customAxis1 = Mathf.Clamp01(__state + delta / divisor);
            LogEngageOnce(aircraft);
        }

        private static ControlInputs GetInputs(PilotPlayerState instance)
        {
            Aircraft aircraft = _pilot(instance)?.aircraft;
            return aircraft != null ? aircraft.GetInputs() : null;
        }

        private static bool MatchesUnitFilter(Aircraft aircraft)
        {
            string filter = ModConfig.RestrictToUnitName.Value;
            if (string.IsNullOrWhiteSpace(filter))
            {
                return true;
            }

            string unitName = aircraft.definition != null ? aircraft.definition.unitName : null;
            return !string.IsNullOrEmpty(unitName)
                && unitName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void LogEngageOnce(Aircraft aircraft)
        {
            if (_loggedEngage)
            {
                return;
            }

            _loggedEngage = true;
            string unitName = aircraft.definition != null ? aircraft.definition.unitName : "<unknown>";
            Plugin.Log?.LogInfo($"Tilt sensitivity curve engaged on '{unitName}'.");
        }
    }
}
