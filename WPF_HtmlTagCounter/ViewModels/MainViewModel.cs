using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPF_HtmlTagCounter.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool inProgressFlag;
        
        private ICommand loadFileCommand;
        private ICommand startCalcCommand;
        private ICommand stopCalcCommand;

        public ICommand LoadFileCommand
        {
            get
            {
                return loadFileCommand ??
                    (loadFileCommand = new DelegateCommand(
                        obj => LoadFromFile(),
                        flag => !inProgressFlag
                    ));
            }
        }

        public ObservableCollection<UrlViewModel> Urls { get; set; }

        public MainViewModel()
        {
            Urls = new ObservableCollection<UrlViewModel>();
        }

        private void LoadFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            if(openFileDialog.ShowDialog() == true)
            {
                var fileLines = File.ReadAllLines(openFileDialog.FileName);
                Urls.Clear();
                foreach (var url in fileLines)
                {
                    Urls.Add(new UrlViewModel(url));
                }

            }
        }
    }
}
