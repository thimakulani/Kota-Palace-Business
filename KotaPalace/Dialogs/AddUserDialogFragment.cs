using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using FFImageLoading;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using Google.Android.Material.TextView;
using KotaPalace.Activities;
using KotaPalace.Models;
using Plugin.FirebaseStorage;
using Refractored.Controls;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Context = Android.Content.Context;

namespace KotaPalace.Dialogs
{
    public class AddUserDialogFragment : DialogFragment
    {
        private Context mContext;
        private AppCompatImageView cancel_iv;

        private CircleImageView UserProfileImg;
        private TextInputEditText InputAddFirstname;
        private TextInputEditText InputAddLastname;
        private TextInputEditText InputAddEmail;
        private TextInputEditText InputAddPhoneNumber;
        private TextInputEditText InputAddPassword;
        private TextInputEditText InputAddConfirmPassword;


        private MaterialButton BtnAddUser;

        private FileResult file;

        public AddUserDialogFragment()
        {
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
            View view = inflater.Inflate(Resource.Layout.fragment_add_user, container, false);

            mContext = view.Context;
            Init(view);

            return view;
        }

        private void Init(View view)
        {
            cancel_iv = view.FindViewById<AppCompatImageView>(Resource.Id.cancel_iv);
            UserProfileImg = view.FindViewById<CircleImageView>(Resource.Id.UserProfileImg);

            InputAddFirstname = view.FindViewById<TextInputEditText>(Resource.Id.InputAddFirstname);
            InputAddLastname = view.FindViewById<TextInputEditText>(Resource.Id.InputAddLastname);
            InputAddEmail = view.FindViewById<TextInputEditText>(Resource.Id.InputAddEmail);
            InputAddPhoneNumber = view.FindViewById<TextInputEditText>(Resource.Id.InputAddPhoneNumber);
            InputAddPassword = view.FindViewById<TextInputEditText>(Resource.Id.InputAddPassword);
            InputAddConfirmPassword = view.FindViewById<TextInputEditText>(Resource.Id.InputAddConfirmPassword);

            //txt_id_copy = view.FindViewById<MaterialTextView>(Resource.Id.txt_id_copy);
            //txt_pdp_copy = view.FindViewById<MaterialTextView>(Resource.Id.txt_pdp_copy);

            //BtnIdCopy = view.FindViewById<MaterialButton>(Resource.Id.BtnIdCopy);
            //BtnPDPCopy = view.FindViewById<MaterialButton>(Resource.Id.BtnPDPCopy);

            BtnAddUser = view.FindViewById<MaterialButton>(Resource.Id.BtnAddUser);

            cancel_iv.Click += (s, e) =>
            {
                Dialog.Dismiss();
            };

            UserProfileImg.Click += async (s, e) =>
            {
                file = await PickAndShow();
            };

            //BtnIdCopy.Click += async (s, e) =>
            //{
            //    id_file = await PickAndShowDoc();
            //};

            //BtnPDPCopy.Click += async (s, e) =>
            //{
            //    pdp_file = await PickAndShowDoc();
            //};

            BtnAddUser.Click += (s, e) =>
            {
                AddNewUser();
            };
        }

        private async void AddNewUser()
        {
            if (string.IsNullOrEmpty(InputAddFirstname.Text) || string.IsNullOrWhiteSpace(InputAddFirstname.Text))
            {
                InputAddFirstname.RequestFocus();
                InputAddFirstname.Error = "Provide your firstname";
                return;
            }

            else if (string.IsNullOrEmpty(InputAddLastname.Text) || string.IsNullOrWhiteSpace(InputAddLastname.Text))
            {
                InputAddLastname.RequestFocus();
                InputAddLastname.Error = "Provide your lastname";
                return;
            }

            else if (string.IsNullOrEmpty(InputAddEmail.Text) || string.IsNullOrWhiteSpace(InputAddEmail.Text))
            {
                InputAddEmail.RequestFocus();
                InputAddEmail.Error = "Provide your email";
                return;
            }

            else if (string.IsNullOrEmpty(InputAddPhoneNumber.Text) || string.IsNullOrWhiteSpace(InputAddPhoneNumber.Text))
            {
                InputAddPhoneNumber.RequestFocus();
                InputAddPhoneNumber.Error = "Provide your email";
                return;
            }

            else if (string.IsNullOrEmpty(InputAddPassword.Text) || string.IsNullOrWhiteSpace(InputAddPassword.Text))
            {
                InputAddPassword.RequestFocus();
                InputAddPassword.Error = "Provide your email";
                return;
            }

            else if (string.IsNullOrEmpty(InputAddConfirmPassword.Text) || string.IsNullOrWhiteSpace(InputAddConfirmPassword.Text))
            {
                InputAddConfirmPassword.RequestFocus();
                InputAddConfirmPassword.Error = "Provide your email";
                return;
            }
            else if(file == null)
            {
                Message("Please uplaod user's profile picture!!!");
            }
            else if(InputAddPassword.Text != InputAddConfirmPassword.Text)
            {
                InputAddConfirmPassword.Error = "Passwords don't match!!!";
                return ;
            }

            //else if(id_file == null)
            //{
            //    Message("Please upload Id copy!");
            //}

            //else if (pdp_file == null)
            //{
            //    Message("Please upload PDP copy!");
            //}

            else
            {
                var st = await file.OpenReadAsync();
                string filename = $"{file.FileName}";
                var result = CrossFirebaseStorage.Current
                    .Instance
                    .RootReference
                    .Child("images")
                    .Child(filename);

                await result.PutStreamAsync(st);

                var url = await result.GetDownloadUrlAsync();

                UserSignUp user = new UserSignUp()
                {
                    Firstname = InputAddFirstname.Text.Trim(),
                    UserType = "Driver",
                    Lastname = InputAddLastname.Text.Trim(),
                    Email = InputAddEmail.Text.Trim(),
                    Password = InputAddPassword.Text.Trim(),
                    PhoneNumber = InputAddPhoneNumber.Text.Trim(),
                    Url = url.ToString()
                };

                try
                {
                    HttpClient client = new HttpClient();
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
                    HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");
                    try
                    {
                        var response = await client.PostAsync($"{API.Url}/account/signup", data);

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
                catch(Exception ex)
                {
                    Message(ex.Message);
                }

            }
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
                    return file;
                }
            }
            catch (Exception ex)
            {
                Message(ex.Message);
            }

            return null;
        }

        private async Task<FileResult> PickAndShowDoc()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions()
                {
                    FileTypes = FilePickerFileType.Pdf
                });

                if (file != null)
                {
                    return file;
                }
            }
            catch (Exception ex)
            {
                Message(ex.Message);
            }

            return null;
        }

        private void Message(string txt)
        {
            AndHUD.Shared.ShowSuccess(mContext,txt,MaskType.None,TimeSpan.FromSeconds(3));
        }
    }

    public class AddUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string PhoneNumber { get; set; }
        public string UserType { get; set; }
        public string Url { get; set; }

    }
}