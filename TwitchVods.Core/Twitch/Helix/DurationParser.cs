using System;
using System.Globalization;

namespace TwitchVods.Core.Twitch.Helix
{
    public interface IDurationParser
    {
        int FromDuration(string duration);
    }

    public class DurationParser : IDurationParser
    {
        public int FromDuration(string duration)
        {
            var hourStart = 0;
            var hourEnd = duration.IndexOf('h');
            var minuteStart = duration.IndexOf('h') + 1;
            var minuteEnd = duration.IndexOf('m');
            var secondStart = duration.IndexOf('m') + 1;
            var secondEnd = duration.IndexOf('s');

            var hours = int.Parse(duration.Substring(hourStart, hourEnd));
            var minutes = int.Parse(duration.Substring(minuteStart, minuteEnd - hourEnd - 1));
            var seconds = int.Parse(duration.Substring(secondStart, secondEnd - minuteEnd - 1));

            var totalSeconds = new TimeSpan(0, hours, minutes, seconds).TotalSeconds;

            return int.Parse(totalSeconds.ToString(CultureInfo.InvariantCulture));
        }
    }
}
