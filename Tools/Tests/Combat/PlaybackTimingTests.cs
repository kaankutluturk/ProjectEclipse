using System;
using Eclipse.Runtime;

// Checks the integer playback-rate math that resizes keyframe segments.
internal static class PlaybackTimingTests
{
    private static int count;
    private static void Check(bool value, string why) { count++; if (!value) throw new Exception(why); }

    public static void Main()
    {
        // Authored speed reproduces the recovered formulas exactly, including
        // negative inputs where the original used truncating division.
        for (int mid = 0; mid <= 20; mid++)
        for (int first = 0; first <= 4; first++)
        for (int frame = -5; frame <= 220; frame++)
        {
            int steps = mid + 1;
            Check(PlaybackTiming.TicksBefore(steps, frame - first + 1, PlaybackTiming.Normal) + 1 == (frame - first + 1) * steps + 1,
                "ToInterpolatedFrame changed at normal rate.");
            Check(first - 1 + PlaybackTiming.SegmentAt(steps, frame - 1, PlaybackTiming.Normal) == first - 1 + (frame - 1) / steps,
                "FromInterpolatedFrame changed at normal rate.");
            Check(PlaybackTiming.SegmentSteps(steps, frame, PlaybackTiming.Normal) == steps, "Normal segment length changed.");
        }

        for (int rate = PlaybackTiming.Minimum; rate <= PlaybackTiming.Maximum; rate += 25)
        for (int mid = 0; mid <= 20; mid++)
        for (int slow = 1; slow <= 2; slow++)
        {
            int steps = (mid + 1) * slow;
            bool playable = PlaybackTiming.IsPlayable(mid, rate);
            Check(playable == ((mid + 1) * 1000 >= rate), "Playable bound disagrees with one tick per segment.");
            if (!playable) continue;
            int total = 0;
            for (int segment = 0; segment < 200; segment++)
            {
                int length = PlaybackTiming.SegmentSteps(steps, segment, rate);
                Check(length >= 1, "A playable rate skipped a keyframe segment.");
                Check(PlaybackTiming.TicksBefore(steps, segment, rate) == total, "Segment lengths do not sum to TicksBefore.");
                for (int tick = total; tick < total + length; tick++)
                    Check(PlaybackTiming.SegmentAt(steps, tick, rate) == segment, "SegmentAt is not the inverse of TicksBefore.");
                total += length;
            }
            // The whole move scales by the rate within one tick of rounding.
            long expected = 200L * steps * 1000 / rate;
            Check(Math.Abs(total - expected) <= 1, "Total move length does not follow the rate.");
        }

        bool rejected = false;
        try { PlaybackTiming.RequirePlayable(0, 1500); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "MidFrames 0 accepted a faster rate.");
        rejected = false;
        try { PlaybackTiming.RequirePlayable(2, 2500); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Rate above maximum accepted.");
        PlaybackTiming.RequirePlayable(2, 2000);
        Console.WriteLine("PASS: " + count + " playback timing checks.");
    }
}
