#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
#endregion

namespace BubblePop
{
    public abstract class SoundInstance
    {
        public abstract void LoadEffect(ContentManager content);

        public abstract void UnloadEffect();

        public abstract int Play();

        public abstract void StopAll();

        public abstract String Name();
    }


    public class MusicInstance : SoundInstance
    {
        Song song;
        String name;
        String location;
        float volume;

        public MusicInstance(String EffectLocation, String Name)
        {
            Initialize(EffectLocation, Name, .8f);
        }

        public MusicInstance(String EffectLocation, String Name, float volume)
        {
            Initialize(EffectLocation, Name, volume);
        }

        public void Initialize(String Location, String Name, float volume)
        {
            this.volume = volume;
            this.location = Location;
            this.name = Name;
        }

        public override void LoadEffect(ContentManager content)
        {
            song = content.Load<Song>(location);
        }

        public override void UnloadEffect()
        {
            song.Dispose();
            song = null;
        }

        public override int Play()
        {
            if (MusicManager.musicOn)
            {
                MediaPlayer.Play(song);
                //MediaPlayer.Volume = volume;
                MediaPlayer.IsRepeating = true;
                MediaPlayer.IsShuffled = false;
            }
            return 0;
        }

        public override void StopAll()
        {
            if (MusicManager.musicOn)
            {
                MediaPlayer.Stop();
            }
        }

        public override String Name()
        {
            return name;
        }
    }


    public class SFXInstance : SoundInstance
    {
        SoundEffect effect;
        String name;
        String effectLocation;
        int maxIntanceCount;
        SoundEffectInstance [] instances;
        bool isMusic;
        float volume;

        public SFXInstance(String EffectLocation, String Name, int MaxInstances)
        {
            Initialize(EffectLocation, Name, MaxInstances, .8f, false);
        }

        public SFXInstance(String EffectLocation, String Name, int MaxInstances, bool isMusic)
        {
            Initialize(EffectLocation, Name, MaxInstances, .8f, isMusic);
        }

        public SFXInstance(String EffectLocation, String Name, int MaxInstances, float volume)
        {
            Initialize(EffectLocation, Name, MaxInstances, volume, false);
        }

        public SFXInstance(String EffectLocation, String Name, int MaxInstances, float volume, bool isMusic)
        {
            Initialize(EffectLocation, Name, MaxInstances, volume, isMusic);
        }

        public void Initialize(String EffectLocation, String Name, int MaxInstances, float volume, bool isMusic)
        {
            this.volume = volume;
            this.effectLocation = EffectLocation;
            this.isMusic = isMusic;
            this.name = Name;
            this.maxIntanceCount = MaxInstances;
        }

        public override void LoadEffect(ContentManager content)
        {
            if (effect == null)
                effect = content.Load<SoundEffect>(effectLocation);
            if (instances == null)
                instances = new SoundEffectInstance[maxIntanceCount];
            for (int i = 0; i < maxIntanceCount; i++)
            {
                if (instances[i] == null)
                {
                    instances[i] = effect.CreateInstance();
                    instances[i].Volume = volume;
                }
                
            }
        }

        public override void UnloadEffect()
        {
            if (instances != null)
            {
                for (int i = 0; i < maxIntanceCount; i++)
                {
                    if (instances[i] != null)
                    {
                        instances[i].Dispose();
                        instances[i] = null;
                    }
                }
                instances = null;
            }

            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
            GC.Collect();
        }

        public override int Play()
        {
            if (AudioManager.SoundFXOn)
            {
                for (int i = 0; i < maxIntanceCount; i++)
                {
                    if (instances[i].State == SoundState.Stopped)
                    {
                        instances[i].Play();
                        return i;
                    }
                }
            }
            return int.MaxValue;
        }

        public override void StopAll()
        {
            if (AudioManager.SoundFXOn)
            {
                for (int i = 0; i < maxIntanceCount; i++)
                {
                    if (instances != null)
                        instances[i].Stop();
                }
            }
        }

        public override String Name()
        {
            return name;
        }
    }
    


    public class AudioManager : GameComponent
    {
        public static AudioManager audioManager = null;
        private List<SoundInstance> SFXList;
        public static bool SoundFXOn = true;


        private AudioManager(Game game)
            : base(game)
        {
            // load up all of the sound files
            SFXList = new List<SoundInstance>();
            SFXList.Add(new SFXInstance("Audio\\explosion", "explosion", 2));
            SFXList.Add(new SFXInstance("Audio\\bubble_pop", "pop", 2));
            SFXList.Add(new SFXInstance("Audio\\crunch", "crunch", 2, .4f));
            SFXList.Add(new SFXInstance("Audio\\whoosh", "whoosh", 2, .4f));

        }


        public static void Initialize(Game game)
        {
            audioManager = new AudioManager(game);
            if (game != null)
            {
                //game.Components.Add(audioManager);
            }
        }



        public void PlaySFX(String soundName)
        {
            for (int i = 0; i < SFXList.Count; i++)
            {
                if (soundName == SFXList[i].Name())
                {
                    SFXList[i].Play();
                }
            }
        }


        public void StopAllSFX(String soundName)
        {
            for (int i = 0; i < SFXList.Count; i++)
            {
                if (soundName == SFXList[i].Name())
                {
                    SFXList[i].StopAll();
                }
            }
        }

        public void LoadSFX(String soundName, ContentManager content)
        {
            for (int i = 0; i < SFXList.Count; i++)
            {
                if (soundName == SFXList[i].Name())
                {
                    SFXList[i].LoadEffect(content);
                    break;
                }
            }
        }

        public void LoadSFX(int index1, int index2)
        {
            for (int i = index1; i <= index2; i++)
                SFXList[i].LoadEffect(BubblePop.sigletonGame.Content);
        }

        public void UnloadSFX(String soundName)
        {
            for (int i = 0; i < SFXList.Count; i++)
            {
                if (soundName == SFXList[i].Name())
                {
                    SFXList[i].UnloadEffect();
                    break;
                }
            }
        }

        public int SFXNum
        {
            get { return SFXList.Count; }
        }


    }
}
