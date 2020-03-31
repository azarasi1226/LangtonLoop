using System.ComponentModel;
using System.Threading.Tasks;

namespace Model
{
    class Automaton: INotifyPropertyChanged
    {
        // オートマトンのルール
        Rule _rule = new Rule();

        // 「世界」のサイズ（正方形の一辺）
        readonly int _size;

        // 「世界」
        private byte[,] _lives;
        private byte[,] _nextLives;
        public byte[,] NextLives { get { return _nextLives; }}

        // 初期値
        private readonly byte[,] Default_lives =
        {
            {0,2,2,2,2,2,2,2,2,0,0,0,0,0,0},
            {2,1,7,0,1,4,0,1,4,2,0,0,0,0,0},
            {2,0,2,2,2,2,2,2,0,2,0,0,0,0,0},
            {2,7,2,0,0,0,0,2,1,2,0,0,0,0,0},
            {2,1,2,0,0,0,0,2,1,2,0,0,0,0,0},
            {2,0,2,0,0,0,0,2,1,2,0,0,0,0,0},
            {2,7,2,0,0,0,0,2,1,2,0,0,0,0,0},
            {2,1,2,2,2,2,2,2,1,2,2,2,2,2,0},
            {2,0,7,1,0,7,1,0,7,1,1,1,1,1,2},
            {0,2,2,2,2,2,2,2,2,2,2,2,2,2,0},
        };

        // 変更を画面に通知するためのイベント
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { ; };

        //停止用フラグ
        private bool _isRunning;
        private bool _isStopped = true;

        // コンストラクター（配列のサイズを与える）
        public Automaton(int size)
        {
            _size = size;
            Create();
        }

        // _lives配列を作成し、初期状態を設定する
        private void Create()
        {
            _lives = new byte[_size, _size];
            _nextLives = new byte[_size, _size];

            
            int defaultRow   = (_size - Default_lives.GetLength(0)) / 2;
            int defaultColmn = (_size - Default_lives.GetLength(1)) / 2;

            // 世界の中心に初期の値を設定
            for(int r0 = 0; r0 < Default_lives.GetLength(0); r0++)
            {
                for(int c0 = 0; c0 < Default_lives.GetLength(1); c0++)
                {
                    int r = defaultRow + r0;
                    int c = defaultColmn + c0;
                    _lives[r, c] = Default_lives[r0, c0];
                }
            }
        }

        // 計算ロジック（処理に時間がかかります）
        private void Update()
        {
            Parallel.For(1, _size - 1, (row) =>
            {
                for (int col = 1; col < _size - 1; col++)
                {
                    var north = _lives[row - 1, col];
                    var east  = _lives[row, col + 1];
                    var south = _lives[row + 1, col];
                    var west  = _lives[row, col - 1];

                    _nextLives[row, col] = _rule.Next(c:_lives[row, col],
                                                      n:north,
                                                      e:east,
                                                      s:south,
                                                      w:west);
                }
            });

            var temp = _lives;
            _lives = _nextLives;
            _nextLives = temp;
        }

        // 計算ロジックを並列で実行し、処理が終わったら更新を呼び出す。
        public async Task RunLoopsAsync()
        {
            _isRunning = true;
            _isStopped = false;

            await Task.Run(() =>
            {
                while (_isRunning)
                {
                    Update();

                    // Update()を極力回すため、非同期メソッドが呼ばれるように
                    PropertyChanged(this, new PropertyChangedEventArgs(null));
                }
            });

            _isStopped = true;
        }

        // 計算を止める
        public async Task StopAsync()
        {
            _isRunning = false;

            while (!_isStopped)
            {
                await Task.Delay(10);
            }
        }
    }
}


