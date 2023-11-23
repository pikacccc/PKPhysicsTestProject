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

        private List<PKBody<Cricle>> bodyList;
        private Color[] colors;

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

            this.camera.GetExtents(out float left, out float right, out float botton, out float top);

            int bodyCount = 10;
            float padding = MathF.Abs(right - left) * 0.05f;
            this.bodyList = new List<PKBody<Cricle>>();
            this.colors = new Color[bodyCount];

            for (int i = 0; i < bodyCount; ++i)
            {
                float x = RandomHelper.RandomSingle(left + padding, right - padding);
                float y = RandomHelper.RandomSingle(botton + padding, top - padding);

                PKBodyUtil.CreateCircleBody(1f, new PKVector(x, y), 2f, false, .5f, out PKBody<Cricle> body, out string message);
                bodyList.Add(body);
                this.colors[i] = RandomHelper.RandomColor();
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

                if (dx!=0||dy!=0)
                {
                    PKVector dir = PKMath.Normalize(new PKVector(dx, dy));
                    PKVector velocity = dir * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    this.bodyList[0].Move(velocity);
                }
            }

            for (int i = 0; i < bodyList.Count - 1; i++)
            {
                var bodyA = bodyList[i];
                for (int j = i + 1; j < bodyList.Count; j++)
                {
                    var bodyB = bodyList[j];
                    if (PKCollisions.IntersectCricles(bodyA.Position, bodyB.Position, bodyA.shape.Radius, bodyB.shape.Radius, out PKVector nor, out float depth))
                    {

                        bodyA.Move(-nor * depth * 0.5f);
                        bodyB.Move(nor * depth * 0.5f);
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
            for (int i = 0; i < bodyList.Count; i++)
            {
                PKBody<Cricle> body = bodyList[i];
                Vector2 pos = PKConverter.ToVector2(body.Position);
                shapes.DrawCircleFill(pos, body.shape.Radius, 360, colors[i]);
                shapes.DrawCircle(pos, body.shape.Radius, 360, Color.White);
            }
            this.shapes.End();
            this.screen.Unset();
            this.screen.Present(this.sprites);
            base.Draw(gameTime);
        }
    }
}