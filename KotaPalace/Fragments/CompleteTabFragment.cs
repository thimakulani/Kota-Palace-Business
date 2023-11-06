using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog;
using Google.Android.Material.FloatingActionButton;
using KotaPalace.Adapters;
using KotaPalace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using Xamarin.Essentials;

namespace KotaPalace.Fragments
{
    public class CompleteTabFragment : Fragment
    {
        private Context context;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            View rootView = inflater.Inflate(Resource.Layout.complete_tab_fragment, container, false);

            context = rootView.Context;

            Init(rootView);
            

            return rootView;
        }
        RecyclerView RecyclerCompletes;
        private readonly List<Order> Orders = new List<Order>();
        Context mContext;
        private void Init(View view)
        {
            mContext = view.Context;
            RecyclerCompletes = view.FindViewById<RecyclerView>(Resource.Id.RecyclerCompletes);
            //
            LoadReadyOrdersAsync();
        }
        private readonly int businessId = Preferences.Get("businessId", 0);

        public CompleteTabFragment(PrepareOrdersFragmentTab prepare)
        {
            mAdapter = new OrderProcessAdapter(Orders);
            prepare.UpdateCompleteTabHandler += Prepare_UpdateCompleteTabHandler; 
        }

        private readonly OrderProcessAdapter mAdapter;
        private void Prepare_UpdateCompleteTabHandler(object sender, PrepareOrdersFragmentTab.UpdateCompleteTab e)
        {
            Orders.Add(e.Order);
            mAdapter.NotifyDataSetChanged();
        }

        private async void LoadReadyOrdersAsync()
        {
            //var Id = Preferences.Get("Id", null);
            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync($"{API.Url}/orders/completed/{businessId}");

                RecyclerView.LayoutManager mLayoutManager = new LinearLayoutManager(mContext);
                RecyclerCompletes.SetLayoutManager(mLayoutManager);
                
                
                RecyclerCompletes.HasFixedSize = true;
                RecyclerCompletes.SetAdapter(mAdapter); 
                mAdapter.ItemClick += MAdapter_ItemClick;

                if (response.IsSuccessStatusCode)
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    var results = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(str_results);

                    foreach (var item in results)
                    {
                        Orders.Add(item);
                        mAdapter.NotifyDataSetChanged();
                    }
                }
                else
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    //Message(str_results);
                    Toast.MakeText(context, str_results, ToastLength.Long).Show();
                }

               // container.StopShimmer();

            }
            catch (Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Long).Show();
            }
        }

        private void MAdapter_ItemClick(object sender, CompleteAdapterClickEventArgs e)
        {
            MaterialAlertDialogBuilder dialogBuilder = new MaterialAlertDialogBuilder(context);
            dialogBuilder.SetTitle("COMPLETE");
            dialogBuilder.SetMessage($"CONFIRM ORDER COLLECTION FOR {Orders[e.Position].Customer.Firstname}");
            dialogBuilder.SetPositiveButton("OK", (sender, args) =>
            {
                UpdateCollection(Orders[e.Position].Id);
            });
            dialogBuilder.SetNegativeButton("Cancel", (sender, args) =>
            {
                dialogBuilder.Dispose();
                
            });
            dialogBuilder.Show();
                
        }
        private async void UpdateCollection(int id)
        {
            //var Id = Preferences.Get("Id", null);
            try
            {
                HttpClient client = new HttpClient();
                var response = await client.GetAsync($"{API.Url}/orders/collection/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    var results = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(str_results);

                }
                else
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    Toast.MakeText(context, str_results, ToastLength.Short).Show();
                }

                // container.StopShimmer();

            }
            catch (Exception ex)
            {
                //Message(ex.Message);
                Toast.MakeText(context, ex.Message, ToastLength.Long).Show();
            }
        }
    }
}