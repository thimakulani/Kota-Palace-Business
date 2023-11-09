using Android.OS;
using Android.App;
using Android.Runtime;
using Android.Views;
using AndroidHUD;
using AndroidX.AppCompat.App;
using Gauravk.BubbleNavigation;
using Gauravk.BubbleNavigation.Listeners;
using KotaPalace.Fragments;
using KotaPalace.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;
using ID.IonBit.IonAlertLib;
using KotaPalace.Dialogs;
using System.Text;
using NUnit.Framework;
using static Android.Icu.Text.Transliterator;
using Google.Android.Material.AppBar;
using Microsoft.AspNetCore.SignalR.Client;
using IsmaelDiVita.ChipNavigationLib;
using Google.Android.Material.Button;

namespace KotaPalace
{
    [Activity(Label = null, Theme = "@style/AppTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity, ChipNavigationBar.IOnItemSelectedListener
    {
        //private BubbleNavigationLinearView navigationView;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            var btn_logout = FindViewById<MaterialButton>(Resource.Id.btn_logout);
            btn_logout.Click += Btn_logout_Click;
            if (savedInstanceState == null)
            {
                LoadDefaultFragment();
            }
            var hubConnection = new HubConnectionBuilder().WithUrl("https://kotapalaceadmin.azurewebsites.net/OrderHub").Build();
            // var hubConnection = new HubConnectionBuilder().Build().with.("https://kotapalaceadmin.azurewebsites.net/OrderHub").Build();
            hubConnection.On<Object>("Order", (message) =>
            {
                var encodedMsg = $"{message}";

            });

            await hubConnection.StartAsync();
            if (hubConnection.State == HubConnectionState.Connected)
            {
                Console.Write("");
            }
            InitializeComponents();
            _ = GetUserId();
        }

        private void Btn_logout_Click(object sender, EventArgs e)
        {
            Preferences.Clear();
            this.Finish();

        }

        //MaterialToolbar toolbar_main;
        private void InitializeComponents()
        {
            //navigationView = FindViewById<BubbleNavigationLinearView>(Resource.Id.bottom_navigation_view_linear);
            //navigationView.SetNavigationChangeListener(this);


            var nav_menu = FindViewById<ChipNavigationBar>(Resource.Id.MenuNav);
           // toolbar_main = FindViewById<MaterialToolbar>(Resource.Id.toolbar_main);
            nav_menu.SetMenuResource(Resource.Menu.nav_menu);
            nav_menu.SetItemSelected(Resource.Id.nav_home);
            nav_menu.SetOnItemSelectedListener(this);
        }

        private void LoadDefaultFragment()
        {
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.fragHost, new OrdersFragment()).Commit();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }



        private async Task GetUserId()
        {
            string userId = Preferences.Get("Id", null);

            if (userId == null)
            {
                return;
            }

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync($"{API.Url}/businesses/specific/{userId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var str = await response.Content.ReadAsStringAsync();
                        var business = Newtonsoft.Json.JsonConvert.DeserializeObject<Business>(str);

                        if (string.IsNullOrEmpty(business.Status) && business.OwnerId == userId)
                        {
                            ShowWarningAlert("Your Business has not been approved yet! \n An email will be sent once your business profile is approved.");
                            Finish();
                        }
                        else if (business.Status.ToUpper() == "ACTIVE")
                        {
                            Preferences.Set("businessId", business.Id);
                            if (business.Address == null && userId == business.OwnerId)
                            {
                                ShowBusinessAddressSetupAlert();
                            }

                        }
                       // toolbar_main.Title = business.Name;
                    }
                    else
                    {
                        // Handle unsuccessful response
                    }
                };
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions and display an error message
                ShowErrorMessage(ex.Message);
            }
        }

        private void ShowWarningAlert(string message)
        {
            var alert = new IonAlert(this, IonAlert.WarningType);
            alert.SetContentText(message);
            alert.DismissEvent += (e, v) => Finish();
            alert.Show();
        }

        private void ShowBusinessAddressSetupAlert()
        {
            var alert = new IonAlert(this, IonAlert.WarningType);
            alert.SetContentText("Setup your business address");
            alert.DismissEvent += async (e, v) => await ShowBusinessAddressDialog();
            alert.SetCancelable(false);
            alert.Show();
        }

        private Task ShowBusinessAddressDialog()
        {
            BusinessAddressDialogFragment fragment = new BusinessAddressDialogFragment();
            fragment.Show(SupportFragmentManager.BeginTransaction(), "");

            fragment.CoordinatesHandler += async (ss, ee) =>
            {
                try
                {
                    using (HttpClient httpClient1 = new HttpClient())
                    {
                        Address address = new Address()
                        {
                            BusinessId = Preferences.Get("businessId", 0),
                            Latitude = ee.Coordinates.Latitude,
                            Longitude = ee.Coordinates.Longitude
                        };

                        var json_string = Newtonsoft.Json.JsonConvert.SerializeObject(address);
                        var str_content = new StringContent(json_string, Encoding.UTF8, "application/json");

                        var resp = await httpClient1.PostAsync($"{API.Url}/businesses/address", str_content);

                        if (resp.IsSuccessStatusCode)
                        {
                            fragment.Dismiss();
                        }
                        else
                        {
                            var response = await resp.Content.ReadAsStringAsync();
                            ShowErrorMessage(response);
                        }
                    };
                }
                catch (HttpRequestException ex)
                {
                    ShowErrorMessage(ex.Message);
                }
            };
            return Task.CompletedTask;
        }

        private void ShowErrorMessage(string errorMessage)
        {
            // Display the error message to the user, e.g., using a dialog or toast
            AndHUD.Shared.ShowError(this, errorMessage, MaskType.None, TimeSpan.FromSeconds(3));
        }

        public void OnItemSelected(int id)
        {
            AndroidX.Fragment.App.Fragment fragment = id switch
            {
                Resource.Id.nav_home => new OrdersFragment(),
                Resource.Id.shop_menu => new MenuFragment(),
                Resource.Id.nav_profile => new ProfileFragment(),
                Resource.Id.nav_business => new BusinessProfileFragment(),
                Resource.Id.nav_report => new ReportFragment(),
                _ => throw new NotImplementedException(),
                //_ => new OrdersFragment(),
            };
            if (fragment != null)
            {
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.fragHost, fragment).Commit();
            }
        }


        /*
                private void ShowErrorMessage(string message)
                {
                    AndHUD.Shared.ShowError(this, message, MaskType.None, TimeSpan.FromSeconds(3));
                }*/
    }
}
