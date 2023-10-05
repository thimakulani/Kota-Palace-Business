using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Fragment.App;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using KotaPalace.Models;
using System;
using System.Net.Http;
using System.Text;
using Context = Android.Content.Context;

namespace KotaPalace.Dialogs
{
    public class ForgotPasswordDialogFragment : DialogFragment
    {
        private Context mContext;
        private ImageView cancel_iv;
        private TextInputEditText InputResetEmail;
        private TextInputEditText InputResetPassword;

        private MaterialButton BtnResetPassword;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override void OnStart()
        {
            base.OnStart();
            Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Dialog.SetCanceledOnTouchOutside(false);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.fragment_reset_password, container, false);

            mContext = view.Context;

            Init(view);
            
            return view;
        }

        private void Init(View view)
        {
            cancel_iv = view.FindViewById<ImageView>(Resource.Id.cancel_iv);
            InputResetEmail = view.FindViewById<TextInputEditText>(Resource.Id.InputResetEmail);
            InputResetPassword = view.FindViewById<TextInputEditText>(Resource.Id.InputResetPassword);

            BtnResetPassword = view.FindViewById<MaterialButton>(Resource.Id.BtnResetPassword);

            BtnResetPassword.Click += (s, e) =>
            {
                ResetPassword();
            };

            cancel_iv.Click += (s, e) =>
            {
                Dialog.Dismiss();
            };
        }

        private async void ResetPassword()
        {
            UserLogin users = new UserLogin()
            {
                Email = InputResetEmail.Text,
                Password = InputResetPassword.Text
            };

            if(string.IsNullOrEmpty(InputResetEmail.Text) || string.IsNullOrEmpty(InputResetEmail.Text))
            {
                InputResetEmail.RequestFocus();
                InputResetEmail.Error = "Provide your email";
                return;
            }else if(string.IsNullOrEmpty(InputResetPassword.Text) || string.IsNullOrEmpty(InputResetPassword.Text))
            {
                InputResetPassword.RequestFocus();
                InputResetPassword.Error = "Provide your email";
                return;
            }
            else
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(users);
                HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpClient httpClient = new HttpClient();

                var results = await httpClient.PostAsync($"{API.Url}/account/resetPassword", data);

                if (results.IsSuccessStatusCode)
                {
                    string output = await results.Content.ReadAsStringAsync();

                    if(output != null)
                    {
                        Message(output);
                        //reset input
                        InputResetEmail.Text = "";
                        InputResetPassword.Text = "";
                    }
                }
                else
                {
                    string output = await results.Content.ReadAsStringAsync();
                    Message(output);
                }
            }
        }

        private void Message(string txt)
        {
            AndHUD.Shared.ShowSuccess(mContext,txt,MaskType.None,TimeSpan.FromSeconds(3));
        }
    }

    public class UserLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}