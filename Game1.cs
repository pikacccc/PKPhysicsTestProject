using Flat;
using Flat.Graphics;
using Flat.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PKPhysics;
using PKPhysics.PKShape;
using System;
using System.Collections.Generic;
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
        private List<Color> Colors;
        private List<Color> OutLineColors;

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

            this.CreateWorld();
        }

        private void CreateWorld()
        {
            this.camera.GetExtents(out float left, out float right, out float botton, out float top);

            int bodyCount = 30;
            float padding = MathF.Abs(right - left) * 0.1f;
            this.Colors = new List<Color>();
            this.OutLineColors = new List<Color>();

            if (!PKBodyUtil.CreateBoxBody(right - left - padding * 2, 3f, new PKVector(0, -10), 1f, true, 0.5f, out PKBody ground, out string error))
            {
                throw new Exception(error);
            }
            PKWorld.AddBody(ground);
            this.Colors.Add(Color.Green);
            this.OutLineColors.Add(Color.White);
            //for (int i = 0; i < bodyCount; ++i)
            //{
            //    float x = RandomHelper.RandomSingle(left + padding, right - padding);
            //    float y = RandomHelper.RandomSingle(botton + padding, top - padding);
            //    int type = RandomHelper.RandomInteger(1, 3);
            //    PKBody body = null;
            //    if (type == (int)ShapeType.Circle)
            //    {
            //        PKBodyUtil.CreateCircleBody(1f, new PKVector(x, y), 2f, false, .5f, out body, out string message);
            //    }
            //    else if (type == (int)ShapeType.Box)
            //    {
            //        PKBodyUtil.CreateBoxBody(1.77f, 1.77f, new PKVector(x, y), 2f, false, .5f, out body, out string message);
            //    }
            //    if (i != 0)
            //    {
            //        body.IsStatic = RandomHelper.RandomBooleon();
            //    }
            //    PKWorld.AddBody(body);
            //    this.Colors.Add(RandomHelper.RandomColor());
            //    this.OutLineColors.Add(Color.White);
            //}
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

            if (mouse.IsLeftMouseButtonPressed())
            {
                float w = RandomHelper.RandomSingle(1f, 2f);
                float h = RandomHelper.RandomSingle(1f, 2f);

                PKVector worldMousePos = PKConverter.ToPKVector2(mouse.GetMouseWorldPosition(this, screen, camera));

                if (!PKBodyUtil.CreateBoxBody(w, h, worldMousePos, 1, false, .6f, out PKBody body, out string error))
                {
                    throw new Exception(error);
                }

                PKWorld.AddBody(body);
                this.Colors.Add(RandomHelper.RandomColor());
                this.OutLineColors.Add(Color.White);
            }

            if (mouse.IsRightMouseButtonPressed())
            {
                PKVector worldMousePos = PKConverter.ToPKVector2(mouse.GetMouseWorldPosition(this, screen, camera));
                float r = RandomHelper.RandomSingle(1f, 2f);
                if (!PKBodyUtil.CreateCircleBody(r, worldMousePos, 1, false, .6f, out PKBody body, out string error))
                {
                    throw new Exception(error);
                }

                PKWorld.AddBody(body);
                this.Colors.Add(RandomHelper.RandomColor());
                this.OutLineColors.Add(Color.White);
            }

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

                if (keyboard.IsKeyClicked(Keys.X))
                {
                    Console.WriteLine($"场景中body个数{this.PKWorld.BodyCount()}");
                }
                //float dx = 0;
                //float dy = 0;
                //float forceSize = 48;
                //if (keyboard.IsKeyDown(Keys.Up)) { dy++; }
                //if (keyboard.IsKeyDown(Keys.Down)) { dy--; }
                //if (keyboard.IsKeyDown(Keys.Right)) { dx++; }
                //if (keyboard.IsKeyDown(Keys.Left)) { dx--; }

                //if (!this.PKWorld.GetBody(0, out PKBody body))
                //{
                //    throw new Exception("无法找到body");
                //}

                //if (dx != 0 || dy != 0)
                //{
                //    PKVector forceDir = new PKVector(dx, dy).Normalized();
                //    PKVector force = forceDir * forceSize;
                //    body.AddForce(force);
                //}

                //if (keyboard.IsKeyDown(Keys.R))
                //{
                //    body.Rotate((float)Math.PI / 2f * (float)gameTime.ElapsedGameTime.TotalSeconds * forceSize);
                //}
            }

            PKWorld.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            //WrapScreen();
            CheckShouldRemove();
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
                    shapes.DrawCircleFill(pos, (body.shape as Cricle).Radius, 360, this.Colors[i]);
                    shapes.DrawCircle(pos, (body.shape as Cricle).Radius, 360, this.OutLineColors[i]);

                }
                else if (body.shape.ShapeType == ShapeType.Polygon)
                {
                    PKConverter.ToVector2Array(body.GetTransformedVertics(), ref verticsBuffer);
                    shapes.DrawPolygonFill(this.verticsBuffer, body.shape.Tiangles, this.Colors[i]);
                    shapes.DrawPolygon(this.verticsBuffer, this.OutLineColors[i]);
                }
            }
            this.shapes.End();
            this.screen.Unset();
            this.screen.Present(this.sprites);
            base.Draw(gameTime);
        }

        private void WrapScreen()
        {
            this.camera.GetExtents(out Vector2 camMin, out Vector2 camMax);

            camMin *= 1.1f;
            camMax *= 1.1f;
            float viewW = camMax.X - camMin.X;
            float viewH = camMax.Y - camMin.Y;
            for (int i = 0; i < PKWorld.BodyCount(); ++i)
            {
                if (!PKWorld.GetBody(i, out var body))
                {
                    throw new Exception();
                }
                if (body.Position.X < camMin.X) body.MoveTo(new PKVector(body.Position.X + viewW, body.Position.Y));
                if (body.Position.X > camMax.X) body.MoveTo(new PKVector(body.Position.X - viewW, body.Position.Y));
                if (body.Position.Y < camMin.Y) body.MoveTo(new PKVector(body.Position.X, body.Position.Y + viewH));
                if (body.Position.Y > camMax.Y) body.MoveTo(new PKVector(body.Position.X, body.Position.Y - viewH));
            }
        }

        private void CheckShouldRemove()
        {
            this.camera.GetExtents(out _, out _, out float viewBottom, out _);
            for (int i = 0; i < PKWorld.BodyCount(); ++i)
            {
                if (!PKWorld.GetBody(i, out var body))
                {
                    throw new ArgumentOutOfRangeException();
                }

                PKAABB aabb = body.GetAABB();

                if (aabb.Max.Y < viewBottom)
                {
                    this.PKWorld.RemoveBody(body);
                    this.Colors.RemoveAt(i);
                    this.OutLineColors.RemoveAt(i);
                }
            }
        }
    }
}