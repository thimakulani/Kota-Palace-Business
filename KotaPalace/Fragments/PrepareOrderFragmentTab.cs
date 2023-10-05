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
using System;
using System.Collections.Generic;
using System.Net.Http;
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
            
            return view;
        }

        private void Init(View view)
        {
            orders_rv = view.FindViewById<RecyclerView>(Resource.Id.orders_rv);
        }

        private async void LoadOrdersAsync()
        {
            var businessId = Preferences.Get("businessId", 0);
            try
            {

                HttpClient client = new HttpClient();
                var response = await client.GetAsync($"{API.Url}/orders/{businessId}"); // car details

                RecyclerView.LayoutManager mLayoutManager = new LinearLayoutManager(context);
                orders_rv.SetLayoutManager(mLayoutManager);
                PrepareOrderAdapter mAdapter = new PrepareOrderAdapter(OrderList);
                mAdapter.BtnClick += (s, e) =>
                {
                    OrderViewFragment order = new OrderViewFragment(OrderList[e.Position]);
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


                //Task startWork = new Task(() =>
                //{
                //    Task.Delay(3000);
                //});
                //startWork.ContinueWith(t =>
                //{
                //    try
                //    {
                //        shimmerFrameLayout1.StopShimmer();
                //        shimmerFrameLayout1.Visibility = ViewStates.Gone;
                //    }
                //    catch (Exception ex)
                //    {

                //        Toast.MakeText(context, ex.Message, ToastLength.Long).Show();
                //    }
                //}, TaskScheduler.FromCurrentSynchronizationContext());
                //startWork.Start();
            }
            catch (Exception ex)
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