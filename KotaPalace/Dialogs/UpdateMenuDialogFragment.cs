using Android.Content;
using Android.OS;
using Android.Views;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.Button;
using Google.Android.Material.Chip;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.TextField;
using KotaPalace.Models;
using Plugin.FirebaseStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Menu = KotaPalace.Models.Menu;

namespace KotaPalace.Dialogs
{
    public class UpdateMenuDialogFragment : DialogFragment
    {
        private Context context;

        private AppCompatImageView imgMenu;
        private TextInputEditText InputItemName;
        private TextInputEditText InputItemPrice;

        private FloatingActionButton FabMenuImg;
        private AppCompatImageView CloseDialogImg;


        private ChipGroup chipGroup;
        private  List<Extras> Items = new List<Extras>();

        private MaterialButton BtnOpenAddDlg;
        private MaterialButton BtnSubmitMenu;

        private string id = Preferences.Get("Id",null);

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public UpdateMenuDialogFragment()
        {
        }

        private int _id;
        private FileResult file;
        private Menu menu;

        public UpdateMenuDialogFragment(int Id)
        {
            this._id = Id;
        }

        public UpdateMenuDialogFragment(Menu menu)
        {
            this.menu = menu;
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
            View view = inflater.Inflate(Resource.Layout.fragment_update_menu, container, false);

            context = view.Context;

            Init(view);

            return view;
        }

        private void Init(View view)
        {
            CloseDialogImg = view.FindViewById<AppCompatImageView>(Resource.Id.CloseDialogImg);

            imgMenu = view.FindViewById<AppCompatImageView>(Resource.Id.imgMenu);
            InputItemName = view.FindViewById<TextInputEditText>(Resource.Id.InputItemName);
            InputItemPrice = view.FindViewById<TextInputEditText>(Resource.Id.InputItemPrice);
            chipGroup = view.FindViewById<ChipGroup>(Resource.Id.chipAddOns);

            FabMenuImg = view.FindViewById<FloatingActionButton>(Resource.Id.FabMenuImg);

            BtnOpenAddDlg = view.FindViewById<MaterialButton>(Resource.Id.BtnOpenAddDlg);

            BtnSubmitMenu = view.FindViewById<MaterialButton>(Resource.Id.BtnSubmitMenu);

            CloseDialogImg.Click += (s, e) =>
            {
                Dismiss();
            };

            FabMenuImg.Click += async (s, e) =>
            {
                //var file = await PickAndShow();
                file = await PickAndShow();
            };

            BtnOpenAddDlg.Click += (s, e) =>
            {
                //OpenAddExtrasDialog(view);
                AddExtrasFragment add = new AddExtrasFragment();
                add.Show(ChildFragmentManager.BeginTransaction(), "");
                add.AddOnAdded += Add_AddOnAdded;
            };

            BtnSubmitMenu.Click += (s, e) =>
            {
                SubmitMenuAsync();
            };
        }

        private void Add_AddOnAdded(object sender, AddExtrasFragment.AddOnHandler e)
        {
            Chip chip = new Chip(context);
            //create chip drawable
            ChipDrawable drawable = ChipDrawable.CreateFromAttributes(context,
                    null, 0, Resource.Style.Widget_MaterialComponents_Chip_Entry);

            //set chip drawable
            chip.SetChipDrawable(drawable);
            chip.Text = e.Item;

            Items.Add( new Extras() { Title =  e.Item });
           
            chipGroup.AddView(chip);
        }

        private async void SubmitMenuAsync()
        {

            if (string.IsNullOrEmpty(InputItemName.Text) || string.IsNullOrWhiteSpace(InputItemName.Text))
            {
                InputItemName.Error = "Please enter the item name";

            }
            else if (string.IsNullOrEmpty(InputItemPrice.Text) || string.IsNullOrWhiteSpace(InputItemPrice.Text))
            {
                InputItemPrice.Error = "Please enter the item's price";
            }else if(file == null)
            {
                Message("Please upload image for the menu");
            }
            else
            {
                try
                {
                   //await Xamarin.Essentials.TextToSpeech.SpeakAsync("Hello");
                    var businessId = Preferences.Get("businessId", 0);

                    var memoryStream = new MemoryStream();
                    var st = await file.OpenReadAsync();
                    string filename = $"{file.FileName}";
                    var results = CrossFirebaseStorage.Current
                        .Instance
                        .RootReference
                        .Child("Menu images")
                        .Child(filename);

                    await results.PutStreamAsync(st);

                    var url = await results.GetDownloadUrlAsync();

                    menu.BusinessId = businessId;
                    menu.Name = InputItemName.Text.Trim();
                    menu.Status = true;
                    menu.Price = Convert.ToDouble(InputItemPrice.Text);
                    menu.Extras = Items;
                    menu.Url = url.ToString();

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(menu);
                    HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpClient httpClient = new HttpClient();

                    var result = await httpClient.PutAsync($"{API.Url}/menus/{menu.Id}", data);

                    if (result.IsSuccessStatusCode)
                    {
                        string str_out = await result.Content.ReadAsStringAsync();
                        Message("Menu was successfully updated!!!");

                        InputItemName.Text = "";
                        InputItemPrice.Text = "";
                        chipGroup.RemoveAllViews();
                        Items.Clear();
                        file = null;
                    }
                }
                catch (Exception ex)
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

        private void Message(string s)
        {
            AndHUD.Shared.ShowSuccess(context, s, MaskType.None, TimeSpan.FromSeconds(6));
        }
        
    }
}