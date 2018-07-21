using Shouldly;
using TwitchVods.Core.Twitch.Helix;
using Xunit;

namespace TwitchVods.Tests
{
    public class DurationParserShould
    {
        [Fact]
        public void return_the_correct_duration_for_seconds_with_double_digit()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("0h0m23s").ShouldBe(23);
        }

        [Fact]
        public void return_the_correct_duration_when_no_hours_specified()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("0m23s").ShouldBe(23);
        }

        [Fact]
        public void return_the_correct_duration_when_no_mins_specified()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("23s").ShouldBe(23);
        }

        [Fact]
        public void return_the_correct_duration_for_seconds_with_single_digit()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("0h0m3s").ShouldBe(3);
        }

        [Fact]
        public void return_the_correct_duration_for_minutes_with_double_digit()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("0h12m00s").ShouldBe(720);
        }

        [Fact]
        public void return_the_correct_duration_for_minutes_with_single_digit()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("0h9m0s").ShouldBe(540);
        }

        [Fact]
        public void return_the_correct_duration_for_hours_with_double_digit()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("11h00m00s").ShouldBe(39600);
        }

        [Fact]
        public void return_the_correct_duration_for_hours_with_single_digit()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("8h0m0s").ShouldBe(28800);
        }

        [Fact]
        public void return_the_correct_duration_for_all_with_single_digit()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("8h7m6s").ShouldBe(29226);
        }

        [Fact]
        public void return_the_correct_duration_for_all_with_double_digit()
        {
            var twitchDuration = new DurationParser();
            twitchDuration.FromDuration("28h47m12s").ShouldBe(103632);
        }
    }
}
