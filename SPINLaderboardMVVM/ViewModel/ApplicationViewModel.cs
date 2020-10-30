using SPINLaderboardMVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Helpers;
using System.Windows.Data;

namespace SPINLaderboardMVVM.ViewModel
{
    public class ApplicationViewModel
    {
        readonly static WebClient wc = new WebClient();
        readonly TimerCallback tm = new TimerCallback(DownloadString);
        readonly Timer timer;
        bool isdata = true;

        string filter = "";
        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
            }
        }

        public ObservableCollection<SPINUser> SPINUsers { get; set; }
        public ICollectionView SPINUsersView { get; private set; }

        public ApplicationViewModel()
        {
            try
            {
                using (StreamReader sr = new StreamReader("UsersData.dat"))
                {
                    SPINUsers = SerializeHelper.Desirialize<ObservableCollection<SPINUser>>(sr.ReadToEnd());
                }
            }
            catch (Exception)
            {
                SPINUsers = new ObservableCollection<SPINUser>();
            }

            SPINUsersView = CollectionViewSource.GetDefaultView(SPINUsers);
            SPINUsersView.Filter = OnFilterTriggered;

            wc.DownloadStringCompleted += Wc_DownloadStringCompleted;

            timer = new Timer(tm, 0, 0, 10000);
        }

        private bool OnFilterTriggered(object item)
        {
            if (item is SPINUser user)
            {
                if (isdata)
                {
                    return (DateTime.UtcNow - user.LastChange.ToUniversalTime()).TotalMinutes <= 5;
                }
                else
                {
                    if (Filter == "") return true;
                    else
                    {
                        return Filter.ToLower().Contains(user.Name.ToLower());
                    }
                }
            }
            else return true;
        }

        public void FilterData()
        {
            CollectionViewSource.GetDefaultView(SPINUsers).Refresh();
        }

        static void DownloadString(object obj)
        {
            try
            {
                wc.DownloadStringAsync(new Uri("https://www.partypoker529.com/en/mobilepoker/api/leaderboard/GetLeaderboardPosFeedData?contentParameters=%7B%22buttonTitle1%22:%22Micro%22,%22buttonTitle2%22:%22Small%22,%22buttonTitle3%22:%22Low%22,%22buttonTitle4%22:%22Mid%22,%22buttonTitle5%22:%22High%22,%22maxRank%22:%221000%22,%22promoId1%22:%22939456%22,%22promoId2%22:%22939458%22,%22promoId3%22:%22939459%22,%22promoId4%22:%22939461%22,%22promoId5%22:%22939462%22%7D&buttonParameters=%5B%7B%22buttonTitle%22:%22Micro%22,%22promoId%22:%22939456%22%7D,%7B%22buttonTitle%22:%22Small%22,%22promoId%22:%22939458%22%7D,%7B%22buttonTitle%22:%22Low%22,%22promoId%22:%22939459%22%7D,%7B%22buttonTitle%22:%22Mid%22,%22promoId%22:%22939461%22%7D,%7B%22buttonTitle%22:%22High%22,%22promoId%22:%22939462%22%7D%5D"));
            }
            catch (Exception ex)
            {
                using (StreamWriter sw = new StreamWriter("errors.txt"))
                {
                    sw.WriteLine($"Ошибка: {ex.Message}|Дата: {DateTime.UtcNow:G}");
                }
            }
        }

        void Wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            GetAllUsersData(Json.Decode(e.Result));
        }

        void GetAllUsersData(dynamic data)
        {
            foreach (var webuser in data["939459"].details)
            {
                var newscore = (double)webuser.points;
                var user = SPINUsers.Where(x => x.Name == webuser.screenName).FirstOrDefault();
                if (user != null)
                {
                    if (user.Score != newscore)
                    {
                        user.Score = newscore;
                        user.LastChange = DateTime.UtcNow;
                    }
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(delegate
                    {
                        SPINUsers.Add(new SPINUser(webuser.rank, webuser.screenName, newscore, DateTime.UtcNow));
                    });
                }
            }
            using (StreamWriter sw = new StreamWriter("UsersData.dat"))
            {
                sw.Write(SerializeHelper.Serialize(SPINUsers));
            }
        }
    }
}
