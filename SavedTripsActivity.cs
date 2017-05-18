using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Ads;
using SQLite;
using Android.Views.InputMethods;
using Android;
using Android.Support.V4.App;
using Android.Graphics;

namespace TripTracker
{
    [Activity(Label = "Saved Trips", Icon = "@drawable/icon")] //ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, 
    public class SavedTripsActivity : Activity
    {
        public List<string> savedItemsList;
        public ListView savedTripsView;
        ArrayAdapter<string> adapter;

        ISharedPreferences pref;
        ISharedPreferencesEditor prefEdit;
        
        string newTripName;
        bool procedeSave;
        bool tripExists;
        string tripName;
        string imagesFolderPath;
        string distanceType;
        string speedType;
        AdView _bannerad;
        SQLiteConnection connection;

        const string WriteExternalStorage = Manifest.Permission.WriteExternalStorage;
        const string ReadExternalStorage = Manifest.Permission.ReadExternalStorage;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Saved);
            savedTripsView = FindViewById<ListView>(Resource.Id.saved_listView1);

            _bannerad = BannerAdWrapper.ConstructStandardBanner(this, AdSize.SmartBanner, "ca-app-pub-6665335742989505/2028275471");
            _bannerad.Bottom = 0;
            _bannerad.CustomBuild();
            var layout = FindViewById<LinearLayout>(Resource.Id.saved_LinearLayout2);
            layout.AddView(_bannerad);


            pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            prefEdit = pref.Edit();
            
            savedItemsList = new List<string>();

            UpdateList();

            adapter = new ArrayAdapter<string>(this, Resource.Layout.SavedListItems, savedItemsList);

            savedTripsView.Adapter = adapter;


            savedTripsView.ItemClick += SavedTripsView_ItemClick;

            savedTripsView.ItemLongClick += SavedTripsView_ItemLongClick;


