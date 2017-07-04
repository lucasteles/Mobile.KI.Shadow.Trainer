using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;
using Android.Graphics;
using Android.Net;
using System.Threading;
using Mobile.KI.Shadow.Trainer.Models;
using Mobile.KI.Shadow.Models;
using Mobile.KI.Shadow.Trainer.Droid.Constants;
using Android.Content.PM;

namespace Mobile.KI.Shadow.Trainer.Droid
{

    [Activity(ScreenOrientation = ScreenOrientation.Portrait)]
    public class TrainerActivity : Activity
    {
        string character;
        string move;
        int atualFrame;
        int hits;
        int frameError = -1;
        int Successes;
        int Misses;
        int lastHit = -1;
        Move thisMove;
        const int oneFrame = 1000 / 60;

        bool cancelationToken;
        Button btnPlay;
        VideoView videoView;
        CheckBox chkDisableBeep;
        TextView lblFrames;
        TextView lblScore;
        ProgressBar seekVideo;
        Thread seekThread;
        IList<RangeData> ranges;

        IDictionary<SoundsEnum, MediaPlayer> sounds;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Trainer);
            FindControls();

            character = Intent.GetStringExtra("char_name") ?? "";
            move = Intent.GetStringExtra("move_name") ?? "";

            Title = $"{character} - {move}";

            btnPlay.Click += BtnPlay_Click;


            thisMove = DataLoader.Characters.Where(e => e.Name == character)
                        .SelectMany(e => e.Moves)
                        .SingleOrDefault(e => e.Name == move);


            videoView.Prepared += (sender, args) =>
            {
                OnVideoIsPrepared();
            };




