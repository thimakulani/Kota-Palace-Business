using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using KotaPalace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace KotaPalace.Activities
{
    [Activity(Label = "SplashScreen", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Create your application here


            MainThread.BeginInvokeOnMainThread(async () =>
            {
                string e = Preferences.Get("e", null);
                string p = Preferences.Get("p", null);
                try
                {
                    if (string.IsNullOrWhiteSpace(e) || string.IsNullOrWhiteSpace(p))
                    {
                        Intent intent = new Intent(Application.Context, typeof(SignIn));
                        StartActivity(intent);
                        OverridePendingTransition(Resource.Animation.Side_in_right, Resource.Animation.Side_out_left);
                        return;
                    }
                    UserLogin userLogin = new UserLogin()
                    {
                        Email = e,
                        Password = p,
                    };
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(userLogin);

                    HttpClient httpClient = new HttpClient();
                    HttpContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync($"{API.Url}/account/business/login", httpContent);
                    if (response.IsSuccessStatusCode)
                    {
                        Intent intent = new Intent(Application.Context, typeof(MainActivity));
                        StartActivity(intent);
                        OverridePendingTransition(Resource.Animation.Side_in_right, Resource.Animation.Side_out_left);
                    }
                    else
                    {
                        Intent intent = new Intent(Application.Context, typeof(SignIn));
                        StartActivity(intent);
                        OverridePendingTransition(Resource.Animation.Side_in_right, Resource.Animation.Side_out_left);
                    }
                }
                catch (Exception)
                {
                    Intent intent = new Intent(Application.Context, typeof(SignIn));
                    StartActivity(intent);
                    OverridePendingTransition(Resource.Animation.Side_in_right, Resource.Animation.Side_out_left);
                }

            });


            /*Task startWork = new Task(() =>
            {
                Task.Delay(3000);
            });
            startWork.ContinueWith(t =>
            {
                try
                {
                    Intent intent = new Intent(Application.Context, typeof(SignIn));
                    StartActivity(intent);
                    OverridePendingTransition(Resource.Animation.Side_in_right, Resource.Animation.Side_out_left);
                }
                catch (Exception ex)
                {
                    
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            startWork.Start();*/

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}