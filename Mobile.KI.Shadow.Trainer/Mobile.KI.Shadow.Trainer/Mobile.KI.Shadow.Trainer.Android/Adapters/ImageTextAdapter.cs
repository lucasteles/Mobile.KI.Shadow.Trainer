using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Mobile.KI.Shadow.Models;
using Mobile.KI.Shadow.Trainer.Droid;
using Object = Java.Lang.Object;


namespace Mobile.KI.Shadow.Trainer.Droid
{
    class ImageTextAdapter : BaseAdapter
    {
        private Context c;
        private JavaList<Character> chars;
        private LayoutInflater inflater;
        public ImageTextAdapter(JavaList<Character> fruits, Context c)
        {
            this.chars = fruits;
            this.c = c;
        }
        public override Object GetItem(int position)
        {
            return chars.Get(position);
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
            nameTxt.Text = chars[position].Name;

            var resourceId = (int)typeof(Resource.Drawable).GetField(chars[position].Thumb).GetValue(null);
            img.SetImageResource(resourceId);

            return convertView;
        }
        public override int Count
        {
            get { return chars.Size(); }
        }
    }
}