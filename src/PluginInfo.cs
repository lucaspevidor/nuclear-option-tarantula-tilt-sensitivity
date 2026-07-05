namespace TarantulaTiltSensitivity
{
    /// <summary>
    /// Compile-time plugin metadata. Single source of truth for the <c>[BepInPlugin]</c>
    /// attribute, the Harmony instance id and log lines.
    /// </summary>
    internal static class PluginInfo
    {
        /// <summary>Unique BepInEx/Harmony identifier (reverse-domain notation).</summary>
        public const string Guid = "com.lucaspevidor.tarantulatiltsensitivity";

        /// <summary>Human-readable plugin name.</summary>
        public const string Name = "TarantulaTiltSensitivity";

        /// <summary>Semantic version.</summary>
        public const string Version = "1.0.0";
    }
}
