namespace TarantulaTiltSensitivity
{
    /// <summary>
    /// Pure, Unity-independent helper that turns the current wing angle into a sensitivity divisor.
    ///
    /// <para>
    /// The divisor scales down the per-frame change applied to the tilt input, giving finer control
    /// near level flight. It is <c>centerDivisor</c> at 0°, tapers linearly to <c>1.0</c> (no change)
    /// at the zone edge, and stays <c>1.0</c> outside the zone — so the transition is continuous and
    /// there is no sensitivity "step" at the boundary.
    /// </para>
    /// </summary>
    internal static class SensitivityCurve
    {
        /// <summary>
        /// Returns the sensitivity divisor for a given wing angle. A return value of <c>1.0</c> means
        /// "leave input unchanged"; values &gt; 1 slow the input down proportionally.
        /// </summary>
        /// <param name="angleDegrees">Current wing angle in degrees (0 = level).</param>
        /// <param name="zoneHalfWidth">Half-width of the fine-control zone, in degrees.</param>
        /// <param name="centerDivisor">Divisor applied at exactly 0°.</param>
        public static float ComputeDivisor(float angleDegrees, float zoneHalfWidth, float centerDivisor)
        {
            if (zoneHalfWidth <= 0f || centerDivisor <= 1f)
            {
                return 1f;
            }

            float distance = angleDegrees < 0f ? -angleDegrees : angleDegrees;
            if (distance >= zoneHalfWidth)
            {
                return 1f;
            }

            // t: 0 at the centre, 1 at the zone edge. Lerp(centerDivisor, 1, t).
            float t = distance / zoneHalfWidth;
            return centerDivisor + (1f - centerDivisor) * t;
        }
    }
}
