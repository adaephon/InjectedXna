using System;
using System.Diagnostics;

namespace InjectedXna
{
    public class InjectedGameClock
    {
        private bool _lastRealTimeValid;
        private TimeSpan _currentTimeBase;
        private TimeSpan _currentTimeOffset;
        private long _baseRealTime;
        private long _lastRealTime;
        private long _timeLostToSuspension;
        private int _suspendCount;
        private long _suspendStartTime;

        public InjectedGameClock()
        {
            Reset();
        }

        public void Reset()
        {
            _currentTimeBase = TimeSpan.Zero;
            _currentTimeOffset = TimeSpan.Zero;
            _baseRealTime = Counter;
            _lastRealTimeValid = false;
        }

        public void Suspend()
        {
            _suspendCount++;
            if (_suspendCount == 1)
            {
                _suspendStartTime = Counter;
            }
        }

        public void Resume()
        {
            _suspendCount--;
            if (_suspendCount <= 0)
            {
                var counter = Counter;
                _timeLostToSuspension += counter - _suspendStartTime;
                _suspendStartTime = 0L;
            }
        }

        public void Step()
        {
            var counter = Counter;
            if (!_lastRealTimeValid)
            {
                _lastRealTime = counter;
                _lastRealTimeValid = true;
            }
            try
            {
                _currentTimeOffset = CounterToTimeSpan(counter - _baseRealTime);
            }
            catch (OverflowException)
            {
                _currentTimeBase += _currentTimeOffset;
                _baseRealTime = _lastRealTime;
                try
                {
                    _currentTimeOffset = CounterToTimeSpan(counter - _baseRealTime);
                }
                catch (OverflowException)
                {
                    _baseRealTime = counter;
                    _currentTimeOffset = TimeSpan.Zero;
                }
            }
            try
            {
                ElapsedTime = CounterToTimeSpan(counter - _lastRealTime);
            }
            catch (OverflowException)
            {
                ElapsedTime = TimeSpan.Zero;
            }
            try
            {
                var diff = _lastRealTime + _timeLostToSuspension;
                ElapsedAdjustedTime = CounterToTimeSpan(counter - diff);
                _timeLostToSuspension = 0L;
            }
            catch (OverflowException)
            {
                ElapsedAdjustedTime = TimeSpan.Zero;
            }
            _lastRealTime = counter;
        }

        public TimeSpan ElapsedAdjustedTime { get; private set; }
        
        public TimeSpan ElapsedTime { get; private set; }

        public TimeSpan CurrentTime
        {
            get { return _currentTimeBase + _currentTimeOffset; }
        }

        #region Static
        public static long Counter { get { return Stopwatch.GetTimestamp(); } }

        public static long Frequency { get { return Stopwatch.Frequency; } }

        public static TimeSpan CounterToTimeSpan(long delta)
        {
            var ticks = (delta * TimeSpan.TicksPerSecond) / Frequency;
            return TimeSpan.FromTicks(ticks);
        } 
        #endregion
    }
}