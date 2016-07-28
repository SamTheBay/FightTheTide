
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

namespace BubblePop
{

    public enum GenerationMode
    {
        Random,
        Linear,
        Grouped,
        Num
    }



    public class GameplayScreen : GameScreen
    {
        public static GameplayScreen singleton;
        public static PlayerSprite player;
        Rectangle screenRect = new Rectangle(0, 0, 480, 800);
        bool isPlaying = false;
        public static int highScore;
        public static string highScoreString;
        Texture2D bg;
        Texture2D waterColor;

        List<GameSprite> sprites = new List<GameSprite>(0);
        float actionSpeedFactor = 1.0f;
        float actionSpeedAdjust = .85f;

        BubbleSprite[] bubbles = new BubbleSprite[100];
        int addBubbleDuration = 1500;
        int addBubbleElapsed = 1500;
        int addBurstDurration = 8000;
        int addBurstElapsed = 8000;
        int addLinearDuration = 13000;
        int addLinearElapsed = 13000;
        int addExtraBubbleDuration = 3000;
        int addExtraBubbleElapsed = 3000;

        MineSprite[] mines = new MineSprite[10];
        int addMineDuration = 10000;
        int addMineElapsed = 0;

        SharkSprite shark = new SharkSprite();
        int addSharkDuration = 15000;
        int addSharkElapsed = 0;

        WaterTopSprite waterTop = new WaterTopSprite();
        int waterHeight = 50;
        int targetHeight = 50;
        int gameOverHeight = 620;


        GenerationMode currentMode = GenerationMode.Linear;
        int changeFunctionDuration = 15000;
        int changeFunctionElapsed = 0;


        public int level = 1;
        int currentPops = 0;
        int nextLevelPops = 20;
        int prevLevelPops = 0;
        int levelUpDuration = 3000;
        int levelUpElapsed = 0;
        int unlockedDuration = 3000;
        int unlockedElapsed = 3000;


        public GameplayScreen()
            : base()
        {
            singleton = this;

            bg = GameSprite.game.Content.Load<Texture2D>("Textures/aquarium_bg");
            waterColor = InternalContentManager.GetTexture("Blue");
            EnabledGestures = GestureType.None;
            player = new PlayerSprite(1);

            ParticleSystem.Initialize(100);

            LoadHighScore();

            for (int i = 0; i < bubbles.Length; i++)
            {
                bubbles[i] = new BubbleSprite();
                sprites.Add(bubbles[i]);
            }

            for (int i = 0; i < mines.Length; i++)
            {
                mines[i] = new MineSprite();
                sprites.Add(mines[i]);
            }
            sprites.Add(shark);

            waterTop.Activate();

            ResetGame(0);
            isPlaying = false;
        }


        public void LoadHighScore()
        {
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();

            IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("HighScore",
                FileMode.OpenOrCreate,
                FileAccess.Read,
                isoFile);

            BinaryReader reader = new BinaryReader(isoStream);

            // read out high score
            try
            {
                highScore = reader.ReadInt32();
            }
            catch (Exception)
            {
                highScore = 0;
            }
            highScoreString = "Best: " + highScore.ToString();

            reader.Close();
            isoStream.Close();
        }


        public void SaveHighScore()
        {
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();

            IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("HighScore",
                FileMode.Create,
                FileAccess.Write,
                isoFile);

            BinaryWriter writer = new BinaryWriter(isoStream);

            // write high score
            writer.Write((Int32)highScore);
            highScoreString = "Best: " + highScore.ToString();

            writer.Close();
            isoStream.Close();
        }


