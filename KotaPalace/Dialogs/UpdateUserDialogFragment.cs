using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using FFImageLoading;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using KotaPalace.Models;
using Refractored.Controls;
using System;
using System.Net.Http;
using System.Text;
using Context = Android.Content.Context;

namespace KotaPalace.Dialogs
{
    public class UpdateUserDialogFragment : DialogFragment
    {
        private Context mContext;
        private AppCompatImageView cancel_iv;

        private CircleImageView UserProfileImg;
        private TextInputEditText InputUpdateFirstname;
        private TextInputEditText InputUpdateLastname;
        private TextInputEditText InputUpdateUserType;
        private TextInputEditText InputUpdatePhoneNumber;

        private MaterialButton BtnUpdateUser;

        private string key;
        private AppUsers users;

        public UpdateUserDialogFragment(AppUsers users)
        {
            this.users = users;
        }

        public UpdateUserDialogFragment(string key)
        {
            this.key = key;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override void OnStart()
        {
            base.OnStart();
            Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            Dialog.SetCanceledOnTouchOutside(false);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.fragment_update_user, container, false);

            mContext = view.Context;

            Init(view);
            GetUserInfo();

            return view;
        }

        private void Init(View view)
        {
            cancel_iv = view.FindViewById<AppCompatImageView>(Resource.Id.cancel_iv);
            UserProfileImg = view.FindViewById<CircleImageView>(Resource.Id.UserProfileImg);

            InputUpdateFirstname = view.FindViewById<TextInputEditText>(Resource.Id.InputUpdateFirstname);
            InputUpdateLastname = view.FindViewById<TextInputEditText>(Resource.Id.InputUpdateLastname);
            InputUpdateUserType = view.FindViewById<TextInputEditText>(Resource.Id.InputUpdateUserType);
            InputUpdatePhoneNumber = view.FindViewById<TextInputEditText>(Resource.Id.InputUpdatePhoneNumber);

            BtnUpdateUser = view.FindViewById<MaterialButton>(Resource.Id.BtnUpdateUser);

            cancel_iv.Click += (s, e) =>
            {
                Dialog.Dismiss();
            };

            BtnUpdateUser.Click += (s, e) =>
            {
                UpdateUser();
            };
        }

        private void GetUserInfo()
        {
            if(users.Url != null)
            {
                ImageService
                    .Instance
                    .LoadUrl(users.Url)
                    .Into(UserProfileImg);
            }
            InputUpdateFirstname.Text = users.Firstname;
            InputUpdateLastname.Text = users.Lastname;
            InputUpdateUserType.Text = users.UserType;
            InputUpdatePhoneNumber.Text = users.PhoneNumber;
        }

        private async void UpdateUser()
        {
            var userId = users.Id;

            HttpClient client = new HttpClient();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(users);
            HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PutAsync($"{API.Url}/account/update/{users.Id}", data);

                if (response.IsSuccessStatusCode)
                {
                    string str = await response.Content.ReadAsStringAsync();
                    Message(str);
                }
            }
            catch (HttpRequestException ex)
            {
                Message(ex.Message);
            }
        }

        private void Message(string txt)
        {
            AndHUD.Shared.ShowSuccess(mContext,txt,MaskType.None,TimeSpan.FromSeconds(3));
        }
    }
}