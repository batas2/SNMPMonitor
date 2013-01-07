using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using ControlUnit.Models;
using log4net;

namespace ControlUnit
{
    /// <summary>
    /// Interaction logic for NewOpinion.xaml
    /// </summary>
    public partial class NewOpinion : Window
    {
        private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<int, string> _hostDict;
        private string _masterAgentIp;

        public NewOpinion()
        {
            InitializeComponent();
            SyndromList = new ObservableCollection<SyndromModel>();
        }

        public ObservableCollection<SyndromModel> SyndromList { get; private set; }
        public event EventHandler GlobalOpinionAdded;
        private int _testCount;

        private bool validSyndrom(string val)
        {
            return val.ToLower().All(c => c == '0' || c == '1' || c == 'x');
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!validSyndrom(SyndromControl.Text))
            {
                MessageBox.Show("Błędnie wpisany syndrom, dozwolone symbole 0, 1, x.", "Błąd walidacji pola Syndrom.",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            if (SyndromControl.Text.Replace("_", "").Length != _testCount)
            {
                MessageBox.Show(String.Format("Błędnie wpisany syndrom, długość powinna wynośić: {0}.", _hostDict.Count), "Błąd walidacji pola Syndrom.",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(HostsControl.Text))
            {
                MessageBox.Show("Proszę wpisać listę ID jednostek niezdatnych.", "Błąd list jednostek.",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            var hosts = HostsControl.Text.Replace("_", "").Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (hosts.Select(host => Convert.ToInt32(host)).Any(hostId => hostId < 0 || hostId >= _hostDict.Count))
            {
                MessageBox.Show(
                    String.Format("Proszę wybrać Id jednostki z zakresu od 0 do {0}. ", _hostDict.Count - 1),
                    "Błąd ID jednostki.", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var hostInput = HostsControl.Text.Replace("_", "");
            var hostsSb = new StringBuilder();
            foreach (string host in hostInput.Split(',').Where(x => !string.IsNullOrEmpty(x)))
            {
                hostsSb.AppendFormat("{0},", _hostDict[Convert.ToInt32(host)]);
            }

            SyndromList.Add(new SyndromModel { Hosts = hostsSb.ToString(), Syndrom = SyndromControl.Text.Replace("_", ""), MasterAgentIp = _masterAgentIp });

            SaveButton.IsEnabled = true;
            HostsControl.Text = "";
            SyndromControl.Text = "";
        }

        public void AssignHostsList(IList<string> hosts, int testCount)
        {
            _testCount = testCount;
            _hostDict = new Dictionary<int, string>();
            for (int i = 0; i < hosts.Count; i++)
            {
                _hostDict.Add(i, hosts[i]);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ClearData();
            SyndromList.Clear();
            var sb = new StringBuilder();
            foreach (var host in _hostDict)
            {
                sb.AppendFormat("{0} - {1}; \t", host.Key, host.Value);
                if (host.Key % 3 == 0 && host.Key > 0)
                {
                    sb.Append("\r\n");
                }
            }
            HostListLabel.Content = sb.ToString();

            var syndromSB = new StringBuilder();
            var unitIdsMask = new StringBuilder();
            var syndromMask = new StringBuilder();
            for (int i = 0; i < _testCount; i++)
            {
                syndromSB.Append("0");
                syndromMask.Append("&");
            }

            for (int i = 0; i < _hostDict.Count; i++)
            {
                unitIdsMask.Append("99.");
            }
            HostsControl.Mask = unitIdsMask.ToString().TrimEnd('.');
            SyndromControl.Mask = syndromMask.ToString().TrimEnd('.');
            OpinionListView.ItemsSource = SyndromList;
            SyndromList.Insert(0, new SyndromModel { Hosts = "[Brak]", Syndrom = syndromSB.ToString() });
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalOpinionAdded != null)
            {
                GlobalOpinionAdded.Invoke(this, new EventArgs());
            }
            Hide();
            ClearData();
        }

        private void ClearData()
        {
            SaveButton.IsEnabled = false;
            HostsControl.Text = "";
            SyndromControl.Text = "";
        }

        public void AssignMasterAgentIp(string Ip)
        {
            _masterAgentIp = Ip;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearData();
            Hide();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Window_Loaded(sender, new RoutedEventArgs());
            }
        }
    }
}