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
    class SharkSprite : AnimatedSprite
    {
        float speed = 3f;
        int direction = 0;

        public SharkSprite()
            : base("Shark", new Point(70, 40), new Point(35, 20), 4, Vector2.Zero, new Vector2(480 / 2 - 32, -200))
        {
            AddAnimation(new Animation("SwimRight", 1, 4, 200, true, SpriteEffects.None));
            AddAnimation(new Animation("SwimLeft", 1, 4, 200, true, SpriteEffects.FlipHorizontally));
            PlayAnimation("SwimRight");
        }

        public override void Activate()
        {
            base.Activate();

            direction = Util.Random.Next(0, 2);
            position.Y = Util.Random.Next(GameplayScreen.singleton.WaterHeight, 700);
            if (direction == 0)
            {
                PlayAnimation("SwimRight");
                position.X = -100;
                currentAnimation.SpriteEffect = SpriteEffects.None;
            }
            else
            {
                PlayAnimation("SwimLeft");
                position.X = 520;
                currentAnimation.SpriteEffect = SpriteEffects.FlipHorizontally;
            }
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (direction == 0)
            {
                position.X += speed;
                if (position.X > 520)
                {
                    Deactivate();
                }
            }
            else
            {
                position.X -= speed;
                if(position.X < - 100)
                {
                    Deactivate();
                }
            }

            if (position.Y < GameplayScreen.singleton.WaterHeight)
            {
                position.Y = GameplayScreen.singleton.WaterHeight;
            }
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            base.CollisionAction(otherSprite);

            if (otherSprite is PlayerSprite)
            {
                if (!GameplayScreen.player.Stunned)
                {
                    AudioManager.audioManager.PlaySFX("crunch");
                    GameplayScreen.player.Stun();
                }
            }

        }
    }
}
