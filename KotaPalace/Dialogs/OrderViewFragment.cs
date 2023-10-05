using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.Chip;
using Java.Util;
using KotaPalace.Adapters;
using KotaPalace.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using XamarinTextDrawable;
using Context = Android.Content.Context;
using Random = System.Random;

namespace KotaPalace.Dialogs
{
    public class OrderViewFragment : DialogFragment
    {
        private Context context;

        private ImageView close_order_view;

        private TextView order_name;
        private TextView order_price;
        private TextView business_Id;
        private TextView order_status;

        private RecyclerView orderItemsRecyclerView;

        private MaterialButton BtnProcess;

        //private int businessId = Preferences.Get("businessId", 0);
        //private string Id = Preferences.Get("Id", null);
        //List<string> options = { "DELIVER", "EAT-IN", "COLLECT" };

        private readonly Order order;
        readonly List<OrderItems> OrderItemList = new List<OrderItems>();

        public OrderViewFragment()
        {
        }

        public OrderViewFragment(Order order)
        {
            this.order = order;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override void OnStart()
        {
            base.OnStart();
            Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View view = inflater.Inflate(Resource.Layout.order_row_view, container, false);
            context = view.Context;
            Init(view);
            LoadOrdersAsync();
            CheckOrderStatus();
            CheckOrderType();

            return view;
        }

        private void Init(View view)
        {
            close_order_view = view.FindViewById<ImageView>(Resource.Id.close_order_view);

            order_name = view.FindViewById<TextView>(Resource.Id.order_name);
            order_price = view.FindViewById<TextView>(Resource.Id.order_price);
            business_Id = view.FindViewById<TextView>(Resource.Id.business_Id);
            order_status = view.FindViewById<TextView>(Resource.Id.order_status);

            order_status.Text = order.Status;

            orderItemsRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.orderItemsRecyclerView);

            BtnProcess = view.FindViewById<MaterialButton>(Resource.Id.BtnProcess);

            close_order_view.Click += (s, e) =>
            {
                Dismiss();
            };

            BtnProcess.Click += (s, e) =>
            {
                ProcessOrderAsync();
            };
        }

        private void CheckOrderStatus()
        {
            if(order.Status == "Pending")
            {
                BtnProcess.Text = "ACCEPT";

            }else if(order.Status == "Accepted")
            {
                BtnProcess.Text = "FINISH";
            }
            else
            {
                BtnProcess.Enabled = false;
                BtnProcess.Text = "DONE";
            }
        }

        private void CheckOrderType()
        {
            //var random = new Random();
            //var list = new List<string> { };
            //int index = random.Next(list.Count);

            //Message(list[index]);
        }

        private void LoadOrdersAsync()
        {
            try
            {
                RecyclerView.LayoutManager mLayoutManager = new LinearLayoutManager(context);
                orderItemsRecyclerView.SetLayoutManager(mLayoutManager);
                OrderItemAdapter mAdapter = new OrderItemAdapter(OrderItemList);
                orderItemsRecyclerView.SetAdapter(mAdapter);

                   
                business_Id.Text = $"Business ID: {order.BusinessId}";
                order_status.Text = $"Status: {order.Status}";

                var extras = order.OrderItems;

                foreach (var item in extras)
                {
                    OrderItemList.Add(item);
                    order_price.Text = $"R{item.Price}";
                    order_name.Text = item.ItemName;
                }
                mAdapter.NotifyDataSetChanged(); 
                
            }
            catch (HttpRequestException e)
            {
                Message(e.Message);
            }
        }

        private async void ProcessOrderAsync()
        {
            HttpClient client = new HttpClient();

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(order);
            HttpContent data = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PutAsync($"{API.Url}/orders/process/{order.Id}", data);

                if (response.IsSuccessStatusCode)
                {
                    string str_out = await response.Content.ReadAsStringAsync();
                    //SuccessMessage(str_out);
                    if(str_out == "Ready")
                    {

                    }
                }
                else
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    Message(str_results);
                }
            }
            catch (HttpRequestException ex)
            {
                Message(ex.Message);
            }
        }

        private void Message(string str_results)
        {
            AndHUD.Shared.ShowError(context, str_results, MaskType.None, TimeSpan.FromSeconds(3));
        }
    }
}