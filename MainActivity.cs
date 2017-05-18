using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using System;
using Android.Gms.Maps.Model;
using Android.Gms.Common.Apis;
using Android.Gms.Ads;
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
using System.Threading.Tasks;
using Android.Graphics;
using static Android.Gms.Maps.GoogleMap;

namespace TripTracker
{

    [Activity(Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@android:style/Theme.NoTitleBar")]  
    public class MainActivity : Activity, IOnMapReadyCallback, ILocationListener, ISnapshotReadyCallback
    {

        private GoogleMap nMap;
        LocationManager locationManager;
        Location location;
        double lat, lng;
        double oldlat, oldlng;
        double startlat, startlng;
        double stoplat, stoplng;
        double stopNowLat, stopNowLng;
        double stopThenLat, stopThenLng;
        double speed;
        double totalDistance = 0;
        double averageSpeed;
        double distance;
        double markerDistance;
        bool tripstarted = false;
        bool broken = false;
        bool tripStopped = false;
        bool showKMsBool;
        bool tripOnScreen = false;
        bool doubleBackToExitPressedOnce = false;
        bool procedeSave = false;
        bool stopped = false;
        bool enableStops;
        bool tripSaved = false;
        bool portraitView;
        bool changeOrientation;
        string provider;
        string avgSpeedString = "-";
        string distanceString = "-";
        string speedNowString = "-";
        string stopCountString = "-";
        string KMsOrMiles;
        string lastSavedTripName;
        string imagesFolderPath;
        string tripName;
        string startDate;
        string finishDate;
        string startTime;
        string finishTime;
        int stopCounter = 0;
        int totalStopDuration = 0;
        int totalSpeed;
        TimeSpan tripDurationStart;
        TimeSpan tripDurationFinish;
        TimeSpan tripDuration;
        TimeSpan stopTimeThen;
        TimeSpan stopTimeNow;
        TimeSpan stopDuration;
        MarkerOptions finishMarker;
        MarkerOptions startMarker;
        LatLng stopLatLng;
        LatLng startLatLng;
        LatLng finishLatLng;
        LatLng markerLatLng;
        LatLng latlng;
        Location stopThen;
        Location stopNow;
        Location markerLocation;
        Location currentLocation;
        List<double> speedList = new List<double>();
        LatLngBounds.Builder boundsBuilder;
        CameraUpdate camera;
        InterstitialAd interstitialAd;
        InterstitialAd FinalAd;
        adlistener intlistener;
        Criteria criteria;
        Button startButton;

        const string WriteExternalStorage = Manifest.Permission.WriteExternalStorage;
        const string ReadExternalStorage = Manifest.Permission.ReadExternalStorage;

        ISharedPreferences pref;
        ISharedPreferencesEditor prefEdit;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            if(bundle != null)
            {
                AssignVariables(bundle);
                startButton = FindViewById<Button>(Resource.Id.startButton);

                if (tripstarted == true)
                {
                    startButton.Enabled = false;
                }

                else if(tripStopped && tripOnScreen)
                {
                    startButton.Text = "Reset Trip";
                }
            }
            
            SetUpMap();

            CreateUserPreferences();
            CheckUserPreferences();
            CreateImagesFolder();

            locationManager = (LocationManager)GetSystemService(Context.LocationService);
            
            criteria = new Criteria();
            criteria.Accuracy = Accuracy.Fine;
            criteria.PowerRequirement = Power.Low;

            provider = locationManager.GetBestProvider(criteria, true);

            locationManager.RequestLocationUpdates(provider, 1000, 1, this);

            location = locationManager.GetLastKnownLocation(provider);

            if (location == null) { GetLocation(); }
                
            try
            {
                lat = location.Latitude;
                lng = location.Longitude;

                LatLng latlng = new LatLng(lat, lng);

                camera = CameraUpdateFactory.NewLatLngZoom(latlng, 12);
                nMap.MoveCamera(camera);
            }

            catch {  }

            
            startButton = FindViewById<Button>(Resource.Id.startButton);

            Button stopButton = FindViewById<Button>(Resource.Id.stopButton);

            ImageButton walkButton = FindViewById<ImageButton>(Resource.Id.walkButton);

            ImageButton carButton = FindViewById<ImageButton>(Resource.Id.carButton);

            ImageButton planeButton = FindViewById<ImageButton>(Resource.Id.planeButton);

            ImageButton menuButton = FindViewById<ImageButton>(Resource.Id.menuButton);

            MapFragment mapFragment = FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map);

            mapFragment.RetainInstance = true; //Keeps map markers etc. when screen orientation changes.


            menuButton.Click += (object sender, EventArgs e) =>
             {
                 PopupMenu menu = new PopupMenu(this, menuButton);
                 menu.MenuInflater.Inflate(Resource.Menu.popup_menu, menu.Menu);


                 menu.MenuItemClick += (s, arg) =>
                 {

                     if (arg.Item.TitleFormatted.ToString() == "Save Trip")
                     { SetTripName(); }//Save Trip Menu Item Clicked

                     else if (arg.Item.TitleFormatted.ToString() == "Saved Trips")
                     {
                         if (tripstarted == false)
                         {
                             Intent savedTripsActivity = new Intent(this, typeof(SavedTripsActivity));
                             StartActivity(savedTripsActivity);
                         }

                         else if (tripstarted == true)
                         { Toast.MakeText(this, String.Format("Please Stop Your Current Trip Before Reviewing Previous Trips."), ToastLength.Long).Show(); }

                     }//Saved Trips Menu Item Clicked
                     
                     else if(arg.Item.TitleFormatted.ToString() == "Options")
                     {
                         Intent optionsActivity = new Intent(this, typeof(OptionsActivity));
                         StartActivity(optionsActivity);
                     }//Options Menu Item Clicked
                     
                 }; //Menu Item Click


                 menu.DismissEvent += (s, arg) =>
                 {
                     // Toast.MakeText(this, String.Format("Menu Dismissed"), ToastLength.Long).Show();
                 };


                 menu.Show();

             };//menuButton.Click


            startButton.Click += (object sender, EventArgs e) =>
            {
                if (startButton.Text == "Start Trip")
                {
                    startlat = lat;
                    startlng = lng;
                    startLatLng = new LatLng(startlat, startlng);

                    if (startlat != 0)
                    {
                        tripDurationStart = DateTime.Now.TimeOfDay;
                        startTime = DateTime.Now.ToString("HH:mm");
                        startDate = DateTime.Now.ToString("dd-MMM-yyy");
                        stopTimeThen = TimeSpan.Zero;
                        tripOnScreen = true;
                        tripstarted = true;

                        startMarker = new MarkerOptions()
                            .SetPosition(startLatLng)
                            .SetTitle("Trip Start")
                            .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));

                        nMap.AddMarker(startMarker);
                        startButton.Enabled = false;

                        SetInvisibleMarker();
                    }

                    else
                    {
                        Toast.MakeText(this,"Current Location Unavailable",ToastLength.Short).Show();

                        locationManager = (LocationManager)GetSystemService(Context.LocationService);
                        
                        provider = locationManager.GetBestProvider(criteria, true);

                        location = locationManager.GetLastKnownLocation(provider);

                        lat = location.Latitude;
                        lng = location.Longitude;
                    }
                    
                }

                else if (startButton.Text == "Reset Trip")
                {
                    tripSaved = false; 
                    stopCounter = 0;
                    nMap.Clear();
                    totalDistance = 0;
                    startButton.Text = "Start Trip";
                    distanceString = "-";
                    SetDistanceText();
                    avgSpeedString = "-";
                    speedNowString = "-";
                    SetSpeedText();
                    tripOnScreen = false;
                    tripStopped = false;
                    tripDurationFinish = TimeSpan.Zero;
                    tripDurationStart = TimeSpan.Zero;
                    stopTimeThen = TimeSpan.Zero;
                    enableStops = pref.GetBoolean("enableStops", true);
                    SetStopText();
                    OrientationChange();
                }

            }; //startButton.Click

