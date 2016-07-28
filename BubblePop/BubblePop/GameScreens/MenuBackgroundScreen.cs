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
    class MenuBackgroundScreen : GameScreen
    {
        Texture2D title;
        Vector2 titleLoc;
        Texture2D bg;
        Texture2D grayOut;

        public MenuBackgroundScreen()
        {
            title = GameSprite.game.Content.Load<Texture2D>("Textures/Title");
            titleLoc = new Vector2(240 - title.Width / 2, 45);
            bg = GameSprite.game.Content.Load<Texture2D>("Textures/aquarium_bg");
            grayOut = InternalContentManager.GetTexture("Gray");
        }


        Vector2 scoreLocation = new Vector2(10, 10);
        public override void  Draw(GameTime gameTime)
        {
 	         base.Draw(gameTime);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // greyed out background
            spriteBatch.Draw(bg, Vector2.Zero, Color.White);
            spriteBatch.Draw(grayOut, BubblePop.ScreenSize, Color.White);


            spriteBatch.Draw(title, titleLoc, Color.White);

            spriteBatch.DrawString(Fonts.DescriptionFont, GameplayScreen.player.PointsString, scoreLocation, Color.White);

            Vector2 stringSize = Fonts.DescriptionFont.MeasureString(GameplayScreen.highScoreString);
            stringSize.X = 470 - stringSize.X;
            stringSize.Y = 10;
            spriteBatch.DrawString(Fonts.DescriptionFont, GameplayScreen.highScoreString, stringSize, Color.White);

            spriteBatch.End();
        }
    }
}
