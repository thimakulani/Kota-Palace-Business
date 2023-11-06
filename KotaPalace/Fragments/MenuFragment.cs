using Android.Content;
using Android.OS;
using Android.Views;
using AndroidHUD;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Button;
using Google.Android.Material.FloatingActionButton;
using KotaPalace.Adapters;
using KotaPalace.Dialogs;
using KotaPalace.Models;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using Xamarin.Essentials;
using Menu = KotaPalace.Models.Menu;

namespace KotaPalace.Fragments
{
    public class MenuFragment : Fragment
    {
        private Context context;
        private FloatingActionButton BtnAddMenu;
        private RecyclerView menu_rv;
        public MenuFragment()
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
            View rootView = inflater.Inflate(Resource.Layout.fragment_menu, container, false);

            context = rootView.Context;

            Init(rootView);
            AddMenu();
            LoadMenusAsync();

            return rootView;
        }

        private void Init(View view)
        {
            BtnAddMenu = view.FindViewById<FloatingActionButton>(Resource.Id.BtnAddMenu);
            menu_rv = view.FindViewById<RecyclerView>(Resource.Id.menu_rv);
            //
        }

        private void AddMenu()
        {
            BtnAddMenu.Click += (s, e) =>
            {
                var dlg = new AddMenuDialogFragment();
                dlg.Show(ChildFragmentManager.BeginTransaction(), "");
                dlg.MenuHandler += (ss, ee) =>
                {
                    MenuList.Add(ee.mMenu);
                    adapter.NotifyDataSetChanged();
                };


            };
        }
        MenuAdapter adapter;
        ObservableCollection<Menu> MenuList = new ObservableCollection<Menu>();
        private async void LoadMenusAsync()
        {
            var businessId = Preferences.Get("businessId", 0);
            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"{API.Url}/menus/all/{businessId}"); // car details

            if (response.IsSuccessStatusCode)
            {
                var str_results = await response.Content.ReadAsStringAsync();
                var results = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<Menu>>(str_results);

                
                adapter = new MenuAdapter(MenuList);
                adapter.BtnClick += (s, e) =>
                {
                    PopupMenu popup = new PopupMenu(context, e.ImgBtn);
                    popup.Menu.Add(IMenu.None, 0, 0, "EDIT");
                    popup.Menu.Add(IMenu.None, 1, 0, "REMOVE");
                    popup.Show();
                    popup.MenuItemClick += (ss, ee) =>
                    {
                        if(ee.Item.ItemId == 0)
                        {
                            UpdateMenuDialogFragment updateMenuDialogFragment = new UpdateMenuDialogFragment(MenuList[e.Pos]);
                            updateMenuDialogFragment.Show(ChildFragmentManager.BeginTransaction(), "");
                            updateMenuDialogFragment.UpdateMenuHandler += (x, y) =>
                            {
                                MenuList[e.Pos] = y.Menu;
                                adapter.NotifyDataSetChanged();
                            };
                        }
                        if (ee.Item.ItemId == 1)
                        {

                            AlertDialog.Builder builder = new AlertDialog.Builder(context);
                            builder.SetTitle("CONFIRM");
                            builder.SetMessage("ARE YOU SURE YOU WANT TO REMOVE THIS ITEM FROM THE MENU");
                            builder.SetPositiveButton("YES", async delegate
                            {
                                try
                                {
                                    int m_id = MenuList[e.Pos].Id;
                                    HttpClient httpClient = new HttpClient();
                                    //var response = await httpClient.DeleteAsync($"{API.Url}/");
                                    var response = await httpClient.DeleteAsync($"{API.Url}/menus/{m_id}");
                                    if (response.IsSuccessStatusCode)
                                    {
                                        var str_results = await response.Content.ReadAsStringAsync();
                                        AndHUD.Shared.ShowSuccess(context, $"{str_results}", MaskType.Clear, TimeSpan.FromSeconds(3));
                                        MenuList.RemoveAt(e.Pos);
                                        adapter.NotifyItemRemoved(e.Pos);

                                    }
                                    else
                                    {
                                        var str_results = await response.Content.ReadAsStringAsync();
                                        AndHUD.Shared.ShowError(context, $"{str_results}".ToUpper(), MaskType.Clear, TimeSpan.FromSeconds(3));

                                    }
                                }
                                catch (Exception ex)
                                {
                                    AndHUD.Shared.ShowError(context, $"Something went wrong \n {ex.Message}".ToUpper(), MaskType.Clear, TimeSpan.FromSeconds(3));

                                }
                            });
                            builder.SetNegativeButton("NO", delegate
                            {
                                builder.Dispose();
                            });
                            builder.Show();
                        }
                    };
                    /*UpdateMenuDialogFragment fragment = new UpdateMenuDialogFragment(MenuList[e.Pos]);
                    fragment.Show(ChildFragmentManager.BeginTransaction(), "");*/
                };
                RecyclerView.LayoutManager mLayoutManager = new LinearLayoutManager(context);
                menu_rv.SetLayoutManager(mLayoutManager);

                menu_rv.HasFixedSize = true;
                menu_rv.SetAdapter(adapter);

                foreach (var item in results)
                {
                    MenuList.Add(item);
                }
                adapter.NotifyDataSetChanged();
            }
            else
            {
                var str_results = await response.Content.ReadAsStringAsync();
                Message(str_results);
            }
        }

        private void Message(string t)
        {
            AndHUD.Shared.ShowError(context, t, MaskType.None, TimeSpan.FromSeconds(3));
        }
    }
}