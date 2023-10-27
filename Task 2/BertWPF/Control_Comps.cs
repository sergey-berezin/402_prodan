using BertAnalizator;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BertWPF
{
    internal class Control_Comps : Components
    {
        private Berttokanalizator berttokanalizator;
        public ObservableCollection<Tab_Elem> Tab_Inset { get; set; } = new ObservableCollection<Tab_Elem>();
        public int Select_Tab { get; set; }
        private int tab_cnt = 1;

        private readonly IErrorSender error_report;
        private readonly IFileDialog dialoge;
        public ICommand New_Tab { get; set; }
        public ICommand Remove_Tab { get; set; }

        public Control_Comps(IErrorSender error_report, IFileDialog dialoge)
        {
            this.dialoge = dialoge;
            this.error_report = error_report;
            New_Tab = new Compil_Command(o => 
            { 
                New_TabHandler(); 
            });
            Remove_Tab = new Compil_Command(o => 
            { 
                Remove_TabHandler(o); 
            });
        }
        public async void BertAnalizator()
        {  
            berttokanalizator = new Berttokanalizator();
            var createTask = berttokanalizator.Exist_Download_Model();
            await createTask;
        }
        private void New_TabHandler()
        {
            Tab_Inset.Add(new Tab_Elem(string.Format("Tab {0}", tab_cnt), berttokanalizator, error_report, dialoge));
            Select_Tab = Tab_Inset.Count - 1;
            RaisePropertyChanged("Tab_Inset");
            RaisePropertyChanged("Select_Tab");
            tab_cnt++;
        }
        private void Remove_TabHandler(object sender)
        {
            Tab_Elem item = sender as Tab_Elem;
            string tab_name = item.Tab_Enum;
            int index = Tab_Inset.IndexOf(item);
            if (Select_Tab == index)
                Select_Tab = Select_Tab - 1;
            Tab_Inset.RemoveAt(index);
            RaisePropertyChanged("Tab_Inset");
            RaisePropertyChanged("Select_Tab");

        }
    }
}
