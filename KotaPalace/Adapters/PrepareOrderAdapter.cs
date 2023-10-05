using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.TextView;
using KotaPalace.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace KotaPalace.Adapters
{
    public class PrepareOrderAdapter : RecyclerView.Adapter
    {
        List<Order> orders = new List<Order>();

        public PrepareOrderAdapter(List<Order> orders)
        {
            this.orders = orders;
        }

        public override int ItemCount => orders.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            OrderViewHolder vh = holder as OrderViewHolder;
            var order = orders[position];

            vh.row_order_no.Text = $"{order.Id}";
            vh.row_order_status.Text = $"{order.Status}";

            //var date = Convert.ToDateTime(order.OrderDate);
            vh.row_order_date.Text = order.OrderDateUtc.ToString("ddd, dd MMM yyyy");
            vh.row_order_time.Text = order.OrderDateUtc.ToString("HH:mm tt");

            if (order.Status == "Ready")
            {
                vh.view_btn.Enabled = false;
                vh.view_btn.Text = "DONE";
            }

            vh.view_btn.Click += (s, e) => { BtnClick.Invoke(vh.ItemView.Context, new OrderBtnClick() { Position = position }); };

            FindUserAsync(order.Customer_Id, vh.row_order_id);
        }
        public event EventHandler<OrderBtnClick> BtnClick;

        public class OrderBtnClick : EventArgs
        {
            public int Position { get; set; }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemview = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.order_row, parent, false);
            OrderViewHolder vh = new OrderViewHolder(itemview);
            return vh;
        }

        private async void FindUserAsync(string id,MaterialTextView textView)
        {
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"{API.Url}/account/{id}");

            if (response.IsSuccessStatusCode)
            {
                string str_out = await response.Content.ReadAsStringAsync();
                var user = Newtonsoft.Json.JsonConvert.DeserializeObject<AppUsers>(str_out);

                if(user != null)
                {
                    textView.Text = $"{user.Firstname}";
                }
            }
        }
    }

    public class OrderViewHolder : RecyclerView.ViewHolder
    {
        public MaterialTextView row_order_id { get; set;}
        public MaterialTextView row_order_status { get; set;}
        public MaterialTextView row_order_no { get; set; }
        public MaterialTextView row_order_date { get; set; }
        public AppCompatTextView row_order_time { get; set; }
        public MaterialButton view_btn { get; set; }

        public OrderViewHolder(View itemview) : base(itemview)
        {
            //row_order_image = itemview.FindViewById<AppCompatImageView>(Resource.Id.row_order_image);
            row_order_no = itemview.FindViewById<MaterialTextView>(Resource.Id.row_order_no);
            row_order_status = itemview.FindViewById<MaterialTextView>(Resource.Id.row_order_status);
            row_order_id = itemview.FindViewById<MaterialTextView>(Resource.Id.row_order_id);
            row_order_date = itemview.FindViewById<MaterialTextView>(Resource.Id.row_order_date);
            row_order_time = itemview.FindViewById<AppCompatTextView>(Resource.Id.row_order_time);

            view_btn = itemview.FindViewById<MaterialButton>(Resource.Id.view_btn);
        }
    }
}