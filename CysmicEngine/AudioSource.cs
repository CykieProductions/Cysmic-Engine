using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace CysmicEngine
{
    public partial class AudioSource : Component
    {
        IWavePlayer audioPlayer = new WaveOutEvent();
        enum State
        {
            STOPPED, PLAYING, PAUSED
        }

        State playState = State.STOPPED;
        Task curPlayTask;
        //TimeSpan resumeTime = TimeSpan.MinValue;

        static AudioClip[] globalSoundBank = new AudioClip[]
        {
            new AudioClip("Gloomstead.mp3", "Gloomstead", true)
        };

        public AudioClip[] audioClips;
        string curClipName = "";

        public Dictionary<string, CancellationTokenSource> oneOffCancelSources = new Dictionary<string, CancellationTokenSource>();

        public AudioSource()
        {
            audioClips = globalSoundBank;
        }
        public AudioSource(AudioClip[] clips)
        {
            audioClips = clips;
        }

        public void Stop()
        {
            //resumeTime = TimeSpan.MinValue;
            audioPlayer.Stop();
            playState = State.STOPPED;
            curPlayTask = null;
        }
        public void Pause()
        {
            if (playState != State.PLAYING)
                return;

            audioPlayer.Pause();
            playState = State.PAUSED;
            curPlayTask = null;
        }

        /// <summary>
        /// Plays an audio clip while ignoring the main output
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loopCancelKey"></param>
        /// <param name="clipBank"></param>
        /// <returns></returns>
        public Task PlayOneOff(string name, out string loopCancelKey, AudioClip[] clipBank = null)//Untested
        {
            loopCancelKey = "[N/A]";
            AudioClip clip;
            if (clipBank == null)
            {
            clip = GetClip(name);
            if (clip == null)
                clip = GetClip(name, globalSoundBank);
            }
            else
                clip = GetClip(name, clipBank);

            if (clip == null)
                return null;

            loopCancelKey = name + "|" + random.Next() + "|" + DateTime.Now + DateTime.Now.Millisecond;
            oneOffCancelSources.Add(loopCancelKey, new CancellationTokenSource());

            return OneOffOutputTask(clip, loopCancelKey);
        }

        async Task OneOffOutputTask(AudioClip clip, string key)//Untested
        {
            using (AudioFileReader file = new AudioFileReader(clip.filePath))
            using (var oneOffPlayer = new WaveOutEvent())
            {
                await Task.Run(() =>
                {
                    while (curPlayTask == null) ;

                    TimeSpan startTime = TimeSpan.Zero;
                    if (clip.resumeTime != TimeSpan.MinValue)
                        file.CurrentTime = clip.resumeTime;
                    else
                        startTime = file.CurrentTime;
                    oneOffPlayer.Init(file);
                    oneOffPlayer.Play();

                    while (!oneOffCancelSources[key].IsCancellationRequested)
                    {
                        bool readyToLoop = false;
                        while (!oneOffCancelSources[key].IsCancellationRequested)
                        {
                            /*if (playState == State.PAUSED)
                            {
                                clip.resumeTime = file.CurrentTime;
                                break;
                            }
                            else */
                            if (file.CurrentTime.TotalSeconds >= file.TotalTime.TotalSeconds - TimeSpan.FromMilliseconds(100).TotalSeconds)
                            {
                                if (!clip.isLooping && file.CurrentTime.TotalSeconds < file.TotalTime.TotalSeconds)//Wait for full finish if not looping
                                { }
                                else
                                {
                                    if(!clip.isLooping)
                                        oneOffCancelSources[key].Cancel();
                                    readyToLoop = true;
                                    break;
                                }
                            }

                            //Task.Delay(1000).Wait();
                            try
                            {
                                oneOffPlayer.Play();
                            }
                            catch
                            {
                                oneOffCancelSources[key].Cancel();
                                break;
                            }
                        }//Loop Complete or Paused/Stopped

                        if (clip.isLooping && readyToLoop)
                            file.CurrentTime = startTime;//Start over and continue the loop if ready
                        else
                        {
                            /*if (playState == State.PLAYING)//if loop complete, then set to STOPPED; else leave paused
                                playState = State.STOPPED;*/
                            break;
                        }
                    }

                    if (oneOffCancelSources[key].IsCancellationRequested)
                        clip.resumeTime = TimeSpan.MinValue;
                    //Task.Delay(1).Wait();
                });
            }
            oneOffCancelSources[key].Cancel();
            oneOffCancelSources[key].Dispose();
            oneOffCancelSources.Remove(key);
        }

        public void Resume() => Play(curClipName);//Just a shortcut method

        public void Play(string name/*, AudioClip[] clipBank = null*/)
        {
            if (curPlayTask != null)
            {
                if(name == curClipName)
                    Pause();
                return;
            }

            if (audioPlayer.PlaybackState == PlaybackState.Paused && name == curClipName)
            {
                //playState = State.PLAYING;//this will break it
                audioPlayer.Play();//Resume play
                return;
            }
            else if (name != curClipName)//Clears the prev resume if another clip is played//Make this optional somehow
            {
                var prevClip = GetClip(curClipName);
                if(prevClip != null)
                    prevClip.resumeTime = TimeSpan.MinValue;
            }

            AudioClip clip;
            //if (clipBank == null)
            //{
            clip = GetClip(name);
            if (clip == null)
                clip = GetClip(name, globalSoundBank);
            /*}
            else
                clip = GetClip(name, clipBank);*/

            if (clip == null)
                return;

            //clip.resumeTime = TimeSpan.MinValue;
            playState = State.PLAYING;
            curClipName = name;
            curPlayTask = MainOutputTask(clip);
        }

        async Task MainOutputTask(AudioClip clip)
        {
            if (curPlayTask != null)
                return;

            using (AudioFileReader file = new AudioFileReader(clip.filePath))
            using (audioPlayer = new WaveOutEvent())
            {
                await Task.Run(() =>
                {
                    while (curPlayTask == null) ;

                    TimeSpan startTime = TimeSpan.Zero;
                    if (clip.resumeTime != TimeSpan.MinValue)
                        file.CurrentTime = clip.resumeTime;
                    else
                        startTime = file.CurrentTime;
                    audioPlayer.Init(file);
                    audioPlayer.Play();

                    while (playState != State.STOPPED)
                    {
                        bool readyToLoop = false;
                        while (playState != State.STOPPED)//audioPlayer.PlaybackState seems very unreliable
                        {
                            if (playState == State.PAUSED)
                            {
                                clip.resumeTime = file.CurrentTime;
                                break;
                            }
                            else if (file.CurrentTime.TotalSeconds >= file.TotalTime.TotalSeconds - TimeSpan.FromMilliseconds(100).TotalSeconds)
                            {
                                if (!clip.isLooping && file.CurrentTime.TotalSeconds < file.TotalTime.TotalSeconds)//Wait for full finish if not looping
                                { }
                                else
                                {
                                    readyToLoop = true;
                                    break;
                                }
                            }

                            //Task.Delay(1000).Wait();
                            try
                            {
                                audioPlayer.Play();
                            }
                            catch { break; }
                        }//Loop Complete or Paused/Stopped

                        if (clip.isLooping && readyToLoop)
                            file.CurrentTime = startTime;//Start over and continue the loop if ready
                        else
                        {
                            if (playState == State.PLAYING)//if loop complete, then set to STOPPED; else leave paused
                                playState = State.STOPPED;
                            break;
                        }
                    }

                    if (playState == State.STOPPED)
                        clip.resumeTime = TimeSpan.MinValue;
                    //Task.Delay(1).Wait();
                });
            }//NAudio.MmException: 'InvalidParameter calling acmStreamClose' error when stopping sometimes//Moving the using statment out of the Task.Run seemed to fix it

            curPlayTask = null;
        }

        AudioClip GetClip(string name, AudioClip[] clips = null, bool getOriginal = true)
        {
            var bank = clips;
            if (bank == null) bank = audioClips;

            for (int i = 0; i < bank.Length; i++)
            {
                if (bank[i].name == name)
                {
                    if (getOriginal)
                        return bank[i];
                    else
                        return new AudioClip(bank[i]);
                }
            }
            return null;
        }

        public class AudioClip
        {
            public string name;
            //AudioFileReader _file;
            //public AudioFileReader file { get => _file; }

            string _filePath;
            public string filePath { get => _filePath;}
            public TimeSpan resumeTime;
            public bool isLooping;

            //public static readonly AudioClip Null;

            public AudioClip(string inputPath, string _name, bool loop = false)
            {
                if (inputPath[0] != '/' || inputPath[0] != '\\')
                    inputPath = "Assets/Audio/Music/" + inputPath;

                _filePath = inputPath;
                //_file = new AudioFileReader(inputPath);
                name = _name;
                resumeTime = TimeSpan.MinValue;
                isLooping = loop;
            }

            public AudioClip(AudioClip audioClip)
            {
                _filePath = audioClip._filePath;
                name = audioClip.name;
                resumeTime = TimeSpan.MinValue;
                isLooping = audioClip.isLooping;
            }
        }
    }

    /*public class SoundBank
    {
        AudioClip[] audioClips;

        public SoundBank(AudioClip[] clips)
        {
            audioClips = clips;
        }

        public AudioClip? Search(string name)
        {
            for (int i = 0; i < audioClips.Length / 2; i++)
            {
                if (audioClips[i].name == name)
                    return audioClips[i];
            }
            return null;
        }

        public struct AudioClip
        {
            public string name;
            AudioFileReader _file;
            public AudioFileReader file { get => _file; }

            AudioClip(string filePath, string _name)
            {
                if (filePath[0] != '/' || filePath[0] != '\\')
                    filePath = "Assets/Audio/Music/" + filePath;

                _file = new AudioFileReader(filePath);
                name = _name;
            }
        }

    }*/
}
