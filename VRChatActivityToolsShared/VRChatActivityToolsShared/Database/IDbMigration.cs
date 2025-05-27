using System.Data.Common;

namespace VRChatActivityToolsShared.Database
{
    public interface IDbMigration
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// DB本体とテーブルを生成する
        /// </summary>
        /// <param name="context"></param>
        void CreateDbAndTables(ActivityContextBase context);

        /// <summary>
        /// 現在のデータベースがどのバージョンで作成されたかを取得します。
        /// </summary>
        /// <returns>
        /// バージョン情報テーブルが無い場合は1を返します。
        /// バージョン情報テーブルがある場合はバージョンの数値を返します。
        /// </returns>
        int GetCurrentVersion(ActivityContextBase context);

        /// <summary>
        /// データベースを更新します。
        /// </summary>
        void UpgradeDatabase(ActivityContextBase context);
    }
}