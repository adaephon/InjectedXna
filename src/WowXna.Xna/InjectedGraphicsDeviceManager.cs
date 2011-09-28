using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WowXna.Xna
{
    public partial class InjectedGraphicsDeviceManager : IGraphicsDeviceManager, IGraphicsDeviceService, IDisposable
    {
        private readonly Engine _engine;
        private GraphicsDevice _device;

        public InjectedGraphicsDeviceManager(Engine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");
            if (engine.Services.GetService(typeof(IGraphicsDeviceManager)) != null)
                throw new ArgumentException("IGraphicsDeviceManager is already present in engine services", "engine");

            engine.Services.AddService(typeof (IGraphicsDeviceManager), this);
            engine.Services.AddService(typeof (IGraphicsDeviceService), this);

            _engine = engine;
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