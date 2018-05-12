using System;
using System.Collections.Generic;
using System.Linq;

using InputHandlers.Keyboard;
using InputHandlers.Mouse;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace InputHandlers.Sample
{
    public class InputHandlersSample : Game
    {
        private const int _screenWidth = 1400;
        private const int _screenHeight = 800;
        private KeyboardInput _keyboard;
        private MouseInput _mouse;
        private SpriteFont _arialfont;
        private readonly GraphicsDeviceManager _graphics;
        private InputHandler _inputHandler;
        private SpriteBatch _spriteBatch;

        public InputHandlersSample()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferredBackBufferWidth = _screenWidth;
            _graphics.PreferredBackBufferHeight = _screenHeight;

            Content.RootDirectory = "Content";

            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            _mouse = new MouseInput();
            _keyboard = new KeyboardInput();

            // You may want to assign keys that are unmanaged by the handler.  For example, you might want to handle the ASDW keys yourself in a fps game.
            // Try uncommenting the following and notice how nothing will happen when you press the keys:
            // _keyboard.UnmanagedKeys.Add(Keys.A);
            // _keyboard.UnmanagedKeys.Add(Keys.S);
            // _keyboard.UnmanagedKeys.Add(Keys.D);
            // _keyboard.UnmanagedKeys.Add(Keys.W);

            // You may want to treat control, alt and delete as keys rather than modifier keys.  Try uncommenting the following line to see this behaviour:
            _keyboard.TreatModifiersAsKeys = true;

            _inputHandler = new InputHandler();
            _inputHandler.Initialise();

            _mouse.Subscribe(_inputHandler);
            _keyboard.Subscribe(_inputHandler);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            IsMouseVisible = true;

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _arialfont = Content.Load<SpriteFont>("arial12");
        }

        protected override void Update(GameTime gameTime)
        {
            _inputHandler.UpdateLabelsBeforePoll();

            _mouse.Poll(Microsoft.Xna.Framework.Input.Mouse.GetState());
            _keyboard.Poll(Microsoft.Xna.Framework.Input.Keyboard.GetState());

            _inputHandler.UpdateLabelsAfterPoll(gameTime, _mouse, _keyboard);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            _inputHandler.DrawLabels(_spriteBatch, _arialfont);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}