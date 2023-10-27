using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BertWPF
{
    public abstract class ComponentsBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}