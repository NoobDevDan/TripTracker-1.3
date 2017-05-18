using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using System;
using Android.Gms.Maps.Model;
//using Android.Gms.Common.Apis;
using Android.Locations;
using Android.Runtime;
using Android.Content;
using Android;
using Android.Support.V4.App;
using System.Collections.Generic;
using System.Linq;
using Android.Content.PM;
using System.IO;
using Android.Views;

using SQLite;
using Android.Views.InputMethods;
using Android.Graphics;

namespace TripTracker
{
    [Activity(Label = "TripStatsActivity", Theme = "@android:style/Theme.NoTitleBar")] //ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, 
    public class TripStatsActivity : Activity
    {
        ISharedPreferences pref;
        ISharedPreferencesEditor prefEdit;

        string selectTripName;
        string distanceType;
        string speedType;
        string imagesFolderPath;
        string imagesFilePath;

        SQLiteConnection connection;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.TripStats);

            pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            prefEdit = pref.Edit();

            selectTripName = pref.GetString("selectTripName", null);

            imagesFolderPath = pref.GetString("imagesFolderPath", null);
            imagesFilePath = System.IO.Path.Combine(imagesFolderPath.ToString(), String.Format("{0}.Jpeg", selectTripName));
            Bitmap mapBitmap = BitmapFactory.DecodeFile(imagesFilePath);

            TextView tripStart = FindViewById<TextView>(Resource.Id.tripStats_TripStart);
            TextView tripFinish = FindViewById<TextView>(Resource.Id.tripStats_TripFinish);
            TextView tripName = FindViewById<TextView>(Resource.Id.tripStats_TripName);
            TextView tripDuration = FindViewById<TextView>(Resource.Id.tripStats_TripDuration);
            TextView tripDistance = FindViewById<TextView>(Resource.Id.tripStats_TripDistance);
            TextView numberOfStops = FindViewById<TextView>(Resource.Id.tripStats_NumberOfStops);
            TextView stopDuration = FindViewById<TextView>(Resource.Id.tripStats_StopDuration);
            TextView avgSpeed = FindViewById<TextView>(Resource.Id.tripStats_AvgSpeed);
            ImageView mapImage = FindViewById<ImageView>(Resource.Id.tripStats_ImageView1);

            var dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Trips.db");

            connection = new SQLiteConnection(dbPath);

            var table = connection.Table<Trip>();

            List<Trip> list = new List<Trip>();
            list = connection.Query<Trip>("SELECT * from Trip Where _TripName=?", selectTripName);
            
            string avgSpeedString = list[0].AVGSpeed.ToString();
            string tripStartString = list[0].TripStart.ToString();
            string tripFinishString = list[0].TripFinish.ToString();
            string tripDurationString = list[0].TripDuration.ToString();
            string tripDistanceString = list[0].Distance.ToString("N1");
            string numberOfStopsString = list[0].StopsCount.ToString();
            string stopDurationString = list[0].StopsDuration.ToString();
            string milesOrKilometers = list[0].MilesOrKms.ToString();

            if (milesOrKilometers == "Kilometers")
            {
                distanceType = "Kms";
                speedType = "Km/h";
            }

            else if (milesOrKilometers == "Miles")
            {
                distanceType = "Miles";
                speedType = "Mi/h";
            }
            
            
            tripName.Text =  selectTripName;
            tripName.PaintFlags = PaintFlags.UnderlineText;
            tripName.Typeface = Typeface.DefaultBold;
            tripStart.Text = "Trip Start:  " + tripStartString;
            tripFinish.Text = "Trip Finish:  " + tripFinishString;
            tripDuration.Text = "Trip Duration(d:h:m:s):  " +  tripDurationString;
            tripDistance.Text = "Trip Distance:  " +  tripDistanceString + " " + distanceType;
            numberOfStops.Text = "Number of Stops:  " + numberOfStopsString;
            stopDuration.Text = "Minutes Stopped:  " + stopDurationString;
            avgSpeed.Text = "Average Speed:  " + avgSpeedString + " " + speedType;
            mapImage.SetImageBitmap(mapBitmap);
        }//OnCreate()
        

    }
}