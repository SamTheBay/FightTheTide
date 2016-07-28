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
    class InstructionsScreen : MenuScreen
    {
        Texture2D bubbles;
        Texture2D mine;
        Texture2D shark;
        int playerIndex = 0;

        public InstructionsScreen(int playerIndex)
        {
            this.playerIndex = playerIndex;
            IsPopup = true;
            bubbles = InternalContentManager.GetTexture("Bubbles");
            shark = InternalContentManager.GetTexture("Shark");
            mine = InternalContentManager.GetTexture("mine");
        }


        public override void HandleInput()
        {
            base.HandleInput();

            if (InputManager.IsLocationPressed(BubblePop.ScreenSize))
            {
                GameplayScreen screen = BubblePop.gameplayScreen;
                screen.ResetGame(playerIndex);
                AddNextScreenAndExit(screen);
            }
        }



        Vector2 instruction1 = new Vector2(480 / 2, 800 / 2 - 150);
        Vector2 instruction2 = new Vector2(480 / 2, 800 / 2 - 50);
        Vector2 instruction3 = new Vector2(480 / 2, 800 / 2 - 20);
        Vector2 boost1 = new Vector2(480 / 2, 800 / 2 + 80);
        Vector2 boost2 = new Vector2(480 / 2, 800 / 2 + 110);
        Vector2 boost3 = new Vector2(480 / 2, 800 / 2 + 140);
        Vector2 boost4 = new Vector2(480 / 2, 800 / 2 + 290);
        Vector2 scoreLocation = new Vector2(10, 10);
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Tilt to Move", instruction1, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Avoid the Mines", instruction2, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "and Sharks", instruction3, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Pop the Bubbles to", boost1, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Keep the Water Level Up", boost2, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "and Earn Points", boost3, Color.White);

            spriteBatch.Draw(bubbles, new Vector2(0, 800 / 2 + 160), Color.White);
            spriteBatch.Draw(mine, new Vector2(160 - mine.Width / 2, 800 / 2 + 5), Color.White);
            spriteBatch.Draw(shark, new Vector2(320 - 70 / 2, 800 / 2 + 5), new Rectangle(0,0,70,40), Color.White);

            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Touch Screen to Start", boost4, Color.Red);

            spriteBatch.End();
        }

    }
}
