using CyTools;
using System.Collections.Generic;

namespace CysmicEngine
{
    public class Animator : Component
    {
        public override bool OnlyOnePerGO() => true;

        //Action<string, bool> OnPlay;
        //IAsyncResult asyncAnimation;
        //List<CancellationTokenSource> animCancelSources = new List<CancellationTokenSource>();

        public SpriteRenderer spriteRenderer;
        public Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();

        Queue<AnimationData> animationSequence = new Queue<AnimationData>();
        AnimationData curAnimation;
        int curFrame;
        float frameTimer;

        public Animator(Dictionary<string, AnimationClip> _animations, SpriteRenderer sRenderer = null)
        {
            animations = _animations;
            spriteRenderer = sRenderer;
        }

        protected override void Start()
        {
            base.Start();
            if (spriteRenderer == null)
                gameObject.TryGetComponent(out spriteRenderer);

            //animCancelSources = new List<CancellationTokenSource>();
            //OnPlay += PlayAnimation;
        }

        protected override void Update()
        {
            base.Update();

            //for (int i = 0; i < curAnimation.frames.Length; i++)
            if (curAnimation != null)
            {
                spriteRenderer.SetSprite(curAnimation.frames[curFrame].sprite);

                frameTimer += Time.deltaTime;
                if (frameTimer >= curAnimation.frames[curFrame].delayAfter)
                {
                    frameTimer = 0;
                    curFrame++;
                    if (curFrame >= curAnimation.frames.Length)
                    {
                        OnCompleteLoop();
                    }
                }
            }
            //no else
            if (curAnimation == null || curAnimation.isFinished)
            {
                if (animationSequence.Count > 0)
                    curAnimation = animationSequence.Dequeue();
            }
        }

        public void BreakAnimationSequence()
        {
            animationSequence.Clear();
        }
        public void Play(string name, bool loop = false, bool interuptSelf = false, bool canBeInterupted = true, int unlockAfter = int.MaxValue)
        {
            if (animationSequence.Count > 0)
                return;

            if (curAnimation == null || curAnimation.canBeInterupted)
            {
                if (curAnimation == null || curAnimation.canInteruptSelf || name != curAnimation.name)
                {
                    if (!animations.TryGetValue(name, out AnimationClip clip))
                        return;

                    curFrame = 0;
                    curAnimation = new AnimationData(name, clip, loop, interuptSelf, canBeInterupted, unlockAfter);
                }
            }
        }

        void OnCompleteLoop()
        {
            curFrame = 0;
            if (curAnimation.isLooping)
            {
                if (curAnimation.timesPlayed.Clamp(0, int.MaxValue - 1) >= curAnimation.unlockAfterPlays)
                {
                    curAnimation.canBeInterupted = true;
                }
            }
            else
            {
                curAnimation.isFinished = true;
                //if(!curAnimation.holdFrameAfter)
                //back to default animation
            }
        }

        public struct AnimationClip
        {
            public (Sprite sprite, float delayAfter)[] frames;
            public float defaultSecondsAfter;

            public AnimationClip((Sprite, float)[] _frames, float secondsAfter = 0.03f)
            {
                frames = _frames;
                defaultSecondsAfter = secondsAfter;
                if (defaultSecondsAfter <= 0)
                    defaultSecondsAfter = 0.03f;

                for (int i = 0; i < frames.Length; i++)
                {
                    if (frames[i].delayAfter <= 0)
                        frames[i].delayAfter = defaultSecondsAfter;
                }
            }
        }
        class AnimationData
        {
            public string name;
            public (Sprite sprite, float delayAfter)[] frames;
            int _timesPlayed = 0;
            public int timesPlayed { get => _timesPlayed; protected set => _timesPlayed = value; }
            ////<summary>Set this <= 0 for infinite looping</summary>
            //int maxPlays = 1;

            //public bool canceled = false;
            public bool isLooping = false;
            public bool isFinished = false;
            //public bool holdFrameAfter = true;
            public bool canInteruptSelf = false;

            public bool canBeInterupted = true;
            public int unlockAfterPlays = int.MaxValue;

            public AnimationData(string nm, AnimationClip clip, bool loop = false, bool interuptSelf = false, bool _canBeInterupted = true, int unlockAfter = int.MaxValue)
            {
                name = nm;
                frames = clip.frames;
                isLooping = loop;
                canInteruptSelf = interuptSelf;
                canBeInterupted = _canBeInterupted;
                unlockAfterPlays = unlockAfter;
            }
        }

        #region OLD Task-Based Animation Code
        /*public void Play(string name, bool isLooping = false, bool canInteruptSelf = false, bool canBeInterupted = true, bool useFixedDeltaTime = false)
        {
            print(animCancelSources.Count);
            if (asyncAnimation == null || (asyncAnimation != null && asyncAnimation.IsCompleted) || canBeInterupted == true)
            {
                if ((canInteruptSelf && curAnimationName == name) || (!canInteruptSelf && (curAnimationName != name || asyncAnimation.IsCompleted) ))
                {
                    //print("Playing new");
                    if (asyncAnimation != null && (!asyncAnimation.IsCompleted || curAnimationName != name))
                    {
                        for (int i = 0; i < animCancelSources.Count; i++)
                        {
                            if (i >= animCancelSources.Count)
                                break;
                            if (animCancelSources[i] == null)
                                continue;

                            animCancelSources[i].Cancel();
                            animCancelSources[i] = null;
                            //animCancelSources.Clear();
                        }
                        animCancelSources.RemoveAll(x => x == null || x.IsCancellationRequested == true);
                    }

                    asyncAnimation = PlayAnimation(name, isLooping, useFixedDeltaTime);//OnPlay.BeginInvoke(name, useFixedDeltaTime, null/*end action*, null);
                    curAnimationName = name;
                }
            }
        }*/

        /*async Task PlayAnimation (string name, bool isLooping, bool useFixedDeltaTime = false)
        {
            await Task.Run(() =>
            {
                var cancelSource = new CancellationTokenSource();
                animCancelSources.Add(cancelSource);
                var curAnimation = animations[name];
                do
                {
                    for (int i = 0; i < curAnimation.frames.Length; i++)
                    {
                        if (cancelSource.IsCancellationRequested)
                            return;

                        spriteRenderer.SetSprite(curAnimation.frames[i].sprite);
                        //Task.Delay(TimeSpan.FromSeconds(curAnimation.frames[i].delayAfter), CancellationToken.None).Wait();

                        float timer = 0;
                        while (timer < curAnimation.frames[i].delayAfter && !cancelSource.IsCancellationRequested)
                        {
                            if (!useFixedDeltaTime)
                            {
                                timer += Time.deltaTime;
                                Task.Delay(TimeSpan.FromSeconds(Time.deltaTime), CancellationToken.None).Wait();
                            }
                            else
                            {
                                timer += Time.fixedDeltaTime;
                                Task.Delay(TimeSpan.FromSeconds(Time.fixedDeltaTime), CancellationToken.None).Wait();
                            }
                        }
                    }

                    //if(isLooping)
                    //asyncAnimation = PlayAnimation(name, isLooping, useFixedDeltaTime);
                }
                while (isLooping && !cancelSource.IsCancellationRequested);
                
                animCancelSources.Remove(cancelSource);
            });
        }*/
        #endregion
    }
}
