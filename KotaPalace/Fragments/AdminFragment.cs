using Android.OS;
using Android.Views;
using AndroidHUD;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using KotaPalace.Adapters;
using KotaPalace.Dialogs;
using KotaPalace.Models;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using Context = Android.Content.Context;

namespace KotaPalace.Fragments
{
    public class AdminFragment : Fragment
    {
        private Context context;
        private RecyclerView usersRecyclerView;
        private MaterialButton BtnAddDriver;

        //string Id = Preferences.Get("Id", null);

        public AdminFragment()
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.fragment_admin, container, false);
            context = view.Context;
            Init(view);
            GetUsersAsync();

            return view;
        }

        private void Init(View view)
        {
            BtnAddDriver = view.FindViewById<MaterialButton>(Resource.Id.btn_add_driver);
            usersRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.usersRecyclerView);

            BtnAddDriver.Click += (s, e) =>
            {
                AddUserDialogFragment add = new AddUserDialogFragment();
                add.Show(ChildFragmentManager.BeginTransaction(), "");
            };
        }

        private async void GetUsersAsync()
        {
            try
            {
                //var businessId = Preferences.Get("businessId", 0);

                HttpClient client = new HttpClient();
                var response = await client.GetAsync($"{API.Url}/account/drivers");

                if (response.IsSuccessStatusCode)
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    var results = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<AppUsers>>(str_results);

                    ObservableCollection<AppUsers> UserList = new ObservableCollection<AppUsers>();
                    UsersAdapter mAdapter = new UsersAdapter(UserList);
                    mAdapter.BtnClick += (s, e) =>
                    {
                        UpdateUserDialogFragment updateUser = new UpdateUserDialogFragment(UserList[e.Position]);
                        updateUser.Show(ChildFragmentManager.BeginTransaction(), "");
                    };
                    RecyclerView.LayoutManager mLayoutManager = new LinearLayoutManager(context);
                    usersRecyclerView.SetLayoutManager(mLayoutManager);

                    usersRecyclerView.HasFixedSize = true;
                    usersRecyclerView.SetAdapter(mAdapter);

                    foreach (var item in results)
                    {
                        UserList.Add(item);
                    }
                    mAdapter.NotifyDataSetChanged();
                }
                else
                {
                    var str_results = await response.Content.ReadAsStringAsync();
                    Message(str_results);
                }
            }
            catch(HttpRequestException ex)
            {
                Message(ex.Message);
            }
        }
        private void Message(string message)
        {
            AndHUD.Shared.ShowError(Context, message, MaskType.None, TimeSpan.FromSeconds(3));
        }
    }
}