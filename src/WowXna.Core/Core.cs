using System;
using System.Diagnostics;
using System.Threading;
using InjectedXna;

namespace WowXna
{
    public class Core : InjectedGame
    {
        #region Static
        
        public static void Launch()
        {
            InjectedGraphicsDeviceManager.HookGraphicsDeviceCreation();
            InjectedGraphicsDeviceManager.XnaDeviceCreated += LaunchCoreAsync;
        }

        private static void LaunchCoreAsync(object sender, EventArgs e)
        {
            new Thread(() => new Core().Run()) {IsBackground = true}.Start();
            InjectedGraphicsDeviceManager.XnaDeviceCreated -= LaunchCoreAsync;
        }

        #endregion
    }
}