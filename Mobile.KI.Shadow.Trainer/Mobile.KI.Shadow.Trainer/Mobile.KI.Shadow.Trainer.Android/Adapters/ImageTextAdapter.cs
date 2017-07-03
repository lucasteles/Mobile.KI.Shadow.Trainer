using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Mobile.KI.Shadow.Models;
using Mobile.KI.Shadow.Trainer.Droid;
using Object = Java.Lang.Object;
using System.Collections.Generic;
using System.Linq;

namespace Mobile.KI.Shadow.Trainer.Droid
{
    public class ImageTextAdapter : BaseAdapter, IFilterable
    {
        private Context c;
        private JavaList<Character> CharsList;
        private JavaList<Character> OriginalCharsList;
        private LayoutInflater inflater;
        Filter titleFilter;

        public ImageTextAdapter(JavaList<Character> fruits, Context c)
        {
            this.CharsList = fruits;
            this.c = c;
            this.titleFilter = new TextFilter(this);
        }
        public override Object GetItem(int position)
        {
            return CharsList.Get(position);
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (inflater == null)
            {
                inflater = (LayoutInflater)c.GetSystemService(Context.LayoutInflaterService);
            }
            if (convertView == null)
            {
                convertView = inflater.Inflate(Resource.Layout.CharacterListItem, parent, false);
            }

            //BIND DATA
            var nameTxt = convertView.FindViewById<TextView>(Resource.Id.GridText);
            var img = convertView.FindViewById<ImageView>(Resource.Id.GridImage);
            nameTxt.Text = CharsList[position].Name;

            var resourceId = (int)typeof(Resource.Drawable).GetField(CharsList[position].Thumb).GetValue(null);
            img.SetImageResource(resourceId);

            return convertView;
        }
        public override int Count
        {
            get { return CharsList.Size(); }
        }

        public Filter Filter => titleFilter;



        public class TextFilter : Filter
        {
            private readonly ImageTextAdapter _adapter;
            public TextFilter(ImageTextAdapter adapter)
            {
                _adapter = adapter;
            }

            protected override FilterResults PerformFiltering(Java.Lang.ICharSequence constraint)
            {
                var returnObj = new FilterResults();
                var results = new List<Character>();
                if (_adapter.OriginalCharsList == null)
                    _adapter.OriginalCharsList = _adapter.CharsList;

                if (constraint == null) return returnObj;

                if (_adapter.OriginalCharsList != null && _adapter.OriginalCharsList.ToList().Any())
                {
                    // Compare constraint to all names lowercased. 
                    // It they are contained they are added to results.
                    var search = constraint.ToString().ToLower();
                    results.AddRange(_adapter.OriginalCharsList
                                .Where(contact => contact.Name.ToLower()
                                          .Contains(search)
                                      )
                                 .OrderByDescending(e=>e.Name.ToLower().StartsWith(search))
                                );
                }
             
                    returnObj.Values = FromArray(results.Select(e=>e.ToJavaObject()).ToArray());

                    returnObj.Count = results.Count;

                    constraint.Dispose();

                    return returnObj;
              
            }

            protected override void PublishResults(Java.Lang.ICharSequence constraint, FilterResults results)
            {
                using (var values = results.Values)
                {
                    var list = values.ToArray<Object>().Select(a => a.ToNetObject<Character>()).ToArray(); 
                    _adapter.CharsList = new JavaList<Character>( list );
                }

                _adapter.NotifyDataSetChanged();

                constraint.Dispose();
                results.Dispose();
            }
        }

    }

   
}