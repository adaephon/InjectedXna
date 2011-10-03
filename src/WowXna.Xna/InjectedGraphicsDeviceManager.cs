using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InjectedXna
{
    public partial class InjectedGraphicsDeviceManager : IGraphicsDeviceManager, IGraphicsDeviceService, IDisposable
    {
        private readonly InjectedGame _injectedGame;
        private GraphicsDevice _device;

        public InjectedGraphicsDeviceManager(InjectedGame injectedGame)
        {
            if (injectedGame == null)
                throw new ArgumentNullException("injectedGame");
            if (injectedGame.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
                throw new ArgumentException("IGraphicsDeviceManager is already present in engine services", "injectedGame");

            injectedGame.Services.AddService(typeof (IGraphicsDeviceManager), this);
            injectedGame.Services.AddService(typeof (IGraphicsDeviceService), this);

            _injectedGame = injectedGame;
        }

        #region Implementation of IGraphicsDeviceManager

        /// <summary>
        /// Called to ensure that the device manager has created a valid device.
        /// </summary>
        public void CreateDevice()
        {
            if (_device != null) return;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts the drawing of a frame.
        /// </summary>
        public bool BeginDraw()
        {
            return GraphicsDevice != null;
        }

        /// <summary>
        /// Called by the game at the end of drawing; presents the final rendering.
        /// </summary>
        public void EndDraw()
        {
            // TODO: see if anything needs to happen here
        }

        #endregion

        #region Implementation of IGraphicsDeviceService

        /// <summary>
        /// Retrieves a graphcs device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return _device; }
        }

        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;
        public event EventHandler<EventArgs> DeviceCreated;

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}