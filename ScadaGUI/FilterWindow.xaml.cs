using System;
using System.IO;
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
using DataConcentrator;
using DataConcentrator.Model;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window
    {
        private List<TagValue> filteredResults = new List<TagValue>();

        public FilterWindow()
        {
            InitializeComponent();
            LoadTags();
        }

        private void LoadTags()
        {
            // Prikazujemo samo AI tagove, a dodajemo i "Praznu" opciju na vrh ako korisnik ne želi da filtrira po tagu
            var aiTags = ContextClass.Instance.Tags.OfType<AnalogInput>().Cast<Tag>().ToList();
            aiTags.Insert(0, new AnalogInput { Name = "--- Svi Tagovi ---" });
            cmbTags.ItemsSource = aiTags;
            cmbTags.SelectedIndex = 0;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Počinjemo sa svim podacima iz tabele TagValues
                var query = ContextClass.Instance.TagValues.AsQueryable();

                // 1. Filtriranje po imenu taga (ako nije izabrana opcija "Svi Tagovi")
                if (cmbTags.SelectedItem is Tag selectedTag && selectedTag.Name != "--- Svi Tagovi ---")
                {
                    query = query.Where(tv => tv.TagName == selectedTag.Name);
                }

                // 2. Filtriranje po početnom vremenu
                if (!string.IsNullOrWhiteSpace(txtTimeFrom.Text))
                {
                    if (DateTime.TryParse(txtTimeFrom.Text, out DateTime timeFrom))
                        query = query.Where(tv => tv.Timestamp >= timeFrom);
                    else
                        MessageBox.Show("Neispravan format za 'Vreme OD'. Pokušajte: yyyy-MM-dd HH:mm:ss", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // 3. Filtriranje po krajnjem vremenu
                if (!string.IsNullOrWhiteSpace(txtTimeTo.Text))
                {
                    if (DateTime.TryParse(txtTimeTo.Text, out DateTime timeTo))
                        query = query.Where(tv => tv.Timestamp <= timeTo);
                    else
                        MessageBox.Show("Neispravan format za 'Vreme DO'. Pokušajte: yyyy-MM-dd HH:mm:ss", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // 4. Filtriranje po donjoj granici vrednosti
                if (!string.IsNullOrWhiteSpace(txtValFrom.Text))
                {
                    if (double.TryParse(txtValFrom.Text, out double valFrom))
                        query = query.Where(tv => tv.Value >= valFrom);
                    else
                        MessageBox.Show("Neispravan format za 'Vrednost OD'. Mora biti broj.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // 5. Filtriranje po gornjoj granici vrednosti
                if (!string.IsNullOrWhiteSpace(txtValTo.Text))
                {
                    if (double.TryParse(txtValTo.Text, out double valTo))
                        query = query.Where(tv => tv.Value <= valTo);
                    else
                        MessageBox.Show("Neispravan format za 'Vrednost DO'. Mora biti broj.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // Izvršavanje upita nad bazom
                filteredResults = query.OrderByDescending(tv => tv.Timestamp).ToList();
                dataGridResults.ItemsSource = filteredResults;

                if (filteredResults.Count == 0)
                {
                    MessageBox.Show("Nema rezultata za zadate kriterijume.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Došlo je do greške pri pretrazi: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateTxt_Click(object sender, RoutedEventArgs e)
        {
            if (filteredResults == null || filteredResults.Count == 0)
            {
                MessageBox.Show("Prvo pretražite podatke pre nego što generišete fajl.", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string filePath = "Izvestaj_Tagova.txt";
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("=== IZVEŠTAJ SCADA SISTEMA ===");
                sb.AppendLine($"Datum generisanja: {DateTime.Now}");
                sb.AppendLine("---------------------------------------------------------");
                sb.AppendLine("TAG NAME\t|\tVREME\t\t\t|\tVREDNOST");
                sb.AppendLine("---------------------------------------------------------");

                foreach (var record in filteredResults)
                {
                    sb.AppendLine($"{record.TagName}\t|\t{record.Timestamp}\t|\t{record.Value}");
                }

                File.WriteAllText(filePath, sb.ToString());

                // Javljamo korisniku i pravimo log (ako je Logger klasa implementirana)
                MessageBox.Show($"Izveštaj je uspešno sačuvan u fajl: {filePath}\n(Fajl se nalazi u bin/Debug folderu projekta)", "Uspeh", MessageBoxButton.OK, MessageBoxImage.Information);

                Logger.Log("Korisnik je generisao TXT izveštaj sa " + filteredResults.Count + " redova.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greška pri upisu u fajl: {ex.Message}", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
