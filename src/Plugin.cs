using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TarantulaTiltSensitivity.Patches;

namespace TarantulaTiltSensitivity
{
    /// <summary>
    /// BepInEx entry point. Binds configuration and applies the Harmony patch that adds a
    /// fine-control sensitivity curve to the Tarantula's manual rotor tilt near 0 degrees.
    /// </summary>
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public sealed class Plugin : BaseUnityPlugin
    {
        /// <summary>Shared logger, exposed so static patch classes can log too.</summary>
        internal static ManualLogSource Log { get; private set; }

        private readonly Harmony _harmony = new Harmony(PluginInfo.Guid);

        private void Awake()
        {
            Log = Logger;
            ModConfig.Init(Config);

            _harmony.PatchAll(typeof(TiltSensitivityPatch));

            int patched = _harmony.GetPatchedMethods().Count();
            if (patched > 0)
            {
                Log.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded — tilt sensitivity patch active.");
            }
            else
            {
                Log.LogWarning(
                    $"{PluginInfo.Name} v{PluginInfo.Version} loaded, but no methods were patched "
                    + "(see errors above). The mod will have no effect.");
            }
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}
