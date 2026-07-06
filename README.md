# TarantulaTiltSensitivity

A [BepInEx 5](https://docs.bepinex.dev/) mod for **Nuclear Option** that adds a fine-control
sensitivity curve to the **Tarantula's** manual rotor tilt near 0°, so small adjustments around
level flight are easy instead of twitchy.

By **Lucas Pevidor**.

## The problem

The Tarantula's rotors tilt from −20° to 90°. To adjust them manually you disable auto-tilt (flight
assist), then hold **D-pad down + RB** (tilt forward) or **D-pad down + LB** (tilt back). That input
feeds a linear accumulator (`customAxis1`) that maps to wing angle at roughly **110°/second** — so a
brief tap near 0° overshoots hard (e.g. **+5° → −2°**), making precise level-ish tilt frustrating.

## The fix

While the **commanded (desired)** tilt is near 0°, the mod scales down the per-frame tilt input,
giving finer control exactly where you need it. It keys off the desired angle — the fast-moving
value you steer — rather than the slower actual rotor angle, so fine control kicks in as soon as you
aim near level, not only once the rotors have physically arrived. The scaling is a smooth curve:
strongest at 0° and tapering to normal by the edge of the zone, so there's no abrupt change.

- Default: **8× slower** at 0°, back to normal by **±10°**.
- Only affects the **Tarantula** (any aircraft with a tilt-wing controller; optionally name-filtered).
- Only affects **manual** tilt — with auto-tilt on, the game drives the rotors itself and the mod
  stays out of the way.
- Doesn't touch yaw/pitch thrust-vectoring, auto-tilt, other aircraft, or networking.

See [`../docs/`](../docs) for the BepInEx/Harmony background and the full investigation.

## Configuration

`BepInEx/config/com.lucaspevidor.tarantulatiltsensitivity.cfg`

| Section / Key | Default | Effect |
|---------------|---------|--------|
| `General / Enabled` | `true` | Toggle the whole mod at runtime. |
| `Sensitivity / ZoneHalfWidthDegrees` | `10` | Fine control applies within ± this many degrees of 0. |
| `Sensitivity / CenterSensitivityDivisor` | `8` | How much slower tilt moves at exactly 0° (tapers to 1× at the edge). |
| `Targeting / RestrictToUnitName` | `Tarantula` | Only affect aircraft whose name contains this (case-insensitive). Empty = any tilt-wing. |

## Building

Requires the .NET SDK and a local Nuclear Option install.

```bash
dotnet build -c Release
# or point at a non-default install:
dotnet build -c Release -p:GameDirectory="D:\SteamLibrary\steamapps\common\Nuclear Option"
```

On success the DLL is auto-deployed to `<GameDir>/BepInEx/plugins/TarantulaTiltSensitivity/`. Launch
the game, fly the Tarantula, and look for `tilt sensitivity patch active` (and, once you tilt near
0°, `Tilt sensitivity curve engaged on 'Tarantula'`) in `BepInEx/LogOutput.log`.

## Compatibility

- Nuclear Option (Unity 2022.3.x, Mono) + BepInEx 5.
- Client-side only — it changes how *your* tilt input is generated, so it is multiplayer-safe and
  does not require the host to run the mod.
- Coexists with UndelayedThrottle (both patch the same input method but different fields).
- If a future update renames the patched method/field, the mod logs an error and disables itself
  rather than crashing.
