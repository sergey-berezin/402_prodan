using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BertAnalizator;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPF_Bert_indiv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public abstract class Components : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
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

            AddToDialogHistory(question);

            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                string answer = await bertAnalizator.QA_text_Model(TextDisplay.Text, question, cancellationTokenSource.Token);
                AddToDialogHistory(answer);
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