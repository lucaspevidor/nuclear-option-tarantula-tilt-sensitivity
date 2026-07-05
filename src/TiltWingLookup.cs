using System.Runtime.CompilerServices;
using UnityEngine;

namespace TarantulaTiltSensitivity
{
    /// <summary>
    /// Caches the <see cref="TiltWingController"/> for each aircraft so the per-frame patch does not
    /// call <c>GetComponentInChildren</c> every update. Entries are keyed by aircraft reference and
    /// evicted automatically when the aircraft is garbage-collected.
    ///
    /// <para>
    /// <see cref="TiltWingController"/> is the game's tilt-wing/rotor system and (as of writing) is
    /// present only on the Tarantula, so "has a controller" doubles as "is a tilt-wing aircraft".
    /// </para>
    /// </summary>
    internal static class TiltWingLookup
    {
        // StrongBox lets us cache a legitimate "no controller" result (null) without re-searching.
        private static readonly ConditionalWeakTable<Aircraft, StrongBox<TiltWingController>> Cache =
            new ConditionalWeakTable<Aircraft, StrongBox<TiltWingController>>();

        /// <summary>
        /// Returns the aircraft's <see cref="TiltWingController"/>, or <c>null</c> if it has none
        /// (i.e. it is not a tilt-wing aircraft). The result is cached per aircraft.
        /// </summary>
        public static TiltWingController Get(Aircraft aircraft)
        {
            if (aircraft == null)
            {
                return null;
            }

            if (Cache.TryGetValue(aircraft, out StrongBox<TiltWingController> box))
            {
                return box.Value;
            }

            TiltWingController controller = aircraft.GetComponentInChildren<TiltWingController>(true);
            Cache.Add(aircraft, new StrongBox<TiltWingController>(controller));
            return controller;
        }
    }
}
