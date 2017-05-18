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

using SQLite;


namespace TripTracker
{
    public class Trip
    {
        [PrimaryKey, AutoIncrement, Column("_ID")]
        public int ID { get; set; }

        [Column("_TripName")]
        public string TripName { get; set; }

        [Column("_AVGSpeed")]
        public double AVGSpeed { get; set; }

        [Column("_StopsCount")]
        public string StopsCount { get; set; }

        [Column("_Distance")]
        public double Distance { get; set; }

        [Column("_StopsDuration")]
        public string StopsDuration { get; set; }

        [Column("_TripDuration")]
        public string TripDuration { get; set; }

        [Column("_TripStart")]
        public String TripStart { get; set; }

        [Column("_TripFinish")]
        public String TripFinish { get; set; }

        [Column("_MilesOrKMs")]
        public String MilesOrKms { get; set; }
        
    }
}