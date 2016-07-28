using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Phone.Tasks;


namespace BubblePop
{
    class OtherGamesScreen : MenuScreen
    {


        public OtherGamesScreen()
        {
            IsPopup = true;

            MenuEntry entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/gumzoobo");
            entry.Position = new Vector2(120 - entry.Texture.Width / 2, 225);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Antastic");
            entry.Position = new Vector2(360 - entry.Texture.Width / 2, 225);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/wormhole");
            entry.Position = new Vector2(240 - entry.Texture.Width / 2, 475);
            MenuEntries.Add(entry);
        }

        void entry_Selected(object sender, EventArgs e)
        {
            if (selectorIndex == 0)
            {
                MarketplaceDetailTask task = new MarketplaceDetailTask();
                task.ContentIdentifier = "f99f089d-cf03-e011-9264-00237de2db9e";
                task.ContentType = MarketplaceContentType.Applications;
                task.Show();
            }
            if (selectorIndex == 1)
            {
                MarketplaceDetailTask task = new MarketplaceDetailTask();
                task.ContentIdentifier = "5d55de49-6d22-e011-854c-00237de2db9e";
                task.ContentType = MarketplaceContentType.Applications;
                task.Show();
            }
            else if (selectorIndex == 2)
            {
                MarketplaceDetailTask task = new MarketplaceDetailTask();
                task.ContentIdentifier = "135a3b7c-3a0d-e011-9264-00237de2db9e";
                task.ContentType = MarketplaceContentType.Applications;
                task.Show();
            }
        }


        Vector2 scoreLocation = new Vector2(10, 10);
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "Gumzoobo!", new Vector2(120, 425), Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "Antastic", new Vector2(360, 425), Color.White);
            Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "Worm Hole", new Vector2(240, 675), Color.White);

            spriteBatch.End();
        }


    }
}
