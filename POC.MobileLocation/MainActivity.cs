using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Xamarin.Essentials;
using System;
using System.Threading.Tasks;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Javax.Crypto;
using POC.MobileLocation.Services;
using Android.Views;
using System.Threading;
using Android.Media;
using Android.Content;

namespace POC.MobileLocation
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private bool isStarted = false;

        Intent startServiceIntent;
        Intent stopServiceIntent;

        Button stopServiceButton;
        Button startServiceButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            CheckForPermissions(this);

            OnNewIntent(this.Intent);

            if (savedInstanceState != null)
            {
                isStarted = savedInstanceState.GetBoolean(Constants.SERVICE_STARTED_KEY, false);
            }

            startServiceIntent = new Intent(this, typeof(Worker));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);

            stopServiceIntent = new Intent(this, typeof(Worker));
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);


            stopServiceButton = FindViewById<Button>(Resource.Id.stopButton);
            startServiceButton = FindViewById<Button>(Resource.Id.startButton);

            if (isStarted)
            {
                stopServiceButton.Click += StopServiceButton_Click;
                stopServiceButton.Enabled = true;
                startServiceButton.Enabled = false;
            }
            else
            {
                startServiceButton.Click += StartServiceButton_Click;
                startServiceButton.Enabled = true;
                stopServiceButton.Enabled = false;
            }

            StartService(startServiceIntent);
            isStarted = true;
        }

        void StartServiceButton_Click(object sender, EventArgs e)
        {
            startServiceButton.Enabled = false;
            startServiceButton.Click -= StartServiceButton_Click;

            StartService(startServiceIntent);

            isStarted = true;
            stopServiceButton.Click += StopServiceButton_Click;

            stopServiceButton.Enabled = true;
        }


        void StopServiceButton_Click(object sender, EventArgs e)
        {
            stopServiceButton.Click -= StopServiceButton_Click;
            stopServiceButton.Enabled = false;

            StopService(stopServiceIntent);
            isStarted = false;

            startServiceButton.Click += StartServiceButton_Click;
            startServiceButton.Enabled = true;
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent == null)
                return;

            var bundle = Intent.Extras;
            if (bundle?.ContainsKey(Constants.SERVICE_STARTED_KEY) == true)
                isStarted = true;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
			outState.PutBoolean(Constants.SERVICE_STARTED_KEY, isStarted);
            base.OnSaveInstanceState(outState);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void CheckForPermissions(AppCompatActivity app)
        {
            if (ContextCompat.CheckSelfPermission(app, Manifest.Permission.AccessFineLocation) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(app, new string[] { Manifest.Permission.AccessFineLocation }, 1);
            }
            if (ContextCompat.CheckSelfPermission(app, Manifest.Permission.AccessCoarseLocation) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(app, new string[] { Manifest.Permission.AccessCoarseLocation }, 2);
            }
            if (ContextCompat.CheckSelfPermission(app, Manifest.Permission.Internet) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(app, new string[] { Manifest.Permission.ForegroundService }, 3);
            }
            if (ContextCompat.CheckSelfPermission(app, Manifest.Permission.Internet) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(app, new string[] { Manifest.Permission.ForegroundService }, 4);
            }
        }

    }
}