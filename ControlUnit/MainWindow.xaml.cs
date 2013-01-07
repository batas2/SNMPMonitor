using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using ControlUnit.Data;
using ControlUnit.Models;
using log4net;

namespace ControlUnit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DataProvider _dataProvider;

        private readonly DeleteSectionForm _deleteSectionForm;
        private readonly NewSectionForm _newSectionForm;
        private readonly SNMPProvider _snmpProvider;
        private readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        private IList _sectionModel;

        public MainWindow()
        {
            InitializeComponent();

            _newSectionForm = new NewSectionForm();
            _deleteSectionForm = new DeleteSectionForm();

            _dataProvider = new DataProvider();
            _snmpProvider = new SNMPProvider();

            _newSectionForm.SectionAdded += _newSectionForm_SectionAdded;
            _deleteSectionForm.SectionDeleted += _deleteSectionForm_SectionDeleted;
            _snmpProvider.TrapRecived += SnmpProviderTrapRecived;

            _sectionModel = new List<TestModel>();
        }

        private void SnmpProviderTrapRecived(object sender, TestModel e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(RefreshResults));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
                _sectionModel = _dataProvider.FindAllSections();
                if (_sectionModel.Count > 0)
                {
                    _snmpProvider.StartListen(IPAddress.Any);
                    SectionGrid.IsEnabled = true;
                }
                UpdateView();
                UpdateStatusBar("Wczytano bazę konfiguracji sekcji.");
        }

        private void UpdateView()
        {
            var unitDict = new Dictionary<string, TestModel>();

            var sections = from test in _sectionModel.Cast<TestModel>()
                           group test by test.MasterAgentIp
                               into unit
                               select new { unit.Key, Tests = unit };

            foreach (var section in sections.ToList())
            {
                IList<TestModel> tests = section.Tests.OrderBy(t => t.TestId).ToList();

                bool allGood = true;
                var syndromSb = new StringBuilder();
                foreach (TestModel test in tests)
                {
                    if (test.Status != "Stan poprawny")
                        allGood = false;
                    syndromSb.Append(test.Status == "Stan poprawny" ? "0" : "1");
                }

                foreach (TestModel test in tests)
                {
                    if (!unitDict.Keys.Contains(test.IpDest))
                    {
                        unitDict.Add(test.IpDest,
                                     new TestModel
                                         {
                                             IpDest = test.IpDest,
                                             MasterAgentIp = section.Key,
                                             Status = "Stan poprawny"
                                         });
                    }
                    else
                    {
                        unitDict[test.IpDest].Status = "Stan poprawny";
                    }
                }

                if (!allGood)
                {
                    IList<SyndromModel> syndromList = _dataProvider.FindGlobalOpinion(section.Key);
                    bool opinionMatch = false;
                    foreach (
                        SyndromModel syndromModel in
                            syndromList.Where(syndromModel => SyndromsMatch(syndromModel.Syndrom, syndromSb.ToString()))
                        )
                    {
                        opinionMatch = true;
                        string[] failedHostsList = syndromModel.Hosts.TrimEnd(',').Split(',');
                        foreach (string host in failedHostsList)
                        {
                            unitDict[host].Status = "Stan NIEpoprawny";
                        }
                    }

                    if (!opinionMatch)
                    {
                        foreach (TestModel test in tests)
                        {
                            unitDict[test.IpDest].Status = "Brak pasującego syndromu";
                        }
                    }
                }
            }

            for (int i = 0; i < unitDict.Count; i++)
            {
                unitDict.ElementAt(i).Value.TestId = i;
            }

            var groupedSectionModels = new ListCollectionView(_sectionModel);
            groupedSectionModels.GroupDescriptions.Add(new PropertyGroupDescription("MasterAgentIp"));

            var groupedUnitModels = new ListCollectionView(unitDict.Values.ToList());
            groupedUnitModels.GroupDescriptions.Add(new PropertyGroupDescription("MasterAgentIp"));

            UnitGrid.ItemsSource = groupedUnitModels;
            SectionGrid.ItemsSource = groupedSectionModels;
        }

        private bool SyndromsMatch(string opinion, string val)
        {
            if (opinion.Length != val.Length) return false;
            for (int i = 0; i < opinion.Length; i++)
            {
                if (opinion[i].ToString().ToLower() != "x" && opinion[i] != val[i])
                    return false;
            }
            return true;
        }

        private void _deleteSectionForm_SectionDeleted(object sender, SectionDeletedEventArgs e)
        {
            IList newSectionModel = new List<TestModel>();
            IList delSectionModel = new List<TestModel>();

            foreach (object item in _sectionModel)
            {
                var testModel = (TestModel)item;
                if (testModel.MasterAgentIp != e.MasterAgentIp)
                {
                    newSectionModel.Add(item);
                }
                else
                {
                    delSectionModel.Add(item);
                }
            }
            _sectionModel = newSectionModel;
            _dataProvider.DeleteSection(delSectionModel);
            UpdateView();
            UpdateStatusBar("Usunięto sekcję.");
        }

        private void _newSectionForm_SectionAdded(object sender, EventArgs e)
        {
            int countInSection = (from test in _sectionModel.Cast<TestModel>()
                                  where test.MasterAgentIp == _newSectionForm.SectionList[0].MasterAgentIp
                                  select test).Count();

            if (countInSection != 0)
            {
                MessageBox.Show("Sekcja o podanym Master Agent Ip już istnieje.", "Błąd dodawania sekcji.",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                return;
            }

            foreach (SyndromModel syndrom in _newSectionForm.SyndromList)
            {
                try
                {
                    logger.Info("Try to save DB Syndrom Model: " + syndrom);
                    _dataProvider.InsertSyndromModel(syndrom);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error while adding syndrom model: {0}", ex);
                }
            }

            foreach (TestModel testModel in _newSectionForm.SectionList)
            {
                logger.Info("Try to save DB Test Model: " + testModel);
                try
                {
                    if (_sectionModel.Contains(testModel)) continue;
                    countInSection = (from test in _sectionModel.Cast<TestModel>()
                                      where test.MasterAgentIp == testModel.MasterAgentIp
                                      select test).Count();

                    testModel.TestId = countInSection;
                    _dataProvider.InsertTestModel(testModel);
                    _sectionModel.Add(testModel);
                    logger.Info("DB Test model saved: " + testModel);
                    _snmpProvider.StartListen(IPAddress.Any);
                    SectionGrid.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error while adding Test model: {0}", ex);
                }
            }

            Update_Click(this, null);
            UpdateView();
            UpdateStatusBar("Dodano nową sekcję. Sekcja zapisana w bazie danych");
        }

        private void NewSectionBtn_Click(object sender, RoutedEventArgs e)
        {
            _newSectionForm.ShowDialog();
        }

        private void DelSectionBtn_Click(object sender, RoutedEventArgs e)
        {
            ISet<string> ipSet = new HashSet<string>();
            foreach (object item in _sectionModel)
            {
                var testModel = (TestModel)item;
                ipSet.Add(testModel.MasterAgentIp);
            }

            _deleteSectionForm.AssginSections(ipSet.ToList());
            _deleteSectionForm.ShowDialog();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshResults();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            foreach (TestModel testModel in _sectionModel)
            {
                logger.Info("Try to update Master Agent Test Model: " + testModel);
                try
                {
                    string status = _snmpProvider.UpdateMasterAgentDB(testModel);
                    UpdateStatusBar(status);
                    logger.Info("Updated Master Agent Test Model: " + testModel);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        private void RefreshResults()
        {
            foreach (TestModel testModel in _sectionModel)
            {
                try
                {
                    logger.Info("Try to refresh Master Agent Test Model: " + testModel);

                    string testResult = _snmpProvider.RefreshTestResults(testModel);
                    testModel.Status = testResult;
                    logger.Info("Refreshed Master Agent Test Model" + testModel);
                }
                catch (Exception ex)
                {
                    testModel.Status = "Błąd podczas aktualizacji.";
                    logger.Error(ex);
                }
            }

            UpdateStatusBar("Wyniki zostały odświeżone...");
            UpdateView();
        }

        private void UpdateStatusBar(string status)
        {
            statusTextBox.Text = DateTime.Now + " - " + status;
        }
    }
}