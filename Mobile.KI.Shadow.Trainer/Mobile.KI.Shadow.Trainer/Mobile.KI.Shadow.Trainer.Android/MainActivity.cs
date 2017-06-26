using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using Mobile.KI.Shadow.Trainer.Droid;
using System.Linq;
using Mobile.KI.Shadow.Models;

namespace Mobile.KI.Shadow.Trainer.Droid
{
    [Activity (Label = "KI Shadow Linker Breaker Trainer", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);
            
            var stream = Assets.Open("data.json");
            DataLoader.LoadCharactersAsync(stream).Wait();

            var grid = FindViewById<GridView>(Resource.Id.GridChars);


            var list = new JavaList<Character>(
                    DataLoader.Characters
                 );


            var adapter = new ImageTextAdapter(list, this);

            grid.Adapter = adapter;
            grid.ItemClick += grid_ItemClick;

        }

        void grid_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //  Toast.MakeText(this, DataLoader.Characters[e.Position].Name, ToastLength.Short).Show();
            var intent = new Intent(this, typeof(MovesActivity));
            intent.PutExtra("char_name", DataLoader.Characters[e.Position].Name);
            StartActivity(intent);
        }


    }
}


