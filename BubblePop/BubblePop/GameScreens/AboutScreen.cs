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
    class AboutScreen : MenuScreen
    {
        public AboutScreen()
        {
            IsPopup = true;
        }



        Vector2 instruction1 = new Vector2(480 / 2, 800 / 2 - 150);

        Vector2 instruction2 = new Vector2(480 / 2, 800 / 2 - 50);
        Vector2 boost3 = new Vector2(480 / 2, 800 / 2 + - 20);
        Vector2 boost4 = new Vector2(480 / 2, 800 / 2 + + 20);

        Vector2 instruction3 = new Vector2(480 / 2, 800 / 2 + 120);
        Vector2 boost1 = new Vector2(480 / 2, 800 / 2 + 160);
        Vector2 boost2 = new Vector2(480 / 2, 800 / 2 + 200);

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Version: 1.4", instruction1, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Publisher:", instruction2, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Awesomer Than Your", boost3, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Games", boost4, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Please send feedback,", instruction3, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "suggestions and bugs to", boost1, Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "s.m.bayless@gmail.com", boost2, Color.White);
           

            spriteBatch.End();
        }
    }
}