            Toast.MakeText(this, "Press and Hold Trip For More Options",ToastLength.Short).Show();
            

        }//OnCreate()
        

        private void SavedTripsView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Trips.db");

            var connection = new SQLiteConnection(dbPath);

            var table = connection.Table<Trip>();

            imagesFolderPath = pref.GetString("imagesFolderPath", null);

            string[] fileList = System.IO.Directory.GetFiles(imagesFolderPath);


            LayoutInflater inflater = (LayoutInflater)this.GetSystemService(Context.LayoutInflaterService);
            View renameOrDelete = inflater.Inflate(Resource.Layout.RanameOrDelete, null);
            PopupWindow window = new PopupWindow(renameOrDelete, WindowManagerLayoutParams.WrapContent, WindowManagerLayoutParams.WrapContent);
            Button btnDelete = renameOrDelete.FindViewById<Button>(Resource.Id.btnDelete);
            Button btnRename = renameOrDelete.FindViewById<Button>(Resource.Id.btnRename);
            Button btnEmail = renameOrDelete.FindViewById<Button>(Resource.Id.btnEmail);


            btnEmail.Click += (s, args) => 
            {
                tripName = savedItemsList[e.Position];
                EmailTrip();
                window.Dismiss();
            };
            

            btnDelete.Click += (s, args) => 
            {
                
                tripName = savedItemsList[e.Position];
                foreach (var item in table)
                {
                    if (item.TripName == tripName)
                        connection.Delete<Trip>(item.ID);
                }

                foreach(string file in fileList)
                {
                    if (file.Contains(String.Format("{0}.Jpeg",tripName)))
                    {System.IO.File.Delete(file); }
                }
                
                UpdateList();
                //Toast.MakeText(this, String.Format("{0} Deleted", tripName), ToastLength.Long).Show();
                Intent savedTripsActivity = new Intent(this, typeof(SavedTripsActivity));
                StartActivity(savedTripsActivity);
                Finish();
                window.Dismiss();
            };

            btnRename.Click += (s, args) => 
            {
                tripName = savedItemsList[e.Position];
                ShowSetTripNameWindow();
                window.Dismiss();
            };
            
            InputMethodManager inputMethodManager = this.GetSystemService(Context.InputMethodService) as InputMethodManager;
            inputMethodManager.ShowSoftInput(renameOrDelete, ShowFlags.Forced);
            inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);

            window.ShowAtLocation(renameOrDelete, GravityFlags.Center, -20, 0);

        }//SavedTripsView_ItemLongClick()


        private void SavedTripsView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            string listTitle = savedItemsList[e.Position];
            //Toast.MakeText(this, String.Format("{0} Clicked", listTitle), ToastLength.Long).Show(); 

            TripExists(listTitle);

            prefEdit.PutString("selectTripName", listTitle);
            prefEdit.Apply();

            if (tripExists == true)
            {
                Intent tripStatsActivity = new Intent(this, typeof(TripStatsActivity));
                StartActivity(tripStatsActivity);
            }

            else
            {
                Intent splashActivity = new Intent(this, typeof(SplashActivity));
                StartActivity(splashActivity);
            }

        }//SavedTripsView_ItemClick()

        
        private void UpdateList()
        {
            var dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Trips.db");

            var connection = new SQLiteConnection(dbPath);

            var table = connection.Table<Trip>();

            foreach (var item in table)
            {
                savedItemsList.Add(item.TripName);
            }
            
        }//UpdateList()


        public override void OnBackPressed()
        {
            Finish();
            base.OnBackPressed();
        }//OnBackPressed


        public void ShowSetTripNameWindow()
        {
            Intent setTripNameActivity = new Intent(this, typeof(SetTripNameActivity));
            StartActivity(setTripNameActivity);

        }//ShowSetTripNameWindow()


        public void RenameTrip()
        {
            if (CheckSelfPermission(WriteExternalStorage) == (int)Android.Content.PM.Permission.Granted)
            {
                var dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Trips.db");

                var connection = new SQLiteConnection(dbPath);

                var table = connection.Table<Trip>();

                newTripName = pref.GetString("lastSavedTripName", null);

                imagesFolderPath = pref.GetString("imagesFolderPath", null);

                string[] fileList = System.IO.Directory.GetFiles(imagesFolderPath);

                foreach (var item in table)
                {
                    if (item.TripName == tripName)
                    {
                        connection.Query<Trip>("UPDATE Trip SET _TripName=? WHERE _Id = ?", newTripName, item.ID);
                        prefEdit.PutBoolean("procedeSave", false);
                        prefEdit.Apply();
                    }
                }

                Java.IO.File oldFile = new Java.IO.File(System.IO.Path.Combine(imagesFolderPath, String.Format("{0}.Jpeg", tripName)));
                Java.IO.File newFile = new Java.IO.File(System.IO.Path.Combine(imagesFolderPath, String.Format("{0}.Jpeg", newTripName)));
                oldFile.RenameTo(newFile);


                Intent savedTripsActivity = new Intent(this, typeof(SavedTripsActivity));
                StartActivity(savedTripsActivity);
            }

            else
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.WriteExternalStorage }, 1);
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage }, 1);
            }

        }//RenameTrip()


        protected override void OnResume()
        {
            if (_bannerad != null) _bannerad.Resume();
            UpdateList();
            base.OnResume();

            procedeSave = pref.GetBoolean("procedeSave", false);

            if (procedeSave)
            { RenameTrip(); }
           
        }//OnResume()


        private bool TripExists(string tripname)
        {

            var dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Trips.db");

            var connection = new SQLiteConnection(dbPath);

            var table = connection.Table<Trip>();

            int count = connection.Query<Trip>("SELECT * from Trip WHERE _TripName=?",tripname).Count;

            if (count > 0)
            {
                return tripExists = true;
            }

            else
            {
                return tripExists = false;
            }
            
        }


        protected override void OnPause()
        {
            if (_bannerad != null) _bannerad.Pause();
            base.OnPause();
        }//OnPause()


        private void EmailTrip()
        {
            var dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Trips.db");

            connection = new SQLiteConnection(dbPath);

            var table = connection.Table<Trip>();

            List<Trip> list = new List<Trip>();
            list = connection.Query<Trip>("SELECT * from Trip Where _TripName=?", tripName);

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
            
            string tripStart = "Trip Start:  " + tripStartString;
            string tripFinish = "Trip Finish:  " + tripFinishString;
            string tripDuration = "Trip Duration(d:h:m:s):  " + tripDurationString;
            string tripDistance = "Trip Distance:  " + tripDistanceString + " " + distanceType;
            string numberOfStops = "Number of Stops:  " + numberOfStopsString;
            string stopDuration = "Minutes Stopped:  " + stopDurationString;
            string avgSpeed = "Average Speed:  " + avgSpeedString + " " + speedType;
            

            string emailBody = "Trip Name: " + tripName;
            emailBody += "\n" + tripStart;
            emailBody += "\n" + tripFinish;
            emailBody += "\n" + tripDuration;
            emailBody += "\n" + tripDistance;
            emailBody += "\n" + numberOfStops;
            emailBody += "\n" + stopDuration;
            emailBody += "\n" + avgSpeed;



            string imagesFilePath = System.IO.Path.Combine(imagesFolderPath.ToString(), String.Format("{0}.Jpeg", tripName));
            Bitmap mapBitmap = BitmapFactory.DecodeFile(imagesFilePath);
            var file = new Java.IO.File(imagesFilePath);
            file.SetReadable(true, false);
            var uri = Android.Net.Uri.FromFile(file);
            Intent email = new Intent(Intent.ActionSend);
            email.PutExtra(Intent.ExtraSubject, string.Format("Trip Tracker App:  {0}", tripName));
            email.PutExtra(Intent.ExtraText, emailBody);
            email.PutExtra(Intent.ExtraStream, uri);
            email.SetType("message/rfrc822");
            StartActivity(Intent.CreateChooser(email, "Send Email Via"));

        }


    }
}