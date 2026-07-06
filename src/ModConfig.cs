using BepInEx.Configuration;

namespace TarantulaTiltSensitivity
{
    /// <summary>
    /// Typed wrapper around the BepInEx config file. Centralises binding so the rest of the mod
    /// reads strongly-typed entries without knowing about section/key strings. Values are live —
    /// editing the .cfg (or the in-game Configuration Manager) takes effect without a restart.
    /// </summary>
    internal static class ModConfig
    {
        /// <summary>Master switch. When <c>false</c> the patch stays installed but is a no-op.</summary>
        public static ConfigEntry<bool> Enabled { get; private set; }

        /// <summary>
        /// Half-width, in degrees, of the fine-control zone around 0°. Inside <c>[-zone, +zone]</c>
        /// tilt sensitivity is reduced; outside it, default sensitivity is used.
        /// </summary>
        public static ConfigEntry<float> ZoneHalfWidthDegrees { get; private set; }

        /// <summary>
        /// Sensitivity divisor at exactly 0°. The effective divisor tapers smoothly from this value
        /// at the centre down to 1.0 (no change) at the zone edge. e.g. 4 = tilt moves 4× slower at 0°.
        /// </summary>
        public static ConfigEntry<float> CenterSensitivityDivisor { get; private set; }

        /// <summary>
        /// Only apply to aircraft whose <c>unitName</c> contains this text (case-insensitive).
        /// Empty = apply to any tilt-wing aircraft (i.e. anything with a TiltWingController).
        /// </summary>
        public static ConfigEntry<string> RestrictToUnitName { get; private set; }

        /// <summary>Binds all config entries. Call once from the plugin's <c>Awake</c>.</summary>
        /// <param name="config">The plugin's <see cref="ConfigFile"/> (i.e. <c>BaseUnityPlugin.Config</c>).</param>
        public static void Init(ConfigFile config)
        {
            Enabled = config.Bind(
                "General",
                "Enabled",
                true,
                "Add a fine-control sensitivity curve near 0 degrees to manual rotor tilt.");

            ZoneHalfWidthDegrees = config.Bind(
                "Sensitivity",
                "ZoneHalfWidthDegrees",
                10f,
                new ConfigDescription(
                    "Within +/- this many degrees of 0, tilt sensitivity is reduced. Outside it, the "
                    + "game's default sensitivity is used.",
                    new AcceptableValueRange<float>(0f, 90f)));

            CenterSensitivityDivisor = config.Bind(
                "Sensitivity",
                "CenterSensitivityDivisor",
                8f,
                new ConfigDescription(
                    "How much slower tilt moves at exactly 0 degrees (e.g. 8 = 8x slower). The effect "
                    + "tapers smoothly to 1x at the edge of the zone. 1 = no change.",
                    new AcceptableValueRange<float>(1f, 20f)));

            RestrictToUnitName = config.Bind(
                "Targeting",
                "RestrictToUnitName",
                "Tarantula",
                "Only affect aircraft whose name contains this text (case-insensitive). "
                + "Leave empty to affect any tilt-wing aircraft.");
        }
    }
}
