using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BertAnalizator;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System;

namespace WPF_Bert_indiv
{
    public partial class MainWindow : Window
    {
        private Berttokanalizator bertAnalizator = new Berttokanalizator();
        private ObservableCollection<DialogItem> dialogHistory = new ObservableCollection<DialogItem>();
        private CancellationTokenSource cancellationTokenSource;
        private string selectedFilePath;
        private const string HistoryFileName = "dialog_history.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadDialogHistory();
            DataContext = this;
        }
        public class DialogItem
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }
        private async void LoadTextButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                string text = File.ReadAllText(selectedFilePath);
                TextDisplay.Text = text;

                dialogHistory.Clear();
                DialogHistoryItemsControl.ItemsSource = dialogHistory;

                await bertAnalizator.Exist_Download_Model();
            }
        }
        private async void GetAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            string question = QuestionInput.Text;

            AddToDialogHistory(item: new DialogItem { Question = question });

            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                string answer = await bertAnalizator.QA_text_Model(TextDisplay.Text, question, cancellationTokenSource.Token);
                AddToDialogHistory(item: new DialogItem { Answer = answer });
            }
            catch (TaskCanceledException)
            {
                AddToDialogHistory(item: new DialogItem { Answer = "Answer operation cancelled" });
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }
        private void AddToDialogHistory(DialogItem item)
        {
            dialogHistory.Add(item);
            SaveDialogHistory();
        }
        private void SaveDialogHistory()
        {
            string json = JsonConvert.SerializeObject(dialogHistory);
            File.WriteAllText(HistoryFileName, json);
        }
        private void LoadDialogHistory()
        {
            if (File.Exists(HistoryFileName))
            {
                try
                {
                    string json = File.ReadAllText(HistoryFileName);
                    dialogHistory = JsonConvert.DeserializeObject<ObservableCollection<DialogItem>>(json);
                    DialogHistoryItemsControl.ItemsSource = dialogHistory;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"failed to load dialog history: {ex.Message}");
                }
            }
        }
    }
}