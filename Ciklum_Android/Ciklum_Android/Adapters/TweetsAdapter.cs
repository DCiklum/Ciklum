using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace Ciklum_Android.Adapters
{
	public class TweetsAdapter : BaseAdapter<Tweet>
	{
		private readonly List<Tweet> Elements;
		private readonly Activity Activity;

		public TweetsAdapter(Activity activity, List<Tweet> elements)
		{
			Activity = activity;
			Elements = elements;
		}
		
		public override long GetItemId(int position)
		{
			return position;
		}

		public override Tweet this[int position]
		{
			get { return Elements[position]; }
		}
		
		public override int Count
		{
			get { return Elements.Count; }
		}
		
		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = Elements[position];
			
			var view = Activity.LayoutInflater.Inflate(Resource.Layout.RowView, null);
			
			view.FindViewById<TextView>(Resource.Id.textViewMessage).Text = item.Text;
			view.FindViewById<TextView>(Resource.Id.textViewDate).Text = item.Posted;
			
			return view;
		}
	}
}

