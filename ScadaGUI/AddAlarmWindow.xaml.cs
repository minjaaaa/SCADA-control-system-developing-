using DataConcentrator;
using DataConcentrator.Model;
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
    /// Interaction logic for AddAlarmWindow.xaml
    /// </summary>
    public partial class AddAlarmWindow : Window
    {
        public AddAlarmWindow()
        {
            InitializeComponent();
            LoadAnalogInputs();
            cmbAlarmType.SelectedIndex = 0;
        }

        private void LoadAnalogInputs()
        {
            // Filtriranje i učitavanje samo AI tagova iz baze
            var aiTags = ContextClass.Instance.Tags.OfType<AnalogInput>().ToList();
            cmbAiTags.ItemsSource = aiTags;
            if (aiTags.Count > 0) cmbAiTags.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            txtErrorBlock.Text = string.Empty;

            string alarmName = txtAlarmName.Text.Trim();
            var selectedTag = cmbAiTags.SelectedItem as AnalogInput;
            string message = txtMessage.Text.Trim();

            if (string.IsNullOrEmpty(alarmName))
            {
                txtErrorBlock.Text = "Polje 'Naziv alarma' ne sme biti prazno.";
                return;
            }

            if (selectedTag == null)
            {
                txtErrorBlock.Text = "Morate izabrati analogni ulazni tag.";
                return;
            }

            if (!double.TryParse(txtLimit.Text, out double limit))
            {
                txtErrorBlock.Text = "Polje 'Granica' mora biti numerička vrednost.";
                return;
            }

            // Provera jedinstvenosti naziva alarma
            if (ContextClass.Instance.Alarms.Any(a => a.Name.ToLower() == alarmName.ToLower()))
            {
                txtErrorBlock.Text = $"Alarm sa nazivom '{alarmName}' već postoji u bazi.";
                return;
            }

            AlarmType type = cmbAlarmType.SelectedIndex == 0 ? AlarmType.HIGH : AlarmType.LOW;

            Alarm newAlarm = new Alarm
            {
                Name = alarmName,
                TagName = selectedTag.Name,
                Limit = limit,
                Type = type,
                Message = message
            };

            try
            {
                ContextClass.Instance.Alarms.Add(newAlarm);
                ContextClass.Instance.SaveChanges();

                MessageBox.Show("Alarm je uspešno kreiran i povezan sa tagom!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                txtErrorBlock.Text = $"Greška pri upisu u bazu: {ex.Message}";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
