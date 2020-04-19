using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Tsp.Wpf.Common
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void RefreshProperties()
        {
            GetType()?.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)?
                .Select(x => x.Name)?
                .ToList()
                .ForEach(OnPropertyChanged);
        }

        protected void SetValue<T>(ref T field, T value, [CallerMemberName]string name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }
            field = value;
            OnPropertyChanged(name);
        }
    }
}
