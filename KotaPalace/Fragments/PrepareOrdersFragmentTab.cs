using Android.OS;
using Android.Views;
using AndroidHUD;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Facebook.Shimmer;
using KotaPalace.Adapters;
using KotaPalace.Dialogs;
using KotaPalace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xamarin.Essentials;
using Context = Android.Content.Context;

namespace KotaPalace.Fragments
{
    public class PrepareOrdersFragmentTab : Fragment
    {
        private Context mContext;
        //private ShimmerFrameLayout container;
        private RecyclerView ready_orders_recyclerView;

        public List<Order> OrderList = new List<Order>();

        private readonly int businessId = Preferences.Get("businessId", 0);
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here

        }
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.ready_orders_fragment_tab, container, false);
            mContext = view.Context;
            Init(view);
            LoadReadyOrdersAsync();
            //container.StartLayoutAnimation();
            return view;
        }
        
        private void Init(View view)
        {
            //container = view.FindViewById<ShimmerFrameLayout>(Resource.Id.container);
            ready_orders_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.ready_orders_recyclerView);
        }
        private readonly OrderProcessAdapter mAdapter;

        public PrepareOrdersFragmentTab(OrderTabFragment orderTabFragment)
        {
            orderTabFragment.UpdatePrepareHandler += OrderTabFragment_UpdatePrepareHandler;
            mAdapter = new OrderProcessAdapter(OrderList);
            
        }

        private void OrderTabFragment_UpdatePrepareHandler(object sender, OrderTabFragment.UpdatePrepare e)
        {
            e.Order.Status = "Accepted";
            OrderList.Add(e.Order);
            mAdapter.NotifyDataSetChanged();
        }
        public event EventHandler<UpdateCompleteTab> UpdateCompleteTabHandler;
        public class UpdateCompleteTab : EventArgs
        {
            public Order Order { get; set; }
        }
        private async void LoadReadyOrdersAsync()
        {
            var Id = Preferences.Get("Id", null);
            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync($"{API.Url}/orders/prepare/{businessId}");

                RecyclerView.LayoutManager mLayoutManager = new LinearLayoutManager(mContext);
                ready_orders_recyclerView.SetLayoutManager(mLayoutManager);
                //mAdapter = new OrderProcessAdapter(OrderList);

                ready_orders_recyclerView.HasFixedSize = true;
                ready_orders_recyclerView.SetAdapter(mAdapter);
                mAdapter.ItemClick += MAdapter_ItemClick; 

                if (response.IsSuccessStatusCode)
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                     var results = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(str_results);
                    /*OrderList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(str_results);
                    mAdapter.NotifyDataSetChanged();*/
                    foreach (var item in results)
                    {
                        var _order = OrderList.Where(x=>x.Id ==  item.Id).FirstOrDefault();
                        if(_order == null)
                        {
                            OrderList.Add(item);
                            mAdapter.NotifyDataSetChanged();
                        }
                    }
                }
                else
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    Message(str_results);
                }

               // container.StopShimmer();

            }
            catch (Exception ex)
            {
                Message(ex.Message);
            }
        }

        private void MAdapter_ItemClick(object sender, CompleteAdapterClickEventArgs e)
        {
            OrderDetailsDialog order = new OrderDetailsDialog(OrderList[e.Position]);
            order.Show(ChildFragmentManager.BeginTransaction(), "");
            order.UpdateOrderHandler += (s, ev) =>
            {
                for (int i = 0; i < OrderList.Count; i++)
                {
                    if (OrderList[i].Id == ev.Id)
                    {
                        UpdateCompleteTabHandler?.Invoke(this, new UpdateCompleteTab() { Order = OrderList[i] });
                        OrderList.RemoveAt(i);
                        mAdapter.NotifyDataSetChanged();
                        order.Dismiss();
                        break;
                    }
                }
            };
        }

        private void Message(string message)
        {
            AndHUD.Shared.ShowError(mContext, message, MaskType.None, TimeSpan.FromSeconds(3));
        }
    }
}