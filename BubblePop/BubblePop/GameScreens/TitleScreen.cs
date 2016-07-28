using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BubblePop
{
    class TitleScreen : MenuScreen
    {
        int playerIndex = 0;
        PlayerSprite player;
        Texture2D arrow;
        public static int[] unlockedPoints = new int[4];
        public static string[] fishNames = new string[4];
        bool hasFocus = true;

        public TitleScreen()
        {
            IsPopup = true;
            restartOnVisible = true;
            arrow = GameSprite.game.Content.Load<Texture2D>("Textures/ArrowRight");
            player = new PlayerSprite(playerIndex);

            unlockedPoints[0] = 0;
            unlockedPoints[1] = 2000;
            unlockedPoints[2] = 10000;
            unlockedPoints[3] = 20000;

            fishNames[0] = "Clown Fish";
            fishNames[1] = "Angel Fish";
            fishNames[2] = "Cow Fish";
            fishNames[3] = "Lion Fish";

            MenuEntry entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/Start");
            entry.PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/StartPress");
            entry.SetStartAnimation(new Vector2(-entry.Texture.Width - 10, 375), new Vector2(240 - entry.Texture.Width / 2, 375), 0, 1000, 1000);
            entry.SetAnimationType(AnimationType.Slide);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/Leaderboard");
            entry.PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/LeaderboardPress");
            entry.SetStartAnimation(new Vector2(490, 455), new Vector2(240 - entry.Texture.Width / 2, 455), 0, 1000, 1000);
            entry.SetAnimationType(AnimationType.Slide);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/Games");
            entry.PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/GamesPress");
            entry.SetStartAnimation(new Vector2(-entry.Texture.Width - 10, 535), new Vector2(240 - entry.Texture.Width / 2, 535), 0, 1000, 1000);
            entry.SetAnimationType(AnimationType.Slide);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/About");
            entry.PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/AboutPress");
            entry.SetStartAnimation(new Vector2(490, 615), new Vector2(240 - entry.Texture.Width / 2, 615), 0, 1000, 1000);
            entry.SetAnimationType(AnimationType.Slide);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/ArrowRight");
            entry.Position = new Vector2(360 - entry.Texture.Width / 2, 275 - entry.Texture.Height / 2);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/ArrowLeft");
            entry.Position = new Vector2(120 - entry.Texture.Width / 2, 275 - entry.Texture.Height / 2);
            MenuEntries.Add(entry);

            ResetScreen();
        }



        void entry_Selected(object sender, EventArgs e)
        {
            if (selectorIndex == 0)
            {
                if (GameplayScreen.highScore >= unlockedPoints[playerIndex])
                {
                    AddNextScreen(new InstructionsScreen(playerIndex));
                }
            }
            else if (selectorIndex == 1)
            {
                AddNextScreen(new LeaderboardScreen());
            }
            else if (selectorIndex == 2)
            {
                AddNextScreen(new OtherGamesScreen());
            }
            else if (selectorIndex == 3)
            {
                AddNextScreen(new AboutScreen());
            }
            else if (selectorIndex == 4)
            {
                playerIndex++;
                playerIndex %= 4;
                player = new PlayerSprite(playerIndex);
            }
            else if (selectorIndex == 5)
            {
                playerIndex--;
                if (playerIndex == -1)
                    playerIndex = 3;
                player = new PlayerSprite(playerIndex);
            }
        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            hasFocus = !otherScreenHasFocus;
        }


        Vector2 scoreLocation = new Vector2(10, 10);
        public override void Draw(GameTime gameTime)
        {
            if (hasFocus)
            {
                base.Draw(gameTime);

                SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
                spriteBatch.Begin();

                Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, fishNames[playerIndex], new Vector2(240, 230), Color.White);

                player.Draw(spriteBatch, new Vector2(240 - player.FrameDimensions.X / 2, 275 - player.FrameDimensions.Y / 2), 0f);

                if (GameplayScreen.highScore >= unlockedPoints[playerIndex])
                {
                    Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "Unlocked", new Vector2(240, 325), Color.White);
                }
                else
                {
                    Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "Unlock at " + unlockedPoints[playerIndex].ToString(), new Vector2(240, 325), Color.Red);
                }


                spriteBatch.End();
            }
        }

        public override void AddNextScreen(GameScreen nextScreen)
        {
            AudioManager.audioManager.PlaySFX("whoosh");
            base.AddNextScreen(nextScreen);
        }


        public override void ResetScreen()
        {
            AudioManager.audioManager.PlaySFX("whoosh");
            base.ResetScreen();
        }

        public override void ExitScreen()
        {
            BubblePop.sigletonGame.Exit();
        }

    }
}
