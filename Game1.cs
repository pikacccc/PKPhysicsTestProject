using Flat;
using Flat.Graphics;
using Flat.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PKPhysics;
using PKPhysics.PKShape;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Screen = Flat.Graphics.Screen;

namespace PKPhysicsTestProject
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch _spriteBatch;
        private Screen screen;
        private Sprites sprites;
        private Shapes shapes;
        private Camera camera;

        public PKWorld PKWorld;
        private Color[] cricleColors;
        private Color[] boxColors;
        private Color[] boxColorsOutLine;

        private Vector2[] verticsBuffer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.SynchronizeWithVerticalRetrace = true;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;

            const double UpdatePerSecond = 60d;
            this.TargetElapsedTime = TimeSpan.FromTicks((long)Math.Round((double)TimeSpan.TicksPerSecond / UpdatePerSecond));
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            FlatUtil.SetRelativeBackBufferSize(this.graphics, 0.85f);
            this.PKWorld = new PKWorld();
            this.screen = new Screen(this, 1280, 758);
            this.sprites = new Sprites(this);
            this.camera = new Camera(this.screen);
            this.shapes = new Shapes(this);
            this.camera.Zoom = 24;

            this.CreateBodys();
        }

        private void CreateBodys()
        {
            this.camera.GetExtents(out float left, out float right, out float botton, out float top);

            int bodyCount = 10;
            float padding = MathF.Abs(right - left) * 0.05f;
            this.cricleColors = new Color[bodyCount];
            this.boxColors = new Color[bodyCount];
            this.boxColorsOutLine = new Color[bodyCount];

            for (int i = 0; i < bodyCount; ++i)
            {
                float x = RandomHelper.RandomSingle(left + padding, right - padding);
                float y = RandomHelper.RandomSingle(botton + padding, top - padding);
                int type = RandomHelper.RandomInteger(1, 3);
                PKBody body = null;
                if (type == (int)ShapeType.Circle)
                {
                    PKBodyUtil.CreateCircleBody(1f, new PKVector(x, y), 2f, false, .5f, out body, out string message);
                    this.cricleColors[i] = RandomHelper.RandomColor();
                }
                else if (type == (int)ShapeType.Box)
                {
                    PKBodyUtil.CreateBoxBody(2f, 2f, new PKVector(x, y), 2f, false, .5f, out body, out string message);
                    this.boxColors[i] = RandomHelper.RandomColor();
                    this.boxColorsOutLine[i] = Color.White;
                }
                PKWorld.AddBody(body);
            }
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            FlatKeyboard keyboard = FlatKeyboard.Instance;
            FlatMouse mouse = FlatMouse.Instance;

            keyboard.Update();
            mouse.Update();

            if (keyboard.IsKeyAvailable)
            {
                if (keyboard.IsKeyClicked(Keys.Escape))
                {
                    this.Exit();
                }

                if (keyboard.IsKeyClicked(Keys.A))
                {
                    this.camera.IncZoom();
                }

                if (keyboard.IsKeyClicked(Keys.Z))
                {
                    this.camera.DecZoom();
                }

                float dx = 0;
                float dy = 0;
                float forceSize = 24;
                if (keyboard.IsKeyDown(Keys.Up)) { dy++; }
                if (keyboard.IsKeyDown(Keys.Down)) { dy--; }
                if (keyboard.IsKeyDown(Keys.Right)) { dx++; }
                if (keyboard.IsKeyDown(Keys.Left)) { dx--; }

                if (!this.PKWorld.GetBody(0, out PKBody body))
                {
                    throw new Exception("无法找到body");
                }

                if (dx != 0 || dy != 0)
                {
                    PKVector forceDir = new PKVector(dx, dy).Normalized();
                    PKVector force = forceDir * forceSize;
                    body.AddForce(force);
                }

                if (keyboard.IsKeyDown(Keys.R))
                {
                    body.Rotate((float)Math.PI / 2f * (float)gameTime.ElapsedGameTime.TotalSeconds * forceSize);
                }
            }

            PKWorld.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.screen.Set();
            GraphicsDevice.Clear(Color.CornflowerBlue);

            this.shapes.Begin(this.camera);
            for (int i = 0; i < PKWorld.BodyCount(); i++)
            {
                PKWorld.GetBody(i, out PKBody body);
                if (body.shape.ShapeType == ShapeType.Circle)
                {
                    Vector2 pos = PKConverter.ToVector2(body.Position);
                    shapes.DrawCircleFill(pos, (body.shape as Cricle).Radius, 360, this.cricleColors[i]);
                    shapes.DrawCircle(pos, (body.shape as Cricle).Radius, 360, Color.White);

                }
                else if (body.shape.ShapeType == ShapeType.Box)
                {
                    PKConverter.ToVector2Array(body.GetTransformedVertics(), ref verticsBuffer);
                    shapes.DrawPolygonFill(this.verticsBuffer, body.shape.Tiangles, this.boxColors[i]);
                    shapes.DrawPolygon(this.verticsBuffer, boxColorsOutLine[i]);
                }
            }
            this.shapes.End();
            this.screen.Unset();
            this.screen.Present(this.sprites);
            base.Draw(gameTime);
        }
    }
}