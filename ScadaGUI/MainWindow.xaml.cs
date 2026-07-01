using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Threading; // Potrebno za DispatcherPriority
using DataConcentrator;
using DataConcentrator.Model; 
using PLCSimulator;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.IO;

namespace ScadaGUI
{
    public partial class MainWindow : Window
    {
        private HashSet<string> blokiraniPopupi = new HashSet<string>();
        private HashSet<string> prikazaniProzori = new HashSet<string>();
        public MainWindow()
        {
            InitializeComponent();

             foreach (AnalogInput ai in ContextClass.Instance.Tags.OfType<AnalogInput>())
             {
                 ai.AlarmActivated += OnAlarmActivated;
                 ai.StartScan();
             }
        }

        private void OpenFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterWindow filterWindow = new FilterWindow();
            filterWindow.ShowDialog();
        }

        // --- DEO KOJI SMO MI DODALI ZA TABELU I DUGMAD ---

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDataGrid();
        }

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                RefreshDataGrid();

                // Pronalazimo taj novododati tag u bazi
                var newTag = ContextClass.Instance.Tags.ToList().LastOrDefault();

                // Odmah mu palimo tred i povezujemo alarm!
                if (newTag is AnalogInput ai)
                {
                    ai.AlarmActivated += OnAlarmActivated;
                    ai.StartScan();
                }
                else if (newTag is DigitalInput di)
                {
                    di.StartScan();
                }
            }
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridTags.SelectedItem is Tag selectedTag)
            {
                MessageBoxResult result = MessageBox.Show($"Da li ste sigurni da želite da obrišete tag '{selectedTag.Name}'?",
                                                          "Potvrda brisanja", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 1. OBAVEZNO ZAUSTAVLJANJE NITI PRE BRISANJA IZ BAZE
                        if (selectedTag is AnalogInput ai) ai.StopScan();
                        else if (selectedTag is DigitalInput di) di.StopScan();

                        // 2. BEZBEDNO BRISANJE UZ ZAKLJUČAVANJE (LOCK)
                        lock (ContextClass.Instance)
                        {
                            ContextClass.Instance.Tags.Remove(selectedTag);
                            ContextClass.Instance.SaveChanges();
                        }

                        RefreshDataGrid();
                        Logger.Log($"Obrisan tag: {selectedTag.Name}", LogCategory.TagManagement);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Greška pri brisanju taga: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Molimo vas da prvo izaberete tag iz tabele koji želite da obrišete.",
                                "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddAlarm_Click(object sender, RoutedEventArgs e)
        {
            AddAlarmWindow alarmWindow = new AddAlarmWindow();
            bool? result = alarmWindow.ShowDialog();
            if (result == true)
            {
                RefreshDataGrid();
            }
        }

        private void RefreshDataGrid()
        {
            var tags = ContextClass.Instance.Tags.ToList();
            var alarms = ContextClass.Instance.Alarms.ToList();
            var activatedAlarms = ContextClass.Instance.ActivatedAlarms.ToList();

            foreach (var tag in tags)
            {
                if (tag is AnalogInput ai)
                {
                    // Pronalaženje svih alarma koji su vezani za ovaj konkretan analogni ulaz
                    var connectedAlarmNames = alarms.Where(a => a.TagName == ai.Name).Select(a => a.Name).ToList();

                    // Provera da li među aktiviranim instancama ima onih koje su u stanju Active ili Acknowledged
                    var activeInstances = activatedAlarms.Where(aa => connectedAlarmNames.Contains(aa.AlarmName)).ToList();

                    if (activeInstances.Any(aa => aa.State == AlarmState.Active))
                    {
                        tag.AlarmStatus = "Active"; // Pokreće crveni okidač u XAML-u
                    }
                    else if (activeInstances.Any(aa => aa.State == AlarmState.Acknowledged))
                    {
                        tag.AlarmStatus = "Acknowledged"; // Pokreće žuti okidač u XAML-u
                    }
                    else
                    {
                        tag.AlarmStatus = "Normal";
                    }
                }
            }

            dataGridTags.ItemsSource = tags;
        }
        private void AcknowledgeAlarm_Click(object sender, RoutedEventArgs e)
        {
            // Proveravamo da li je selektovan tag i da li je taj tag tipa AnalogInput
            if (dataGridTags.SelectedItem is AnalogInput ai)
            {
                // 1. GLAVNA PROVERA: Da li je sam TAG trenutno u crvenom alarmu?
                if (ai.AlarmStatus == "Active")
                {
                    // Odmah menjamo status taga u Acknowledged (Ovo će prebojiti red u žuto)
                    ai.AlarmStatus = "Acknowledged";

                    // 2. Ažuriramo istoriju alarma u bazi (ActivatedAlarms tabela) ako postoji
                    var connectedAlarmNames = ContextClass.Instance.Alarms
                        .Where(a => a.TagName == ai.Name)
                        .Select(a => a.Name).ToList();

                    var activeAlarms = ContextClass.Instance.ActivatedAlarms
                        .Where(aa => connectedAlarmNames.Contains(aa.AlarmName) && aa.State == AlarmState.Active)
                        .ToList();

                    // Ako ih ima u istoriji, menjamo i njima stanje
                    foreach (var aa in activeAlarms)
                    {
                        aa.State = AlarmState.Acknowledged;
                    }

                    // 3. Čuvamo sve promene u bazi
                    ContextClass.Instance.SaveChanges();

                    // 4. RESETUJEMO ZASTAVICU PROZORA! 
                    // (Ovo omogućava da iskoči pop-up sledeći put kada vrednost padne u normalu pa opet poraste)
                    prikazaniProzori.Remove(ai.Name);

                    Logger.Log($"Alarm potvrđen za tag: {ai.Name}", LogCategory.Alarms);

                    // 5. Nasilno osvežavanje DataGrid-a da bi sigurno povukao novu (žutu) boju
                    dataGridTags.ItemsSource = null;
                    dataGridTags.ItemsSource = ContextClass.Instance.Tags.ToList();

                    MessageBox.Show("Alarm je uspešno potvrđen (Acknowledged).", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (ai.AlarmStatus == "Acknowledged")
                {
                    MessageBox.Show("Alarm na ovom tagu je VEĆ potvrđen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ovaj tag je u NORMALNOM stanju i nema aktivnih alarma za potvrdu.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Molimo vas da iz tabele izaberete analogni ulaz (AI) koji je u alarmu.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // --- ORIGINALNI KOSTUR KOJI SE BAVI NIKOVANJEM I ALARMIMA ---

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // OVO OTKOMENTARIŠEMO KADA IMPLEMENTIRAMO StopScan
             //abort input threads
             foreach(AnalogInput ai in ContextClass.Instance.Tags.OfType<AnalogInput>())
             {
                 ai.StopScan();
             }
             foreach(DigitalInput di in ContextClass.Instance.Tags.OfType<DigitalInput>())
             {
                 di.StopScan();
             }

             //abort simulator threads
             if (PLC.Instance != null)
             {
                 PLC.Instance.Abort(); // U tvom kosturu je ovde zvalo Abort(), ali preko omotaca je StopSimulator()
             }

             ContextClass.Instance.SaveChanges();
             ContextClass.Instance.Dispose();
        }

        private void OnAlarmActivated(Alarm triggeredAlarm, double currentValue, string units)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                bool prikažiProzor = false;

                // 1. ODMAH ZAKLJUČAVAMO I UPISUJEMO STATUS U BAZU
                lock (ContextClass.Instance)
                {
                    var tag = ContextClass.Instance.Tags.FirstOrDefault(t => t.Name == triggeredAlarm.TagName);

                    // Ako je tag pronađen i u normalnom je stanju (ili je status null/prazan pri kreiranju)
                    if (tag != null && (tag.AlarmStatus == "Normal" || string.IsNullOrEmpty(tag.AlarmStatus)))
                    {
                        tag.AlarmStatus = "Active";
                        ContextClass.Instance.SaveChanges(); // Odmah upisujemo u bazu!
                        prikažiProzor = true;
                    }
                }

                // 2. AKO JE STATUS USPEŠNO PROMENJEN U "ACTIVE"
                if (prikažiProzor)
                {
                    // Prvo proveravamo našu memorijsku listu otvorenih popup-ova da sprečimo dupliranje
                    if (!prikazaniProzori.Contains(triggeredAlarm.TagName))
                    {
                        prikazaniProzori.Add(triggeredAlarm.TagName); // Blokiramo sledeće brze prozore

                        // 3. ODMAH OSVEŽAVAMO TABELU (Vrsta postaje crvena NA EKRANU pre nego što iskoči prozor!)
                        dataGridTags.ItemsSource = null;
                        dataGridTags.ItemsSource = ContextClass.Instance.Tags.ToList();
                        RefreshDataGrid();

                        Logger.Log($"Alarm aktiviran: {triggeredAlarm.TagName} (Vrednost: {currentValue:F2} {units})", LogCategory.Alarms);

                        // 4. TEK SADA PRIKAZUJEMO PROZOR 
                        // Tabela iza prozora je već crvena i spremna za ACK čim se prozor zatvori
                        string upozorenje = $"🚨 KRITIČNO UPOZORENJE: ALARM AKTIVIRAN! 🚨\n\n" +
                                            $"Senzor: {triggeredAlarm.TagName}\n" +
                                            $"Trenutna vrednost: {currentValue:F2} {units}\n\n" +
                                            $"Pritisnite OK da zatvorite ovo obaveštenje, a zatim potvrdite alarm na ACK dugme.";

                        MessageBox.Show(upozorenje, "SCADA Alarmni Sistem", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }));
        }

        private void OpenLogs_Click(object sender, RoutedEventArgs e)
        {
            LogSettingsWindow logWindow = new LogSettingsWindow();
            logWindow.ShowDialog();
        }

        private void ExportJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Nameštanje TypeNameHandling.Auto je KLJUČNO! Ono čuva informaciju da li je tag AI, AO, itd.
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented };

                var allTags = ContextClass.Instance.Tags.ToList();
                string json = JsonConvert.SerializeObject(allTags, settings);

                SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "JSON files (*.json)|*.json" };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, json);
                    MessageBox.Show("Konfiguracija uspešno eksportovana!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger.Log("Izvršen JSON export tagova.", LogCategory.ImportExport);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri eksportu: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log($"Greška pri eksportu: {ex.Message}", LogCategory.Errors);
            }
        }

        private void ImportJson_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" };
                if (openFileDialog.ShowDialog() == true)
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

                    var importedTags = JsonConvert.DeserializeObject<List<Tag>>(json, settings);

                    if (importedTags != null && importedTags.Count > 0)
                    {
                        // DOZVOLJENE ADRESE
                        List<string> allowedAI = new List<string> { "ADDR001", "ADDR002", "ADDR003", "ADDR004" };
                        List<string> allowedAO = new List<string> { "ADDR005", "ADDR006", "ADDR007", "ADDR008" };
                        List<string> allowedDI = new List<string> { "ADDR009", "ADDR011", "ADDR012", "ADDR013" };
                        List<string> allowedDO = new List<string> { "ADDR010", "ADDR014", "ADDR015", "ADDR016" };

                        List<Tag> validTags = new List<Tag>();
                        List<string> errorMessages = new List<string>();

                        // 1. ČITANJE TRENUTNOG STANJA IZ BAZE
                        List<string> existingNames = new List<string>();
                        List<string> existingAddresses = new List<string>();

                        lock (ContextClass.Instance)
                        {
                            existingNames = ContextClass.Instance.Tags.Select(t => t.Name.ToLower()).ToList();
                            existingAddresses = ContextClass.Instance.Tags.Select(t => t.IOAddress.ToUpper()).ToList();
                        }

                        // Praćenje šta smo već obradili unutar samog JSON fajla (da sprečimo duplikate u samom fajlu)
                        HashSet<string> currentBatchNames = new HashSet<string>();
                        HashSet<string> currentBatchAddresses = new HashSet<string>();

                        // 2. PRED-VALIDACIJA SVAKOG TAGA
                        foreach (var tag in importedTags)
                        {
                            string name = tag.Name?.Trim();
                            string lowerName = name?.ToLower();
                            string address = tag.IOAddress?.Trim().ToUpper();
                            bool isValidType = false;

                            // A. Provera duplikata imena (ID taga)
                            if (existingNames.Contains(lowerName) || currentBatchNames.Contains(lowerName))
                            {
                                errorMessages.Add($"- Tag '{name}': Tag sa ovim imenom već postoji.");
                                continue;
                            }

                            // B. Provera konflikta adrese
                            if (existingAddresses.Contains(address) || currentBatchAddresses.Contains(address))
                            {
                                errorMessages.Add($"- Tag '{name}': Adresa '{address}' je već zauzeta.");
                                continue;
                            }

                            // C. Provera da li je adresa u dozvoljenom opsegu za taj tip
                            if (tag is AnalogInput && allowedAI.Contains(address)) isValidType = true;
                            else if (tag is AnalogOutput && allowedAO.Contains(address)) isValidType = true;
                            else if (tag is DigitalInput && allowedDI.Contains(address)) isValidType = true;
                            else if (tag is DigitalOutput && allowedDO.Contains(address)) isValidType = true;

                            if (!isValidType)
                            {
                                errorMessages.Add($"- Tag '{name}': Adresa '{address}' nije dozvoljena za njegov tip.");
                                continue;
                            }

                            // Ako je prošao sve provere, dodajemo ga u validne
                            currentBatchNames.Add(lowerName);
                            currentBatchAddresses.Add(address);
                            validTags.Add(tag);
                        }

                        // Ako nijedan tag ne valja, prekidamo akciju
                        if (validTags.Count == 0)
                        {
                            MessageBox.Show("Nijedan novi tag iz JSON fajla nije dodatan. Svi već postoje ili imaju konflikt adresa.\n\nDetalji:\n" + string.Join("\n", errorMessages), "Info o Importu", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        // 3. DODAVANJE NOVIH TAGOVA U BAZU (Stari ostaju netaknuti)
                        lock (ContextClass.Instance)
                        {
                            ContextClass.Instance.Tags.AddRange(validTags);
                            ContextClass.Instance.SaveChanges();
                        }

                        // 4. POKRETANJE NITI SAMO ZA NOVE TAGOVE
                        foreach (var newTag in validTags)
                        {
                            if (newTag is AnalogInput ai)
                            {
                                ai.AlarmActivated += OnAlarmActivated;
                                ai.StartScan();
                            }
                            else if (newTag is DigitalInput di)
                            {
                                di.StartScan();
                            }
                        }

                        RefreshDataGrid();

                        // 5. PRIKAZ ADEKVATNE PORUKE
                        if (errorMessages.Count > 0)
                        {
                            string warnMsg = $"Uspešno dodato {validTags.Count} novih tagova.\n\nNeki tagovi su preskočeni jer već postoje ili prave konflikt:\n{string.Join("\n", errorMessages)}";
                            MessageBox.Show(warnMsg, "Delimičan uspeh", MessageBoxButton.OK, MessageBoxImage.Warning);
                            Logger.Log($"Delimičan JSON import. Dodato: {validTags.Count}, Preskočeno: {errorMessages.Count}.", LogCategory.ImportExport);
                        }
                        else
                        {
                            MessageBox.Show("Svi tagovi iz konfiguracije su uspešno dodati i pokrenuti!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                            Logger.Log("Izvršen kompletan JSON import novih tagova.", LogCategory.ImportExport);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri importu: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Log($"Greška pri importu: {ex.Message}", LogCategory.Errors);
            }
        }

        // --- TAČKA 1: Ručni upis vrednosti (samo za AO i DO) ---
        private void WriteValue_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridTags.SelectedItem is Tag selectedTag)
            {
                if (selectedTag is AnalogInput || selectedTag is DigitalInput)
                {
                    MessageBox.Show("Vrednosti se mogu upisivati samo u izlazne tagove (AO i DO). Ulazi se čitaju sa senzora.", "Nije dozvoljeno", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (double.TryParse(txtManualValue.Text.Trim(), out double newValue))
                {
                    if (selectedTag is AnalogOutput ao)
                    {
                        if (newValue < ao.LowLimit || newValue > ao.HighLimit)
                        {
                            MessageBox.Show($"Vrednost mora biti između {ao.LowLimit} i {ao.HighLimit}.", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        ao.InitialValue = newValue;
                    }
                    else if (selectedTag is DigitalOutput doTag)
                    {
                        if (newValue != 0 && newValue != 1)
                        {
                            MessageBox.Show("Digitalni izlaz može imati samo vrednost 0 ili 1.", "Greška", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        doTag.InitialValue = newValue;
                    }

                    ContextClass.Instance.SaveChanges();
                    RefreshDataGrid();
                    Logger.Log($"Ručno upisana vrednost {newValue} u tag {selectedTag.Name}", LogCategory.TagManagement);
                    MessageBox.Show("Vrednost je uspešno upisana!", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtManualValue.Clear();
                }
                else
                {
                    MessageBox.Show("Molimo unesite validan broj.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Molimo izaberite izlazni tag (AO ili DO) iz tabele.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // --- TAČKA 2: Detalji (prikaz svih alarma za izabrani AI tag) ---
        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridTags.SelectedItem is AnalogInput ai)
            {
                var tagAlarms = ContextClass.Instance.Alarms.Where(a => a.TagName == ai.Name).ToList();

                if (tagAlarms.Count == 0)
                {
                    MessageBox.Show($"Tag '{ai.Name}' trenutno nema definisanih alarma.", "Detalji o alarmima", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string details = $"Alarmi vezani za tag: {ai.Name}\n";
                details += "--------------------------------------------------\n";
                foreach (var alarm in tagAlarms)
                {
                    details += $"- TIP: {alarm.Type} | GRANICA: {alarm.Limit}\n";
                }

                MessageBox.Show(details, "Detalji o alarmima", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Molimo izaberite Analog Input (AI) tag iz tabele za prikaz detalja.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}