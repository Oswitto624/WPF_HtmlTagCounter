using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF_HtmlTagCounter.Models;

namespace WPF_HtmlTagCounter.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool inProgressFlag;

        private CancellationTokenSource cancelToken;

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
                        obj => StartCalc(),
                        flag => !inProgressFlag && Urls.Count != 0
                    ));
            }
        }

        public ICommand StopCalcCommand
        {
            get
            {
                return stopCalcCommand ??
                    (stopCalcCommand = new DelegateCommand(
                        obj => StopProcess(),
                        flag => inProgressFlag
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
            cancelToken = new CancellationTokenSource();

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

        private void StartCalc()
        {
            var taskFactory = Task.Factory.StartNew(async () => 
            {
                inProgressFlag = true;
                
                foreach (var item in Urls)
                {
                    var task = Task.Factory.StartNew(async () =>
                    {
                        item.TagCount = await CounterLogic.StartCounterAsync(item.Url, cancelToken.Token);
                    }, cancelToken.Token);
                }
                await Task.WhenAll();
                
            }, cancelToken.Token);

            if (taskFactory.IsCanceled)
                taskFactory.Dispose();
            
            inProgressFlag = false;   
        }

        private void StopProcess()
        {
            cancelToken.Cancel();
            inProgressFlag = false;
        }
    }
}
