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
    class BubbleSprite : AnimatedSprite
    {
        float moveSpeed = 3f;
        int size;

        public BubbleSprite()
            : base("Bubbles", new Point(120, 120), new Point(60, 60), 9, Vector2.Zero, new Vector2(480 / 2 - 32, -200))
        {
            size = Util.Random.Next(0, 4);

            AddAnimation(new Animation("Idle", size + 1, size + 1, 100, false, SpriteEffects.None));
            PlayAnimation("Idle");
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            position.Y -= moveSpeed;

            if (position.Y < -frameDimensions.Y)
            {
                Pop();
            }
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            base.CollisionAction(otherSprite);
            Pop();

            if (otherSprite is WaterTopSprite)
            {
                if (size == 0)
                    GameplayScreen.singleton.LowerWaterHeight(5);
                else if (size == 1)
                    GameplayScreen.singleton.LowerWaterHeight(10);
                else if (size == 2)
                    GameplayScreen.singleton.LowerWaterHeight(20);
                else
                    GameplayScreen.singleton.LowerWaterHeight(40);
            }
            else if (otherSprite is PlayerSprite)
            {
                GameplayScreen.singleton.BubblePopped();
                if (size == 0)
                    GameplayScreen.player.RewardPoints(5);
                else if (size == 1)
                    GameplayScreen.player.RewardPoints(10);
                else if (size == 2)
                    GameplayScreen.player.RewardPoints(20);
                else
                    GameplayScreen.player.RewardPoints(40);
            }

        }


        public void Pop()
        {
            Deactivate();
            AudioManager.audioManager.PlaySFX("pop");
        }


        public override void Activate()
        {
            base.Activate();

            position.Y = 900;
            position.X = Util.Random.Next(0, 480 - frameDimensions.X);
            moveSpeed = Util.Random.Next(2, 6);
            size = Util.Random.Next(0, 100);
            if (size < 40)
            {
                SetSize(0);
            }
            else if (size < 70)
            {
                SetSize(1);
            }
            else if (size < 90)
            {
                SetSize(2);
            }
            else
            {
                SetSize(3);
            }
            ResetAnimation();
        }


        public void SetSpeed(int speed)
        {
            moveSpeed = speed;
        }


        public void SetSize(int newsize)
        {
            currentAnimation.StartingFrame = currentAnimation.EndingFrame = newsize + 1;
            size = newsize;
            ResetAnimation();
        }
    }
}
