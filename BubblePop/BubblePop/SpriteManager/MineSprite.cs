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
    class MineSprite : AnimatedSprite
    {
        float moveSpeed = 3f;

        public MineSprite()
            : base("mine", new Point(34, 36), new Point(17, 18), 1, Vector2.Zero, new Vector2(480 / 2 - 32, -200))
        {
            AddAnimation(new Animation("Idle", 1, 1, 100, false, SpriteEffects.FlipHorizontally));
            PlayAnimation("Idle");
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            position.Y -= moveSpeed;

            if (position.Y < -frameDimensions.Y)
            {
                Explode();
            }
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            base.CollisionAction(otherSprite);
            Explode();

            if (otherSprite is PlayerSprite)
            {
                PlayerSprite player = (PlayerSprite)otherSprite;
                player.Stun();
            }

        }


        public void Explode()
        {
            Deactivate();
            ParticleSystem.AddParticles(position + new Vector2(frameDimensions.X / 2, frameDimensions.Y / 2),
                ParticleType.Explosion, sizeScale: 2f, lifetimeScale: 1f);
            AudioManager.audioManager.PlaySFX("explosion");
        }


        public override void Activate()
        {
            base.Activate();

            position.Y = 900;
            position.X = Util.Random.Next(0, 480 - frameDimensions.X);
            moveSpeed = Util.Random.Next(2, 6);
        }
    }
}
