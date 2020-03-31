using Model;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LangtonsLoops
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        const int SIZE = 200;

        Automaton _langtonsLoops = new Model.Automaton(SIZE);
        WriteableBitmap _bitmap = BitmapFactory.New(SIZE, SIZE);
        Color[] _cellColors = new Color[]
        {
            Colors.Black, Colors.Blue, Colors.Red, Colors.Green, Colors.Yellow, Colors.Magenta, Colors.White, Colors.Cyan
        };

        private bool _isRunning;
        private bool _isUpdating = false;

        DateTimeOffset _startTime;
        private int _stepCount = 0;
        private int _drawCount = 0;
         

        public MainWindow()
        {
            _startTime = DateTimeOffset.Now;
            InitializeComponent();
            _langtonsLoops.PropertyChanged += LangtonsHandle;
            this.Table.Source = _bitmap;

            ThreadPool.SetMaxThreads(14, 14);
        }

        private void InitializeLangtonLoops()
        {
            UpdateBitmap(_langtonsLoops.NextLives);
        }

        private void UpdateBitmap(byte[,] lives)
        {
            _bitmap.ForEach((x, y) => _cellColors[lives[x, y]]);
        }

        private async void LangtonsHandle(object sender, PropertyChangedEventArgs e)
        {
            _stepCount++;
            //画面の更新が終わっていなければ無視する
            if (_isUpdating) return;

            _isUpdating = true;

            //イベントで呼び出されるときに別スレッドなので、UIスレッドで実行されるようにする
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateBitmap(_langtonsLoops.NextLives);
                    _drawCount++;

                    TimeSpan duratino = DateTimeOffset.Now.Subtract(_startTime);
                    ProgressStr.Text = $"{(duratino.TotalMilliseconds /_stepCount / 1000.0):0.0000}秒";

                    int drawRate = (int)(_drawCount * 100.0 / _stepCount);
                    //ProgressStr.Text = $"描画率：{drawRate}%({_drawCount} / {_stepCount})";
                });
            });

            _isUpdating = false;
        }


        private async void StartAndStop_Click(object sender, RoutedEventArgs s)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                StartAndStop.Content = "Stop";
                await _langtonsLoops.RunLoopsAsync();
            }
            else
            {
                await _langtonsLoops.StopAsync();
                _isRunning = false;
                StartAndStop.Content = "Start";
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _langtonsLoops = new Automaton(SIZE);
            UpdateBitmap(_langtonsLoops.NextLives);
        }
    }
}
