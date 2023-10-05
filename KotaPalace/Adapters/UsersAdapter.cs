using Android.App;
using Android.Views;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using FFImageLoading;
using Google.Android.Material.Button;
using KotaPalace.Models;
using Refractored.Controls;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using XamarinTextDrawable;
using Context = Android.Content.Context;

namespace KotaPalace.Adapters
{
    public class UsersAdapter : RecyclerView.Adapter
    {
        Context context;

        public ObservableCollection<AppUsers> UserList;

        public UsersAdapter(ObservableCollection<AppUsers> userList)
        {
            UserList = userList;
        }

        public override int ItemCount => UserList.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            UserViewHolder vh = holder as UserViewHolder;
            var user = UserList[position];
            context = vh.ItemView.Context;

            vh.Name.Text = $"{user.Firstname}" ;
            vh.Email.Text = $"{user.Email}";
            vh.PhoneNumber.Text = $"{user.PhoneNumber}";

            //TextDrawable drawable = TextDrawable.Builder()
            //    .buildRect("A", Color.Red);

            if (user.Url != null)
            {
                ImageService
                    .Instance
                    .LoadUrl(user.Url)
                    .Into(vh.Image);
            }

            vh.BtnUpdate.Click += (s, e) =>
            {
                BtnClick.Invoke(vh.ItemView.Context, new UserBtnClick { Position = position});
            };

            vh.BtnDelete.Click += (s, e) =>
            {
                RemoveItem(user.Id);
            };   
        }

        public event EventHandler<UserBtnClick> BtnClick;

        public class UserBtnClick: EventArgs
        {
            public int Position { get; set; }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.user_row, parent, false);
            UserViewHolder vh = new UserViewHolder(itemView);
            return vh;
        }


        private void RemoveItem(string id)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle("DELETE USER");
            builder.SetMessage("Are you sure want to delete user?");
            builder.SetNegativeButton("No", delegate
            {
                builder.Dispose();
            });
            builder.SetPositiveButton("Yes", delegate
            {
                //HttpClient httpClient = new HttpClient();

                //var result = await httpClient.DeleteAsync($"{API.Url}/menus/{id}");

                //if (result.IsSuccessStatusCode)
                //{
                //    string str_out = await result.Content.ReadAsStringAsync();
                //    AndHUD.Shared.ShowSuccess(context, str_out, MaskType.None, TimeSpan.FromSeconds(3));
                //}
                //else
                //{
                //    string str_out = await result.Content.ReadAsStringAsync();
                //    AndHUD.Shared.ShowError(context, str_out, MaskType.None, TimeSpan.FromSeconds(3));
                //}
                AndHUD.Shared.ShowError(context, $"{id}", MaskType.None, TimeSpan.FromSeconds(3));
                builder.Dispose();
            });
            builder.Show();
        }
    }

    public class UserViewHolder : RecyclerView.ViewHolder
    {
        public AppCompatTextView Name { get; set; }
        public AppCompatTextView Email { get; set; }
        public AppCompatTextView PhoneNumber { get; set; }

        public CircleImageView Image { get; set; }

        public MaterialButton BtnUpdate { get; set; }
        public MaterialButton BtnDelete { get; set; }

        public UserViewHolder(View itemview) : base(itemview)
        {
            Name = itemview.FindViewById<AppCompatTextView>(Resource.Id.row_user_name);
            Email = itemview.FindViewById<AppCompatTextView>(Resource.Id.row_user_email);
            PhoneNumber = itemview.FindViewById<AppCompatTextView>(Resource.Id.row_phone_number);

            Image = itemview.FindViewById<CircleImageView>(Resource.Id.row_user_image);

            BtnUpdate = itemview.FindViewById<MaterialButton>(Resource.Id.btn_update_user);
            BtnDelete = itemview.FindViewById<MaterialButton>(Resource.Id.btn_delete_user);
        }
    }
}