using Android.Content;
using Android.OS;
using Android.Views;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using FFImageLoading;
using Google.Android.Material.Button;
using Google.Android.Material.Chip;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.ImageView;
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
    public class AddMenuDialogFragment : DialogFragment
    {
        private Context context;

        private ShapeableImageView imgMenu;
        private TextInputEditText InputItemName;
        private TextInputEditText InputItemPrice;

        private FloatingActionButton FabMenuImg;
        private AppCompatImageView CloseDialogImg;


        private ChipGroup chipGroup;
        private readonly List<Extras> Items = new List<Extras>();

        private MaterialButton BtnOpenAddDlg;
        private MaterialButton BtnSubmitMenu;

        //private readonly string id = Preferences.Get("Id", null);

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }


        private FileResult file;


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
            View view = inflater.Inflate(Resource.Layout.fragment_add_menu, container, false);

            context = view.Context;

            Init(view);

            //SubmitMenu();
            //OpenAddExtrasDialog(view);

            return view;
        }

        private void Init(View view)
        {
            CloseDialogImg = view.FindViewById<AppCompatImageView>(Resource.Id.CloseDialogImg);

            imgMenu = view.FindViewById<ShapeableImageView>(Resource.Id.imgMenu);
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

            Items.Add(new Extras() { Title = e.Item });

            //chip.SetOnCloseIconClickListener(v1->
            //        chipGroup.removeView(chip));

            chipGroup.AddView(chip);
        }

        private async void SubmitMenuAsync()
        {

            if (string.IsNullOrEmpty(InputItemName.Text) || string.IsNullOrWhiteSpace(InputItemName.Text))
            {
                InputItemName.Error = "Please the item name";

            }
            else if (string.IsNullOrEmpty(InputItemPrice.Text) || string.IsNullOrWhiteSpace(InputItemPrice.Text))
            {
                InputItemPrice.Error = "Please the item's price";
            }
            else if (file == null)
            {
                Message("Please upload image for the menu");
            }
            else
            {
                try
                {

                    var businessId = Preferences.Get("businessId", 0);
                    Uri url = null;
                    if (file != null)
                    {
                        var memoryStream = new MemoryStream();
                        var st = await file.OpenReadAsync();
                        string filename = $"{file.FileName}";
                        var results = CrossFirebaseStorage.Current
                            .Instance
                            .RootReference
                            .Child("Menu images")
                            .Child(filename);

                        await results.PutStreamAsync(st);

                        url = await results.GetDownloadUrlAsync();
                    }


                    Menu menu = new Menu()
                    {
                        BusinessId = businessId,
                        Name = InputItemName.Text.Trim(),
                        Price = Convert.ToDouble(InputItemPrice.Text),
                        Extras = Items,
                        Status = true,
                        Url = url?.ToString()
                    }; 

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(menu);
                    HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpClient httpClient = new HttpClient();

                    var result = await httpClient.PostAsync($"{API.Url}/menus", data);

                    if (result.IsSuccessStatusCode)
                    {
                        string str_out = await result.Content.ReadAsStringAsync();

                        InputItemName.Text = "";
                        InputItemPrice.Text = "";
                        chipGroup.RemoveAllViews();
                        Items.Clear();
                        Message("Menu added successfully!!!");
                        MenuHandler.Invoke(this, new AddedMenuArgs() { mMenu = menu });
                    }
                }
                catch (Exception ex)
                {
                    Message(ex.Message);
                }
                finally
                {
                    
                }
            }
        }
        public event EventHandler<AddedMenuArgs> MenuHandler;
        public class AddedMenuArgs : EventArgs
        {
            public Menu mMenu { get; set; }
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
                    ImageService.Instance
                        .LoadFile(file.FullPath)
                        .Into(imgMenu);

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

    public class MenuImage
    {
        public int Id { get; set; }
        public string Url { get; set; }
    }
}