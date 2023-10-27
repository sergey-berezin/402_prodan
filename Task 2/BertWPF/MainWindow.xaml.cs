using System;
using System.Windows;
using Microsoft.Win32;

namespace BertWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Control_Comps control_compts = new Control_Comps(new Error_ReporterBox(), new Load_Dialoge());
            control_compts.BertAnalizator();
            DataContext = control_compts;
        }
        public class Error_ReporterBox : IErrorSender
        {
            public void Error_Reporter(string message)
            {
                MessageBox.Show(message);
            }
        }
        public class Load_Dialoge : IFileDialog
        {
            public string OpenFileDialog()
            {  
                string txtfile = "";
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                    txtfile = openFileDialog.FileName;
                return txtfile;
            }
        }
    }
}