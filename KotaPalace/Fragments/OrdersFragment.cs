using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Facebook.Shimmer;
using Google.Android.Material.MaterialSwitch;
using Google.Android.Material.Tabs;
using KotaPalace.Models;
using Microsoft.AspNet.SignalR.Client;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Essentials;

namespace KotaPalace.Fragments
{
    public class OrdersFragment : Fragment
    {
        private ShimmerFrameLayout shimmer_container;
        private TabLayout tabHost;
        private ViewPager2 viewpager;

        private readonly int[] tabIcons = { Resource.Drawable.ic_order, Resource.Drawable.ic_restaurant_menu };
        private HubConnection hubConnection;
        public OrdersFragment()
        {
            
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_orders, container, false);

            Init(view);
            SetViewPager();

            return view;
        }
        private MaterialSwitch status;
        private void Init(View view)
        {
            tabHost = view.FindViewById<TabLayout>(Resource.Id.TabHost);
            viewpager = view.FindViewById<ViewPager2>(Resource.Id.viewpager);
            status = view.FindViewById<MaterialSwitch>(Resource.Id.status);
            status.Click += Status_Click;
        }

        private async void Status_Click(object sender, System.EventArgs e)
        {
            var businessId = Preferences.Get("businessId", 0);
            try
            {
                HttpClient httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"{API.Url}/businesses/online/{businessId}");
                if(response.IsSuccessStatusCode)
                {
                    var str = await response.Content.ReadAsStringAsync();
                    var bus = Newtonsoft.Json.JsonConvert.DeserializeObject<Business>(str);
                    if(bus.Online == "ONLINE")
                    {
                        status.TextOn = bus.Online;
                    }
                    else
                    {
                        status.TextOn = bus.Online;
                    }
                }
            }
            catch (System.Exception)
            {

            }
        }

        private void SetViewPager()
        {
            var adapter = new Adapter(this);
            adapter.AddFragment(new PrepareOrderFragmentTab(), "ORDERS");
            adapter.AddFragment(new ReadyOrdersFragmentTab(), "READY");

            viewpager.Adapter = adapter;

            // Pass tabIcons array to TabConfigurationStrategy
            new TabLayoutMediator(tabHost, viewpager, new TabConfigurationStrategy(adapter, tabIcons)).Attach();
        }
    }

    class Adapter : FragmentStateAdapter
    {
        private readonly List<Fragment> fragments = new List<Fragment>();
        private readonly List<string> fragmentTitles = new List<string>();

        public Adapter(Fragment fragment) : base(fragment)
        {
        }

        public void AddFragment(Fragment fragment, string title)
        {
            fragments.Add(fragment);
            fragmentTitles.Add(title);
        }

        public override int ItemCount => fragments.Count;

        public override Fragment CreateFragment(int position)
        {
            return fragments[position];
        }

        public string GetPageTitle(int position)
        {
            return fragmentTitles[position];
        }
    }

    class TabConfigurationStrategy : Java.Lang.Object, TabLayoutMediator.ITabConfigurationStrategy
    {
        private readonly Adapter adapter;
        private readonly int[] tabIcons;

        public TabConfigurationStrategy(Adapter adapter, int[] tabIcons)
        {
            this.adapter = adapter;
            this.tabIcons = tabIcons;
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            tab.SetText(adapter.GetPageTitle(position));
            tab.SetIcon(tabIcons[position]);
        }
    }
}
