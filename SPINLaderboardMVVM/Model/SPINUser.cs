using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SPINLaderboardMVVM.Model
{
    [Serializable]
    public class SPINUser : INotifyPropertyChanged
    {
        int rank;
        string name;
        double score;
        DateTime lastchange;
        public ObservableCollection<DateTime> AllChanges { get; }

        public int Rank
        {
            get
            {
                return rank;
            }
            set
            {
                rank = value;
                OnPropertyChanged("Rank");
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public double Score
        {
            get
            {
                return score;
            }
            set
            {
                score = value;
                OnPropertyChanged("Score");
            }
        }

        public DateTime LastChange
        {
            get
            {
                return lastchange;
            }
            set
            {
                AllChanges.Add(lastchange);
                lastchange = value;
                OnPropertyChanged("LastChange");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public SPINUser()
        {
            AllChanges = new ObservableCollection<DateTime>();
        }

        public SPINUser(int rank, string name, double score, DateTime lastChange)
        {
            AllChanges = new ObservableCollection<DateTime>();
            Rank = rank;
            Name = name;
            Score = score;
            LastChange = lastChange;
        }
    }
}
