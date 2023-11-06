using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Facebook.Shimmer;
using Firebase.Firestore.Auth;
using KotaPalace.Adapters;
using KotaPalace.Dialogs;
using KotaPalace.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace KotaPalace.Fragments
{
    public class OrderTabFragment : Fragment
    {
        private Context context;
        private RecyclerView orders_rv;
        private readonly List<Order> OrderList = new List<Order>();

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }
        public event EventHandler<UpdatePrepare> UpdatePrepareHandler;
        public class UpdatePrepare : EventArgs
        {
            public Order Order { get; set; }
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.prepare_orders_fragment_tab, container, false);
            context = view.Context;
            Init(view);
            LoadOrdersAsync();
            
            return view;
        }
       // readonly HubConnection hubConnection;
        private void Init(View view)
        {
            orders_rv = view.FindViewById<RecyclerView>(Resource.Id.orders_rv);
        }
        OrderAdapter mAdapter;

        public OrderTabFragment() { 
            
        }

        private async void LoadOrdersAsync()
        {
            var businessId = Preferences.Get("businessId", 0);
            try
            {

                HttpClient client = new HttpClient();
                var response = await client.GetAsync($"{API.Url}/orders/{businessId}"); 

                RecyclerView.LayoutManager mLayoutManager = new LinearLayoutManager(context);
                orders_rv.SetLayoutManager(mLayoutManager);
                mAdapter = new OrderAdapter(OrderList);
                mAdapter.ItemClick += MAdapter_ItemClick;

                orders_rv.HasFixedSize = true;
                orders_rv.SetAdapter(mAdapter);

                if (response.IsSuccessStatusCode)
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    var results = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(str_results);

                    foreach (var item in results)
                    {
                        OrderList.Add(item);
                    }
                    mAdapter.NotifyDataSetChanged();
                }
                else
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    Message(str_results);
                }


                
            }
            catch (Exception ex)
            {
                Message(ex.Message);
            }
            finally
            {
                //InitiateFirestoreListener(businessId);
                LodadSignalR(businessId);
            }
        }

        private void MAdapter_ItemClick(object sender, OrderAdapterClickEventArgs e)
        {
           // PrepareOrdersFragmentTab prepareOrdersFragmentTab = new PrepareOrdersFragmentTab();
            
            OrderDetailsDialog order = new OrderDetailsDialog(OrderList[e.Position]);
            order.Show(ChildFragmentManager.BeginTransaction(), "");
            order.UpdateOrderHandler += (s, ev) =>
            {
                for (int i = 0; i < OrderList.Count; i++)
                {
                    if (OrderList[i].Id == ev.Id)
                    {
                        // prepareOrdersFragmentTab.OrderList.Add(OrderList[i]);
                        UpdatePrepareHandler?.Invoke(this, new UpdatePrepare() { Order = OrderList[i] });
                        OrderList.RemoveAt(i);
                        mAdapter.NotifyDataSetChanged();
                        order.Dismiss();
                        break;
                    }
                }
            };
        }

       

        private void MAdapter_ViewClick(object sender, OrderAdapterClickEventArgs e)
        {
            
        }

        public async void LodadSignalR(int businessId)
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl("https://kotapalaceadmin.azurewebsites.net/OrderHub")
                .Build();
            hubConnection.On<string>(businessId.ToString(), (item) =>
            {
                //create a thread
                try
                {
                    
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var order_item = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(item);
                        OrderList.Add(order_item);
                        mAdapter.NotifyDataSetChanged();
                        Toast.MakeText(context, $"New Order {item}", ToastLength.Long).Show();
                    });

                    //Toast.MakeText(context, $"New Order {item}", ToastLength.Long).Show();
                    /*RunOnUiThread(() =>
                    {
                        OrderList.Add(order_item);
                        mAdapter.NotifyDataSetChanged();
                        Toast.MakeText(context, $"New Order {item}", ToastLength.Long).Show();
                    });*/

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Log: " + ex.Message);
                }
            });

            await hubConnection.StartAsync();
            if (hubConnection.State == HubConnectionState.Connected)
            {
                Toast.MakeText(context, "CONNECTED", ToastLength.Long).Show();
            }
            
        }
        private void Message(string str_results)
        {
            AndHUD.Shared.ShowError(context, str_results, MaskType.None, TimeSpan.FromSeconds(3));
        }
    }
    public class OrderHelper
    {
        [Id]
        public string Id { get; set; }
        [MapTo("order_id")]
        public int OrderId { get; set; }
        [MapTo("business_id")]
        public int BusinessId { get; set; }
    }
}