            BuildRange();
            BuildSounds();

        }

        void BuildSounds()
        {
            sounds = new Dictionary<SoundsEnum, MediaPlayer>
            {
                [SoundsEnum.BEEP] = MediaPlayer.Create(this, Resource.Raw.beep),
                [SoundsEnum.ONE] = MediaPlayer.Create(this, Resource.Raw.one),
                [SoundsEnum.TWO] = MediaPlayer.Create(this, Resource.Raw.two),
                [SoundsEnum.THREE] = MediaPlayer.Create(this, Resource.Raw.three),
                [SoundsEnum.LOCKOUT] = MediaPlayer.Create(this, Resource.Raw.lockout),
                [SoundsEnum.COMBOBREAKER] = MediaPlayer.Create(this, Resource.Raw.combobreaker)
            };
        }


        void BtnPlay_Click(object sender, EventArgs e)
        {
            if (!videoView.IsPlaying)
            {
                btnPlay.Text = "Break";
                Play();
                return;
            }

            var hit = GetHit();
            if (hit && lastHit != atualFrame)
            {
                lastHit = atualFrame;
                hits++;
                btnPlay.Text = $"Break {hits}";



                var sound = sounds[(SoundsEnum)hits];
                sound.Start();

                if (hits >= 3)
                {
                    StopThread();
                    Stop();
                    Successes++;
                  
                   
                    sounds[SoundsEnum.COMBOBREAKER].Start();
                }
            }
            else
            {
                StopThread();
                hits = 0;
                Misses++;
                sounds[SoundsEnum.LOCKOUT].Start();
                Stop();

            }


            UpdateScore();

        }

        private void StopBeep()
        {
           
            sounds[SoundsEnum.BEEP].SeekTo(0);
            sounds[SoundsEnum.BEEP].Pause();
        }

        void FindControls()
        {
            btnPlay = FindViewById<Button>(Resource.Id.cmdPlay);
            videoView = FindViewById<VideoView>(Resource.Id.VideoPlayer);
            seekVideo = FindViewById<ProgressBar>(Resource.Id.seekBar);
            lblFrames = FindViewById<TextView>(Resource.Id.lblFrames);
            lblScore = FindViewById<TextView>(Resource.Id.lblScore);
            chkDisableBeep = FindViewById<CheckBox>(Resource.Id.chkBeep);
        }

        void UpdateScore()
        {
            lblScore.Text = $"Breaks={Successes} / Miss={Misses}";
        }

        bool GetHit() => ranges.Any(x => x.InRange(atualFrame));
        void BuildRange()
        {
            var rangeData = new List<RangeData>();
            var time = thisMove.StartGap + thisMove.Freeze;
            var rangesArray = thisMove.Ranges.ToArray();
            for (int i = 0; i < rangesArray.Count(); i++)
            {
                var r = rangesArray[i];
                if (r.Type == TimeRangeType.ACTIVE)
                    rangeData.Add(new RangeData(time, time + rangesArray[i].Size));

                time += r.Size;

            }
            ranges = rangeData;
        }

        void SetVideo()
        {
            atualFrame = 0;
            var move_file = thisMove.VideoSrc;
            var resourceId = (int)typeof(Resource.Raw).GetField(move_file).GetValue(null);
            var uri = Android.Net.Uri.Parse("android.resource://" + Application.PackageName + "/" + resourceId.ToString());
            videoView.SetVideoURI(uri);
            seekVideo.Max = 0;


        
        }



        void Play()
        {
            videoView.StopPlayback();
            
            var cbSound = sounds[SoundsEnum.COMBOBREAKER];
            if (cbSound.IsPlaying)
            {

                cbSound.SeekTo(0);
                cbSound.Pause();

            }

            SetVideo();


        }

        void OnVideoIsPrepared()
        {
        

            seekVideo.Progress = 0;
            seekVideo.Max = videoView.Duration;


            videoView.SetBackgroundColor(Color.Transparent);
            videoView.Start();
            videoView.SetZOrderOnTop(true);
            videoView.SetZOrderMediaOverlay(true);

            StartThread();
        }

        void Stop()
        {
            frameError = atualFrame;
            hits = 0;
            lastHit = -1;
            StopBeep();
            btnPlay.Text = "Play";
           
            atualFrame = 0;
            videoView.Pause();
            lblFrames.SetBackgroundColor(Color.Black);

            lblFrames.Text = $"{frameError}f";
        }


        void StartThread()
        {
            if (seekThread != null)
            {
                StopThread();

            }
            var move = DataLoader.Characters.Where(e => e.Name == character)
                      .SelectMany(e => e.Moves)
                      .Where(e => e.Name == this.move);

            
            seekThread = new Thread(new ThreadStart(() =>
            {
              
                var current =0;
                while (videoView.IsPlaying && !cancelationToken)
                {
                    Thread.Sleep(1000 / 120);

                    if (current < videoView.CurrentPosition)
                        current = videoView.CurrentPosition;


                    if (videoView.Duration > 0 && current > 0 && videoView.IsPlaying)
                    {
                      
                        atualFrame = videoView.CurrentPosition / oneFrame;
                      
                        RunOnUiThread(() =>
                        {
                            var isHit = GetHit();

                            if (seekThread == null || !seekThread.IsAlive)
                                return;

                            seekVideo.Progress = current;

                            lblFrames.Text = $"{atualFrame}f";
                            lblFrames.SetBackgroundColor(isHit ? Color.Red : Color.Black);

                            if (!chkDisableBeep.Checked) { 
                                if (isHit)
                                {
                                    if (!sounds[SoundsEnum.BEEP].IsPlaying)
                                        sounds[SoundsEnum.BEEP].Start();
                                }
                                else if (sounds[SoundsEnum.BEEP].IsPlaying)
                                {
                                    StopBeep();
                                }
                            }
                        });




                    }
                }
                if (!cancelationToken)
                {
                    StopBeep();
                    seekVideo.Progress = videoView.Duration;
                    RunOnUiThread(() =>
                    {
                        btnPlay.Text = "Play Again";
                        lblFrames.SetBackgroundColor(Color.Black);
                    });
                }
            }));

            seekThread.Start();
        }

        private void StopThread()
        {
            cancelationToken = true;
           // seekThread.Abort();
            while (seekThread.IsAlive)
                Thread.Sleep(100);
            cancelationToken = false;
        }
    }
}