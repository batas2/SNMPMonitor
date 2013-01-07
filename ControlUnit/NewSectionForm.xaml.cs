using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using ControlUnit.Models;

namespace ControlUnit
{
    public partial class NewSectionForm : Window
    {
        private readonly NewOpinion _newOpinion;
        private ObservableCollection<SyndromModel> _syndromList;

        public NewSectionForm()
        {
            InitializeComponent();

            _newOpinion = new NewOpinion();
            _newOpinion.GlobalOpinionAdded += _newOpinion_GlobalOpinionAdded;

            _syndromList = new ObservableCollection<SyndromModel>();
            OpinionListView.ItemsSource = _syndromList;
        }

        public ObservableCollection<TestModel> SectionList { get; private set; }

        public IEnumerable<SyndromModel> SyndromList
        {
            get { return _syndromList; }
        }

        private void _newOpinion_GlobalOpinionAdded(object sender, EventArgs e)
        {
            _syndromList = _newOpinion.SyndromList;
            OpinionListView.ItemsSource = _syndromList;
            SaveButton.IsEnabled = true;
            OpinionListView.UpdateLayout();
        }

        public event EventHandler SectionAdded;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearData();
            Hide();
        }

        private bool valiIP(string v)
        {
            var r = new IPAddress(0);
            return IPAddress.TryParse(v, out r);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var ipDestVal = IpDestControl.Text.Replace(',', '.').Replace("_", "");
            var ipSrcVal = IpSrcControl.Text.Replace(',', '.').Replace("_", "");
            var masterAgentIpVal = MasterAgentIpControl.Text.Replace(',', '.').Replace("_", "");

            if (!valiIP(ipSrcVal) || !valiIP(ipDestVal) || !valiIP(masterAgentIpVal))
            {
                MessageBox.Show("Błędnie wpisany adres IP.", "Błąd IP.", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            var testCount = (from t in SectionList
                             where t.IpDest == ipDestVal && t.IpSrc == ipSrcVal
                             select t).Count();
            if (testCount != 0) return;

            var testModel = new TestModel
                {
                    ControlUnitIp = IpList.SelectedValue.ToString(),
                    TestId = SectionList.Count,
                    IpSrc = ipSrcVal,
                    IpDest = ipDestVal,
                    MasterAgentIp = masterAgentIpVal
                };

            SectionList.Add(testModel);

            IpSrcControl.Text = "";
            IpDestControl.Text = "";
            MasterAgentIpControl.IsEnabled = false;
            AddGlobalOpinionBtn.IsEnabled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList,
                                                      a => a.AddressFamily == AddressFamily.InterNetwork);
            IpList.ItemsSource = ipv4Addresses;

            IpList.SelectedIndex = 0;

            ClearData();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (SectionAdded != null)
                SectionAdded.Invoke(this, new EventArgs());

            ClearData();
            Hide();
        }

        private void ClearData()
        {
            SectionList = new ObservableCollection<TestModel>();
            TestListControl.ItemsSource = SectionList;
            _newOpinion.SyndromList.Clear();

            if (MasterAgentIpControl != null)
            {
                MasterAgentIpControl.IsEnabled = true;
                MasterAgentIpControl.Value = "";
            }

            if (IpSrcControl != null)
            {
                IpSrcControl.IsEnabled = false;
                IpSrcControl.Value = "";
            }

            if (IpDestControl != null)
            {
                IpDestControl.IsEnabled = false;
                IpDestControl.Value = "";
            }

            if (AddButton != null)
                AddButton.IsEnabled = false;

            if (SaveButton != null)
                SaveButton.IsEnabled = false;
        }

        private void SectionIpControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IpSrcControl != null)
                IpSrcControl.IsEnabled = true;
            if (IpSrcControl != null)
                IpDestControl.IsEnabled = true;
            if (AddButton != null)
                AddButton.IsEnabled = true;
        }

        private void AddGlobalOpinionBtn_Click(object sender, RoutedEventArgs e)
        {
            _newOpinion.AssignMasterAgentIp(MasterAgentIpControl.Text.Replace(',', '.').Replace("_", ""));
            var hosts = (from s in SectionList
                         select s.IpDest).Distinct().ToList();

            _newOpinion.AssignHostsList(hosts, SectionList.Count);
            _newOpinion.ShowDialog();
        }
    }
}