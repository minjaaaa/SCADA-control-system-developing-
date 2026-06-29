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

            // Osnovne provere za zajednička polja (dodato ToUpper() za adresu)
            string name = txtTagName.Text.Trim();
            string description = txtDescription.Text.Trim();
            string ioAddress = txtIOAddress.Text.Trim().ToUpper();

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

            // --- POČETAK NOVE VALIDACIJE ZA HARDVERSKE ADRESE ---

            List<string> allowedAI = new List<string> { "ADDR001", "ADDR002", "ADDR003", "ADDR004" };
            List<string> allowedAO = new List<string> { "ADDR005", "ADDR006", "ADDR007", "ADDR008" };
            List<string> allowedDI = new List<string> { "ADDR009", "ADDR011", "ADDR012", "ADDR013" };
            List<string> allowedDO = new List<string> { "ADDR010", "ADDR014", "ADDR015", "ADDR016" };

            // 1. Određivanje liste dozvoljenih adresa na osnovu izabranog tipa taga
            List<string> currentAllowedPins = new List<string>();
            if (selectedType == "AI") currentAllowedPins = allowedAI;
            else if (selectedType == "AO") currentAllowedPins = allowedAO;
            else if (selectedType == "DI") currentAllowedPins = allowedDI;
            else if (selectedType == "DO") currentAllowedPins = allowedDO;

            // 2. Preuzimanje svih zauzetih adresa koje su trenutno u bazi
            var usedAddresses = ContextClass.Instance.Tags.Select(t => t.IOAddress.ToUpper()).ToList();

            // 3. Filtriranje: Nalazimo preseke - koji dozvoljeni pinovi NISU u listi zauzetih
            var availablePins = currentAllowedPins.Where(pin => !usedAddresses.Contains(pin)).ToList();

            // Formatiramo tekst za prikaz (ako je lista prazna, dajemo posebno obaveštenje)
            string availablePinsText = availablePins.Count > 0
                                       ? string.Join(", ", availablePins)
                                       : "NEMA SLOBODNIH PINOVA ZA OVAJ TIP!";

            // 4. Da li je uneta adresa uopšte validna za taj tip?
            if (!currentAllowedPins.Contains(ioAddress))
            {
                txtErrorBlock.Text = $"I/O Adresa '{ioAddress}' nije dozvoljena za tip '{selectedType}'.";

                MessageBox.Show($"Pokušali ste unos adrese '{ioAddress}' koja nije dozvoljena za tip taga {selectedType}.\n\n" +
                                $"Dostupni (slobodni) pinovi koje možete iskoristiti su:\n{availablePinsText}",
                                "Nevalidna Adresa", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 5. Da li je adresa već zauzeta od strane drugog taga?
            if (usedAddresses.Contains(ioAddress))
            {
                txtErrorBlock.Text = $"Konflikt: Adresa '{ioAddress}' je već zauzeta!";

                MessageBox.Show($"I/O Adresa '{ioAddress}' je validna za {selectedType}, ali je VEĆ ZAUZETA od strane drugog senzora ili aktuatora!\n\n" +
                                $"Molimo izaberite neku od preostalih slobodnih adresa:\n{availablePinsText}",
                                "Konflikt Adresa", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- KRAJ NOVE VALIDACIJE ---

            Tag newTag = null;

            try
            {
                // ... (OSTATAK TVOG KODA OSTJE ISTI ZA KREIRANJE OBJEKATA I TRY/CATCH) ...
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
                    // Ostatak validacije za AO...
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
                    // Ostatak validacije za DI...
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
                    // Ostatak validacije za DO...
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
