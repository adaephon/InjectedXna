using System;
using System.Collections.Generic;
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
        private AutoResetEvent _renderThreadInitializedEvent;
        private InjectedGameTime _gameTime;
        private TimeSpan _totalGameTime;

        public InjectedGame()
        {
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

        private void EndSceneFirstRun(object sender, EndSceneEventArgs e)
        {
            Initialize();
            BeginRun();
            _gameTime.ElapsedGameTime = TimeSpan.Zero;
            _gameTime.TotalGameTime = _totalGameTime;
            _gameTime.IsRunningSlowly = false;
            Update(_gameTime);
            InjectedGraphicsDeviceManager.EndScene -= EndSceneFirstRun;
            InjectedGraphicsDeviceManager.EndScene += EndScene;
            _renderThreadInitializedEvent.Set();
        }

        private void EndScene(object sender, EndSceneEventArgs e)
        {
            throw new NotImplementedException();
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
            
        }

        #endregion
    }
}