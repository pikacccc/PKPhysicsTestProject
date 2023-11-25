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

        private List<PKBody<Cricle>> bodyListCricle;
        private List<PKBody<Box>> bodyListBox;
        private List<Color> cricleColors;
        private List<Color> boxColors;
        private List<Color> boxColorsOutLine;

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
            this.bodyListCricle = new List<PKBody<Cricle>>();
            this.bodyListBox = new List<PKBody<Box>>();
            this.cricleColors = new List<Color>();
            this.boxColors = new List<Color>();
            this.boxColorsOutLine = new List<Color>();

            for (int i = 0; i < bodyCount; ++i)
            {
                float x = RandomHelper.RandomSingle(left + padding, right - padding);
                float y = RandomHelper.RandomSingle(botton + padding, top - padding);
                int type = RandomHelper.RandomInteger(1, 3);

                if (type == (int)ShapeType.Circle)
                {
                    PKBodyUtil.CreateCircleBody(1f, new PKVector(x, y), 2f, false, .5f, out PKBody<Cricle> body, out string message);
                    bodyListCricle.Add(body);
                    this.cricleColors.Add(RandomHelper.RandomColor());
                }
                else if (type == (int)ShapeType.Box)
                {
                    PKBodyUtil.CreateBoxBody(2f, 2f, new PKVector(x, y), 2f, false, .5f, out PKBody<Box> body, out string message);
                    bodyListBox.Add(body);
                    this.boxColors.Add(RandomHelper.RandomColor());
                    this.boxColorsOutLine.Add(Color.White);
                }
            }
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
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
                float speed = 8;
                if (keyboard.IsKeyDown(Keys.Up)) { dy++; }
                if (keyboard.IsKeyDown(Keys.Down)) { dy--; }
                if (keyboard.IsKeyDown(Keys.Right)) { dx++; }
                if (keyboard.IsKeyDown(Keys.Left)) { dx--; }

                if (dx != 0 || dy != 0)
                {
                    PKVector dir = PKMath.Normalize(new PKVector(dx, dy));
                    PKVector velocity = dir * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    this.bodyListCricle[0].Move(velocity);
                    //this.bodyListBox[0].Move(velocity);
                }
            }

            for (int i = 0; i < bodyListBox.Count; i++)
            {
                //this.bodyListBox[i].Rotate((float)Math.PI / 2f * (float)gameTime.ElapsedGameTime.TotalSeconds);
                this.boxColorsOutLine[i] = Color.White;
            }

            for (int i = 0; i < bodyListCricle.Count - 1; i++)
            {
                var bodyA = bodyListCricle[i];
                for (int j = i + 1; j < bodyListCricle.Count; j++)
                {
                    var bodyB = bodyListCricle[j];
                    if (PKCollisions.IntersectCricles(bodyA.Position, bodyB.Position, bodyA.shape.Radius, bodyB.shape.Radius, out PKVector nor, out float depth))
                    {

                        bodyA.Move(-nor * depth * 0.5f);
                        bodyB.Move(nor * depth * 0.5f);
                    }
                }
            }

            for (int i = 0; i < bodyListBox.Count - 1; i++)
            {
                var bodyA = bodyListBox[i];
                for (int j = i + 1; j < bodyListBox.Count; j++)
                {
                    var bodyB = bodyListBox[j];
                    if (PKCollisions.IntersectPolygons(bodyA.GetTransformedVertics(), bodyB.GetTransformedVertics(), out PKVector nor, out float depth))
                    {
                        this.boxColorsOutLine[i] = Color.Red;
                        this.boxColorsOutLine[j] = Color.Red;
                        bodyA.Move(-nor * depth * 0.5f);
                        bodyB.Move(nor * depth * 0.5f);
                    }
                }
            }

            for (int i = 0; i < bodyListBox.Count - 1; i++)
            {
                var bodyA = bodyListBox[i];
                for (int j = 0; j < bodyListCricle.Count; j++)
                {
                    var bodyB = bodyListCricle[j];
                    if (PKCollisions.IntersectPolygonsAndCricle(bodyA.GetTransformedVertics(), bodyB.Position, bodyB.shape.Radius, out PKVector nor, out float depth))
                    {
                        bodyA.Move(nor * depth * 0.5f);
                        bodyB.Move(-nor * depth * 0.5f);
                    }
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            this.screen.Set();
            GraphicsDevice.Clear(Color.CornflowerBlue);

            this.shapes.Begin(this.camera);
            for (int i = 0; i < bodyListCricle.Count; i++)
            {
                PKBody<Cricle> body = bodyListCricle[i];
                Vector2 pos = PKConverter.ToVector2(body.Position);
                shapes.DrawCircleFill(pos, body.shape.Radius, 360, this.cricleColors[i]);
                shapes.DrawCircle(pos, body.shape.Radius, 360, Color.White);
            }

            for (int i = 0; i < bodyListBox.Count; i++)
            {
                PKBody<Box> body = bodyListBox[i];
                PKConverter.ToVector2Array(body.GetTransformedVertics(), ref verticsBuffer);
                shapes.DrawPolygonFill(this.verticsBuffer, body.shape.Tiangles, this.boxColors[i]);
                shapes.DrawPolygon(this.verticsBuffer, boxColorsOutLine[i]);
            }
            this.shapes.End();
            this.screen.Unset();
            this.screen.Present(this.sprites);
            base.Draw(gameTime);
        }
    }
}