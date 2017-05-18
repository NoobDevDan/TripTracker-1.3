using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Gms.Ads;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android;
using Android.Support.V4.App;
using SQLite;
using System.Threading.Tasks;

namespace TripTracker
{
    [Activity(Label = "TripTracker",  MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar")] //ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
    public class SplashActivity : Activity
    {
        ISharedPreferences pref;
        ISharedPreferencesEditor prefEdit;
        AdView _bannerad;

        string imagesFolderPath;
        bool doubleBackToExitPressedOnce;
        bool portraitView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Splash);
            
            _bannerad = BannerAdWrapper.ConstructStandardBanner(this, AdSize.SmartBanner, "ca-app-pub-6665335742989505/3563884270");
            _bannerad.Bottom = 0;
            _bannerad.CustomBuild();
            var layout = FindViewById<LinearLayout>(Resource.Id.splash_LinearLayout1);
            layout.AddView(_bannerad);

            pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            prefEdit = pref.Edit();

            if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted) { CheckFolderPermissions(); }
            if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted) { CheckGPSPermissions(); }
            if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Android.Content.PM.Permission.Granted) { CreateImagesFolder(); }

            CreateDatabase();
            
            
            prefEdit.PutBoolean("procedeSave", false);
            prefEdit.Apply();

            if(pref.Contains("enableStops") == false)
            {
                prefEdit.PutBoolean("enableStops", true);
                prefEdit.Apply();
            }

            if (pref.Contains("showKMs") == false)
            {
                prefEdit.PutBoolean("showKMs", true);
                prefEdit.Apply();
            }

            if (pref.Contains("portraitView") == false)
            {
                prefEdit.PutBoolean("portraitView", true);
                prefEdit.Apply();
            }

            if (pref.Contains("changeOrientation") == false)
            {
                prefEdit.PutBoolean("changeOrientation", true);
                prefEdit.Apply();
            }


            portraitView = pref.GetBoolean("portraitView", true);


            Button splash_NewButton = FindViewById<Button>(Resource.Id.splash_NewButton);

            Button splash_SavedButton = FindViewById<Button>(Resource.Id.splash_SavedButton);

            Button splash_OptionsButton = FindViewById<Button>(Resource.Id.splash_OptionsButton);


            splash_NewButton.Click += (object sender, EventArgs e) =>
            {
                
                if (portraitView)
                {
                    Intent mainActivity = new Intent(this, typeof(MainActivity));
                    StartActivity(mainActivity);
                }

                else
                {
                    Intent mainActivityLandscape = new Intent(this, typeof(MainActivityLandscape));
                    StartActivity(mainActivityLandscape);
                }

                Finish();
            };


            splash_SavedButton.Click += (object sender, EventArgs e) =>
            {
                Intent savedTripsActivity = new Intent(this, typeof(SavedTripsActivity));
                StartActivity(savedTripsActivity);
            };

            splash_OptionsButton.Click += (object sender, EventArgs e) =>
            {
                Intent optionsActivity = new Intent(this, typeof(OptionsActivity));
                StartActivity(optionsActivity);
            };

        }//OnCreate


        public Task CheckFolderPermissions()
        {
            return Task.Factory.StartNew(() =>
            ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 1));
        }//CheckFolderPermissions()

        
        public Task CheckGPSPermissions()
        {
            return Task.Factory.StartNew(() =>  ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.AccessFineLocation }, 1));
        }//CheckGPSPermissions()


        private string CreateDatabase()
        {
            var dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Trips.db");

            try
            {
                var connection = new SQLiteConnection(dbPath);
                {
                    connection.CreateTable<Trip>();
                    return "Database created";
                }
                
            }

            catch (SQLiteException ex)
            {
                return ex.Message;
            }

        }//CreateDatabase()


        public override void OnBackPressed()
        {
            if (doubleBackToExitPressedOnce)
            {
                Finish();
                Java.Lang.JavaSystem.Exit(0);
               // base.OnBackPressed();
            }
            
            this.doubleBackToExitPressedOnce = true;
            Toast.MakeText(this, "Press Back Twice to Exit", ToastLength.Short).Show();

            new Handler().PostDelayed(() =>
            {
                doubleBackToExitPressedOnce = false;
            }, 2000);
        
        }//OnBackPressed()


        protected override void OnResume()
        {
            if (_bannerad != null) _bannerad.Resume();
            base.OnResume();
        }//OnResume()


        public  void CreateImagesFolder()
        {
            try
            {
                imagesFolderPath = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).ToString(), "TripImages");
                Java.IO.File dir = new Java.IO.File(imagesFolderPath);
                if (dir.Exists() == false)
                {
                    Directory.CreateDirectory(imagesFolderPath);
                    prefEdit.PutBoolean("imagesFolderExists", true);
                    prefEdit.Apply();
                }

                prefEdit.PutString("imagesFolderPath", imagesFolderPath.ToString());
                prefEdit.Apply();
            }

            catch {  }
        }//CreateImagesFolder


        protected override void OnPause()
        {
            if (_bannerad != null) _bannerad.Pause();
            base.OnPause();
        }//OnPause()



    }
}