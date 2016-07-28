using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.GamerServices;

namespace BubblePop
{

    public class HighScoreRecord
    {
        public string name;
        public int score;
        public int index;

        public HighScoreRecord(XElement element)
        {
            IEnumerable<XAttribute> childrenNodes = element.Attributes();
            foreach (XAttribute attribute in childrenNodes)
            {
                if (attribute.Name == "Name")
                {
                    name = CleanString(attribute.Value);
                }
                else if (attribute.Name == "Score")
                {
                    score = Convert.ToInt32(attribute.Value);
                }
                else if (attribute.Name == "Index")
                {
                    index = Convert.ToInt32(attribute.Value);
                }

            }
        }


        public string CleanString(string orig)
        {
            char[] scrubbedName = new char[orig.Length];
            int scrubLength = 0;
            for (int i = 0; i < orig.Length && scrubLength < 10; i++)
            {
                if (char.IsLetterOrDigit(orig[i]) && Fonts.HeaderFont.Characters.Contains(orig[i]))
                {
                    scrubbedName[scrubLength] = orig[i];
                    scrubLength++;
                }
            }
            return new string(scrubbedName, 0, scrubLength);
        }

    }


    class LeaderboardScreen : MenuScreen
    {
        static LeaderboardScreen instance;
        HttpWebRequest webRequest;
        IAsyncResult saveResult;
        HighScoreRecord[,] records = new HighScoreRecord[3,10];
        int[] youScores = new int[3];
        static int lastSentScore = 0;
        bool scoreToSend = false;
        bool sendingScore = false;
        bool sendFailed = false;
        static int requestIndex = 0;
        int selectedSet = 0;
        int sendScoreElapsed = 0;
        int sendScoreDuration = 20000;
        bool isGettingName = false;
        string sendName;
        IAsyncResult nameResult = null;
        Texture2D bg;
        Texture2D grayOut;
        Texture2D title;
        Vector2 titleLoc;
        static string lastSendName = "";

        public LeaderboardScreen()
        {
            //IsPopup = true;
            instance = this;
            LoadHighScores();
            LoadLastScoreName();

            title = GameSprite.game.Content.Load<Texture2D>("Textures/Leaderboard");
            titleLoc = new Vector2(240 - title.Width / 2, 35);
            bg = GameSprite.game.Content.Load<Texture2D>("Textures/aquarium_bg");
            grayOut = InternalContentManager.GetTexture("Gray");

            MenuEntry entry;
            if (GameplayScreen.player.Points != 0 && GameplayScreen.player.Points != lastSentScore)
            {
                scoreToSend = true;
                entry = new MenuEntry("");
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                entry.Font = Fonts.HeaderFont;
                entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/Submit");
                entry.PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/SubmitPress");
                entry.Position = new Vector2(240 - entry.Texture.Width / 2, 720 - entry.Texture.Height);
                MenuEntries.Add(entry);
            }
            else
            {
                scoreToSend = false;
                entry = new MenuEntry("");
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                entry.Font = Fonts.HeaderFont;
                entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/SubmitOut");
                entry.PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/SubmitOut");
                entry.Position = new Vector2(240 - entry.Texture.Width / 2, 720 - entry.Texture.Height);
                MenuEntries.Add(entry);
            }

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/Month");
            entry.PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/MonthPress");
            entry.Position = new Vector2(40, 135);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/Week");
            entry.PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/WeekPress");
            entry.Position = new Vector2(240 - entry.Texture.Width / 2, 135);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Font = Fonts.HeaderFont;
            entry.Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/Day");
            entry.PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/DayPress");
            entry.Position = new Vector2(440 - entry.Texture.Width, 135);
            MenuEntries.Add(entry);
        }


        public override void OnRemoval()
        {
            base.OnRemoval();

            if (sendingScore)
            {
                webRequest.Abort();
            }
        }


        public void SendScoreRequest()
        {
            string cypher = Encryption64.Encrypt("FightTheTide+addNewScore+" + sendName + "+" + GameplayScreen.player.Points.ToString(), "q{Fe%xR:q{Fe%xR:");
            webRequest = (HttpWebRequest)WebRequest.CreateHttp(new Uri("http://69.67.29.80:8080/FightTheTide?id=" + cypher, UriKind.Absolute));
            //webRequest = (HttpWebRequest)WebRequest.CreateHttp(new Uri("http://127.0.0.1:8080/Wormhole?id=" + cypher, UriKind.Absolute));
            saveResult = webRequest.BeginGetResponse(new AsyncCallback(HTTPCallBack), null);
            sendScoreElapsed = 0;
            requestIndex++;
        }



