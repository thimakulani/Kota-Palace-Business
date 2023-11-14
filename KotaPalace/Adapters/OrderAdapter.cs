using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;
using KotaPalace.Models;
using System;
using System.Collections.Generic;

namespace KotaPalace.Adapters
{
    internal class OrderAdapter : RecyclerView.Adapter
    {
        public event EventHandler<OrderAdapterClickEventArgs> ItemClick;
        public event EventHandler<OrderAdapterClickEventArgs> ViewClick;
        public event EventHandler<OrderAdapterClickEventArgs> ItemLongClick;
        //string[] items;
        readonly List<Order> items = new List<Order>();
        public OrderAdapter(List<Order> data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = null;
            var id = Resource.Layout.order_row;
            itemView = LayoutInflater.From(parent.Context).
                   Inflate(id, parent, false);

            var vh = new OrderAdapterViewHolder(itemView, OnClick, OnLongClick, OnViewClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {


            // Replace the contents of the view with that element
            var vh = viewHolder as OrderAdapterViewHolder;
            var order = items[position];

            // vh.row_order_customer.Text = $"{order.Id}";
            vh.Row_order_status.Text = $"{order.Status}";
            vh.Row_order_id.Text = $"ORDER# {order.Id}";

            //var date = Convert.ToDateTime(order.OrderDate);
            vh.Row_order_date.Text = order.OrderDateUtc.ToString("ddd, dd MMM yyyy");
            vh.Row_order_time.Text = order.OrderDateUtc.ToString("HH:mm tt");

            if (order.Status == "Ready")
            {
                vh.View_btn.Enabled = false;
                vh.View_btn.Text = "DONE";
            }
            if (order.Customer != null)
            {
                vh.Row_order_customer.Text = $"{order?.Customer.Firstname} {order?.Customer.Lastname}";
            }

        }

        public override int ItemCount => items.Count;

        void OnClick(OrderAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnViewClick(OrderAdapterClickEventArgs args) => ViewClick?.Invoke(this, args);
        void OnLongClick(OrderAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class OrderAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public MaterialTextView Row_order_id { get; set; }
        public MaterialTextView Row_order_status { get; set; }
        public MaterialTextView Row_order_customer { get; set; }
        public MaterialTextView Row_order_date { get; set; }
        //public MaterialTextView row_order_id { get; set; }
        public AppCompatTextView Row_order_time { get; set; }
        public MaterialButton View_btn { get; set; }

        public OrderAdapterViewHolder(View itemView, Action<OrderAdapterClickEventArgs> clickListener,
                            Action<OrderAdapterClickEventArgs> longClickListener, Action<OrderAdapterClickEventArgs> viewClickListener) : base(itemView)
        {
            //TextView = v;
            Row_order_customer = itemView.FindViewById<MaterialTextView>(Resource.Id.row_order_customer);
            Row_order_status = itemView.FindViewById<MaterialTextView>(Resource.Id.row_order_status);
            Row_order_id = itemView.FindViewById<MaterialTextView>(Resource.Id.row_order_id);
            Row_order_date = itemView.FindViewById<MaterialTextView>(Resource.Id.row_order_date);
            Row_order_time = itemView.FindViewById<AppCompatTextView>(Resource.Id.row_order_time);

            View_btn = itemView.FindViewById<MaterialButton>(Resource.Id.view_btn);

            View_btn.Click += (sender, e) => viewClickListener(new OrderAdapterClickEventArgs { View = itemView, Position = AbsoluteAdapterPosition });
            itemView.Click += (sender, e) => clickListener(new OrderAdapterClickEventArgs { View = itemView, Position = AbsoluteAdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new OrderAdapterClickEventArgs { View = itemView, Position = AbsoluteAdapterPosition });
        }
    }

    public class OrderAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}