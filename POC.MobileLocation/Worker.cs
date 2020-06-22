using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using POC.MobileLocation.Services;

namespace POC.MobileLocation
{
    [Service]
    public class Worker : Service
    {
        static readonly string TAG = typeof(Worker).FullName;

        private PositionService positionService;
        private StreamLabelsApiService streamLabelsApiService;

        private Handler handler;
        private Action runnable;
        private bool isStarted = false;

        public override IBinder OnBind(Intent intent) => null;

        public override void OnCreate()
        {
            base.OnCreate();

            handler = new Handler();
            positionService = new PositionService();
            streamLabelsApiService = new StreamLabelsApiService("https://tranquiliza.dynu.net/streamlabelapi");

            runnable = new Action(async () =>
            {
                var position = await positionService.RequestPosition().ConfigureAwait(false);
                await streamLabelsApiService.PostUpdateToApi(position).ConfigureAwait(false);

                var msg = $"lat: {position.Latitude}, lon: {position.Longitude}";
                Console.WriteLine(msg);

                //Intent i = new Intent(Constants.NOTIFICATION_BROADCAST_ACTION);
                //i.PutExtra(Constants.BROADCAST_MESSAGE_KEY, msg);
                //Android.Support.V4.Content.LocalBroadcastManager.GetInstance(this).SendBroadcast(i);
                handler.PostDelayed(runnable, Constants.DELAY_BETWEEN_LOG_MESSAGES);
            });
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
            {
                if (isStarted)
                {
                    Log.Info(TAG, "OnStartCommand: The service is already running.");
                }
                else
                {
                    Log.Info(TAG, "OnStartCommand: The service is starting.");
                    RegisterForegroundService();
                    handler.PostDelayed(runnable, Constants.DELAY_BETWEEN_LOG_MESSAGES);
                    isStarted = true;
                }
            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                Log.Info(TAG, "OnStartCommand: The service is stopping.");
                StopForeground(true);
                StopSelf();
                isStarted = false;
            }
            else if (intent.Action.Equals(Constants.ACTION_RESTART_TIMER))
            {
                Log.Info(TAG, "OnStartCommand: Restarting the timer.");
            }

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            // We need to shut things down.
            Log.Info(TAG, "OnDestroy: The started service is shutting down.");

            // Stop the handler.
            handler.RemoveCallbacks(runnable);

            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

            isStarted = false;
            streamLabelsApiService.Dispose();
            base.OnDestroy();
        }

        private void RegisterForegroundService()
        {
            var channelId = CreateNotificationChannel("BackgroundWorker", "My position Updater");

            var notificationBuilder = new NotificationCompat.Builder(this, channelId);
            var notification = notificationBuilder
                .SetOngoing(true)
                .SetSmallIcon(Resource.Drawable.ic_stat_name)
                .SetCategory(Notification.CategoryService)
                .SetContentIntent(BuildIntentToShowMainActivity())
                .Build();

            //var notification = new Notification.Builder(this)
            //    .SetContentTitle(Resources.GetString(Resource.String.app_name))
            //    .SetContentText(Resources.GetString(Resource.String.notification_text))
            //    .SetSmallIcon(Resource.Drawable.ic_stat_name)
            //    .SetContentIntent(BuildIntentToShowMainActivity())
            //    .SetOngoing(true)
            //    .AddAction(BuildRestartTimerAction())
            //    .AddAction(BuildStopServiceAction())
            //    .Build();

            // Enlist this instance of the service as a foreground service
            StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
        }

        private string CreateNotificationChannel(string channelId, string channelName)
        {
            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.None);
            channel.LightColor = Color.Blue;
            channel.LockscreenVisibility = NotificationVisibility.Private;
            var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager.CreateNotificationChannel(channel);

            return channelId;
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        private PendingIntent BuildIntentToShowMainActivity()
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.SetAction(Constants.ACTION_MAIN_ACTIVITY);
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            notificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);

            return PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
        }

        /// <summary>
        /// Builds a Notification.Action that will instruct the service to restart the timer.
        /// </summary>
        /// <returns>The restart timer action.</returns>
        private Notification.Action BuildRestartTimerAction()
        {
            var restartTimerIntent = new Intent(this, GetType());
            restartTimerIntent.SetAction(Constants.ACTION_RESTART_TIMER);
            var restartTimerPendingIntent = PendingIntent.GetService(this, 0, restartTimerIntent, 0);

            var builder = new Notification.Action.Builder(Resource.Drawable.ic_action_restart_timer,
                                              GetText(Resource.String.restart_timer),
                                              restartTimerPendingIntent);

            return builder.Build();
        }

        /// <summary>
        /// Builds the Notification.Action that will allow the user to stop the service via the
        /// notification in the status bar
        /// </summary>
        /// <returns>The stop service action.</returns>
        private Notification.Action BuildStopServiceAction()
        {
            var stopServiceIntent = new Intent(this, GetType());
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
            var stopServicePendingIntent = PendingIntent.GetService(this, 0, stopServiceIntent, 0);

            var builder = new Notification.Action.Builder(Android.Resource.Drawable.IcMediaPause,
                                                          GetText(Resource.String.stop_service),
                                                          stopServicePendingIntent);
            return builder.Build();
        }
    }
}