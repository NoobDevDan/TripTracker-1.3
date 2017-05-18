using System;
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

namespace TripTracker
{
    [Activity(Label = "Options",  Icon = "@drawable/icon")]//ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait,
    public class OptionsActivity : Activity
    {

        ISharedPreferences pref;
        ISharedPreferencesEditor prefEdit;
        bool enableStops;
        bool showKMs;
        bool portraitView;
        AdView _bannerad;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Options);

            _bannerad = BannerAdWrapper.ConstructStandardBanner(this, AdSize.LargeBanner, "ca-app-pub-6665335742989505/9551542272");
            _bannerad.Bottom = 0;
            _bannerad.CustomBuild();
            var layout = FindViewById<LinearLayout>(Resource.Id.options_LinearLayout1);
            layout.AddView(_bannerad);

            pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            prefEdit = pref.Edit();

            enableStops = pref.GetBoolean("enableStops", true);
            showKMs = pref.GetBoolean("showKMs", true);
            portraitView = pref.GetBoolean("portraitView", true);

            SetRadioButtons();

            RadioButton kilometersRadioButton = FindViewById<RadioButton>(Resource.Id.options_KilometersRadioButton);
            RadioButton milesRadioButton = FindViewById<RadioButton>(Resource.Id.options_MilesRadioButton);
            RadioButton enableStopsRadioButton = FindViewById<RadioButton>(Resource.Id.options_EnableStopsRadioButton);
            RadioButton disableStopsRadioButton = FindViewById<RadioButton>(Resource.Id.options_DisableStopsRadioButton);
            RadioButton portraitRadioButton = FindViewById<RadioButton>(Resource.Id.options_PortraitRadioButton);
            RadioButton landscapeRadioButton = FindViewById<RadioButton>(Resource.Id.options_LandscapeRadioButton);



            kilometersRadioButton.Click += (sender, e) =>
            {
                prefEdit.PutBoolean("showKMs", true);
                prefEdit.Apply();
            };

            milesRadioButton.Click += (sender, e) =>
            {
                prefEdit.PutBoolean("showKMs", false);
                prefEdit.Apply();
            };


            enableStopsRadioButton.Click += (sender, e) =>
            {
                prefEdit.PutBoolean("enableStops", true);
                prefEdit.Apply();
                Toast.MakeText(this, "Changes Made to Stops Will Only Affect New Trips", ToastLength.Short).Show();
            };


            disableStopsRadioButton.Click += (sender, e) =>
            {
                prefEdit.PutBoolean("enableStops", false);
                prefEdit.Apply();
                Toast.MakeText(this, "Changes Made to Stops Will Only Affect New Trips", ToastLength.Short).Show();
            };


            portraitRadioButton.Click += (sender, e) => 
            {
                prefEdit.PutBoolean("portraitView", true);
                prefEdit.Apply();
                Toast.MakeText(this, "Changes Made to Screen Orientation Won't Affect Started Trips", ToastLength.Short).Show();
                prefEdit.PutBoolean("changeOrientation", true);
                prefEdit.Apply();
            };


            landscapeRadioButton.Click += (sender, e) =>
            {
                prefEdit.PutBoolean("portraitView", false);
                prefEdit.Apply();
                Toast.MakeText(this, "Changes Made to Screen Orientation Won't Affect Started Trips", ToastLength.Short).Show();
                prefEdit.PutBoolean("changeOrientation", true);
                prefEdit.Apply();
            };


            

        }//OnCreate()


        protected override void OnResume()
        {
            if (_bannerad != null) _bannerad.Resume();
            base.OnResume();
            enableStops = pref.GetBoolean("enableStops", true);
            showKMs = pref.GetBoolean("showKMs", true);
            SetRadioButtons();
        }//OnResume()


        protected override void OnPause()
        {
            if (_bannerad != null) _bannerad.Pause();
            base.OnPause();
        }//OnPause()


        private void SetRadioButtons()
        {
            RadioButton kilometersRadioButton = FindViewById<RadioButton>(Resource.Id.options_KilometersRadioButton);
            RadioButton milesRadioButton = FindViewById<RadioButton>(Resource.Id.options_MilesRadioButton);
            RadioButton enableStopsRadioButton = FindViewById<RadioButton>(Resource.Id.options_EnableStopsRadioButton);
            RadioButton disableStopsRadioButton = FindViewById<RadioButton>(Resource.Id.options_DisableStopsRadioButton);
            RadioButton portraitRadioButton = FindViewById<RadioButton>(Resource.Id.options_PortraitRadioButton);
            RadioButton landscapeRadioButton = FindViewById<RadioButton>(Resource.Id.options_LandscapeRadioButton);

            if (enableStops)
            {
                enableStopsRadioButton.Checked = true;
                disableStopsRadioButton.Checked = false;
            }
            else
            {
                enableStopsRadioButton.Checked = false;
                disableStopsRadioButton.Checked = true;
            }

            if (showKMs)
            {
                kilometersRadioButton.Checked = true;
                milesRadioButton.Checked = false;
            }
            else
            {
                kilometersRadioButton.Checked = false;
                milesRadioButton.Checked = true;
            }


            if (portraitView)
            {
                portraitRadioButton.Checked = true;
                landscapeRadioButton.Checked = false;
            }
            else
            {
                landscapeRadioButton.Checked = true;
                portraitRadioButton.Checked = false;
            }
        }//SetRadioButtons()


        public override void OnBackPressed()
        {
            Finish();
            base.OnBackPressed();
        }


    }
}