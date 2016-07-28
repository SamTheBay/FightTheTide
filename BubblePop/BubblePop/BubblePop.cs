using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Microsoft.Advertising.Mobile.Xna;
using Microsoft.Phone.Shell;

namespace BubblePop
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BubblePop : Microsoft.Xna.Framework.Game
    {
        public static BubblePop Instance;
        public static GraphicsDeviceManager graphics;
        public static ScreenManager screenManager;
        public static AdControlManager adControlManager;
        public static Game sigletonGame;
        public static int masterController = 0;

        public static Rectangle ScreenSize = new Rectangle(0, 0, 480, 800);
        private static Vector2 gameTimeDrawLoc = new Vector2(100, 100);
        private static Vector2 maxGameTimeDrawLoc = new Vector2(100, 130);
        private static Vector2 memoryDrawLoc = new Vector2(100, 160);
        public static GameplayScreen gameplayScreen;

        public BubblePop()
        {
            Instance = this;
            sigletonGame = this;
            graphics = new GraphicsDeviceManager(this);

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Pre-auto scale settings.
            graphics.PreferredBackBufferWidth = 480;
            graphics.PreferredBackBufferHeight = 800;
            graphics.IsFullScreen = true;
            graphics.SupportedOrientations = DisplayOrientation.Portrait;

            Content.RootDirectory = "Content";

            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            // add the screen manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            GameSprite.game = this;

            adControlManager = new AdControlManager(Components, false);
            adControlManager.ShowAds = true;

            Activated += new EventHandler<EventArgs>(BubblePopOnActivated);
            Deactivated += new EventHandler<EventArgs>(BubblePopDeactivated);
        }


        protected override void Initialize()
        {
            InputManager.Initialize();

            base.Initialize();

            Fonts.LoadContent(Content);
            Accelerometer.Initialize();
            InternalContentManager.Load();
            AudioManager.Initialize(BubblePop.sigletonGame);
            AudioManager.audioManager.LoadSFX(0, 3);
            new MusicManager();
            //MusicManager.SingletonMusicManager.LoadTune("intro", TunnelDecent.sigletonGame.Content);

            gameplayScreen = new GameplayScreen();
            screenManager.AddScreen(new MenuBackgroundScreen());
            screenManager.AddScreen(new TitleScreen());
        }

        void BubblePopOnActivated(object sender, EventArgs args)
        {
            if (adControlManager != null)
            {
                adControlManager.Load();
            }

            // check if we have a game currently running
            if (GameplayScreen.singleton != null && gameplayScreen.IsPlaying && !(screenManager.GetScreens()[screenManager.GetScreens().Length - 1] is PauseScreen))
            {
                screenManager.AddScreen(new PauseScreen());
            }
        }

        void BubblePopDeactivated(object sender, EventArgs e)
        {
            if (adControlManager != null)
            {
                adControlManager.UnLoad();
            }
        }


        protected override void LoadContent()
        {
            base.LoadContent();

            adControlManager.Load();
        }


        protected override void UnloadContent()
        {
            Fonts.UnloadContent();

            base.UnloadContent();
        }


        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            base.Update(gameTime);

            adControlManager.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);
        }

    }
}
