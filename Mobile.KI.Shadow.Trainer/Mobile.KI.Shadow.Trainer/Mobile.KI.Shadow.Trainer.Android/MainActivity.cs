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

        EditText txtSeach;
        GridView grid;
        ImageTextAdapter adapter;

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);
            
            var stream = Assets.Open("data.json");
            DataLoader.LoadCharactersAsync(stream).Wait();

            grid = FindViewById<GridView>(Resource.Id.GridChars);
            txtSeach = FindViewById<EditText>(Resource.Id.txtSearch);

            grid.ItemClick += grid_ItemClick;
            txtSeach.TextChanged += TxtSeach_TextChanged;
            LoadGrid();

        }

        private void TxtSeach_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            adapter.Filter.InvokeFilter(txtSeach.Text);
        }

        void grid_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            //  Toast.MakeText(this, DataLoader.Characters[e.Position].Name, ToastLength.Short).Show();
            var intent = new Intent(this, typeof(MovesActivity));
           
            intent.PutExtra("char_name", adapter.CharsList[e.Position].Name);
            StartActivity(intent);
        }

        void LoadGrid()
        {

            var list = new JavaList<Character>(
                    DataLoader.Characters
                 );


            adapter = new ImageTextAdapter(list, this);

            grid.Adapter = adapter;
        }
    }
}


