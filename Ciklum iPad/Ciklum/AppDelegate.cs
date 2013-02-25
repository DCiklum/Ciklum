using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Json;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
//using MonoTouch.Twitter; // older version of request services
using MonoTouch.Social;  // newer version of request services  

namespace Ciklum
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		UINavigationController nav;
		UINavigationController navFav;
		DialogViewController rootVC;
		DialogViewController rootVCFav;
		RootElement rootElement;
		RootElement rootFav;
		UIBarButtonItem favButton;
		UIBarButtonItem refreshButton;
		UIBarButtonItem backButton;
		int sinceTweet = 1;				// page number 
		JsonArray jTweetsArray = new JsonArray();			// array of tweets
		JsonArray jFavoritesArray;		// array of favorite tweets

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds); 

			rootElement = new RootElement ("Ciklum on Twitter");
			rootFav = new RootElement ("Favorites");

			rootVC = new DialogViewController (rootElement);	// Dialog for main screen
			nav = new UINavigationController (rootVC);

			rootVCFav = new DialogViewController (rootFav);		// Dialog for Favorites screen
			navFav = new UINavigationController (rootVCFav);

			ProcessTweets( 20, "ciklum" );	// reading twitter data, creating elements with InvokeOnMainThread 
			CreateJSONModelFavorites();		// Creating JSON Model data for Favorites screen

			favButton = new UIBarButtonItem (UIBarButtonSystemItem.Bookmarks, delegate (object sender, EventArgs e) {
				window.RootViewController = navFav;
				ProcessFavorites();
			});
			rootVC.NavigationItem.RightBarButtonItem = favButton;
			 
			refreshButton = new UIBarButtonItem (UIBarButtonSystemItem.Refresh, delegate (object sender, EventArgs e) {
				ProcessTweets( 20, "ciklum" );
			});
			rootVC.NavigationItem.LeftBarButtonItem = refreshButton;

			backButton = new UIBarButtonItem ("< Back", UIBarButtonItemStyle.Bordered, delegate (object sender, EventArgs e) {
				window.RootViewController = nav;
				ProcessTweets( 20, "ciklum" );
			});
			rootVCFav.NavigationItem.LeftBarButtonItem = backButton;

			rootVC.SetToolbarItems( new UIBarButtonItem[] {
				new UIBarButtonItem("< Previous", UIBarButtonItemStyle.Bordered, (s,e) => {
					sinceTweet++;
					ProcessTweets( 20, "ciklum" );
				})
				, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) { Width = 50 }
				, new UIBarButtonItem("Next >", UIBarButtonItemStyle.Bordered, (s,e) => {
					if (sinceTweet>1) sinceTweet--;
					ProcessTweets( 20, "ciklum" );
				}) { Width = 80 }
			}, false);
			nav.ToolbarHidden = false;

			window.RootViewController = nav;
			window.MakeKeyAndVisible ();

			return true;
		}

		// Creates MonoTouch.Dialog Elements using JSON Model (jFavoritesElements)
		private void ProcessFavorites ()
		{
			JsonObject jFavoritesElements = new JsonObject(); 
			JsonArray jSections = new JsonArray();

			jFavoritesElements.Add( "elements", jFavoritesArray );
			jSections.Add( jFavoritesElements );
			jFavoritesElements.Add( "header", "Click a star to delete Tweet" );
			jFavoritesElements.Add( "sections", jSections );

			var jsonEl = JsonElement.FromJson( jFavoritesElements );

			rootFav.Clear();
			rootFav.Add( jsonEl );
			foreach ( JsonObject jObj in jFavoritesArray ) 
			{
				((BoolElement)jsonEl[jObj["id"]]).ValueChanged += delegate(object sender, EventArgs e) {
					NSIndexPath pt = ((BoolElement)sender).IndexPath;
					DeleteTweetFromFavorites((int)pt.Row);
				}; 
			}
		}

		// Gets string request from Twitter and creates MonoTouch.Dialog Elements using JSON data from CreateJSONModel
		private void ProcessTweets (int numTweets, string user)
		{
			// Tweet using MonoTouch.Social
			var parameters = new NSDictionary ();
			var request = SLRequest.Create (SLServiceKind.Twitter,
			                                SLRequestMethod.Get,
			                                new NSUrl ("https://api.twitter.com/1/statuses/user_timeline.json?screen_name=" + user + "&count=" + numTweets.ToString() + "&page=" + sinceTweet.ToString()),
			                                parameters);
			request.PerformRequest ((data, response, error) => {
				try
				{
					if (response.StatusCode == 200) {
						InvokeOnMainThread (() => {
							// creating MonoTouch.Dialog Elements
							var jsonEl = JsonElement.FromJson(CreateJSONModelTweets( data.ToString(NSStringEncoding.UTF8) ));
							rootElement.Clear();
							rootElement.Add(jsonEl);
							foreach ( JsonObject jObj in jTweetsArray ) 
							{
								((BoolElement)jsonEl[jObj["id"]]).ValueChanged += delegate(object sender, EventArgs e) {
									Boolean bElement = ((BoolElement)sender).Value;
									if (!bElement)
									{
										((BoolElement)sender).Value = true;	// tweet is already in Favorites
									}
									else
									{
										NSIndexPath pt = ((BoolElement)sender).IndexPath;
										AddTweetToFavorites ((int)pt.Row);
									}
								}; 
							}
						});
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					//UIAlertView aView = new UIAlertView("@ciklum tweeets", "We are sorry! Some problem with twitter request.", null, "OK", "Cancel");
					//aView.Show();
				}
				
			});

			// Tweet using MonoTouch.Twitter
			/*NSDictionary dic = new NSDictionary();
			TWRequest request = new TWRequest(new NSUrl("https://api.twitter.com/1/statuses/user_timeline.json?screen_name=ciklum&count=1"), dic, TWRequestMethod.Get);
		
			try
			{
				request.PerformRequest((nsdata, urlresponse, error) => {
					if (urlresponse.StatusCode == 200)
					{
						sResult = nsdata.ToString(NSStringEncoding.UTF8);
					}
					else
					{
						Console.WriteLine("Nothing from Twitter :(");
					}
				}); 
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			} */
		}

		// Adds tweet to Favorites if not already exists
		private void AddTweetToFavorites (int iPos)
		{
			string sText = "";
			string sID = "";

			// Searching tweet data
			foreach (JsonValue jObj in jTweetsArray) {
				if (jObj ["pos"] == (int)iPos) {
					sText = jObj ["caption"];
					sID = jObj ["id"];
				}
			}

			// Adding new tweet to Favorites if its unique 
			bool unique = true;
			foreach (JsonValue jObj in jFavoritesArray) {
				if (jObj ["id"] == sID)
					unique = false; 
			}

			if (unique) {
				JsonObject jElement = new JsonObject();
				jElement.Add( "id", sID );
				jElement.Add( "type", "boolean" );
				jElement.Add( "caption", sText );
				jElement.Add( "value", "true" );
				jElement.Add( "on", "favorited.png" );
				jElement.Add( "off", "~/favorite.png" );

				jFavoritesArray.Add( jElement );
			}

			SaveFavoritesToFile();
		}

		// Deletes tweet from Favorites and saves to file
		private void DeleteTweetFromFavorites (int iPos)
		{
			jFavoritesArray.RemoveAt(iPos);
			SaveFavoritesToFile();
			ProcessFavorites();
		}

		// Creates Model of Data from Tweets request
		private JsonObject CreateJSONModelTweets (string s)
		{
			JsonObject jElements = new JsonObject();
			JsonObject jElement;
			JsonArray jSections = new JsonArray();
			int i = 0;		// tweet position in screen

			jTweetsArray = new JsonArray();
			var Obj = JsonValue.Parse (s);
			JsonArray jArray = ( JsonArray ) Obj;

			foreach ( JsonObject jObj in jArray ) {
				Obj = ( JsonValue ) jObj;
				jElement = new JsonObject();
				jElement.Add( "pos", i.ToString() );
				jElement.Add( "id", Obj["id_str"] );
				jElement.Add( "type", "boolean" );
				jElement.Add( "caption", GetPosted( Obj["created_at"], Obj["text"].ToString() ));
				jElement.Add( "value", "false" );
				jElement.Add( "on", "favorited.png" );
				jElement.Add( "off", "~/favorite.png" );

				jTweetsArray.Add( jElement );
				i++;
			}

			jElements.Add( "elements", jTweetsArray );
			jSections.Add( jElements );
			jElements.Add( "header", "Click a star to add Tweet" );
			jElements.Add( "sections", jSections );

			return jElements;
		}

		// Creates Model of Data (jFavoritesArray) from favorites.json
		private void CreateJSONModelFavorites ()
		{
			try
			{
				string filePath = GetAppPath() + "/favorite.json";
				FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read);
				jFavoritesArray = ( JsonArray ) JsonValue.Load(fs);
				fs.Close();
			} 
			catch (Exception e)
			{
				jFavoritesArray = ( JsonArray ) JsonValue.Parse("[]");
			}
		}

		private void SaveFavoritesToFile ()
		{
			string filePath = GetAppPath() + "/favorite.json";
			File.WriteAllText(filePath, jFavoritesArray.ToString());
		}

		private string GetAppPath ()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		}

		// Formatting <created_at> from Twitter
		private string GetPosted (string sInput, string sText)
		{
			//Thu Feb 21 10:13:57 +0000 2013 <---Input
			//XXXXFeb 21XXXXXXXXXXXXXXX 2013 <---Output
			return sInput.Remove( sInput.Length - 21, 15 ).Remove( 0, 4 ) + " " + sText.Replace(@"\", @"/");
		}
	}
}

