using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BertAnalizator;
using Microsoft.Win32;
using System.Collections.Generic;

namespace WPF_Bert_indiv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private Berttokanalizator bertAnalizator = new Berttokanalizator();
        private List<string> dialogHistory = new List<string>();
        private CancellationTokenSource cancellationTokenSource;
        private string selectedFilePath;
        public MainWindow() => InitializeComponent();
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

            AddToDialogHistory($"Question: {question}"); //не вышло красить ответ и вопрос в разные цвета

            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                string answer = await bertAnalizator.QA_text_Model(TextDisplay.Text, question, cancellationTokenSource.Token);
                AddToDialogHistory($"Answer: {answer}");
            }
            catch (TaskCanceledException)
            {
                AddToDialogHistory("Answer operation cancelled");
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }
        private void AddToDialogHistory(string item)
        {
            dialogHistory.Add(item);
            DialogHistoryItemsControl.Items.Refresh();
        }
    }
}