            stopButton.Click += (object sender, EventArgs e) =>
            {
                startButton.Enabled = true;

                if (tripDurationFinish == TimeSpan.Zero)
                {
                    tripDurationFinish = DateTime.Now.TimeOfDay;
                    tripDuration = tripDurationFinish - tripDurationStart;
                }

                if (tripstarted)
                {
                    Toast.MakeText(this, string.Format("Trip Duration: {0} Hours, {1} Minutes", tripDuration.Hours, tripDuration.Minutes), ToastLength.Long).Show();
                    finishTime = DateTime.Now.ToString("HH:mm");
                    finishDate = DateTime.Now.ToString("dd-MMM-yyy");

                    stoplat = lat;
                    stoplng = lng;

                    finishLatLng = new LatLng(stoplat, stoplng);
                    finishMarker = new MarkerOptions()
                        .SetPosition(finishLatLng)
                        .SetTitle("Trip Finish");

                    nMap.AddMarker(finishMarker);

                    tripstarted = false;
                    tripStopped = true;

                    startButton.Text = "Reset Trip";

                    CheckDistanceForZoom();
                    DisplayAd();
                }

                else if (tripOnScreen == false)
                { Toast.MakeText(this, string.Format("You Haven't Started Your Trip Yet!"), ToastLength.Long).Show(); }

                else
                {
                    CheckDistanceForZoom();
                    Toast.MakeText(this, string.Format("Trip Duration: {0} Hours, {1} Minutes", tripDuration.Hours, tripDuration.Minutes), ToastLength.Long).Show();
                }

            }; //stopButton.Click


