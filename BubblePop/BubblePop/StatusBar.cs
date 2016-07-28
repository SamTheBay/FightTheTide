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
    class StatusBar
    {
        Rectangle back;
        Rectangle fill;
        float state = 0f;
        Texture2D tex;

        public StatusBar(int height, int width)
        {
            back = new Rectangle(0, 0, width, height);
            fill = new Rectangle(0, 0, width, height);
            tex = InternalContentManager.GetTexture("Blank");
        }


        public void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            back.X = x;
            back.Y = y;
            fill.X = x;
            fill.Y = y;
            fill.Width = (int)(back.Width * state);

            spriteBatch.Draw(tex, back, Color.DarkGray);
            spriteBatch.Draw(tex, fill, Color.Red);
        }



        public float State
        {
            set { state = value; }
            get { return state; }
        }


    }
}
