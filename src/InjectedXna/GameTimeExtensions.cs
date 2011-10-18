using System;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace InjectedXna
{
    public static class GameTimeExtensions
    {
        private static readonly PropertyInfo ElapsedGameTimeProperty =
            typeof (GameTime).GetProperty("ElapsedGameTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly PropertyInfo IsRunningSlowlyProperty =
            typeof(GameTime).GetProperty("IsRunningSlowly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly PropertyInfo TotalGameTimeProperty =
            typeof(GameTime).GetProperty("TotalGameTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public static void SetElapsedGameTime(this GameTime gameTime, TimeSpan value)
        {
            ElapsedGameTimeProperty.SetValue(gameTime, value, null);
        }

        public static void SetIsRunningSlowly(this GameTime gameTime, bool value)
        {
            IsRunningSlowlyProperty.SetValue(gameTime, value, null);
        }

        public static void SetTotalGameTime(this GameTime gameTime, TimeSpan value)
        {
            TotalGameTimeProperty.SetValue(gameTime, value, null);
        }
    }
}