            startButton.LongClick += StartButton_LongClick;
            

                walkButton.Click += (object sender, EventArgs e) =>
            {
                latlng = new LatLng(lat, lng);
                MarkerOptions options = new MarkerOptions()
                    .SetPosition(latlng);


                camera = CameraUpdateFactory.NewLatLngZoom(latlng, 17);
                nMap.MoveCamera(camera);
            };

            carButton.Click += (object sender, EventArgs e) =>
            {
                latlng = new LatLng(lat, lng);
                MarkerOptions options = new MarkerOptions()
                    .SetPosition(latlng);


                camera = CameraUpdateFactory.NewLatLngZoom(latlng, 12);
                nMap.MoveCamera(camera);
            };

            planeButton.Click += (object sender, EventArgs e) =>
            {
                latlng = new LatLng(lat, lng);
                MarkerOptions options = new MarkerOptions()
                    .SetPosition(latlng);


                camera = CameraUpdateFactory.NewLatLngZoom(latlng, 4);
                nMap.MoveCamera(camera);
            };


            try { locationManager.RequestLocationUpdates(provider, 1000, 1, this); }

            catch { broken = true; }

            boundsBuilder = new LatLngBounds.Builder();

            enableStops = pref.GetBoolean("enableStops", true);

            SetSpeedText();
            SetDistanceType();
            SetDistanceText();
            SetStopText();

            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);//WakeLock

            LoadAdTask();

        } //OnCreate


        private void StartButton_LongClick(object sender, View.LongClickEventArgs e)
        {
            if (startButton.Text == "Start Trip")
            {
                startlat = lat;
                startlng = lng;
                startLatLng = new LatLng(startlat, startlng);

                tripDurationStart = DateTime.Now.TimeOfDay;
                startTime = DateTime.Now.ToString("HH:mm");
                startDate = DateTime.Now.ToString("dd-MMM-yyy");
                stopTimeThen = TimeSpan.Zero;
                tripOnScreen = true;
                tripstarted = true;

                startMarker = new MarkerOptions()
                    .SetPosition(startLatLng)
                    .SetTitle("Trip Start")
                    .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));

                nMap.AddMarker(startMarker);
                startButton.Enabled = false;

                SetInvisibleMarker();
            }
        }//StartButton_LongClick()


        private void SaveTrip()
        {
            string MilesOrKMs = "";
            
            prefEdit.PutBoolean("procedeSave", false);
            prefEdit.Apply();

            procedeSave = false;

            tripName = pref.GetString("lastSavedTripName", null);

            var dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Trips.db");

            var connection = new SQLiteConnection(dbPath);

            var table = connection.Table<Trip>();

            int count = 0;

            string tripDurationString = string.Format("{0}:{1}:{2}:{3}", tripDuration.Days.ToString(), tripDuration.Hours.ToString(), tripDuration.Minutes.ToString(), tripDuration.Seconds.ToString());

            foreach (var item in table)
            {
                if (item.TripName == tripName)
                { count++; }
            }

            try
            {

                if (count < 1)
                {
                    if (showKMsBool == true)
                    {
                        MilesOrKMs = "Kilometers";
                    }

                    else if (showKMsBool == false)
                    {
                        MilesOrKMs = "Miles";
                    }

                    Trip trip = new Trip();
                    trip.TripName = tripName;
                    trip.TripStart = startDate + "  " +  startTime;
                    trip.TripFinish = finishDate + "  " + finishTime; 
                    trip.TripDuration = tripDurationString;
                    trip.StopsCount = stopCountString;
                    if(enableStops) trip.StopsDuration = totalStopDuration.ToString() + " Minutes";
                    else if(!enableStops) trip.StopsDuration = "N/A";

                    trip.AVGSpeed = averageSpeed;
                    trip.Distance = totalDistance;
                    trip.MilesOrKms = MilesOrKMs;
                    connection.Insert(trip);

                    try
                    { nMap.Snapshot(this); }

                    catch { Toast.MakeText(this, String.Format("Could Not Take Map Image"), ToastLength.Long).Show(); }
                   
                    Toast.MakeText(this, String.Format("Save Successful"), ToastLength.Long).Show();
                    tripSaved = true;

                }
                

            }//try

            catch
            { Toast.MakeText(this, String.Format("Something Went Wrong!"), ToastLength.Long).Show(); }
            

        }//SaveTrip()

        
        private void CheckUserPreferences()
        { showKMsBool = pref.GetBoolean("showKMs", true); }//CheckUserPreferences()


        private void CreateUserPreferences()
        {
            pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            prefEdit = pref.Edit();

            if (pref.Contains("showKMs") == false)
            {
                prefEdit.PutBoolean("showKMs", true);
                prefEdit.PutString("lastSavedTripName", string.Empty);
                prefEdit.PutBoolean("procedeSave", false);
                prefEdit.PutString("previousTab", "MainActivity");
                prefEdit.Apply();
            }
        }//CreateUserPreferences()


        private void CheckDistanceForZoom()
        {
            Location startLocation = new Location("");
            startLocation.Latitude = startlat;
            startLocation.Longitude = startlng;

            Location finishLocation = new Location("");
            finishLocation.Latitude = stoplat;
            finishLocation.Longitude = stoplng;

            markerDistance = startLocation.DistanceTo(finishLocation);

            if (markerDistance >= 0) //markerDistance is in meters
            {
                boundsBuilder.Include(finishLatLng);
                boundsBuilder.Include(startLatLng);
                nMap.MoveCamera(CameraUpdateFactory.NewLatLngBounds(boundsBuilder.Build(), 135));
            }

        }//CheckDistanceForZoom()


        private void CheckForStopped()
        {
            stopTimeNow = DateTime.Now.TimeOfDay;
            stopNowLat = lat;
            stopNowLng = lng;

            stopNow = new Location("");
            stopNow.Latitude = stopNowLat;
            stopNow.Longitude = stopNowLng;


            if (stopTimeThen == TimeSpan.Zero)
            {
                stopTimeThen = DateTime.Now.TimeOfDay;
                stopThenLat = lat;
                stopThenLng = lng;

                stopThen = new Location("");
                stopThen.Latitude = stopThenLat;
                stopThen.Longitude = stopThenLng;
            }

            stopDuration = stopTimeNow.Subtract(stopTimeThen);

            if (stopThen.DistanceTo(stopNow) > 100 && stopDuration.Minutes < 3)
            {
                stopTimeThen = TimeSpan.Zero;
                stopped = false;
            }

            else if (stopThen.DistanceTo(stopNow) < 100 && stopDuration.Minutes > 3)
            {
                stopped = true;
            }

            if (tripstarted && enableStops && stopped && stopThen.DistanceTo(stopNow) > 100)
            {
                stopCounter += 1;

                stopLatLng = new LatLng(stopThenLat, stopThenLng);
                MarkerOptions options = new MarkerOptions()
                    .SetPosition(stopLatLng)
                    .SetTitle(string.Format("Stop  #{0}", stopCounter))
                    .SetSnippet(string.Format("Stop  Duration: {0} Minutes", stopDuration.Minutes))
                    .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueOrange));

                nMap.AddMarker(options);
                stopTimeNow = TimeSpan.Zero;
                stopTimeThen = TimeSpan.Zero;

                stopThenLat = lat;
                stopThenLng = lng;

                boundsBuilder.Include(stopLatLng);

                totalStopDuration += stopDuration.Minutes;

                stopped = false;
                SetStopText();
            }

        }//CheckForStopped()


        public void GetSpeed()
        {
            totalSpeed = 0;

            locationManager = (LocationManager)GetSystemService(Context.LocationService);

            Criteria criteria = new Criteria();
            criteria.Accuracy = Accuracy.Fine;
            criteria.PowerRequirement = Power.Low;

            provider = locationManager.GetBestProvider(criteria, true);

            location = locationManager.GetLastKnownLocation(provider);


            if (tripstarted)
            {

                if (showKMsBool == true)
                { speed = location.Speed * 3.8; } //Google says to multiply by 3.6 - adjusted so that app speed matched my car speedo.

                else if (showKMsBool == false)
                { speed = location.Speed * 2.23694; }


                if (location.Speed >= 0.5)
                {
                    speedList.Add(speed);

                    totalSpeed = speedList.Sum(x => Convert.ToInt32(x));

                    averageSpeed = totalSpeed / speedList.Count();
                    avgSpeedString = averageSpeed.ToString();
                    SetSpeedText();
                }

                else
                {
                    speed = 0;

                    if (stopTimeThen == TimeSpan.Zero)
                    { stopTimeThen = DateTime.Now.TimeOfDay; }


                    totalSpeed = speedList.Sum(x => Convert.ToInt32(x));

                    try
                    {
                        averageSpeed = totalSpeed / speedList.Count();
                        avgSpeedString = averageSpeed.ToString();
                    }

                    catch { }//Probably Devide by zero error
                    
                    SetSpeedText();
                }

                speedNowString = speed.ToString("N1");


                if (speedList.Count() > 1000)
                {
                    speedList.Clear();
                    speedList.Add(averageSpeed);
                }

            } //if(tripstarted)

        }//GetSpeed()


        public void GetDistance()
        {
            if (oldlat != 0 && tripstarted && stopped == false)
            {
                Location newLocation = new Location("");
                newLocation.Latitude = lat;
                newLocation.Longitude = lng;

                Location oldLocation = new Location("");
                oldLocation.Latitude = oldlat;
                oldLocation.Longitude = oldlng;

                distance = oldLocation.DistanceTo(newLocation); // distance in meters

                if (showKMsBool == true)
                {
                    distance = distance / 1000;
                } //divide distance by 1000 to get Km's

                else if (showKMsBool == false)
                {
                    distance = distance / 1609.34;
                }//divide distance by 1609.34 to get miles


                //float distancefloat = Convert.ToSingle(distance);
                //distanceList.Add(distancefloat);

                totalDistance += distance;
                distanceString = totalDistance.ToString("N1"); //N1 is the number of decimal places
                SetDistanceText();
            }

        }//GetDistance()


        public Task GetLocation()
        {
            return Task.Factory.StartNew(() => LocationLoop());
        }//GetLocation()


        private void SetDistanceType()
        {
            bool changeKMsBool = (showKMsBool != pref.GetBoolean("showKMs", true));

            if (changeKMsBool && showKMsBool == true) //Set as Miles Per Hour
            {
                averageSpeed = averageSpeed * 0.621371;
                avgSpeedString = averageSpeed.ToString();
                totalDistance = totalDistance * 0.621371;
                showKMsBool = false;
            }


            else if (changeKMsBool == true && showKMsBool == false)//Set as Kilometers Per Hour
            {
                averageSpeed = averageSpeed * 1.60934;
                avgSpeedString = averageSpeed.ToString();
                totalDistance = totalDistance * 1.60934;
                showKMsBool = true;
            }

            SetSpeedText();
            SetDistanceText();

        }//SetDistanceType


        private void SetTripName()
        {

            procedeSave = pref.GetBoolean("procedeSave", false);

            if (tripOnScreen && tripStopped)
            {
                ShowSetTripNameWindow();
            }

            else if (tripOnScreen && tripStopped == false)
            { Toast.MakeText(this, String.Format("You Need to Stop the Trip Before You Can Save It!"), ToastLength.Long).Show(); }

            else { Toast.MakeText(this, String.Format("You Need to Start the Trip Before You Can Save It!"), ToastLength.Long).Show(); }

        }//SetTripName()


        private void SetSpeedText()
        {

            if (showKMsBool == true)
            {
                KMsOrMiles = "km";
            }

            else if (showKMsBool == false)
            {
                KMsOrMiles = "mi";
            }


            var speedText = FindViewById<TextView>(Resource.Id.speedText);
            var speedNowText = FindViewById<TextView>(Resource.Id.speedNowText);
            speedText.Text = "AVG Speed: " + avgSpeedString + " " + KMsOrMiles + "/h";
            speedNowText.Text = "Current Speed: " + speedNowString + " " + KMsOrMiles + "/h";
        }//SetSpeedText()


        private void SetDistanceText()
        {

            if (showKMsBool == true)
            {
                KMsOrMiles = "km's";
            }

            else if (showKMsBool == false)
            {
                KMsOrMiles = "miles";
            }


            var distanceText = FindViewById<TextView>(Resource.Id.distanceText);
            distanceText.Text = "Distance: " + distanceString + " " + KMsOrMiles;
        }//SetDistanceText()


        private void SetStopText()
        {
            if (enableStops) { stopCountString = stopCounter.ToString(); }
            else if (enableStops == false) { stopCountString = "N/A"; }

            var stopCountText = FindViewById<TextView>(Resource.Id.stopCountText);
            stopCountText.Text = "Stop Count: " + stopCountString;
        }//SetStopText()


        private void SetUpMap()
        {
            if (nMap == null)
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.map).GetMapAsync(this);
        }//SetUpMap()


        public void ShowSetTripNameWindow()
        {
            Intent setTripNameActivity = new Intent(this, typeof(SetTripNameActivity));
            StartActivity(setTripNameActivity);

        }//ShowSetTripNameWindow()


        public void MapChange()
        {
            LatLng latlng = new LatLng(lat, lng);
            MarkerOptions options = new MarkerOptions()
                .SetPosition(latlng);

            CameraUpdate camera = CameraUpdateFactory.NewLatLng(latlng);
            nMap.MoveCamera(camera);
        }//MapChange()


        public void OnLocationChanged(Location location)
        {
            //Toast.MakeText(this, string.Format("TripStarted = {0}, TripStopped = {1}", tripstarted.ToString(), tripStopped.ToString()), ToastLength.Short).Show();

            if (tripstarted) { CheckForStopped(); }
            
            oldlat = lat;
            oldlng = lng;

            lat = location.Latitude;
            lng = location.Longitude;
            
            currentLocation = new Location("");
            currentLocation.Latitude = lat;
            currentLocation.Longitude = lng;
            
            if (tripstarted)
            {
                PolylineOptions line = new PolylineOptions()
                    .InvokeColor(0x66ce0e14)//Old color 14d124
                    .InvokeWidth(13)
                    .Add(new LatLng(oldlat, oldlng))
                    .Add(new LatLng(lat, lng));

                Polyline polyLine = nMap.AddPolyline(line);

                MapChange();
            }
            
            if(tripstarted && currentLocation.DistanceTo(markerLocation) > 100) { SetInvisibleMarker(); }

            GetDistance();
            GetSpeed();
        }//OnLocationChanged()


        public void OnMapReady(GoogleMap googleMap)
        {
            /*
            if (tripstarted == false)
            {
                nMap = googleMap;
                nMap.MapType = GoogleMap.MapTypeNormal;
                nMap.MyLocationEnabled = true;
            }*/

            if (nMap == null)
            {
                nMap = googleMap;
                nMap.MapType = GoogleMap.MapTypeNormal;
                nMap.MyLocationEnabled = true;
            }


            LatLng latlng = new LatLng(lat, lng);
            MarkerOptions options = new MarkerOptions()
                .SetPosition(latlng);


            CameraUpdate camera = CameraUpdateFactory.NewLatLng(latlng);
            nMap.MoveCamera(camera);
            
            
        }//OnMapReady()


        protected override void OnResume()
        {
            base.OnResume();

            locationManager.RequestLocationUpdates(provider, 1000, 1, this);
            LoadAdTask();

            if (location == null)
            { location = locationManager.GetLastKnownLocation(provider); }

            if (location != null)
            {
                lat = location.Latitude;
                lng = location.Longitude;
            }
            

            procedeSave = pref.GetBoolean("procedeSave", false);
            prefEdit.Apply();

            if (procedeSave)
            {
                SaveTrip();
            }

            OrientationChange(); 
            

            SetDistanceType();
            SetStopText();
        }//OnResume()
        

        public override void OnBackPressed()
        {

            if (tripOnScreen)
            {
                if(tripSaved == false) { SaveTripPopUp(); }
                tripSaved = true;

                if (doubleBackToExitPressedOnce)
                {
                    Finish();
                    base.OnBackPressed();
                    //Java.Lang.JavaSystem.Exit(0);
                    Intent splashActivity = new Intent(this, typeof(SplashActivity));
                    StartActivity(splashActivity);
                    return;
                }


                this.doubleBackToExitPressedOnce = true;
                Toast.MakeText(this, "Press Back Twice to Exit", ToastLength.Short).Show();

                new Handler().PostDelayed(() =>
                {
                    doubleBackToExitPressedOnce = false;
                }, 2000);
            }

            else
            {
                Finish();
                Intent splashActivity = new Intent(this, typeof(SplashActivity));
                StartActivity(splashActivity);
            }
        }//OnBackPressed()


        public void OnProviderDisabled(string provider)
        {
            locationManager.RequestLocationUpdates(provider, 1000, 1, this);
        }//OnProviderDisabled()


        public void OnProviderEnabled(string provider)
        {
            locationManager.RequestLocationUpdates(provider, 1000, 1, this);
        }//OnProviderEnabled()


        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            locationManager.RequestLocationUpdates(provider, 1000, 1, this);
        }//OnStatusChanged()

    
        public void OnSnapshotReady(Bitmap snapshot)
        {
            if (CheckSelfPermission(WriteExternalStorage) == (int)Android.Content.PM.Permission.Granted)
            {
                Bitmap mapSnap = snapshot;
                var FolderPath = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).ToString(),"TripImages");
                string fileName = String.Format("{0}.Jpeg", tripName);
                var filePath = System.IO.Path.Combine(FolderPath.ToString(), fileName);
                
                using (FileStream mapstream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    mapSnap.Compress(Bitmap.CompressFormat.Jpeg, 90, mapstream);
                    mapstream.Close();
                }
                
            }

            else
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.WriteExternalStorage }, 1);
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage }, 1);
            }
        }//OnSnapshotReady()

        
        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutDouble("lat", lat);
            outState.PutDouble("lng", lng);
            outState.PutDouble("oldlat", oldlat);
            outState.PutDouble("oldlng", oldlng);
            outState.PutDouble("startlat", startlat);
            outState.PutDouble("startlng", startlng);
            outState.PutDouble("stoplat", stoplat);
            outState.PutDouble("stoplng", stoplng);
            outState.PutDouble("stopNowLat", stopNowLat);
            outState.PutDouble("stopNowLng", stopNowLng);
            outState.PutDouble("stopThenLat", stopThenLat);
            outState.PutDouble("stopThenLng", stopThenLng);
            outState.PutDouble("speed", speed);
            outState.PutDouble("totalDistance", totalDistance);
            outState.PutDouble("averageSpeed", averageSpeed);
            outState.PutDouble("distance", distance);
            outState.PutDouble("markerDistance", markerDistance);
            outState.PutBoolean("tripstarted", tripstarted);
            outState.PutBoolean("tripStopped", tripStopped);
            outState.PutBoolean("showKMsBool", showKMsBool);
            outState.PutBoolean("tripOnScreen", tripOnScreen);
            outState.PutBoolean("doubleBackToExitPressedOnce", doubleBackToExitPressedOnce);
            outState.PutBoolean("procedeSave", procedeSave);
            outState.PutBoolean("stopped", stopped);
            outState.PutBoolean("enableStops", enableStops);
            outState.PutBoolean("tripSaved", tripSaved);
            outState.PutString("provider", provider);
            outState.PutString("avgSpeedString", avgSpeedString);
            outState.PutString("distanceString", distanceString);
            outState.PutString("speedNowString", speedNowString);
            outState.PutString("stopCountString", stopCountString);
            outState.PutString("KMsOrMiles", KMsOrMiles);
            outState.PutString("lastSavedTripName", lastSavedTripName);
            outState.PutString("imagesFolderPath", imagesFolderPath);
            outState.PutString("tripName", tripName);
            outState.PutString("startDate", startDate);
            outState.PutString("finishDate", finishDate);
            outState.PutString("startTime", startTime);
            outState.PutString("finishTime", finishTime);
            outState.PutInt("stopCounter", stopCounter);
            outState.PutInt("totalStopDuration", totalStopDuration);
            outState.PutInt("totalSpeed", totalSpeed);
            outState.PutString("tripDurationStart", tripDurationStart.ToString());
            outState.PutString("tripDurationFinish", tripDurationFinish.ToString());
            outState.PutString("tripDuration", tripDuration.ToString());
            outState.PutString("stopTimeThen", stopTimeThen.ToString());
            outState.PutString("stopTimeNow", stopTimeNow.ToString());
            outState.PutString("stopDuration", stopDuration.ToString());

            double[] speedArray = speedList.ToArray();

            outState.PutDoubleArray("speedArray", speedArray);

            // always call the base implementation!
            base.OnSaveInstanceState(outState);
        }//OnSaveInstanceState()


        public void CreateImagesFolder()
        {

            try
            {
                imagesFolderPath = System.IO.Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).ToString(), "TripImages");
                Java.IO.File dir = new Java.IO.File(imagesFolderPath);
                if (dir.Exists() == false)
                {
                    Directory.CreateDirectory(imagesFolderPath);
                    prefEdit.PutString("imagesFolderPath", imagesFolderPath.ToString());
                    prefEdit.PutBoolean("imagesFodlerExists", true);
                    prefEdit.Apply();
                }
            }

            catch {  }
        }//CreateImagesFolder


        public void LocationLoop()
        {
            while(location == null)
            { location = locationManager.GetLastKnownLocation(provider); }

            lat = location.Latitude;
            lng = location.Longitude;

            LatLng latlng = new LatLng(lat, lng);

            camera = CameraUpdateFactory.NewLatLngZoom(latlng, 12);
            nMap.MoveCamera(camera);

            Toast.MakeText(this, "Current Location Is Now Available", ToastLength.Long).Show();
        }//LocationLoop()


        private void LoadAd()
        {
            FinalAd = FullSizeAdWrapper.ConstructFullPageAdd(this, "ca-app-pub-6665335742989505/1947550274");
            intlistener = new TripTracker.adlistener();
        }//LoadAd()


        private void DisplayAd()
        {
            intlistener.AdLoaded += () => { if (FinalAd.IsLoaded) FinalAd.Show(); };
            FinalAd.AdListener = intlistener;
            FinalAd.CustomBuild();
        }//DisplayAd()


        public Task SaveTripPopUpTask()
        {
            return Task.Factory.StartNew(() => SaveTripPopUp());
        }//SaveTripPopUpTask()


        public void SaveTripPopUp()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("Save Trip Before Exiting?");
            alert.SetPositiveButton("Yes", (senderAlert, args) => {
                SetTripName() ;
            });

            alert.SetNegativeButton("No", (senderAlert, args) => {
                alert.Dispose();
                Intent splashActivity = new Intent(this, typeof(SplashActivity));
                StartActivity(splashActivity);
                Finish();
            });

            Dialog dialog = alert.Create();
            dialog.Show();
        }//SaveTripPopUp()


        public Task LoadAdTask()
        {
            return Task.Factory.StartNew(() => LoadAd());
        }//LoadAdTask()


        private void SetInvisibleMarker()
        {
            markerLocation = new Location("");
            markerLocation.Latitude = lat;
            markerLocation.Longitude = lng;

            markerLatLng = new LatLng(lat, lng);
            MarkerOptions options = new MarkerOptions()
                .SetPosition(markerLatLng)
                .SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueBlue))
                .Visible(false);

            nMap.AddMarker(options);

            boundsBuilder.Include(markerLatLng);
        }//SetInvisibleMarker()


        private void AssignVariables(Bundle bundle)
        {
            lat = bundle.GetDouble("lat");
            lng = bundle.GetDouble("lng");
            oldlat = bundle.GetDouble("oldlat");
            oldlng = bundle.GetDouble("oldlng");
            startlat = bundle.GetDouble("startlat");
            startlng = bundle.GetDouble("startlng");
            stoplat = bundle.GetDouble("stoplat");
            stoplng = bundle.GetDouble("stoplng");
            stopNowLat = bundle.GetDouble("stopNowLat");
            stopNowLng = bundle.GetDouble("stopNowLng");
            stopThenLat = bundle.GetDouble("stopThenLat");
            stopThenLng = bundle.GetDouble("stopThenLng");
            speed = bundle.GetDouble("speed");
            totalDistance = bundle.GetDouble("totalDistance");
            averageSpeed = bundle.GetDouble("averageSpeed");
            distance = bundle.GetDouble("distance");
            markerDistance = bundle.GetDouble("markerDistance");
            tripstarted = bundle.GetBoolean("tripstarted");
            tripStopped = bundle.GetBoolean("tripStopped");
            showKMsBool = bundle.GetBoolean("showKMsBool");
            tripOnScreen = bundle.GetBoolean("tripOnScreen");
            doubleBackToExitPressedOnce = bundle.GetBoolean("doubleBackToExitPressedOnce");
            procedeSave = bundle.GetBoolean("procedeSave");
            stopped = bundle.GetBoolean("stopped");
            enableStops = bundle.GetBoolean("enableStops");
            tripSaved = bundle.GetBoolean("tripSaved");
            provider = bundle.GetString("provider");
            avgSpeedString = bundle.GetString("avgSpeedString");
            distanceString = bundle.GetString("distanceString");
            speedNowString = bundle.GetString("speedNowString");
            stopCountString = bundle.GetString("stopCountString");
            KMsOrMiles = bundle.GetString("KMsOrMiles");
            lastSavedTripName = bundle.GetString("lastSavedTripName");
            imagesFolderPath = bundle.GetString("imagesFolderPath");
            tripName = bundle.GetString("tripName");
            startDate = bundle.GetString("startDate");
            finishDate = bundle.GetString("finishDate");
            startTime = bundle.GetString("startTime");
            finishTime = bundle.GetString("finishTime");
            stopCounter = bundle.GetInt("stopCounter");
            totalStopDuration = bundle.GetInt("totalStopDuration");
            totalSpeed = bundle.GetInt("totalSpeed");

            stopLatLng = new LatLng(stoplat, stoplng);
            startLatLng = new LatLng(startlat, startlng);
            finishLatLng = new LatLng(stoplat, stoplng);
            stopThen = new Location("");
            stopThen.Latitude = stopThenLat;
            stopThen.Longitude = stopThenLng;
            stopNow = new Location("");
            stopNow.Latitude = stopNowLat;
            stopNow.Longitude = stopNowLng;
            markerLocation = new Location("");
            markerLocation.Latitude = lat;
            markerLocation.Longitude = lng;
            tripDurationStart = TimeSpan.Parse(bundle.GetString("tripDurationStart"));
            tripDurationFinish = TimeSpan.Parse(bundle.GetString("tripDurationFinish"));
            tripDuration = TimeSpan.Parse(bundle.GetString("tripDuration"));
            stopTimeThen = TimeSpan.Parse(bundle.GetString("stopTimeThen"));
            stopTimeNow = TimeSpan.Parse(bundle.GetString("stopTimeNow"));
            stopDuration = TimeSpan.Parse(bundle.GetString("stopDuration"));

            double[] speedArray = bundle.GetDoubleArray("speedArray"); 

            foreach(double d in speedArray)
            {
                speedList.Add(d);
            }

        }//AssignVariables()


        private void OrientationChange()
        {

            portraitView = pref.GetBoolean("portraitView", true);
            changeOrientation = pref.GetBoolean("changeOrientation", false);

            if (changeOrientation && !tripstarted && !tripOnScreen && portraitView)
            {
                Finish();
                Intent mainActivity = new Intent(this, typeof(MainActivity));
                StartActivity(mainActivity);
                prefEdit.PutBoolean("changeOrientation", false);
                prefEdit.Apply();
            }

            else if (changeOrientation && !tripstarted && !tripOnScreen && !portraitView)
            {
                Finish();
                Intent mainActivityLandscape = new Intent(this, typeof(MainActivityLandscape));
                StartActivity(mainActivityLandscape);
                prefEdit.PutBoolean("changeOrientation", false);
                prefEdit.Apply();
            }

        }//OrientationChange()


    }
}

