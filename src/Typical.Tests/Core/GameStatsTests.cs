using System;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using TUnit;
using Typical.Core.Events;
using Typical.Core.Statistics;

namespace Typical.Tests
{
    public class GameStatsTests
    {
        [Test]
        public async Task InitialState_ShouldBeDefaults()
        {
            var eventAggregator = new EventAggregator();
            var stats = new GameStats(eventAggregator, null, NullLogger<GameStats>.Instance);

            await Assert.That(stats.WordsPerMinute).IsEqualTo(0);
            await Assert.That(stats.Accuracy).IsEqualTo(100);
            await Assert.That(stats.IsRunning).IsFalse();
        }

        [Test]
        public async Task Start_ShouldSetIsRunningTrue()
        {
            var fakeTime = new FakeTimeProvider();
            var eventAggregator = new EventAggregator();
            var stats = new GameStats(eventAggregator, fakeTime, NullLogger<GameStats>.Instance);

            stats.Start();

            await Assert.That(stats.IsRunning).IsTrue();
        }

        [Test]
        public async Task Stop_ShouldSetIsRunningFalse()
        {
            var fakeTime = new FakeTimeProvider();
            var eventAggregator = new EventAggregator();
            var stats = new GameStats(eventAggregator, fakeTime, NullLogger<GameStats>.Instance);

            stats.Start();
            fakeTime.Advance(TimeSpan.FromSeconds(1));
            stats.Stop();

            await Assert.That(stats.IsRunning).IsFalse();
        }

        [Test]
        public async Task Update_ShouldCalculateAccuracy()
        {
            var fakeTime = new FakeTimeProvider();
            var eventAggregator = new EventAggregator();
            var stats = new GameStats(eventAggregator, fakeTime, NullLogger<GameStats>.Instance);

            stats.Start();
            fakeTime.Advance(TimeSpan.FromSeconds(1));
            string target = "hello";
            string input = "hxllo"; // 1 incorrect out of 5

            foreach (var (c, i) in target.Zip(input))
            {
                var type = c == i ? KeystrokeType.Correct : KeystrokeType.Incorrect;
                eventAggregator.Publish(new KeyPressedEvent(i, type, 0));
            }
            await Assert.That(stats.Accuracy).IsEqualTo(80);
        }

        [Test]
        public async Task Update_ShouldCalculateWordsPerMinute()
        {
            var fakeTime = new FakeTimeProvider();
            var eventAggregator = new EventAggregator();
            var stats = new GameStats(eventAggregator, fakeTime, NullLogger<GameStats>.Instance);

            stats.Start();
            fakeTime.Advance(TimeSpan.FromSeconds(1));
            string target = "hello world";
            string input = "hello";

            foreach (var (c, i) in target.Zip(input))
            {
                var type = c == i ? KeystrokeType.Correct : KeystrokeType.Incorrect;
                eventAggregator.Publish(new KeyPressedEvent(i, type, 0));
            }

            await Assert.That(stats.WordsPerMinute).IsEqualTo(60);
        }
    }
}
