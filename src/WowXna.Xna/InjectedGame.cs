using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace InjectedXna
{
    public class InjectedGame : IDisposable
    {
        private IGraphicsDeviceService _graphicsDeviceService;

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