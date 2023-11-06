using Android.Views;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Chip;
using Google.Android.Material.TextView;
using KotaPalace.Models;
using System.Collections.Generic;

namespace KotaPalace.Adapters
{
    public class OrderItemAdapter : RecyclerView.Adapter
    {
        private readonly List<OrderItems> orders = new List<OrderItems>();

        public OrderItemAdapter(List<OrderItems> orders)
        {
            this.orders = orders;
        }

        public override int ItemCount => orders.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            OrderItemViewHolder vh = holder as OrderItemViewHolder;
            var orderItems = orders[position];

            

            vh.Name.Text = $"Customer: {orderItems.ItemName}";
            vh.Price.Text = $"Price: R{orderItems.Price}";
            var items = orderItems.Extras;
            var extras = items.Split('#');

            foreach (var i in extras)
            {
                if(!string.IsNullOrEmpty(i))
                {
                    Chip chip = new Chip(vh.ItemView.Context)
                    {
                        Text = i,
                    };
                    vh.ChipGroup.AddView(chip);
                }
            }
        }
        //public event EventHandler<OrderBtnClick> BtnClick;

        //public class OrderBtnClick : EventArgs
        //{
        //    public int Position { get; set; }
        //}

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemview = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.order_items_row, parent, false);
            OrderItemViewHolder vh = new OrderItemViewHolder(itemview);
            return vh;
        }
    }

    public class OrderItemViewHolder : RecyclerView.ViewHolder
    {
        public MaterialTextView Price { get; set;}
        public MaterialTextView Name { get; set; }
        public ChipGroup ChipGroup { get; set;}


        public OrderItemViewHolder(View itemview) : base(itemview)
        {
            Name = itemview.FindViewById<MaterialTextView>(Resource.Id.order_item_name);
            Price = itemview.FindViewById<MaterialTextView>(Resource.Id.order_item_price);
            ChipGroup = itemview.FindViewById<ChipGroup>(Resource.Id.AddOnsChips);

        }
    }
}