using BertAnalizator;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BertWPF
{
    public abstract class Components : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class Async_Compil_Command : ICommand
    {
        private readonly Func<object, bool> can_exe;
        private readonly Func<object, Task> async_exe;
        private bool isexec;

        public Async_Compil_Command(Func<object, Task> async_exe, Func<object, bool>? can_exe = null)
        {
            this.async_exe = async_exe ?? throw new ArgumentNullException(nameof(async_exe));
            this.can_exe = can_exe ?? throw new ArgumentNullException(nameof(can_exe));
        }
        public event EventHandler? CanExecuteChanged
        {
            add 
            { 
                CommandManager.RequerySuggested += value; 
            }
            remove 
            { 
                CommandManager.RequerySuggested -= value; 
            }
        }
        public bool CanExecute(object? parameter)
        {
            if (isexec)
            {
                return false;
            }
            else
            {
                return can_exe is null || can_exe(arg: parameter);
            }
        }
        public void Execute(object? parameter)
        {
            if (!isexec)
            {
                isexec = true;
                _ = async_exe(arg: parameter).ContinueWith(_ =>
                {
                    isexec = false;
                    CommandManager.InvalidateRequerySuggested();
                }, scheduler: TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
    }
    class Compil_Command : ICommand
    {
        private readonly Action<object> exec;
        private readonly Func<object, bool> can_exec;
        public Compil_Command(Action<object> exec, Func<object, bool>? can_exec = null)
        {
            this.exec = exec;
            this.can_exec = can_exec;
        }
        public event EventHandler? CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
        public bool CanExecute(object parameter)
        {
            return can_exec == null ? true : can_exec(parameter);
        }
        public void Execute(object parameter)
        {
            exec?.Invoke(parameter);
        }
    }
    public interface IErrorSender
    {
        void Error_Reporter(string message);
    }
    public interface IFileDialog
    {
        public string OpenFileDialog();
    }
    public class Tab_Elem : Components, IDataErrorInfo
    {
        private readonly IErrorSender error_reporter;
        private readonly IFileDialog file_dialog;
        private Berttokanalizator bertanalizmodel;
        private CancellationTokenSource token_source;
        public string Tab_Enum { get; set; }
        public bool Cancel_enabled { get; set; } = false;
        public string Text_From_File { get; set; } = "";
        public string Question_To_Text { get; set; } = "";
        public string Answer_From_Text { get; set; } = "No text? No answer";
        public string Error { get; }
        public ICommand Load_Text_File { get; private set; }
        public ICommand Get_Answer { get; private set; }
        public ICommand Cancel_Answer { get; private set; }
        public Tab_Elem(string tab_enum, Berttokanalizator bertanalizmodel, IErrorSender error_reporter, IFileDialog file_dialog)
        {
            this.Tab_Enum =  tab_enum;
            this.file_dialog = file_dialog;
            this.error_reporter = error_reporter;
            this.bertanalizmodel = bertanalizmodel;
            this.token_source = new CancellationTokenSource();
            Load_Text_File = new Compil_Command(_ => 
            { 
                Load_Text_File_Handler(); 
            });
            Cancel_Answer = new Compil_Command(_ => 
            {
                Cancel_enabled = false;
                RaisePropertyChanged("Cancel_enabled");
                token_source.Cancel();
                token_source = new CancellationTokenSource();
            });
            Get_Answer = new Async_Compil_Command(async _ =>
            {
                Cancel_enabled = true;
                RaisePropertyChanged("Cancel_enabled");
                CancellationToken token = token_source.Token;
                await Async_Question_Answering(bertanalizmodel, Text_From_File, Question_To_Text, token);
                Cancel_enabled = false;
                RaisePropertyChanged("Cancel_enabled");
            },
            _ =>
            {
                return string.IsNullOrEmpty(this["Text_From_File"]) && string.IsNullOrEmpty(this["Question_To_Text"]);
            });
        }
        private void Load_Text_File_Handler()
        {
            string filename = file_dialog.OpenFileDialog();
            if (!string.IsNullOrEmpty(filename))
            {
                Load_File(filename);
                RaisePropertyChanged("Text_From_File");
            }
        }
        private void Load_File(string filename)
        {
            Text_From_File = File.ReadAllText(path: filename);
        }
        private async Task Async_Question_Answering(Berttokanalizator berttokanalizator, string text, string question, CancellationToken token)
        {
            string answer = await berttokanalizator.QA_Text_Model(text, question, token); 
            Answer_From_Text = answer;
            RaisePropertyChanged("Answer_From_Text");
        }
        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "Text_From_File":
                        if (Text_From_File.Length == 0)
                        {
                            error = "Enter OR Load Text";
                        }
                        break;
                    case "Question_To_Text":
                        if (Question_To_Text.Length == 0)
                        {
                            error = "Enter question";
                        }
                        break;
                }
                return error;
            }
        }
    }
}
