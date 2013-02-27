using System;
using System.Threading;
using System.Json;
using System.Collections.Generic;
using System.Net;
using System.IO;

using Ciklum_Android.Adapters;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace Ciklum_Android
{
	[Activity (Label = "Ciklum_Android", MainLauncher = true)]
	public class TweetsActivity : Activity
	{
		private ListView lwTweets;
		private List<Tweet> lstTweets;
		private TweetsAdapter tweetsAdapter;
		private JsonArray jTweetsArray = new JsonArray();	// array of tweets

		private int sinceTweet = 1;				// page number

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);
			lwTweets = FindViewById<ListView> (Resource.Id.listTweets);
			tweetsAdapter = new TweetsAdapter(this, new List<Tweet>());

			// Filling Tweets Activity in new thread
			ThreadPool.QueueUserWorkItem(lt => FillingMainActivity());
			// Filling Favorites Activity in main thread

		}

		// Fills Main Activity
		private void FillingMainActivity ()
		{
			CreateJTweetsArray (GetTwitterJSON ( 20, "ciklum" ));
			lstTweets = CreateTweetList();


			//Filling TweetsActivity

		}

		// Creates list of Tweets from JSON Model
		private List<Tweet> CreateTweetList()
		{
			lstTweets = new List<Tweet> ();

			if (jTweetsArray.Count == 0) 
				return lstTweets;

			foreach (JsonObject jObj in jTweetsArray)
			{
				lstTweets.Add( new Tweet(jObj["id"], jObj["text"], jObj["posted"]) );
			}

			return lstTweets;
		}

		// Creates JSON Model (located in jTweetsArray) current Tweets
		private void CreateJTweetsArray(string jsonTwitterData)
		{
			jTweetsArray = new JsonArray ();

			if (String.IsNullOrEmpty(jsonTwitterData))
				return;

			JsonObject jElements = new JsonObject();
			JsonObject jElement;

			var Obj = JsonValue.Parse (jsonTwitterData);
			JsonArray jArray = ( JsonArray ) Obj;
			
			foreach ( JsonObject jObj in jArray ) {
				Obj = ( JsonValue ) jObj;
				jElement = new JsonObject();
				jElement.Add( "id", Obj["id_str"] );
				jElement.Add( "text", Obj["text"].ToString() );
				jElement.Add( "posted", Obj["created_at"].ToString() );
				jTweetsArray.Add( jElement );
			} 
		}

		// Gets Twitter JSON Data
		private string GetTwitterJSON(int numTweets, string user)
		{
			string jsonTwitterData = string.Empty;
			
			try
			{
				string url = "https://api.twitter.com/1/statuses/user_timeline.json?screen_name=" + user + "&count=" + numTweets.ToString() + "&page=" + sinceTweet.ToString();
				var request = WebRequest.Create(url);
				request.ContentType = "application/json";
				request.Method = "GET";
				
				using (var response = request.GetResponse() as HttpWebResponse)
				{
					if (response != null && response.StatusCode == HttpStatusCode.OK)
						using (var reader = new StreamReader(response.GetResponseStream()))
					{
						jsonTwitterData =  reader.ReadToEnd();
					}
					
				}
			}
			catch(Exception)
			{
				Console.WriteLine("Some problem with request from Twitter...");
			}

			return jsonTwitterData;
		}
	}
}