        public void ResetGame(int playerIndex)
        {
            waterHeight = 50;
            targetHeight = 50;
            waterTop.position.Y = waterHeight - waterTop.FrameDimensions.Y;
            addBubbleElapsed = addBubbleDuration;
            addLinearElapsed = addLinearDuration;
            addBurstElapsed = addBurstDurration;
            addMineElapsed = addMineDuration;
            level = 1;
            currentPops = 0;
            nextLevelPops = 30;
            prevLevelPops = 0;
            actionSpeedFactor = 1.0f;
            isPlaying = true;

            player = new PlayerSprite(playerIndex);

            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i] != null && sprites[i].IsActive)
                {
                    sprites[i].Deactivate();
                }
            }
        }





        public override void LoadContent()
        {
            ScreenManager.Game.ResetElapsedTime();
        }



        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive && !coveredByOtherScreen)
            {
                ParticleSystem.Update(gameTime);

                if (isPlaying)
                {
                    if (levelUpElapsed < levelUpDuration)
                    {
                        levelUpElapsed += gameTime.ElapsedGameTime.Milliseconds;
                    }
                    if (unlockedElapsed < unlockedDuration)
                    {
                        unlockedElapsed += gameTime.ElapsedGameTime.Milliseconds;
                    }

                    // update player
                    player.Update(gameTime);

                    RunBubbleActivation(gameTime);


                    for (int i = 0; i < sprites.Count; i++)
                    {
                        if (sprites[i] != null && sprites[i].IsActive)
                        {
                            sprites[i].Update(gameTime);
                        }
                    }

                    if (targetHeight > waterHeight && waterHeight <= gameOverHeight)
                    {
                        waterHeight++;
                        if (waterHeight > gameOverHeight && !player.IsDead)
                        {
                            player.Die();
                        }
                    }
                    else if (targetHeight < waterHeight)
                    {
                        waterHeight--;
                    }

                    waterTop.position.Y = waterHeight - waterTop.FrameDimensions.Y;
                    waterTop.Update(gameTime);

                    // check for collision
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        if (sprites[i] != null && sprites[i].IsActive)
                        {
                            bool collision = player.CollisionDetect(sprites[i]);
                            if (collision)
                                sprites[i].CollisionAction(player);
                            else
                            {
                                collision = waterTop.CollisionDetect(sprites[i]);
                                if (collision)
                                    sprites[i].CollisionAction(waterTop);
                            }
                        }
                    }


                    // check for end of game
                    if (player.IsActive == false)
                    {
                        isPlaying = false;
                        if (player.Points > highScore)
                        {
                            highScore = player.Points;
                            SaveHighScore();
                        }
                    }

                }
                else
                {
                    AddNextScreenAndExit(new LeaderboardScreen());
                }

            }
        }



        public void FishUnlocked()
        {
            unlockedElapsed = 0;
        }

        public override void HandleInput()
        {
            if (InputManager.IsBackTriggered())
            {
                AddNextScreen(new PauseScreen());
            }
        }


        public void LevelUp()
        {
            actionSpeedFactor *= actionSpeedAdjust;
            levelUpElapsed = 0;
            int lastnext = nextLevelPops;
            int increase = (int)((nextLevelPops - prevLevelPops) * 1.4f);
            if (increase > 200)
                increase = 200;
            nextLevelPops = nextLevelPops + increase;
            prevLevelPops = lastnext;
            level++;
            targetHeight -= 100;
            if (targetHeight < 50)
                targetHeight = 50;

            if (level == 3)
            {
                currentMode = GenerationMode.Grouped;
                changeFunctionElapsed = 0;
            }
            else if (level == 4)
            {
                currentMode = GenerationMode.Linear;
                changeFunctionElapsed = 0;
            }
        }


        Vector2 scoreLocation = new Vector2(10, 10);
        Vector2 centerText = new Vector2(480 / 2, 800 / 2);
        Vector2 centerText2 = new Vector2(480 / 2, 800 / 2 + 30);
        Rectangle waterRect = new Rectangle(0,50, 480, 800);
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            spriteBatch.Draw(bg, Vector2.Zero, Color.White);

            waterRect.Y = WaterHeight;
            spriteBatch.Draw(waterColor, waterRect, Color.White);

            player.Draw(spriteBatch, 0f);

            spriteBatch.DrawString(Fonts.DescriptionFont, player.PointsString, scoreLocation, Color.White);

            Vector2 stringSize = Fonts.DescriptionFont.MeasureString(highScoreString);
            stringSize.X = 470 - stringSize.X;
            stringSize.Y = 10;
            spriteBatch.DrawString(Fonts.DescriptionFont, highScoreString, stringSize, Color.White);

            for (int i = 0; i < sprites.Count; i++)
            {
                if (sprites[i] != null && sprites[i].IsActive)
                {
                    sprites[i].Draw(spriteBatch, 0f);
                }
            }

            waterTop.Draw(spriteBatch, 0f);

            int height = (int)(800f * (currentPops - prevLevelPops) / (nextLevelPops - prevLevelPops));
            spriteBatch.Draw(InternalContentManager.GetTexture("Blank"),
                new Rectangle(0, 800 - height, 5, height),
                Color.Purple);

            if (levelUpElapsed < levelUpDuration)
            {
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Level " + level.ToString(), centerText, Color.Azure);
            }
            else if (unlockedElapsed < unlockedDuration)
            {
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "New Fish Unlocked!", centerText, Color.Azure);
            }
            else if (player.Stunned)
            {
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Tap Screen Quickly", centerText, Color.Azure);
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "to Recover", centerText2, Color.Azure);
            }

            spriteBatch.End();

            ParticleSystem.Draw(spriteBatch, gameTime);
        }



        public void LowerWaterHeight(int amount)
        {
            targetHeight += amount;
        }



        public void RunBubbleActivation(GameTime gameTime)
        {

            changeFunctionElapsed += gameTime.ElapsedGameTime.Milliseconds;
            if (changeFunctionElapsed > changeFunctionDuration)
            {
                changeFunctionElapsed = 0;
                currentMode = (GenerationMode)Util.Random.Next(0, (int)GenerationMode.Num);
                addBubbleElapsed = addBubbleDuration;
                addBurstElapsed = addBurstDurration;
                addLinearElapsed = addLinearDuration;
            }



            if (currentMode == GenerationMode.Grouped && level >= 3)
            {
                addBurstElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (addBurstElapsed > (int)addBurstDurration * actionSpeedFactor)
                {
                    Vector2 center = new Vector2(Util.Random.Next(10, 370), 810);
                    for (int i = 0; i < 10; i++)
                    {
                        ActivateBubble(center + new Vector2((float)Util.Random.NextDouble() * 100, (float)Util.Random.NextDouble() * 100), 4);
                    }
                    addBurstElapsed = 0;
                }
            }
            else if (currentMode == GenerationMode.Linear && level >= 4)
            {
                addLinearElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (addLinearElapsed > (int)addLinearDuration * actionSpeedFactor)
                {
                    int direction = Util.Random.Next(0, 2);
                    if (direction == 0)
                    {
                        Vector2 center = new Vector2(20, 810);
                        while (center.X < 360)
                        {
                            ActivateBubble(center, 3, 1);
                            center.X += 30;
                            center.Y += 30;
                        }
                    }
                    else
                    {
                        Vector2 center = new Vector2(360, 810);
                        while (center.X > 10)
                        {
                            ActivateBubble(center, 3, 1);
                            center.X -= 30;
                            center.Y += 30;
                        }
                    }
                    addLinearElapsed = 0;
                }
            }
            else
            {
                addBubbleElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (addBubbleElapsed > (int)addBubbleDuration * actionSpeedFactor)
                {
                    ActivateBubble();
                    addBubbleElapsed = 0;
                }
            }

            addExtraBubbleElapsed += gameTime.ElapsedGameTime.Milliseconds;
            if (addExtraBubbleElapsed > (int)addExtraBubbleDuration * actionSpeedFactor)
            {
                ActivateBubble();
                addExtraBubbleElapsed = 0;
            }


            addMineElapsed += gameTime.ElapsedGameTime.Milliseconds;
            if (addMineElapsed > (int)addMineDuration * actionSpeedFactor && level >= 2)
            {
                ActivateMine();
                addMineElapsed = 0;
            }


            if (level >= 5)
            {
                addSharkElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (addSharkElapsed > (int)addSharkDuration * actionSpeedFactor)
                {
                    if (shark.IsActive == false)
                    {
                        shark.Activate();
                        addSharkElapsed = 0;
                    }
                }
            }
        }


        public void ActivateBubble()
        {
            for (int i = 0; i < bubbles.Length; i++)
            {
                if (bubbles[i].IsActive == false)
                {
                    bubbles[i].Activate();
                    return;
                }
            }
        }


        public void ActivateBubble(Vector2 position, int speed, int size)
        {
            for (int i = 0; i < bubbles.Length; i++)
            {
                if (bubbles[i].IsActive == false)
                {
                    bubbles[i].Activate();
                    bubbles[i].position = position;
                    bubbles[i].SetSpeed(speed);
                    bubbles[i].SetSize(size);
                    return;
                }
            }
        }



        public void ActivateBubble(Vector2 position, int speed)
        {
            for (int i = 0; i < bubbles.Length; i++)
            {
                if (bubbles[i].IsActive == false)
                {
                    bubbles[i].Activate();
                    bubbles[i].position = position;
                    bubbles[i].SetSpeed(speed);
                    return;
                }
            }
        }



        public void ActivateMine()
        {
            for (int i = 0; i < mines.Length; i++)
            {
                if (mines[i].IsActive == false)
                {
                    mines[i].Activate();
                    return;
                }
            }
        }


        public void BubblePopped()
        {
            currentPops++;
            if (currentPops > nextLevelPops)
            {
                LevelUp();
            }
        }


        public int WaterHeight
        {
            get { return waterHeight; }
        }


        public bool IsPlaying
        {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

    }
}
