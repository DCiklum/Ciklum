using System;

namespace Ciklum
{
	public class Elements
	{   
		public Elements ()
		{
		}
		
		public string type { get; set; }
		public string caption { get; set; }
		public string value { get; set; }
		public string on { get; set; }
		public string off { get; set; }
		
	}
	
	public class Favorites
	{
		public Favorites ()
		{
		}
		
		public string title { get; set; }
		public Elements element { get; set; }
	}

	public class AllTweets
	{
		public AllTweets ()
		{
		}

		public Tweet tweets { get; set; }
		
	}

	public class Tweet
	{
		public Tweet ()
		{
		}
		
		public string created_at { get; set; }
		public string text { get; set; }
		public ulong id { get; set; }
	}

}

