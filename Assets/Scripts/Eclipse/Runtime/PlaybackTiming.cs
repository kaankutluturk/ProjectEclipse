using System;

namespace Eclipse.Runtime
{
    /// <summary>
    /// Integer timing for a move's playback rate. A move advances one keyframe
    /// segment at a time; at normal speed each segment lasts <c>normalSteps</c>
    /// ticks ((MidFrames + 1) times the slow-mode factor). A rate in permille
    /// stretches or compresses that tick count while every interval, action and
    /// event stays attached to its keyframe. Segment lengths are derived from the
    /// segment index alone, so playback stays deterministic and stateless.
    /// </summary>
    public static class PlaybackTiming
    {
        public const int Normal = 1000;
        public const int Minimum = 500;
        public const int Maximum = 2000;

        /// <summary>Ticks elapsed before keyframe segment <paramref name="segment"/> begins.</summary>
        public static int TicksBefore(int normalSteps, int segment, int ratePermille)
        {
            if (ratePermille == Normal) return segment * normalSteps;
            long scaled = (long)segment * normalSteps * Normal;
            // Floor division that also holds for negative segments.
            long ticks = scaled >= 0 ? scaled / ratePermille : -((-scaled + ratePermille - 1) / ratePermille);
            return (int)ticks;
        }

        /// <summary>Ticks spent drawing keyframe segment <paramref name="segment"/>.</summary>
        public static int SegmentSteps(int normalSteps, int segment, int ratePermille)
        {
            return TicksBefore(normalSteps, segment + 1, ratePermille) - TicksBefore(normalSteps, segment, ratePermille);
        }

        /// <summary>The segment being drawn <paramref name="ticks"/> ticks after segment 0 began.</summary>
        public static int SegmentAt(int normalSteps, int ticks, int ratePermille)
        {
            // Authored speed keeps the recovered truncating division exactly.
            if (ratePermille == Normal) return ticks / normalSteps;
            // Largest k with TicksBefore(k) <= ticks, i.e. k * S * 1000 < (ticks + 1) * R.
            long numerator = (long)(ticks + 1) * ratePermille;
            long denominator = (long)normalSteps * Normal;
            return (int)(CeilingDivide(numerator, denominator) - 1);
        }

        /// <summary>
        /// A rate is playable when it is in range and every segment keeps at least one
        /// tick at normal slow-mode, so no keyframe (and its intervals or actions) is skipped.
        /// </summary>
        public static bool IsPlayable(int midFrames, int ratePermille)
        {
            return ratePermille >= Minimum && ratePermille <= Maximum && midFrames >= 0 &&
                (long)(midFrames + 1) * Normal >= ratePermille;
        }

        public static void RequirePlayable(int midFrames, int ratePermille)
        {
            if (!IsPlayable(midFrames, ratePermille))
                throw new ArgumentOutOfRangeException(nameof(ratePermille),
                    "Playback rate must be " + Minimum + ".." + Maximum + " permille and at most " +
                    ((long)(midFrames + 1) * Normal) + " for a move with MidFrames " + midFrames + ".");
        }

        private static long CeilingDivide(long value, long divisor)
        {
            long quotient = value / divisor;
            return (value % divisor != 0 && (value > 0) == (divisor > 0)) ? quotient + 1 : quotient;
        }
    }
}
