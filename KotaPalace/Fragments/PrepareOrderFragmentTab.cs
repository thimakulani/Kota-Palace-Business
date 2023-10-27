using Android.Content;
using Android.OS;
using Android.Views;
using AndroidHUD;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Facebook.Shimmer;
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
    public class PrepareOrderFragmentTab : Fragment
    {
        private Context context;
        private RecyclerView orders_rv;

        List<Order> OrderList = new List<Order>();

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.prepare_orders_fragment_tab, container, false);
            context = view.Context;
            Init(view);
            LoadOrdersAsync();
            hubConnection = new HubConnectionBuilder().WithUrl("https://kotapalaceadmin.azurewebsites.net/OrderHub").Build();
            hubConnection.On<Order>("Order", (response) =>
            {
                OrderList.Add(response);
                mAdapter.NotifyDataSetChanged();
            });
            hubConnection.StartAsync();
            if (hubConnection.State == HubConnectionState.Connected)
            {
                Console.WriteLine("Connected to the hub.");
                // You can now interact with the hub.
            }
            hubConnection.Reconnected += HubConnection_Reconnected;
            return view;
        }
        HubConnection hubConnection;
        private void Init(View view)
        {
            orders_rv = view.FindViewById<RecyclerView>(Resource.Id.orders_rv);
        }
        PrepareOrderAdapter mAdapter;
        private void InitiateFirestoreListener(int business_id)
        {
            CrossCloudFirestore
                .Current
                .Instance
                .Collection("Order")
                .WhereEqualsTo("business_id", business_id)
                .AddSnapshotListener((data, error) =>
                {
                    foreach (var item in data.DocumentChanges)
                    {
                        OrderHelper orderHelper = item.Document.ToObject<OrderHelper>();


                        switch (item.Type)
                        {
                            case DocumentChangeType.Added:
                                var order = OrderList.Where(x=>x.Id == orderHelper.OrderId).FirstOrDefault();
                                 if(order == null)
                                {
                                    //get data from api
                                    GetOrder(orderHelper.OrderId);
                                }
                                break;
                            case DocumentChangeType.Modified:
                                break;
                            case DocumentChangeType.Removed:
                                break;
                        }
                    }
                });
        }
        private async void GetOrder(int id)
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync($"{API.Url}/orders/single/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var str_content = await response.Content.ReadAsStringAsync();   
                    var order = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(str_content);
                    OrderList.Add(order);
                    mAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception ex)
            {
            }
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
                mAdapter = new PrepareOrderAdapter(OrderList);
                mAdapter.BtnClick += (s, e) =>
                {
                    OrderDetailsDialog order = new OrderDetailsDialog(OrderList[e.Position]);
                    order.Show(ChildFragmentManager.BeginTransaction(), "");
                };

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
                InitiateFirestoreListener(businessId);
            }
        }
        public void LodadSignalR(int businessId)
        {
           
        }

        private System.Threading.Tasks.Task HubConnection_Reconnected(string arg)
        {
            Console.WriteLine($"{arg}");
            return Task.CompletedTask;
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