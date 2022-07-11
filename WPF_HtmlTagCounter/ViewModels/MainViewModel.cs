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
using WPF_HtmlTagCounter.Models;

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
                        async obj => await LoadFromFileAsync(),
                        flag => !inProgressFlag
                    ));
            }
        }

        public ICommand StartCalcCommand
        {
            get
            {
                return startCalcCommand ??
                    (startCalcCommand = new DelegateCommand(
                        async obj => await StartCalcAsync(),
                        flag => !inProgressFlag
                    ));
            }
        }

        public ObservableCollection<UrlViewModel> Urls { get; set; }

        public MainViewModel()
        {
            Urls = new ObservableCollection<UrlViewModel>();
        }

        private async Task LoadFromFileAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            if(openFileDialog.ShowDialog() == true)
            {
                var fileLines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                Urls.Clear();
                foreach (var url in fileLines)
                {
                    Urls.Add(new UrlViewModel(url));
                }
            }
        }

        private async Task StartCalcAsync()
        {

            foreach (var item in Urls)
            {
                item.TagCount = await CounterLogic.StartCounterAsync(item.Url);
            }
        }
    }
}