        public void SendFailed()
        {
            sendFailed = true;
            sendingScore = false;
            MenuEntries[0].Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/Submit");
            MenuEntries[0].PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/SubmitPress");
            scoreToSend = true;
        }


        void entry_Selected(object sender, EventArgs e)
        {
            if (selectorIndex == 0)
            {
                if (scoreToSend == true)
                {
                    nameResult = Guide.BeginShowKeyboardInput(PlayerIndex.One, "Name", "Up to 13 characters", lastSendName, null, null);
                    isGettingName = true;
                }
            }
            else if (selectorIndex == 1)
            {
                selectedSet = 2;
            }
            else if (selectorIndex == 2)
            {
                selectedSet = 1;
            }
            else if (selectorIndex == 3)
            {
                selectedSet = 0;
            }
        }


        private static void SaveHighScores(XDocument root)
        {
            try
            {
                IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();

                IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("Leaderboard",
                    FileMode.Create,
                    FileAccess.Write,
                    isoFile);

                root.Save(isoStream);

                isoStream.Close();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                instance.ClearData();
            }
        }


        private void LoadHighScores()
        {
            IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();

            IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("Leaderboard",
                FileMode.OpenOrCreate,
                FileAccess.Read,
                isoFile);

            try
            {
                XDocument root = XDocument.Load(isoStream);
                ParseHighScores(root);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ClearData();
            }

            isoStream.Close();
        }



        private static void SaveLastScoreName()
        {
            IsolatedStorageFileStream isoStream = null;
            BinaryWriter writer = null;

            try
            {
                IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();

                isoStream = new IsolatedStorageFileStream("ScoreName",
                    FileMode.Create,
                    FileAccess.Write,
                    isoFile);

                writer = new BinaryWriter(isoStream);

                writer.Write(lastSendName);
            }
            catch (Exception)
            {
                lastSendName = "";
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (isoStream != null)
                    isoStream.Close();
            }
        }


        private void LoadLastScoreName()
        {
            BinaryReader reader = null;
            IsolatedStorageFileStream isoStream = null;

            try
            {
                IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();

                isoStream = new IsolatedStorageFileStream("ScoreName",
                    FileMode.OpenOrCreate,
                    FileAccess.Read,
                    isoFile);


                reader = new BinaryReader(isoStream);
                lastSendName = reader.ReadString();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ClearData();
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (isoStream != null)
                    isoStream.Close();
            }

            
        }


        public void ClearData()
        {
            for (int i = 0; i < records.GetLength(0); i++)
            {
                for (int j = 0; j < records.GetLength(1); j++)
                {
                    records[i, j] = null;
                }
            }
            for (int i = 0; i < youScores.Length; i++)
            {
                youScores[i] = 0;
            }
        }


        private static void HTTPCallBack(IAsyncResult asyncResult)
        {
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)instance.webRequest.EndGetResponse(asyncResult);
                instance.ClearData();

                Stream stream = response.GetResponseStream();
                XDocument root = XDocument.Load(stream);
                instance.ParseHighScores(root);

                instance.sendingScore = false;
                instance.scoreToSend = false;
                lastSentScore = GameplayScreen.player.Points;

