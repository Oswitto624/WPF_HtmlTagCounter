using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private int maxCount;
        private int totalCount;

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

        public int TotalProgress
        {
            get { return totalCount; }
            set
            {
                totalCount = value;
                OnPropertyChanged();
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private void StartCalc()
        {
            maxCount = 0;
            inProgressFlag = true;
            cancelToken = new CancellationTokenSource();
            var progress = new Progress<int>();

            var taskFactory = Task.Factory.StartNew(() => 
            {
                foreach (var item in Urls)
                {
                    var task = Task.Factory.StartNew(() =>
                    {
                        item.TagCount = CounterLogic.StartCounterAsync(item.Url, progress, cancelToken.Token).Result;
                        CheckMaxCount(item);

                        progress.ProgressChanged += (sender, e) =>
                        {
                            item.ReadingProgress = e;
                            CalcTotalProgress();
                        };

                    }, cancelToken.Token);
                }
                Task.WhenAll();
                
            }, cancelToken.Token);

            taskFactory.Wait();
        }

        private void StopProcess()
        {
            cancelToken.Cancel();
            inProgressFlag = false;
        }

        private void CheckMaxCount(UrlViewModel item)
        {
            if (item.TagCount >= maxCount)
            {
                foreach (var resultItem in Urls)
                {
                    resultItem.HasMaxCount = false;
                }

                maxCount = item.TagCount;
                item.HasMaxCount = true;
            }
        }

        private void CalcTotalProgress()
        {
            if (Urls.Count == 1)
                TotalProgress = 100;
            TotalProgress = Urls.Sum(t => t.ReadingProgress) / Urls.Count;
        }
    }
}
