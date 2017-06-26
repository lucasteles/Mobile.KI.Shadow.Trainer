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

namespace Mobile.KI.Shadow.Trainer.Droid
{

    [Activity()]
    public class TrainerActivity : Activity 
    {
        string _character;
        string _move;

        Button btnPlay;
        VideoView videoView;
        MediaPlayer player;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Trainer);

            _character = Intent.GetStringExtra("char_name") ?? "";
            _move = Intent.GetStringExtra("move_name") ?? "";

            Title = $"{_character} - {_move}";

            btnPlay = FindViewById<Button>(Resource.Id.cmdPlay);
            videoView = FindViewById<VideoView>(Resource.Id.VideoPlayer);


            btnPlay.Click += BtnPlay_Click;

            Play();


        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            Toast.MakeText(this, "hello", ToastLength.Short);
        }



        void Play()
        {

            var move_file = DataLoader.Characters.Where(e => e.Name == _character)
                        .SelectMany(e => e.Moves)
                        .Where(e => e.Name == _move)
                        .Select(e => e.VideoSrc)
                        .SingleOrDefault();

            var resourceId = (int)typeof(Resource.Raw).GetField(move_file).GetValue(null);


            var uri = Android.Net.Uri.Parse("android.resource://" + Application.PackageName + "/" + resourceId.ToString());


            videoView.SetVideoURI(uri);
            videoView.SetBackgroundColor(Color.Transparent);
            videoView.Start();


            videoView.SetZOrderOnTop(true);

           
        }
   }
}