                SaveHighScores(root);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                instance.SendFailed();
                instance.ClearData();
            }
        }



        private void ParseHighScores(XDocument root)
        {

            IEnumerable<XElement> childrenNodes3 = root.Elements();
            foreach (XElement element3 in childrenNodes3)
            {
                if (element3.Name == "HighScores")
                {
                    IEnumerable<XElement> childrenNodes = element3.Elements();
                    foreach (XElement element in childrenNodes)
                    {
                        int highScoreIndex = 0;
                        if (element.Name == "Day" || element.Name == "Week" || element.Name == "Month")
                        {
                            int set = 0;
                            if (element.Name == "Day")
                            {
                                set = 0;
                            }
                            else if (element.Name == "Week")
                            {
                                set = 1;
                            }
                            else if (element.Name == "Month")
                            {
                                set = 2;
                            }

                            IEnumerable<XElement> childrenNodes2 = element.Elements();
                            foreach (XElement element2 in childrenNodes2)
                            {
                                if (element2.Name == "HighScore")
                                {
                                    instance.records[set, highScoreIndex] = new HighScoreRecord(element2);
                                    highScoreIndex++;
                                }
                                else if (element2.Name == "You")
                                {
                                    IEnumerable<XAttribute> childrenNodes4 = element2.Attributes();
                                    foreach (XAttribute at in childrenNodes4)
                                    {
                                        if (at.Name == "Index")
                                        {
                                            instance.youScores[set] = Convert.ToInt32(at.Value);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (isGettingName == true && nameResult != null && nameResult.IsCompleted == true)
            {
                sendName = Guide.EndShowKeyboardInput(nameResult);
                nameResult = null;
                isGettingName = false;

                if (sendName == null || sendName == "")
                    return;

                // scrub name
                char[] scrubbedName = new char[sendName.Length];
                int scrubLength = 0;
                for (int i = 0; i < sendName.Length && scrubLength < 10; i++)
                {
                    if (char.IsLetterOrDigit(sendName[i]))
                    {
                        scrubbedName[scrubLength] = sendName[i];
                        scrubLength++;
                    }
                }
                sendName = new string(scrubbedName, 0, scrubLength);

                if (sendName != null && sendName != "")
                {
                    lastSendName = sendName;
                    SaveLastScoreName();
                    SendScoreRequest();
                    sendingScore = true;
                    MenuEntries[0].Texture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/SubmitOut");
                    MenuEntries[0].PressTexture = GameSprite.game.Content.Load<Texture2D>("Textures/Buttons/SubmitOut");
                }
            }

            if (sendingScore)
            {
                sendScoreElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (sendScoreElapsed > sendScoreDuration)
                {
                    webRequest.Abort();
                    SendFailed();
                }
            }
        }


        Vector2 textLoc = new Vector2();
        Vector2 scoreLoc = new Vector2(240, 630);
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // greyed out background
            spriteBatch.Draw(bg, Vector2.Zero, Color.White);
            spriteBatch.Draw(grayOut, BubblePop.ScreenSize, Color.White);
            spriteBatch.Draw(title, titleLoc, Color.White);

            SpriteFont font = Fonts.DescriptionFont;

            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, GameplayScreen.player.PointsString, scoreLoc, Color.Green);

            if (sendingScore)
            {
                textLoc.X = 240;
                textLoc.Y = 350;
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Sending score...", textLoc, Color.Green);
            }
            else if (sendFailed)
            {
                textLoc.X = 240;
                textLoc.Y = 350;
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Send failed.", textLoc, Color.Red);
                textLoc.Y += 50;
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Make sure your", textLoc, Color.Red);
                textLoc.Y += 50;
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "phone has", textLoc, Color.Red);
                textLoc.Y += 50;
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "internet service", textLoc, Color.Red);
            }
            else if (records[selectedSet, 0] != null)
            {
                int lineOffset = 38;
                int ystart = 225;
                textLoc.X = 100;
                textLoc.Y = ystart;
                for (int i = 0; i < records.GetLength(1); i++)
                {
                    if (records[selectedSet, i] != null)
                    {
                        string indexString = records[selectedSet, i].index.ToString();
                        textLoc.X = 100 - font.MeasureString(indexString).X;
                        if (records[selectedSet, i].index == youScores[selectedSet])
                        {
                            spriteBatch.DrawString(font, indexString, textLoc, Color.Green);
                        }
                        else
                        {
                            spriteBatch.DrawString(font, indexString, textLoc, Color.White);
                        }
                    }
                    textLoc.Y += lineOffset;
                }

                textLoc.X = 130;
                textLoc.Y = ystart;
                for (int i = 0; i < records.GetLength(1); i++)
                {
                    if (records[selectedSet, i] != null)
                    {
                        if (records[selectedSet, i].index == youScores[selectedSet])
                        {
                            spriteBatch.DrawString(font, records[selectedSet, i].name, textLoc, Color.Green);
                        }
                        else
                        {
                            spriteBatch.DrawString(font, records[selectedSet, i].name, textLoc, Color.White);
                        }
                    }
                    textLoc.Y += lineOffset;
                }

                textLoc.X = 250;
                textLoc.Y = ystart;
                for (int i = 0; i < records.GetLength(1); i++)
                {
                    if (records[selectedSet, i] != null)
                    {
                        string scoreString = records[selectedSet, i].score.ToString();
                        textLoc.X = 440 - font.MeasureString(scoreString).X;
                        if (records[selectedSet, i].index == youScores[selectedSet])
                        {
                            spriteBatch.DrawString(font, scoreString, textLoc, Color.Green);
                        }
                        else
                        {
                            spriteBatch.DrawString(font, scoreString, textLoc, Color.White);
                        }
                    }
                    textLoc.Y += lineOffset;
                }
            }
            else if (!scoreToSend)
            {
                textLoc.X = 240;
                textLoc.Y = 350;
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Play a round to", textLoc, Color.White);
                textLoc.Y += 30;
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "get a score to submit", textLoc, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


    }
}
