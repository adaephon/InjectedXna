using System;
using Microsoft.Xna.Framework.Content;

namespace WowXna.Xna
{
    public class Engine : IDisposable
    {
        public Engine()
        {
            Services = new EngineServiceContainer();
            Content = new ContentManager(Services);
        }

        public ContentManager Content { get; private set; }

        public EngineServiceContainer Services { get; private set; }

        #region IDisposable
        
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