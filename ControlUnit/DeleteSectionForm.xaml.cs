using System;
using System.Collections;
using System.Windows;
using ControlUnit.Models;

namespace ControlUnit
{
    /// <summary>
    /// Interaction logic for DeleteSectionForm.xaml
    /// </summary>
    public partial class DeleteSectionForm : Window
    {
        private IList _sections;

        public DeleteSectionForm()
        {
            InitializeComponent();
        }

        public event EventHandler<SectionDeletedEventArgs> SectionDeleted;

        public void AssginSections(IList sections)
        {
            _sections = sections;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Sections.ItemsSource = _sections;
            if (_sections.Count > 0)
                Sections.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (SectionDeleted != null)
            {
                SectionDeleted.Invoke(this, new SectionDeletedEventArgs(Sections.SelectedValue.ToString()));
            }
            _sections.Clear();
            Hide();
        }
    }
}