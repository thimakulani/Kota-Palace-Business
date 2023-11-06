using Android.App;
using Android.Views;
using AndroidHUD;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using FFImageLoading;
using Google.Android.Material.Button;
using Google.Android.Material.Chip;
using Google.Android.Material.Internal;
using KotaPalace.Models;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using Context = Android.Content.Context;
using Menu = KotaPalace.Models.Menu;

namespace KotaPalace.Adapters
{
    public class MenuAdapter : RecyclerView.Adapter
    {
        Context context;

        public ObservableCollection<Menu> MenuList;

        public MenuAdapter(ObservableCollection<Menu> menuList)
        {
            MenuList = menuList;
        }

        public override int ItemCount => MenuList.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            MenuViewHolder vh = holder as MenuViewHolder;
            var menu = MenuList[position];
            context = vh.ItemView.Context;

            vh.Name.Text = $"{menu.Name}" ;
            vh.Price.Text = $"R{menu.Price}";
            //vh.MenuId.Text = $"{menu.Id}";

            if(menu.Url != null)
            {
                ImageService
                    .Instance
                    .LoadUrl(menu.Url)
                    .Into(vh.Row_menuIcon);
            }
            

            vh.ChipGroup.RemoveAllViews();

            foreach (var i in menu.Extras)
            {
                Chip chip = new Chip(vh.ItemView.Context)
                {
                    Text = i.Title
                };
                vh.ChipGroup.AddView(chip);
            }
            vh.BtnOption.Click += (sender, e) =>
            {
                BtnClick.Invoke(context, new MenuBtnClick() { ImgBtn = vh.BtnOption, Pos = position });
            };

            /*vh.BtnUpdate.Click += (s, e) =>
            {
                BtnClick.Invoke(vh.ItemView.Context, new MenuBtnClick { Pos = position});
            };

            vh.BtnDelete.Click += (s, e) =>
            {
                RemoveItem(menu.Id);
            }; */
        }

        public event EventHandler<MenuBtnClick> BtnClick;

        public class MenuBtnClick: EventArgs
        {
            public int Pos { get; set; }
            public CheckableImageButton ImgBtn { get; set; }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.menu_row, parent, false);
            MenuViewHolder vh = new MenuViewHolder(itemView);
            return vh;
        }
    }

    public class MenuViewHolder : RecyclerView.ViewHolder
    {
        public AppCompatTextView Name { get; set; }
        public AppCompatTextView Price { get; set; }

        public ChipGroup ChipGroup { get; set; }

        public AppCompatTextView MenuId { get;set; }

        public AppCompatImageView Row_menuIcon { get; set; }

        public CheckableImageButton BtnOption { get; set; } 
       // public MaterialButton BtnDelete { get; set; }

        public MenuViewHolder(View itemview) : base(itemview)
        {
            Name = itemview.FindViewById<AppCompatTextView>(Resource.Id.row_name);
            Price = itemview.FindViewById<AppCompatTextView>(Resource.Id.row_price);
           // MenuId = itemview.FindViewById<AppCompatTextView>(Resource.Id.row_menu_id);
            Row_menuIcon = itemview.FindViewById<AppCompatImageView>(Resource.Id.row_menuIcon);

            ChipGroup = itemview.FindViewById<ChipGroup>(Resource.Id.AddOnsChips);

            BtnOption = itemview.FindViewById<CheckableImageButton>(Resource.Id.cart_btn_more);
            /*
            BtnDelete = itemview.FindViewById<MaterialButton>(Resource.Id.row_btn_delete);*/

        }
    }
}