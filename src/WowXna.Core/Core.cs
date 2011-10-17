using System;
using System.Diagnostics;
using System.Threading;
using InjectedXna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WowXna
{
    public class Core : InjectedGame
    {
        private SpriteBatch _spriteBatch;
        private SpriteFont _courierNew;
        private GeometricPrimitive _primitive;
        private RasterizerState _defaultState;
        private InjectedGraphicsDeviceManager _graphics;

        public Core()
        {
            _graphics = new InjectedGraphicsDeviceManager(this);
            Content.RootDirectory = @"D:\Projects\wow-xna\src\WowXna.Core\bin\x86\Debug\WowXna.Content";
        }

        protected override void Draw(GameTime gameTime)
        {
            float time;
            if (gameTime is InjectedGameTime)
                time = (float) (gameTime as InjectedGameTime).TotalGameTime.TotalSeconds;
            else
                time = (float)gameTime.TotalGameTime.TotalSeconds;

            float yaw = time * 0.4f;
            float pitch = time * 0.7f;
            float roll = time * 1.1f;

            Vector3 cameraPosition = new Vector3(0, 0, 2.5f);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            Matrix world = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1.0f, 10.0f);

            GeometricPrimitive currentPrimitive = _primitive;
            Color color = Color.Red;
            color.A = 0xF0;

            GraphicsDevice.RasterizerState = _defaultState;

            currentPrimitive.Draw(world, view, projection, color);

            GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            _spriteBatch.Begin();
            //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, GraphicsDevice.SamplerStates[0],
            //                   GraphicsDevice.DepthStencilState, GraphicsDevice.RasterizerState);

            string text = "A or tap top of screen = Change primitive\n" +
                          "B or tap bottom left of screen = Change color\n" +
                          "Y or tap bottom right of screen = Toggle wireframe";
            _spriteBatch.DrawString(_courierNew, text, new Vector2(48, 48), Color.Green);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _courierNew = Content.Load<SpriteFont>("CourierNew");
            _primitive = new CubePrimitive(GraphicsDevice);

            _defaultState = new RasterizerState
                                {
                                    FillMode = FillMode.Solid,
                                    CullMode = CullMode.CullCounterClockwiseFace,
                                    MultiSampleAntiAlias = true,
                                    Name = "Default test"
                                };
        }

        #region Static
        
        public static void Launch()
        {
            //Debugger.Launch();
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