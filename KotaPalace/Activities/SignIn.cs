using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.App;
using Firebase.Firestore;
using Firebase.Firestore.Auth;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Google.Android.Material.TextView;
using ID.IonBit.IonAlertLib;
using Java.Net;
using KotaPalace.Dialogs;
using KotaPalace.Models;
using Newtonsoft.Json;
using Plugin.FirebaseStorage;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Xamarin.Essentials;
using static Android.Provider.ContactsContract.CommonDataKinds;

namespace KotaPalace.Activities
{
    [Android.App.Activity(Label = "SignIn", Theme = "@style/AppTheme", MainLauncher = false)]
    public class SignIn : AppCompatActivity
    {


        private MaterialButton BtnLogin;
        private TextView ForgotPassword;
        private TextInputEditText InputEmail;
        private TextInputEditText InputPassword;
        private IonAlert loadingDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_sign_in);

            Init();
        }

        private void Init()
        {
            // context = Context;
            BtnLogin = FindViewById<MaterialButton>(Resource.Id.BtnLogin);
            InputEmail = FindViewById<TextInputEditText>(Resource.Id.LoginInputEmail);
            InputPassword = FindViewById<TextInputEditText>(Resource.Id.LoginInputPassword);
            ForgotPassword = FindViewById<MaterialTextView>(Resource.Id.TxtForgotPassword);


            ForgotPassword.Click += (s, e) =>
            {
                try
                {
                    ForgotPasswordDialogFragment fragment = new ForgotPasswordDialogFragment();
                    fragment.Show(SupportFragmentManager.BeginTransaction(), "");
                }
                catch (WebException ex)
                {
                    Message(ex.Message);
                }
            };
            BtnLogin.Click += BtnLogin_Click;


        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (IsValid())
            {
                loadingDialog = new IonAlert(this, IonAlert.ProgressType);
                loadingDialog.SetSpinKit("DoubleBounce")
                    .ShowCancelButton(false)
                    .Show();
                try
                {

                    UserLogin userLogin = new UserLogin()
                    {
                        Email = InputEmail.Text.Trim(),
                        Password = InputPassword.Text.Trim(),
                    };
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(userLogin);

                    using (var httpClient = new HttpClient())
                    {
                        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync($"{API.Url}/account/business/login", httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            var user = JsonConvert.DeserializeObject<AppUsers>(result);

                            // Store user data in preferences
                            Preferences.Set("Id", user.Id);
                            Preferences.Set("e", user.Email);
                            Preferences.Set("p", userLogin.Password);

                            // Start the HomeActivity
                            var intent = new Intent(this, typeof(MainActivity));
                            StartActivity(intent);
                        }
                        else
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            AndHUD.Shared.ShowError(this, result, MaskType.Clear, TimeSpan.FromSeconds(3));
                        }
                    };



                }
                catch (HttpRequestException ex)
                {
                    AndHUD.Shared.ShowError(this, ex.Message, MaskType.Clear, TimeSpan.FromSeconds(3));
                }
                catch(UnknownHostException ex)
                {
                    AndHUD.Shared.ShowError(this, ex.Message, MaskType.Clear, TimeSpan.FromSeconds(3));
                }
                finally
                {
                    loadingDialog.Dismiss();
                }
            }
        }
        private bool IsValid()
        {
            bool valid = true;
            if (string.IsNullOrEmpty(InputEmail.Text))
            {
                valid = false;
                InputEmail.Error = "provide your email";
            }
            if (string.IsNullOrEmpty(InputPassword.Text))
            {
                valid = false;
                InputPassword.Error = "provide your password";
            }
            return valid;
        }



        private void Message(string t)
        {
            AndHUD.Shared.ShowError(this, t, MaskType.None, TimeSpan.FromSeconds(3));
        }
    }

    public class UserLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserSignUp
    {
        public UserSignUp()
        {
        }

        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string PhoneNumber { get; set; }
        public string UserType { get; set; }
        public string Url { get; set; }
    }
}