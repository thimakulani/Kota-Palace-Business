using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.TextView;
using KotaPalace.Models;
using System;
using System.Collections.Generic;

namespace KotaPalace.Adapters
{
    internal class OrderProcessAdapter : RecyclerView.Adapter
    {
        public event EventHandler<CompleteAdapterClickEventArgs> ItemClick;
        public event EventHandler<CompleteAdapterClickEventArgs> ItemLongClick;
        readonly List<Order> items = new List<Order>();

        public OrderProcessAdapter(List<Order> data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = null;
            var id = Resource.Layout.process_order_row;
            itemView = LayoutInflater.From(parent.Context).
                   Inflate(id, parent, false);

            var vh = new CompleteAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var order = items[position];

            // Replace the contents of the view with that element
            var vh = viewHolder as CompleteAdapterViewHolder;
            //holder.TextView.Text = items[position];
            

            //vh.row_order_no.Text = order.Id.ToString();
            vh.row_order_option.Text = order.Option;
            vh.row_order_status.Text = order.Status;
            vh.row_order_date.Text = order.OrderDateUtc.ToString("ddd, dd MMM yyyy HH:mm");
            vh.row_order_id.Text = $"{order.Customer.Firstname} {order.Customer.Lastname}";
        }

        public override int ItemCount => items.Count;

        void OnClick(CompleteAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(CompleteAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class CompleteAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public AppCompatImageView row_order_image { get; set; }
        public MaterialTextView row_order_id { get; set; }
        public MaterialTextView row_order_status { get; set; }
        public MaterialTextView row_order_no { get; set; }
        public MaterialTextView row_order_date { get; set; }
        public MaterialTextView row_order_option { get; set; }

        public CompleteAdapterViewHolder(View itemView, Action<CompleteAdapterClickEventArgs> clickListener,
                            Action<CompleteAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            //TextView = v;
            row_order_no = itemView.FindViewById<MaterialTextView>(Resource.Id.row_order_no);
            row_order_status = itemView.FindViewById<MaterialTextView>(Resource.Id.row_order_status);
            row_order_id = itemView.FindViewById<MaterialTextView>(Resource.Id.row_order_id);
            row_order_date = itemView.FindViewById<MaterialTextView>(Resource.Id.row_order_date);
            row_order_option = itemView.FindViewById<MaterialTextView>(Resource.Id.row_order_option);
            itemView.Click += (sender, e) => clickListener(new CompleteAdapterClickEventArgs { View = itemView, Position = AbsoluteAdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new CompleteAdapterClickEventArgs { View = itemView, Position = AbsoluteAdapterPosition });
        }
    }

    public class CompleteAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}