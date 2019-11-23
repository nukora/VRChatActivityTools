using System;
using System.Collections.Generic;
using System.Text;

namespace VRChatActivityLogger
{
    /// <summary>
    /// コマンドライン引数のラップクラス
    /// </summary>
    class Argument
    {
        private string[] args;
        private Dictionary<string, string> namedParameters = new Dictionary<string, string>();

        /// <summary>
        /// 名前なし引数の一覧
        /// </summary>
        public List<string> NamelessParameters { get; set; } = new List<string>();

        /// <summary>
        /// 名前あり引数の一覧
        /// </summary>
        public Dictionary<string, string> NamedParameters
        {
            get
            {
                return namedParameters;
            }
            set
            {
                namedParameters = value;
                Initialize();
            }
        }

        /// <summary>
        /// 引数を解析します。
        /// </summary>
        /// <param name="args"></param>
        public Argument(string[] args)
        {
            this.args = args;
            Initialize();
        }

        private void Initialize()
        {
            NamelessParameters.Clear();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Length > 1 && args[i].StartsWith("-"))
                {
                    var key = args[i].Substring(1);
                    var value = "";
                    while (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        i++;
                        value += " " + args[i];
                    }
                    NamedParameters[key] = value.Trim();
                }
                else
                {
                    NamelessParameters.Add(args[i]);
                }
            }
        }
    }
}
