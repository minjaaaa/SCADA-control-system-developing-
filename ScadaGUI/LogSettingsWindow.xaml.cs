using DataConcentrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for LogSettingsWindow.xaml
    /// </summary>
    public partial class LogSettingsWindow : Window
    {
        public LogSettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int trace = Logger.TraceWord;
            // Provera pomoću Bitwise AND operatora
            chkAppStart.IsChecked = (trace & (int)LogCategory.AppStart) != 0;
            chkAlarms.IsChecked = (trace & (int)LogCategory.Alarms) != 0;
            chkTagManagement.IsChecked = (trace & (int)LogCategory.TagManagement) != 0;
            chkImportExport.IsChecked = (trace & (int)LogCategory.ImportExport) != 0;
            chkErrors.IsChecked = (trace & (int)LogCategory.Errors) != 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            int newTraceWord = 0;

            // Sabiramo vrednosti bitova ako je checkbox štikliran
            if (chkAppStart.IsChecked == true) newTraceWord += (int)LogCategory.AppStart; // +1
            if (chkAlarms.IsChecked == true) newTraceWord += (int)LogCategory.Alarms;     // +2
            if (chkTagManagement.IsChecked == true) newTraceWord += (int)LogCategory.TagManagement; // +4
            if (chkImportExport.IsChecked == true) newTraceWord += (int)LogCategory.ImportExport;   // +8
            if (chkErrors.IsChecked == true) newTraceWord += (int)LogCategory.Errors;     // +16

            Logger.SaveTraceWord(newTraceWord);
            MessageBox.Show($"TraceWord uspešno sačuvan! Numerička vrednost: {newTraceWord}", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}
