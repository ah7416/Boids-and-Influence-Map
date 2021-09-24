using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Boids
{
    //This is the main starting point of the application
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Boid[] boidArray;
        Texture2D tex, blankTexture;
        InfluenceMap influenceMap;
        int windowHeight, windowWidth, maxBoids;
        public static int numberOfBoids = 0;
        public static Random random;
        public static bool debug = false, influenceMapDebug = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        //Some initialization logic
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            windowWidth = graphics.PreferredBackBufferWidth = 1000;
            windowHeight = graphics.PreferredBackBufferHeight = 1000;
            maxBoids = 600;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            IsMouseVisible = true;
            base.Initialize();
        }

        //Load content and final initialization steps
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            tex = Content.Load<Texture2D>("Boid");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            influenceMap = new InfluenceMap();
            DrawingExtensions.tileTex = DrawingExtensions.CreateSquare(graphics.GraphicsDevice, 1);
            int cellSize = 25;
            influenceMap.Initialize(windowWidth, windowHeight, cellSize);
            random = new Random(Guid.NewGuid().GetHashCode());
            boidArray = new Boid[maxBoids];
            blankTexture = DrawingExtensions.CreateSquare(GraphicsDevice, 1);
            CreateArrayFlock(150);
        }

        //Create a given number of boids
        private void CreateArrayFlock(int amount)
        {
            //Do not create more than Boid Array has capacity for
            if (numberOfBoids + amount < maxBoids)
            {
                for (int i = 0; i < amount; i++)
                {
                    Boid temp = new Boid(tex, blankTexture, new Vector2(random.Next(0, windowWidth), random.Next(0, windowHeight)), windowWidth, windowHeight);
                    boidArray[i] = temp;
                    numberOfBoids++;
                }
            }      
        }

        //Main update loop of application
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyMouseReader.Update();
            CreateBoid();
            influenceMap.Update(boidArray);
            
            //Update all boids
            for (int i = 0; i < numberOfBoids; i++)
            {
                boidArray[i].Update(gameTime, boidArray, influenceMap);
            }

            UpdateDebugToggles();      

            base.Update(gameTime);
        }

        //Spawn a boid by left clicking
        protected void CreateBoid()
        {
            if (numberOfBoids < maxBoids)
            {
                if (KeyMouseReader.LeftClick())
                {
                    Boid temp = new Boid(tex, blankTexture, new Vector2(KeyMouseReader.mouseState.Position.X, KeyMouseReader.mouseState.Position.Y), windowWidth, windowHeight);
                    boidArray[numberOfBoids++] = temp;
                }
            }          
        }

        //Check for keypresses activating debug visuals
        private void UpdateDebugToggles()
        { 
            //Press d to toggle debug displays on or off
            if (!debug && KeyMouseReader.KeyPressed(Keys.D))
            {
                debug = true;
            }
            else if (debug && KeyMouseReader.KeyPressed(Keys.D))
            {
                debug = false;
            }

            //Press space to toggle InfluenceMapdebug
            if (!influenceMapDebug && KeyMouseReader.KeyPressed(Keys.Space))
            {
                influenceMapDebug = true;
            }
            else if (influenceMapDebug && KeyMouseReader.KeyPressed(Keys.Space))
            {
                influenceMapDebug = false;
            }
        }

        //Draw boids and influenceMap
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightBlue);
            spriteBatch.Begin();

            if (influenceMapDebug)
            {
                influenceMap.DrawGrid(spriteBatch);
            }

            for (int i = 0; i < numberOfBoids; i++)
            {
                boidArray[i].Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
