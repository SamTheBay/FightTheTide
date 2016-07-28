using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;


namespace BubblePop
{
    class WaterTopSprite : AnimatedSprite
    {

        public WaterTopSprite()
            : base("waterTop", new Point(480, 40), new Point(240, 20), 1, Vector2.Zero, new Vector2(0, 0))
        {
            AddAnimation(new Animation("Idle", 1, 4, 200, true, SpriteEffects.FlipHorizontally));
            PlayAnimation("Idle");
        }

    }
}
