
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using System;
using System.Collections.Generic;
using Microcharts.Droid;
using Facebook.Shimmer;
using Microcharts;
using SkiaSharp;
using System.Net.Http;
using KotaPalace.Models;
using Xamarin.Essentials;
using AndroidHUD;
using OperationCanceledException = System.OperationCanceledException;
using Google.Android.Material.Button;
using System.Globalization;

namespace KotaPalace.Fragments
{
    public class ReportFragment : Fragment
    {
        private Context context;

        private MaterialButton btn_chart_type;
        private ShimmerFrameLayout container;
        private ChartView chartReport;

        private readonly List<string> months = new List<string>();
        private readonly List<int> counter = new List<int>();

        private readonly int businessId = Preferences.Get("businessId", 0);

        public ReportFragment()
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
            View view = inflater.Inflate(Resource.Layout.fragment_report, container, false);
            context = view.Context;
            Init(view);
            LoadGraphsAsync();

            return view;
        }

        private void Init(View view)
        {
            btn_chart_type = view.FindViewById<MaterialButton>(Resource.Id.btn_chart_type);
            //btn_year = view.FindViewById<MaterialButton>(Resource.Id.btn_year);
            container = view.FindViewById<ShimmerFrameLayout>(Resource.Id.shimmer_view_container);
            
            chartReport = view.FindViewById<ChartView>(Resource.Id.chartReport);

            btn_chart_type.Click += (s, e) =>
            {
                PopupMenu popupMenu = new PopupMenu(context, btn_chart_type);
                popupMenu.Menu.Add(IMenu.First, 0, 1, "Bar Chart");
                popupMenu.Menu.Add(IMenu.First, 1, 1, "Line Chart");
                popupMenu.Menu.Add(IMenu.First, 2, 1, "Point Chart");
                popupMenu.Menu.Add(IMenu.First, 3, 1, "Donut Chart");
                popupMenu.Menu.Add(IMenu.First, 4, 1, "Radar Chart");
                popupMenu.Show();
                popupMenu.MenuItemClick += PopupMenu_MenuItemClick;
            };
        }

        private void PopupMenu_MenuItemClick(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            type = e.Item.ItemId;
            //btn_year.Text = e.Item.TitleFormatted.ToString();
            DrawCharts();
        }

        private int type = 0;
        private async void LoadGraphsAsync()
        {
            container.StartShimmer(); // If auto-start is set to false

            string[] monthNames = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            foreach (var m in monthNames)
            {
                months.Add(m);
                counter.Add(0);
            }

            HttpClient client = new HttpClient();
            try
            {
                var response = await client.GetAsync($"{API.Url}/orders/{businessId}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var order = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(data);

                    if (order != null)
                    {
                        foreach (var item in order)
                        {

                            

                            CultureInfo culture = new CultureInfo("en-US");
                            var date = DateTime.Parse(item.OrderDateUtc.ToString("ddd, dd MMM yyyy"));

                            if (months.Contains(item.OrderDateUtc.ToString("MMM")))
                            {
                                int pos = months.IndexOf(item.OrderDateUtc.ToString("MMM"));
                                counter[pos] = counter[pos] + 1;
                            }
                        }
                        DrawCharts();
                    }
                }
                else
                {
                    var data = await response.Content.ReadAsStringAsync();
                    Message(data);
                }
            }
            catch (HttpRequestException ex)
            {
                Message(ex.Message);
            }
        }

        private void DrawCharts()
        {
            List<ChartEntry> DataEntry = new List<ChartEntry>();
            string[] colors = { "#157979", "#154779", "#5F1C80", "#801C59",
                            "#9CBDD6", "#75863D" , "#1E1011", "#48D53B",
                            "#48D5C7", "#6761F0", "#8A80A3", "#D3C6F4"
            };

            try
            {
                for (int i = 0; i < months.Count; i++)
                {
                    DataEntry.Add(new ChartEntry(counter[i])
                    {
                        Label = months[i],
                        Color = SKColor.Parse(colors[i]),
                        ValueLabel = counter[i].ToString(),
                        TextColor = SKColor.Parse(colors[i]),
                        ValueLabelColor = SKColor.Parse(colors[i])
                    });
                    if (months[i].Contains(DateTime.Now.ToString("MMMM")))
                    {
                        break;
                    }
                }

                if (type == 0)
                {
                    var chart = new BarChart()
                    {
                        Entries = DataEntry,
                    };
                    chartReport.Chart = chart;
                }
                if (type == 1)
                {
                    var chart = new LineChart()
                    {
                        Entries = DataEntry,
                    };
                    chartReport.Chart = chart;
                }
                if (type == 2)
                {
                    var chart = new PointChart()
                    {
                        Entries = DataEntry,
                    };
                    chartReport.Chart = chart;
                }
                if (type == 3)
                {
                    var chart = new DonutChart()
                    {
                        Entries = DataEntry,
                    };
                    chartReport.Chart = chart;
                }
                if (type == 4)
                {
                    var chart = new RadarChart()
                    {
                        Entries = DataEntry,
                    };
                    chartReport.Chart = chart;
                }

                //var chart = new RadarChart()
                //{
                //    Entries = DataEntry,
                //};
                //chartReport.Chart = chart;
            }
            catch (OperationCanceledException ex)
            {
                Message(ex.Message);
            }
        }

        private void Message(string message)
        {
            AndHUD.Shared.ShowError(context, message,MaskType.None,TimeSpan.FromSeconds(3));
        }

    }
}