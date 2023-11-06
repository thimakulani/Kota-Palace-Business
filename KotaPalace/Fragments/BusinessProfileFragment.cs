
using Android.Content;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using FFImageLoading;
using Google.Android.Material.Button;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.MaterialSwitch;
using Google.Android.Material.TextField;
using Google.Android.Material.TextView;
using KotaPalace.Activities;
using KotaPalace.Dialogs;
using KotaPalace.Models;
using Plugin.FirebaseStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Address = Android.Locations.Address;

namespace KotaPalace.Fragments
{
    public class BusinessProfileFragment : Fragment
    {
        //private textin
        private AppCompatImageView ImgBusinessLogo;

        private AppCompatTextView OutputStatus;
        private AppCompatTextView OutputBusinessOwner;

        private TextInputEditText OutputBusinessName;
        private TextInputEditText OutputBusinessDesc;
        private TextInputEditText OutputBusinessAddress;
        private TextInputEditText OutputBusinessPhoneNumber;



        private MaterialButton BtnUpdateProfile;
        private Context context;
        readonly private string id = Preferences.Get("Id", null);

        public BusinessProfileFragment()
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View rootView = inflater.Inflate(Resource.Layout.fragment_business_profile, container, false);
            context = rootView.Context;
            Init(rootView);
            GetBusinessAsync();

            return rootView;
        }

        private void Init(View view)
        {
            ImgBusinessLogo = view.FindViewById<AppCompatImageView>(Resource.Id.ImgBusinessLogo);
            var FabProfileImg = view.FindViewById<FloatingActionButton>(Resource.Id.FabProfileImg);
            OutputBusinessOwner = view.FindViewById<AppCompatTextView>(Resource.Id.OutputBusinessOwner);
            OutputStatus = view.FindViewById<AppCompatTextView>(Resource.Id.OutputBusinessStatus);
            OutputBusinessName = view.FindViewById<TextInputEditText>(Resource.Id.OutputBusinessName);
            OutputBusinessDesc = view.FindViewById<TextInputEditText>(Resource.Id.OutputBusinessDesc);
            OutputBusinessAddress = view.FindViewById<TextInputEditText>(Resource.Id.OutputBusinessAddress);
            OutputBusinessPhoneNumber = view.FindViewById<TextInputEditText>(Resource.Id.OutputBusinessPhoneNumber);
            var BtnMapLocation = view.FindViewById<MaterialButton>(Resource.Id.BtnMapLocation);
            BtnMapLocation.Click += BtnMapLocation_Click;
            FabProfileImg.Click += FabProfileImg_Click; 
            //btn_mf_open = view.FindViewById<MaterialButton>(Resource.Id.btn_mf_open);
            //btn_mf_close = view.FindViewById<MaterialButton>(Resource.Id.btn_mf_close);
            //btn_sat_open = view.FindViewById<MaterialButton>(Resource.Id.btn_sat_open);
            //btn_sat_close = view.FindViewById<MaterialButton>(Resource.Id.btn_sat_close);
            //btn_sun_open = view.FindViewById<MaterialButton>(Resource.Id.btn_sun_open);
            //btn_sun_close = view.FindViewById<MaterialButton>(Resource.Id.btn_sun_close);

            BtnUpdateProfile = view.FindViewById<MaterialButton>(Resource.Id.BtnUpdateProfile);

            BtnUpdateProfile.Click += (s, e) =>
            {
                UpdateUserDetails();
            };
        }

