using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TextSeeker
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Visibility _textBlockVisibility;

        public ViewModel() 
        {
            TextBlockVisibility = Visibility.Collapsed;
        }

        public Visibility TextBlockVisibility
        {
            get => _textBlockVisibility;
            set
            {
                if (_textBlockVisibility != value)
                {
                    _textBlockVisibility = value;
                    OnPropertyChanged(nameof(TextBlockVisibility));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
