using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KotaPalace.Dialogs
{
    public class AddExtrasFragment : DialogFragment
    {
        private AppCompatImageView close_add_ons;
        private TextInputEditText AddOnName;
        private MaterialButton BtnAddChip;

       

        public AddExtrasFragment() 
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
            Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            Dialog.SetCanceledOnTouchOutside(false);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view =  inflater.Inflate(Resource.Layout.fragment_add_extras, container, false);

            Init(view);
            AddAddOns();

            return view;
        }

        private void Init(View view)
        {
            close_add_ons = view.FindViewById<AppCompatImageView>(Resource.Id.close_add_ons);
            AddOnName = view.FindViewById<TextInputEditText>(Resource.Id.AddOnName);
            BtnAddChip = view.FindViewById<MaterialButton>(Resource.Id.BtnAddChip);

            close_add_ons.Click += (s, e) =>
            {
                Dismiss();
            };
        }
        public event EventHandler<AddOnHandler> AddOnAdded;
        public class AddOnHandler : EventArgs
        {
            public string Item { get; set; }
        }
        private void AddAddOns()
        {
            
            BtnAddChip.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(AddOnName.Text) || string.IsNullOrWhiteSpace(AddOnName.Text))
                {
                    AddOnName.RequestFocus();
                    AddOnName.Error = "Provide menu extras";
                    return;
                }
                else
                {
                    AddOnAdded.Invoke(this, new AddOnHandler() { Item = AddOnName.Text });
                    AddOnName.Text = "";
                    Dismiss();
                }
            };

        }
    }
}