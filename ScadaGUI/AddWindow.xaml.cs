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
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        public AddWindow()
        {
            InitializeComponent();
            cmbTagType.SelectedIndex = 0; // Podrazumevano postavi na AI prilikom otvaranja
        }

        // 1. Dinamičko upravljanje poljima u zavisnosti od tipa taga
        private void CmbTagType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTagType.SelectedItem == null) return;

            string selectedType = (cmbTagType.SelectedItem as ComboBoxItem).Content.ToString();

            // Resetuj sva polja na podrazumevano omogućena
            txtScanTime.IsEnabled = true;
            chkScanOn.IsEnabled = true;
            txtLowLimit.IsEnabled = true;
            txtHighLimit.IsEnabled = true;
            txtUnits.IsEnabled = true;
            txtInitialValue.IsEnabled = true;
            txtDeadband.IsEnabled = true;
            txtHysteresis.IsEnabled = true;

            // Selektivno onemogućavanje i čišćenje neadekvatnih polja
            switch (selectedType)
            {
                case "AI":
                    txtInitialValue.IsEnabled = false;
                    txtInitialValue.Text = string.Empty;
                    break;

                case "AO":
                    txtScanTime.IsEnabled = false;
                    txtScanTime.Text = string.Empty;
                    chkScanOn.IsEnabled = false;
                    chkScanOn.IsChecked = false;
                    txtDeadband.IsEnabled = false;
                    txtDeadband.Text = string.Empty;
                    txtHysteresis.IsEnabled = false;
                    txtHysteresis.Text = string.Empty;
                    break;

                case "DI":
                    txtLowLimit.IsEnabled = false;
                    txtLowLimit.Text = string.Empty;
                    txtHighLimit.IsEnabled = false;
                    txtHighLimit.Text = string.Empty;
                    txtUnits.IsEnabled = false;
                    txtUnits.Text = string.Empty;
                    txtInitialValue.IsEnabled = false;
                    txtInitialValue.Text = string.Empty;
                    txtDeadband.IsEnabled = false;
                    txtDeadband.Text = string.Empty;
                    txtHysteresis.IsEnabled = false;
                    txtHysteresis.Text = string.Empty;
                    break;

                case "DO":
                    txtScanTime.IsEnabled = false;
                    txtScanTime.Text = string.Empty;
                    chkScanOn.IsEnabled = false;
                    chkScanOn.IsChecked = false;
                    txtLowLimit.IsEnabled = false;
                    txtLowLimit.Text = string.Empty;
                    txtHighLimit.IsEnabled = false;
                    txtHighLimit.Text = string.Empty;
                    txtUnits.IsEnabled = false;
                    txtUnits.Text = string.Empty;
                    txtDeadband.IsEnabled = false;
                    txtDeadband.Text = string.Empty;
                    txtHysteresis.IsEnabled = false;
                    txtHysteresis.Text = string.Empty;
                    break;
            }
        }

        // 2. Validacija unetih podataka i kreiranje objekta
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            txtErrorBlock.Text = string.Empty;

            // Osnovne provere za zajednička polja
            string name = txtTagName.Text.Trim();
            string description = txtDescription.Text.Trim();
            string ioAddress = txtIOAddress.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                txtErrorBlock.Text = "Polje 'Tag name (ID)' ne sme biti prazno.";
                return;
            }

            if (string.IsNullOrEmpty(ioAddress))
            {
                txtErrorBlock.Text = "Polje 'I/O Adresa' ne sme biti prazno.";
                return;
            }

            // Provera jedinstvenosti primarnog ključa (Name) u bazi podataka
            if (ContextClass.Instance.Tags.Any(t => t.Name.ToLower() == name.ToLower()))
            {
                txtErrorBlock.Text = $"Tag sa imenom '{name}' već postoji u bazi podataka.";
                return;
            }

            string selectedType = (cmbTagType.SelectedItem as ComboBoxItem).Content.ToString();
            Tag newTag = null;

            try
            {
                if (selectedType == "AI")
                {
                    if (!double.TryParse(txtScanTime.Text, out double scanTime) || scanTime <= 0)
                    {
                        txtErrorBlock.Text = "Vreme skeniranja (Scan Time) mora biti pozitivan broj.";
                        return;
                    }
                    if (!double.TryParse(txtLowLimit.Text, out double lowLimit))
                    {
                        txtErrorBlock.Text = "Donja granica (Low Limit) mora biti broj.";
                        return;
                    }
                    if (!double.TryParse(txtHighLimit.Text, out double highLimit) || highLimit <= lowLimit)
                    {
                        txtErrorBlock.Text = "Gornja granica (High Limit) mora biti broj veći od donje granice.";
                        return;
                    }
                    if (!double.TryParse(txtDeadband.Text, out double deadband) || deadband < 0)
                    {
                        txtErrorBlock.Text = "Deadband mora biti pozitivan broj ili nula.";
                        return;
                    }
                    if (!double.TryParse(txtHysteresis.Text, out double hysteresis) || hysteresis < 0)
                    {
                        txtErrorBlock.Text = "Hysteresis mora biti pozitivan broj ili nula.";
                        return;
                    }

                    newTag = new AnalogInput
                    {
                        Name = name,
                        Description = description,
                        IOAddress = ioAddress,
                        ScanTime = scanTime,
                        IsScanOn = chkScanOn.IsChecked ?? false,
                        LowLimit = lowLimit,
                        HighLimit = highLimit,
                        Units = txtUnits.Text.Trim(),
                        Deadband = deadband,
                        Hysteresis = hysteresis
                    };
                }
                else if (selectedType == "AO")
                {
                    if (!double.TryParse(txtLowLimit.Text, out double lowLimit))
                    {
                        txtErrorBlock.Text = "Donja granica (Low Limit) mora biti broj.";
                        return;
                    }
                    if (!double.TryParse(txtHighLimit.Text, out double highLimit) || highLimit <= lowLimit)
                    {
                        txtErrorBlock.Text = "Gornja granica (High Limit) mora biti broj veći od donje granice.";
                        return;
                    }
                    if (!double.TryParse(txtInitialValue.Text, out double initValue) || initValue < lowLimit || initValue > highLimit)
                    {
                        txtErrorBlock.Text = "Početna vrednost (Initial Value) mora biti unutar opsega donje i gornje granice.";
                        return;
                    }

                    newTag = new AnalogOutput
                    {
                        Name = name,
                        Description = description,
                        IOAddress = ioAddress,
                        LowLimit = lowLimit,
                        HighLimit = highLimit,
                        Units = txtUnits.Text.Trim(),
                        InitialValue = initValue
                    };
                }
                else if (selectedType == "DI")
                {
                    if (!double.TryParse(txtScanTime.Text, out double scanTime) || scanTime <= 0)
                    {
                        txtErrorBlock.Text = "Vreme skeniranja (Scan Time) mora biti pozitivan broj.";
                        return;
                    }

                    newTag = new DigitalInput
                    {
                        Name = name,
                        Description = description,
                        IOAddress = ioAddress,
                        ScanTime = scanTime,
                        IsScanOn = chkScanOn.IsChecked ?? false
                    };
                }
                else if (selectedType == "DO")
                {
                    if (!double.TryParse(txtInitialValue.Text, out double initValue) || (initValue != 0 && initValue != 1))
                    {
                        txtErrorBlock.Text = "Početna vrednost digitalnog izlaza mora biti 0 ili 1.";
                        return;
                    }

                    newTag = new DigitalOutput
                    {
                        Name = name,
                        Description = description,
                        IOAddress = ioAddress,
                        InitialValue = initValue
                    };
                }

                if (newTag != null)
                {
                    // Dodavanje u bazu preko Singleton instance i čuvanje izmena
                    ContextClass.Instance.Tags.Add(newTag);
                    ContextClass.Instance.SaveChanges();

                    MessageBox.Show("Tag uspešno kreiran i sačuvan u bazi!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                txtErrorBlock.Text = $"Greška pri čuvanju taga: {ex.Message}";
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
