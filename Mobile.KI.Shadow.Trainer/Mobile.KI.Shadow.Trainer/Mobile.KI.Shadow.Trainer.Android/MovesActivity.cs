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
using Mobile.KI.Shadow.Models;

namespace Mobile.KI.Shadow.Trainer.Droid
{
    [Activity()]
    public class MovesActivity : Activity
    {
        string _character;
        string[] _moves;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Moves);
            

            _character = Intent.GetStringExtra("char_name") ?? "";
            Title = $"{_character} moves";
            

            var grid = FindViewById<GridView>(Resource.Id.GridMoves);

             _moves = DataLoader
                        .Characters
                        .Where(e => e.Name == _character)
                        .SelectMany(e => e.Moves)
                        .Select(e=>e.Name)
                        .OrderBy(e=>e)
                        .ToArray();


            grid.Adapter = new ArrayAdapter<string>(this, Resource.Layout.TextViewItem, _moves);
            grid.ItemClick += grid_ItemClick;

        }


        void grid_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            
            var intent = new Intent(this, typeof(TrainerActivity));
            intent.PutExtra("char_name", _character);
            intent.PutExtra("move_name", _moves[e.Position]);
            StartActivity(intent);
        }



    }
}