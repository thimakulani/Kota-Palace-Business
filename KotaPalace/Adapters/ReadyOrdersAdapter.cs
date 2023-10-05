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
    public class ReadyOrdersAdapter : RecyclerView.Adapter
    {
        List<Order> orders = new List<Order>();

        public ReadyOrdersAdapter(List<Order> orders)
        {
            this.orders = orders;
        }

        public override int ItemCount => orders.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ReadyOrderViewHolder vh = holder as ReadyOrderViewHolder;
            var order = orders[position];

            vh.row_order_no.Text = order.Id.ToString();
            vh.row_order_status.Text = order.Status;
            vh.row_order_date.Text = order.OrderDateUtc.ToString("ddd, dd MMM yyyy");

            FindUserAsync(order.Customer_Id, vh.row_order_id);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemview = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ready_order_row, parent, false);
            ReadyOrderViewHolder vh = new ReadyOrderViewHolder(itemview);
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

    public class ReadyOrderViewHolder : RecyclerView.ViewHolder
    {
        public AppCompatImageView row_order_image { get; set; }
        public MaterialTextView row_order_id { get; set;}
        public MaterialTextView row_order_status { get; set;}
        public MaterialTextView row_order_no { get; set; }
        public MaterialTextView row_order_date { get; set; }
        public MaterialButton view_btn { get; set; }

        public ReadyOrderViewHolder(View itemview) : base(itemview)
        {
            //row_order_image = itemview.FindViewById<AppCompatImageView>(Resource.Id.row_order_image);
            row_order_no = itemview.FindViewById<MaterialTextView>(Resource.Id.row_order_no);
            row_order_status = itemview.FindViewById<MaterialTextView>(Resource.Id.row_order_status);
            row_order_id = itemview.FindViewById<MaterialTextView>(Resource.Id.row_order_id);
            row_order_date = itemview.FindViewById<MaterialTextView>(Resource.Id.row_order_date);
        }
    }
}