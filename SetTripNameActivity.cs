using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;
using System.IO;

namespace TripTracker
{
    [Activity(Label = "SetTripNameActivity")]
    public class SetTripNameActivity : Activity
    {

        ISharedPreferences pref;
        ISharedPreferencesEditor prefEdit;


        string lastSavedTripName;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SetTripName);
            

            Button okButton = FindViewById<Button>(Resource.Id.SetTripName_okButton);
            var entry = FindViewById<EditText>(Resource.Id.SetTripName_editText1);

            okButton.Click += (s, e) =>
            {
                var regexAlphanumeric = new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9 ]*$");
                var regexNumeric = new System.Text.RegularExpressions.Regex("^[0-9 ]*$");

                if (entry.Text.Length > 0)
                {
                    if (regexAlphanumeric.IsMatch(entry.Text))
                    {
                        if (regexNumeric.IsMatch(entry.Text[0].ToString()) == false)
                        {
                            try
                            {
                                lastSavedTripName = entry.Text;


                                var dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Trips.db");

                                var connection = new SQLiteConnection(dbPath);

                                var table = connection.Table<Trip>();

                                int count = 0;

                                foreach (var item in table)
                                {
                                    if (item.TripName == lastSavedTripName)
                                    { count++; }
                                }


                                if (count < 1)
                                {
                                    pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
                                    prefEdit = pref.Edit();

                                    prefEdit.PutString("lastSavedTripName", lastSavedTripName);
                                    prefEdit.PutBoolean("procedeSave", true);
                                    prefEdit.Apply();

                                    base.OnBackPressed();
                                }

                                else { Toast.MakeText(this, String.Format("Trip Name Already Exists"), ToastLength.Long).Show(); }
                            }

                            catch { Toast.MakeText(this, String.Format("Name Format Error"), ToastLength.Long).Show(); }

                        }//(regexNumeric.IsMatch(entry.Text[0].ToString()) == false)

                        else { Toast.MakeText(this, String.Format("First Character Cannot Be a Number"), ToastLength.Long).Show(); }

                    }//(regexItem.IsMatch(entry.Text))

                    else { Toast.MakeText(this, String.Format("Please Use Only Alpha and Numeric Characters"), ToastLength.Long).Show(); }

                }//if(entry.Text.Length > 0)

                else { Toast.MakeText(this, String.Format("Trip Name Cannot be Empty"), ToastLength.Long).Show(); }
            };
        }
        

   }
}