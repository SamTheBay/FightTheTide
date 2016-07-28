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
    public class PlayerSprite : AnimatedSprite
    {
        public int lastHighScoreIndex = 11;
        private const float AccelerometerScale = 1.75f;
        protected Direction lastDirection = Direction.Right;

        // input
        protected bool previousMoveRightPressed = false;
        protected bool previousMoveLeftPressed = false;
        protected bool previousJumpPressed = false;
        protected bool previousSpecialPressed = false;
        protected bool previousShootPressed = false;
        protected bool moveRightPressed = false;
        protected bool moveLeftPressed = false;
        protected bool moveRightTriggered = false;
        protected bool moveLeftTriggered = false;
        protected bool previousScreenTapPressed = false;
        protected bool screenTapPressed = false;
        protected bool screenTapTriggered = false;

        // characteristics
        protected float movementSpeed = 15f;
        protected int deadDuration = 3000;
        int stunDuration = 10;
        int stunElapsed = 10;
        StatusBar statusBar = new StatusBar(10, 32);

        // state
        protected bool isDead = false;
        protected int deadElapsed = 0;

        // stats
        protected int points = 0;

        Vector2 particleOffset;


        public PlayerSprite(int index)
            : base(TextureFromIndex(index), FrameSizeFromIndex(index), new Point(18, 13), 9, Vector2.Zero, new Vector2(480 / 2 - 32, 800 - 80 - 120))
        {
            AddAnimation(new Animation("SwimRight", 1, 4, 200, true, SpriteEffects.None));
            AddAnimation(new Animation("SwimLeft", 1, 4, 200, true, SpriteEffects.FlipHorizontally));
            AddAnimation(new Animation("IdleRight", 1, 1, 200, false, SpriteEffects.None));
            AddAnimation(new Animation("IdleLeft", 1, 1, 200, false, SpriteEffects.FlipHorizontally));
            AddAnimation(new Animation("DieRight", 5, 7, 200, false, SpriteEffects.None));
            AddAnimation(new Animation("DieLeft", 5, 7, 200, false, SpriteEffects.FlipHorizontally));
            AddAnimation(new Animation("ReviveRight", 7, 9, 200, false, SpriteEffects.None));
            AddAnimation(new Animation("ReviveLeft", 7, 9, 200, false, SpriteEffects.FlipHorizontally));
            PlayAnimation("SwimRight");
            particleOffset = new Vector2(frameDimensions.X / 2, frameDimensions.Y - 10);
            Activate();
        }



        static public string TextureFromIndex(int index)
        {
            if (index == 0)
            {
                return "Fishy";
            }
            else if (index == 1)
            {
                return "AngelFish";
            }
            else if (index == 2)
            {
                return "CowFish";
            }
            else if (index == 3)
            {
                return "LionFish";
            }
            return "Fishy";
        }

        static public Point FrameSizeFromIndex(int index)
        {
            if (index == 0)
            {
                return new Point(36, 26);
            }
            else if (index == 1)
            {
                return new Point(36, 40);
            }
            else if (index == 2)
            {
                return new Point(38, 26);
            }
            else if (index == 3)
            {
                return new Point(36, 26);
            }
            return new Point(36, 26);
        }
        public override void Draw(SpriteBatch spriteBatch, float layerDepth)
        {
            base.Draw(spriteBatch, layerDepth);

            if (Stunned && !isDead)
            {
                statusBar.State = (float)stunElapsed / (float)stunDuration;
                statusBar.Draw(spriteBatch, (int)position.X, (int)position.Y - 14);
            }
        }




        public Direction LastDirection
        {
            get { return lastDirection; }
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            Die();
        }



        Vector2 movement = new Vector2(0, 0);
        public Vector2 CaptureInput()
        {
            AccelerometerState accelState = Accelerometer.GetState();

            movement.X = 0;
            movement.Y = 0;
            if (accelState.IsActive)
            {
                // set our movement speed
                movement.X = MathHelper.Clamp(accelState.Acceleration.X * AccelerometerScale, -1f, 1f);
                movement.Y = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale * 1.7f, .5f, 2.5f);
                movement.Y -= 1.5f;

                // set values of move left or move right
                if (!isDead && !Stunned && !(baseAnimationString == "Revive" && !IsPlaybackComplete))
                {
                    if (movement.X > 0.15f)
                    {
                        PlayAnimation("Swim", Direction.Right);
                        moveRightPressed = true;
                    }
                    else if (movement.X > 0f)
                    {
                        PlayAnimation("Idle", Direction.Right);
                        moveRightPressed = false;
                    }
                    else if (movement.X < -0.15f)
                    {
                        PlayAnimation("Swim", Direction.Left);
                        moveLeftPressed = true;
                    }
                    else
                    {
                        PlayAnimation("Idle", Direction.Left);
                        moveLeftPressed = false;
                    }
                }
            }

            // debugging input
            if (InputManager.IsKeyPressed(Keys.Left))
            {
                moveLeftPressed = true;
                movement.X = -.75f;
            }
            else
                moveLeftPressed = false;
            if (InputManager.IsKeyPressed(Keys.Right))
            {
                moveRightPressed = true;
                movement.X = .75f;
            }
            else
                moveRightPressed = false;
            if (InputManager.IsKeyPressed(Keys.Up))
            {
                movement.Y = -.75f;
            }
            if (InputManager.IsKeyPressed(Keys.Down))
            {
                movement.Y = .75f;
            }

            screenTapPressed = InputManager.IsLocationPressed(BubblePop.ScreenSize);

            if (!previousMoveLeftPressed && moveLeftPressed)
            {
                moveLeftTriggered = true;
            }
            else
            {
                moveLeftTriggered = false;
            }

            if (!previousMoveRightPressed && moveRightPressed)
            {
                moveRightTriggered = true;
            }
            else
            {
                moveRightTriggered = false;
            }

            if (!previousScreenTapPressed && screenTapPressed)
            {
                screenTapTriggered = true;
            }
            else
            {
                screenTapTriggered = false;
            }


            previousMoveRightPressed = moveRightPressed;
            previousMoveLeftPressed = moveLeftPressed;
            previousScreenTapPressed = screenTapPressed;


            return movement;
        }


        bool wasStunned = false;
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Vector2 movement = CaptureInput();

            if (isDead)
            {
                deadElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (deadElapsed > deadDuration)
                    Deactivate();
                return;
            }

            if (Stunned && screenTapTriggered)
            {
                stunElapsed++;
            }

            if (wasStunned == true && Stunned == false)
            {
                if (movement.X > 0)
                    PlayAnimation("Revive", Direction.Right);
                else
                    PlayAnimation("Revive", Direction.Left);
            }
            wasStunned = Stunned;

            // handle movement input
            if (!Stunned)
            {
                position.X += movementSpeed * movement.X;
                position.Y += movementSpeed * movement.Y;
            }
            else
            {
                position.Y -= 2;
            }

            if (position.X + frameDimensions.X > 480)
                position.X = 480 - frameDimensions.X;
            if (position.X < 0)
                position.X = 0;
            if (position.Y + frameDimensions.X > 720)
                position.Y = 720 - frameDimensions.X;
            if (position.Y < GameplayScreen.singleton.WaterHeight)
                position.Y = GameplayScreen.singleton.WaterHeight;
        }




        public void RewardPoints(int points)
        {
            // check for unlock
            for (int i = 0; i < TitleScreen.unlockedPoints.Length; i++)
            {
                if (this.points < TitleScreen.unlockedPoints[i] && this.points + points > TitleScreen.unlockedPoints[i] &&
                    GameplayScreen.highScore < TitleScreen.unlockedPoints[i])
                {
                    GameplayScreen.singleton.FishUnlocked();
                }
            }

            this.points += (int)(points * (1f + ((GameplayScreen.singleton.level - 1)/2)));
        }


        public void Stun()
        {
            stunElapsed = 0;
            if (movement.X > 0)
                PlayAnimation("Die", Direction.Right);
            else
                PlayAnimation("Die", Direction.Left);
        }


        public virtual void Die()
        {
            deadElapsed = 0;
            isDead = true;
            if (movement.X > 0)
                PlayAnimation("Die", Direction.Right);
            else
                PlayAnimation("Die", Direction.Left);
        }


        // Accessors
        public bool IsDead
        {
            get { return isDead; }
        }


        public int Points
        {
            get { return points; }
        }

        public String PointsString
        {
            get { return points.ToString(); }
        }

        public bool Stunned
        {
            get { return stunElapsed < stunDuration; }
        }


    }
}