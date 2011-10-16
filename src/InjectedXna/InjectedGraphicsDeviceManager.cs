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

        #region Event Handlers

        protected virtual void OnDeviceCreated(object sender, EventArgs e)
        {
            var handler = DeviceCreated;
            if (handler != null)
                handler(sender, e);
        }

        protected virtual void OnDeviceDisposing(object sender, EventArgs e)
        {
            var handler = DeviceDisposing;
            if (handler != null)
                handler(sender, e);
        }

        protected virtual void OnDeviceReset(object sender, EventArgs e)
        {
            var handler = DeviceReset;
            if (handler != null)
                handler(sender, e);
        }

        protected virtual void OnDeviceResetting(object sender, EventArgs e)
        {
            var handler = DeviceResetting;
            if (handler != null)
                handler(sender, e);
        }

        #endregion

        #region Implementation of IGraphicsDeviceManager

        /// <summary>
        /// Called to ensure that the device manager has created a valid device.
        /// </summary>
        public void CreateDevice()
        {
            if (_device != null) return;
            var dev = GetXnaDevice();
            if (dev == null)
                throw new NoSuitableGraphicsDeviceException(
                    "No graphics device has yet been created. There is no suitable XNA Graphics Device to use.");
            _device = dev;
            _device.DeviceResetting += OnDeviceResetting;
            _device.DeviceReset += OnDeviceReset;
            _device.Disposing += OnDeviceDisposing;
            OnDeviceCreated(this, EventArgs.Empty);
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
            if (disposing)
            {
                if (_injectedGame != null)
                {
                    if (_injectedGame.Services.GetService(typeof(IGraphicsDeviceService)) == this)
                        _injectedGame.Services.RemoveService(typeof(IGraphicsDeviceService));
                }
            }
        }

        #endregion
    }
}