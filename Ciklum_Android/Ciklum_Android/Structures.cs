namespace Ciklum_Android
{
	public class Tweet
	{
		public Tweet()
		{
		}

		public string Id_str { get; set; }
		public string Text { get; set; }
		public string Posted { get; set; }
		
		public Tweet(string id, string text, string posted)
		{
			Id_str = id;
			Text = text;
			Posted = posted;
		}
	}
}