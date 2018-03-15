using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using Newtonsoft.Json;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        
        public MainWindow()
        {
            InitializeComponent();
            ReadTempData();
            ReadConfig();

        }

        public void ReadTempData()
        {
            var path = "tempData.json";


            using (StreamReader r = new StreamReader(path))
            {

                var json = r.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var fileData = JsonConvert.DeserializeObject<List<TorqueData>>(json);
                    foreach (var value in fileData)
                    {
                        tb1.Items.Add(value);
                    }
                }


            }


        }
        public void ReadConfig()
        {
            var path = "config.json";
            if (File.Exists(path))
            {
                using (StreamReader r = new StreamReader(path))
                {

                    var json = r.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var fileData = JsonConvert.DeserializeObject<Configuration>(json);

                        textBoxOperator.Text = fileData.Operator;
                        cmbBoxListe.SelectedIndex = fileData.Unit;
                        textBoxBin.Text = fileData.Bin.ToString();
                        textBoxLot.Text = fileData.Lot.ToString();
                        textBoxFrom.Text = fileData.Lot.ToString();
                        textBoxTo.Text = fileData.Lot.ToString();
                        
                    }
                    r.Close();
                    r.Dispose();
                }
            }
            else
            {
                WriteConfig();
            }
        }
        private string EmptyStringToZero(string emptyString)
        {
            if (string.IsNullOrWhiteSpace(emptyString))
            {
                return "0";
            }
            return emptyString;
        }
        private bool boolWriteToConfig(TextBox bin, TextBox lot, TextBox from, TextBox to, TextBox operatorName, ComboBox units)
        {
            if (string.IsNullOrEmpty(bin?.Text)
                || string.IsNullOrEmpty(lot?.Text)
                || string.IsNullOrEmpty(from?.Text)
                || string.IsNullOrEmpty(to?.Text)
                || string.IsNullOrEmpty(operatorName?.Text)
                || string.IsNullOrEmpty(units?.SelectedIndex.ToString()))
            {
                return false;
            }
            return true;
        }
        public void WriteConfig()
        {
            if (boolWriteToConfig(textBoxBin, textBoxLot, textBoxFrom, textBoxTo, textBoxOperator, cmbBoxListe))
            {
                var path = "config.json";
                var bin = EmptyStringToZero(textBoxBin?.Text);
                var lot = EmptyStringToZero(textBoxLot?.Text);
                var from = EmptyStringToZero(textBoxFrom?.Text);
                var to = EmptyStringToZero(textBoxTo?.Text);


                var configItem =
                     new Configuration()
                     {
                         Operator = textBoxOperator?.Text ?? "Operator",
                         Unit = cmbBoxListe?.SelectedIndex ?? 0,
                         Bin = Convert.ToInt32(bin),
                         Lot = Convert.ToInt32(lot),
                         RangeFrom = Convert.ToInt32(from),
                         RangeTo = Convert.ToInt32(to)
                     };

                try
                {
                    StreamWriter file = File.CreateText(path);
                    var serializer = new JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Serialize(file, configItem);
                    file.Close();
                    file.Dispose();
                }
                catch (IOException ex)
                {

                }
            }
            

        }
        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public  async Task<List<TorqueData>> GetData()
        {

           var allData = new List<TorqueData>();
            var itemToRecord =
                 new TorqueData() {
                     Operator = textBoxOperator.Text,
                     Unit = ((ComboBoxItem)(cmbBoxListe.SelectedItem)).Content.ToString(),
                     Status = "Fail",
                     Reading = "Reading",
                     Bin = Convert.ToInt32(textBoxBin.Text),
                     Lot = Convert.ToInt32(textBoxLot.Text),
                     Retest = checkBoxRetest?.IsChecked ?? false,
                    DateRecorded = DateTime.Now
               
            };
            allData.Add(itemToRecord);
            var path = "tempData.json";
           

            using (StreamReader r = new StreamReader(path))
            {

                var json = r.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var fileData = JsonConvert.DeserializeObject<List<TorqueData>>(json);
                    foreach (var value in fileData)
                    {
                        allData.Add(value);
                    }                  
                }             

               
            }
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, allData);
            }

            return await Task.Run(() =>
            {
                Thread.Sleep(5000); // mocking latency
                return allData;
            });     
          
        }

        private bool MessagIfEmpty(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show(string.Format("Please Add:{0}", name));
                return false;
            }
            return true;
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        private async  void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            tb1.Items.Add(await GetData());

        }
        public class TorqueData
        {
            public string Status { get; set; }
            public string Reading { get; set;  }
            public int Bin { get; set; }
            public int Lot { get; set; }
            public string Operator { get; set; }

            public bool Retest { get; set; }
            public string Unit { get; set; }

            public DateTime DateRecorded { get; set; }
        }
        public class Configuration
        {
            public string Operator { get; set; }
            public int RangeFrom { get; set; }
            public int RangeTo { get; set; }
            public int Lot { get; set; }
            public int Bin { get; set; }

            public int Unit { get; set; }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
         
        }

        private void textBoxOperator_TextChanged(object sender, TextChangedEventArgs e)
        {
            WriteConfig();
        }

        private void textBoxFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            WriteConfig();
        }

        private void textBoxTo_TextChanged(object sender, TextChangedEventArgs e)
        {
            WriteConfig();
        }

        private void textBoxLot_TextChanged(object sender, TextChangedEventArgs e)
        {
            WriteConfig();
        }

        private void textBoxBin_TextChanged(object sender, TextChangedEventArgs e)
        {
            WriteConfig();
        }

        private void cmbBoxListe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WriteConfig();
        }

        private void cmbBoxListe_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            WriteConfig();
        }

        private void textBoxBin_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            WriteConfig();
        }
    }
}