        private async void FabProfileImg_Click(object sender, EventArgs e)
        {
            file = await PickAndShow();
        }
        private async Task<FileResult> PickAndShow()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions()
                {
                    FileTypes = FilePickerFileType.Images
                });

                if (file != null)
                {
                    ImageService
                        .Instance
                        .LoadFile(file.FullPath)
                        .Into(ImgBusinessLogo);
                    return file;
                }
            }
            catch (Exception ex)
            {
                Message(ex.Message);
            }

            return null;
        }
        private void BtnMapLocation_Click(object sender, EventArgs e)
        {
            BusinessAddressDialogFragment dlg = new BusinessAddressDialogFragment();
            dlg.Show(ChildFragmentManager.BeginTransaction(), "");
            dlg.CoordinatesHandler += async (s, o) =>
            {
                try
                {
                    using (HttpClient httpClient1 = new HttpClient())
                    {
                        Models.Address address = new Models.Address()
                        {
                            BusinessId = Preferences.Get("businessId", 0),
                            Latitude = o.Coordinates.Latitude,
                            Longitude = o.Coordinates.Longitude
                        };
                        LatLng = new LatLng(address.Latitude, address.Longitude);
                        var json_string = Newtonsoft.Json.JsonConvert.SerializeObject(address);
                        var str_content = new StringContent(json_string, Encoding.UTF8, "application/json");

                        var resp = await httpClient1.PostAsync($"{API.Url}/businesses/address", str_content);

                        if (resp.IsSuccessStatusCode)
                        {
                            dlg.Dismiss();
                        }
                        else
                        {
                            var response = await resp.Content.ReadAsStringAsync();
                            //ShowErrorMessage(response);
                        }
                    };
                }
                catch (HttpRequestException ex)
                {
                    Toast.MakeText(context, ex.Message, ToastLength.Short).Show();
                }
            };
        }


        LatLng LatLng;
        int business_id = 0;
        private async void GetBusinessAsync()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{API.Url}/businesses/specific/{id}");

                if (response.IsSuccessStatusCode)
                {
                    string str_out = await response.Content.ReadAsStringAsync();
                    var business = Newtonsoft.Json.JsonConvert.DeserializeObject<Business>(str_out);

                    if (business != null)
                    {
                        business_id = business.Id;
                        owner_id = business.OwnerId;
                        status = business.Status;
                        online = business.Online;
                        if (business.Online == "Online")
                        {
                            
                            OutputStatus.Text = business.Online;
                            OutputStatus.SetTextColor(Android.Graphics.Color.ParseColor("#4CBB17"));
                        }
                        else
                        {
                            OutputStatus.Text = business.Online;
                            OutputStatus.SetTextColor(Android.Graphics.Color.ParseColor("#e6d9534f"));
                        }

                        OutputBusinessName.Text = business.Name;
                        var address = await ReverseGeocodeCurrentLocation(business.Address.Latitude, business.Address.Longitude);
                        OutputBusinessAddress.Text = address?.GetAddressLine(0);
                        //LatLng = new LatLng(business.Address.Latitude, business.Address.Longitude);
                        OutputBusinessDesc.Text = business.Description;
                        //OutputBusinessEmail.Text = business.E;
                        LatLng = new LatLng(business.Address.Latitude, business.Address.Longitude);
                        OutputBusinessPhoneNumber.Text = business.PhoneNumber;
                        try
                        {
                            if (business.ImgUrl != null)
                            {
                                ImageService
                                    .Instance
                                    .LoadUrl(business.ImgUrl)
                                    .Into(ImgBusinessLogo);
                            }
                        }
                        catch (Exception ex)
                        {
                            Message(ex.Message);
                        }

                        //get owner details
                        GetOwnerName(business.OwnerId, OutputBusinessOwner);
                    }

                }
            }
            catch (HttpRequestException ex)
            {
                Message(ex.Message);
            }
        }

        private async void GetOwnerName(string id, AppCompatTextView name)
        {
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{API.Url}/account/{id}");

            if (response.IsSuccessStatusCode)
            {
                string str_out = await response.Content.ReadAsStringAsync();
                var user = Newtonsoft.Json.JsonConvert.DeserializeObject<AppUsers>(str_out);

                if (user != null)
                {
                    name.Text = $"{user.Firstname} {user.Lastname}";
                }
            }
        }
        private FileResult file;
        async Task<Address> ReverseGeocodeCurrentLocation(double lat, double lon)
        {
            Geocoder geocoder = new Geocoder(context);
            IList<Address> addressList = await geocoder.GetFromLocationAsync(lat, lon, 5);

            Address address = addressList?.FirstOrDefault();
            return address;
        }
        private string status;
        private string online;
        private string img_url;
        private string owner_id;
        private async void UpdateUserDetails()
        {
            try
            {
                if(file != null)
                {
                    var memoryStream = new MemoryStream();
                    var st = await file.OpenReadAsync();
                    string filename = $"{file.FileName}";
                    var results = CrossFirebaseStorage.Current
                        .Instance
                        .RootReference
                        .Child("Profile_images")
                        .Child(filename);

                    await results.PutStreamAsync(st);

                    var url = await results.GetDownloadUrlAsync();
                    img_url = url.ToString();
                }
                Business data = new Business()
                {
                    Address = new Models.Address() { Latitude = LatLng.Latitude, Longitude = LatLng.Longitude },
                    Name = OutputBusinessName.Text,
                    PhoneNumber = OutputBusinessPhoneNumber.Text,
                    Status = status,
                    Online = online,
                    ImgUrl = img_url,
                    OwnerId = owner_id,
                    Description = OutputBusinessDesc.Text,
                    

                };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

                HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                HttpClient httpClient = new HttpClient();
                var response = await httpClient.PutAsync($"{API.Url}/businesses/{business_id}", httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var str_resp = await response.Content.ReadAsStringAsync();
                    AndHUD.Shared.ShowSuccess(context, str_resp, MaskType.Black, TimeSpan.FromSeconds(2));
                    //ViewState(false);
                }
                else
                {
                    var str_resp = await response.Content.ReadAsStringAsync();
                    AndHUD.Shared.ShowSuccess(context, str_resp, MaskType.Black, TimeSpan.FromSeconds(2));
                }
            }
            catch (HttpRequestException ex)
            {

                AndHUD.Shared.ShowSuccess(context, ex.Message, MaskType.Black, TimeSpan.FromSeconds(2));
            }
        }
    

        private void Message(string message)
        {
            AndHUD.Shared.ShowError(Context, message, MaskType.None, TimeSpan.FromSeconds(3));
        }
    }
}