using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WPF_HtmlTagCounter.ViewModels
{
    public class UrlViewModel : INotifyPropertyChanged
    {
        private string url;
        private int tagCount;

        public UrlViewModel(string Url)
        {
            url = Url;
        }

        public string Url
        {
            get { return url; }
            set
            {
                if (url == value)
                    return;
                url = value;
                OnPropertyChanged();
            }
        }

        public int TagCount
        {
            get { return tagCount; }
            set
            {
                if(tagCount == value)
                    return ;
                tagCount = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
