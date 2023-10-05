using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextField;
using KotaPalace.Models;
using System;
using System.Net.Http;
using System.Text;
using Context = Android.Content.Context;

namespace KotaPalace.Dialogs
{
    public class SelectDriverDialogFragment : DialogFragment
    {
        private Context mContext;
        private AppCompatImageView cancel_iv;
        private RecyclerView select_user_recyclerView;

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
            cancel_iv = view.FindViewById<AppCompatImageView>(Resource.Id.cancel_iv);
            select_user_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.select_user_recyclerView);

            cancel_iv.Click += (s, e) =>
            {
                Dialog.Dismiss();
            };
        }

        private void Message(string txt)
        {
            AndHUD.Shared.ShowSuccess(mContext,txt,MaskType.None,TimeSpan.FromSeconds(3));
        }
    }
}