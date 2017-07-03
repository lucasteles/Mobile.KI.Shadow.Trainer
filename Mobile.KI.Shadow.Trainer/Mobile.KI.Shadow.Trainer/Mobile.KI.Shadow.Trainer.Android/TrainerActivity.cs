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

namespace Mobile.KI.Shadow.Trainer.Droid
{

    [Activity()]
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
        bool prepared;

        Button btnPlay;
        VideoView videoView;
        TextView lblFrames;
        TextView lblScore;
        SeekBar seekVideo;
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





            BuildRange();
            BuildSounds();
            SetVideo();

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
                    Stop();
                    Successes++;
                    sounds[SoundsEnum.BEEP].SeekTo(0);
                    sounds[SoundsEnum.BEEP].Pause();
                    sounds[SoundsEnum.COMBOBREAKER].Start();
                }
            }
            else
            {
                hits = 0;
                Misses++;
                sounds[SoundsEnum.LOCKOUT].Start();
                Stop();

            }


            UpdateScore();

        }

        void FindControls()
        {
            btnPlay = FindViewById<Button>(Resource.Id.cmdPlay);
            videoView = FindViewById<VideoView>(Resource.Id.VideoPlayer);
            seekVideo = FindViewById<SeekBar>(Resource.Id.seekVideo);
            lblFrames = FindViewById<TextView>(Resource.Id.lblFrames);
            lblScore = FindViewById<TextView>(Resource.Id.lblScore);
        }

        void UpdateScore()
        {
            lblScore.Text = $"Breaks={Successes} / Misses={Misses}";
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
            videoView.SetBackgroundColor(Color.Transparent);
            seekVideo.Max = 0;
            prepared = false;


            videoView.Prepared += (sender, args) =>
            {
                prepared = true;
            };



        }



        void Play()
        {


            if (!prepared)
                return;

            var cbSound = sounds[SoundsEnum.COMBOBREAKER];
            if (cbSound.IsPlaying)
            {
               
                cbSound.SeekTo(0);
                cbSound.Pause();

            }

            seekVideo.Progress = 0;
            seekVideo.Max = videoView.Duration;

            if (videoView.IsPlaying)
                videoView.Resume();
            else
                videoView.Start();
            UpdateBar();


            videoView.SetZOrderOnTop(true);


        }
        void Stop()
        {
            frameError = atualFrame;
            atualFrame = 0;
            hits = 0;
            lastHit = -1;
            seekVideo.Progress = 0;
            btnPlay.Text = "Play";
            videoView.SeekTo(0);
            videoView.Pause();
            lblFrames.SetBackgroundColor(Color.Black);
           


        }


        void UpdateBar()
        {
            if (seekThread != null)
            {
                seekThread.Abort();
                
            }
            var move = DataLoader.Characters.Where(e => e.Name == character)
                      .SelectMany(e => e.Moves)
                      .Where(e => e.Name == this.move);

            seekThread = new Thread(new ThreadStart(() =>
            {


                while (videoView.IsPlaying)
                {
                    Thread.Sleep(1000 / 120);

                    seekVideo.Max = videoView.Duration;
                    if (videoView.Duration > 0 && videoView.CurrentPosition > 0 && videoView.IsPlaying)
                    {
                        seekVideo.Progress = videoView.CurrentPosition;
                        atualFrame = videoView.CurrentPosition / oneFrame;
                        var isHit = GetHit();
                        RunOnUiThread(() =>
                        {
                            lblFrames.Text = $"{atualFrame}f";
                            lblFrames.SetBackgroundColor(isHit ? Color.Red : Color.Black);

                            if (isHit)
                            {

                                sounds[SoundsEnum.BEEP].Start();
                            }
                            else if (sounds[SoundsEnum.BEEP].IsPlaying)
                            {
                                sounds[SoundsEnum.BEEP].SeekTo(0);
                                sounds[SoundsEnum.BEEP].Pause();

                            }

                        });




                    }
                }
                //Stop();
            }));

            seekThread.Start();
        }


    }
}