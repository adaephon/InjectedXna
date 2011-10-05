using System;
using Microsoft.Xna.Framework;

namespace InjectedXna
{
    public class InjectedGameTime : GameTime
    {
        public InjectedGameTime()
        {
        }

        public InjectedGameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime, bool isRunningSlowly = false)
        {
            TotalGameTime = totalGameTime;
            ElapsedGameTime = elapsedGameTime;
            IsRunningSlowly = isRunningSlowly;
        }

        public new TimeSpan TotalGameTime { get; set; }
        public new TimeSpan ElapsedGameTime { get; set; }
        public new bool IsRunningSlowly { get; set; }
    }
}