using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace InjectedXna
{
    public class InjectedGame : IDisposable
    {
        private IGraphicsDeviceService _graphicsDeviceService;
        private IGraphicsDeviceManager _graphicsDeviceManager;
        private readonly AutoResetEvent _renderThreadInitializedEvent;
        private readonly GameTime _gameTime;
        private TimeSpan _totalGameTime;
        private StateBlock _renderStateBlock;
        private readonly InjectedGameClock _clock;
        private TimeSpan _lastFrameElapsedGameTime;

        public InjectedGame()
        {
            _clock = new InjectedGameClock();
            _lastFrameElapsedGameTime = TimeSpan.Zero;
            _renderThreadInitializedEvent = new AutoResetEvent(false);
            _gameTime = new InjectedGameTime();
            _totalGameTime = TimeSpan.Zero;
            Services = new GameServiceContainer();
            Content = new ContentManager(Services);
        }

        public ContentManager Content { get; private set; }

        public GameServiceContainer Services { get; private set; }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                var graphicsDeviceService = _graphicsDeviceService;
                if (graphicsDeviceService == null)
                {
                    graphicsDeviceService = Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
                    if (graphicsDeviceService == null)
                    {
                        throw new InvalidOperationException("Unable to find a graphics device service");
                    }
                }
                return graphicsDeviceService.GraphicsDevice;
            }
        }

        public void Run()
        {
            _graphicsDeviceManager = Services.GetService(typeof (IGraphicsDeviceManager)) as IGraphicsDeviceManager;
            if (_graphicsDeviceManager != null)
                _graphicsDeviceManager.CreateDevice();
            InjectedGraphicsDeviceManager.EndScene += EndSceneFirstRun;
            _renderThreadInitializedEvent.WaitOne();
        }

        public void Tick()
        {
            // TODO: handle ShouldExit
            _clock.Step();
            var elapsedAdjustedTime = _clock.ElapsedAdjustedTime;
            if (elapsedAdjustedTime < TimeSpan.Zero)
                elapsedAdjustedTime = TimeSpan.Zero;

            // NOTE: always uses non-fixed timestep
            try
            {
                _lastFrameElapsedGameTime = elapsedAdjustedTime;
                _gameTime.SetElapsedGameTime(_lastFrameElapsedGameTime);
                _gameTime.SetTotalGameTime(_totalGameTime);
                _gameTime.SetIsRunningSlowly(false);
                Update(_gameTime);
                // TODO: suppress draw
            }
            finally
            {
                _totalGameTime += elapsedAdjustedTime;
            }

            DrawFrame();
        }

        private void DrawFrame()
        {
            try
            {
                _gameTime.SetTotalGameTime(_totalGameTime);
                _gameTime.SetElapsedGameTime(_lastFrameElapsedGameTime);
                _gameTime.SetIsRunningSlowly(false);
                Draw(_gameTime);
            }
            finally
            {
                _lastFrameElapsedGameTime = TimeSpan.Zero;
            }
        }

        private void EndSceneFirstRun(object sender, EndSceneEventArgs e)
        {
            Initialize();
            BeginRun();
            _gameTime.SetElapsedGameTime(TimeSpan.Zero);
            _gameTime.SetTotalGameTime(_totalGameTime);
            _gameTime.SetIsRunningSlowly(false);
            Update(_gameTime);
            InjectedGraphicsDeviceManager.EndScene -= EndSceneFirstRun;
            InjectedGraphicsDeviceManager.EndScene += EndScene;
            _renderThreadInitializedEvent.Set();
        }

        private void EndScene(object sender, EndSceneEventArgs e)
        {
            var captured = false;
            int hr;
            if (_renderStateBlock == null)
                _renderStateBlock = new StateBlock(e.DevicePointer);
            try
            {
                hr = _renderStateBlock.Capture();
                Marshal.ThrowExceptionForHR(hr);
                captured = true;

                Tick();
            }
            finally
            {
                if (captured)
                {
                    hr = _renderStateBlock.Apply();
                    Marshal.ThrowExceptionForHR(hr);
                }

            }
        }

        private void HookDeviceEvents()
        {
            _graphicsDeviceService = Services.GetService(typeof (IGraphicsDeviceService)) as IGraphicsDeviceService;
            
            if (_graphicsDeviceService == null) return;

            _graphicsDeviceService.DeviceCreated += DeviceCreated;
            _graphicsDeviceService.DeviceResetting += DeviceResetting;
            _graphicsDeviceService.DeviceReset += DeviceReset;
            _graphicsDeviceService.DeviceDisposing += DeviceDisposing;
        }

        private void DeviceDisposing(object sender, EventArgs e)
        {
            Content.Unload();
            UnloadContent();
        }

        private void DeviceReset(object sender, EventArgs e)
        {
            
        }

        private void DeviceResetting(object sender, EventArgs e)
        {
            if (_renderStateBlock == null) return;
            
            _renderStateBlock.Dispose();
            _renderStateBlock = null;
        }

        private void DeviceCreated(object sender, EventArgs e)
        {
            LoadContent();
        }

        #region Protected Xna Game Implementation

        protected virtual bool BeginDraw()
        {
            return _graphicsDeviceManager == null || _graphicsDeviceManager.BeginDraw();
        }

        protected virtual void BeginRun()
        {
        }

        protected virtual void Update(GameTime gameTime)
        {
        }

        protected virtual void Draw(GameTime gameTime)
        {
        } 

        protected virtual void Initialize()
        {
            HookDeviceEvents();
            if (_graphicsDeviceService != null && _graphicsDeviceService.GraphicsDevice != null)
                LoadContent();
        }

        protected virtual void LoadContent()
        {
        }

        protected virtual void UnloadContent()
        {
        }

        #endregion

        #region IDisposable

        ~InjectedGame()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // TODO
        }

        #endregion
    }
}