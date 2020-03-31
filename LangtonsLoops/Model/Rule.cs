using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Model
{
    class Rule
    {
        // ルールがキーになっている辞書
        private Dictionary<string, int> _rules = new Dictionary<string, int>();

        public Rule()
        {
            // テキストファイルからルールの種を呼び出す
            var ruleSeed = File.ReadLines(@"C:\text\ルール.txt")
                            .Select(str => new
                            {
                                Key = str.Substring(0, 5),
                                Value = str[str.Length - 1] - '0'
                            })
                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // 読み込んだルールの種を咲かせる（4回転させる）
            foreach (var item in ruleSeed)
            {
                var c = item.Key[0];
                var n = item.Key[1];
                var e = item.Key[2];
                var s = item.Key[3];
                var w = item.Key[4];
                var v = item.Value;

                _rules[$"{c}{n}{e}{s}{w}"] = v;
                _rules[$"{c}{w}{n}{e}{s}"] = v;
                _rules[$"{c}{s}{w}{n}{e}"] = v;
                _rules[$"{c}{e}{s}{w}{n}"] = v;
            }
        }
       
        public byte Next(byte c, byte n, byte e, byte s, byte w)
        // c:中央 n:北 e：東 s:南 w:西 (それぞれ0~7の整数)
        {
            string key = $"{c}{n}{e}{s}{w}";

            // ガード節：もしキーが無かったら0で弾く
            if (!_rules.ContainsKey(key)) return 0;

            return (byte)_rules[key];
        }
    }
}
