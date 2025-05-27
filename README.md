VRChatActivityTools
====

VRChatのログを解析し、joinしたワールドや会った人の履歴などをデータベースに保存します。
Analyzes VRChat logs and stores the history of worlds joined and people met in a database. (English documentation is available under the Japanese version)

# 日本語

## インストール

### ダウンロード

以下のページからダウンロードする事ができます。

https://github.com/nukora/VRChatActivityTools/releases

Boothでの配布も行っていますので、そちらからもダウンロードできます。

https://nukora.booth.pm/items/1690568

もしくは自分でコンパイルしてください。

### VRChatActivityTools_v.0.0.zip

起動に別途 .NET Core 3.1 Runtime のインストールが必要になりますが、ファイルサイズが小さく、起動にかかる時間も短いです。

ランタイムは以下の場所にあります。

https://dotnet.microsoft.com/download/dotnet-core/3.1

.NET Core RuntimeとDesktop Runtimeの最新版をインストールしてください。

InstallersのWindowsのx64を選択すれば大丈夫だと思います。

### VRChatActivityTools_v.0.0_SelfContained.zip

ランタイムのインストール無しで起動できますが、ファイルサイズが大きく、初回起動にかなりの時間がかかります。

なるべくランタイムをインストールする事をおすすめします。

### インストール

ダウンロードしたzipファイルを解凍して適当なフォルダに配置してください。

※Program Filesなどの書き込み制限のあるフォルダには置かないでください。

このプログラムは64bit Windows専用です。

## 使い方

### VRChatActivityLogger.exe
VRChatのログを解析し、活動履歴のデータベースを作成します。

既にデータベースが作成されている場合は追加登録されていきます。

タスクスケジューラなどで定期実行されるようにすると便利かもしれません。

コンソール画面を表示しないで実行したい場合は以下の起動オプションを使用してください。

Windows以外では本起動オプションは無効となります。
```
VRChatActivityLogger.exe -console false
```

### VRChatActivityLogViewer.exe
データベースの内容をGUIで表示します。

データベースをまだ作成していない場合は、先にVRChatActivityLogger.exeを実行してください。

画面上のLoggerボタンをクリックする事でも実行する事ができます。

### appsettings.json

記入例は下記です。
``` json
{
  "ConnectionStrings": {
    "VRChatActivityLog": "Server=localhost;Database=VRChatActivityLog;Username=usr;Password=pswd;"
  },
  "DbKind": "MariaDB",

  "VRChat": {
    "LogFileDir": ""
  }
}
```
#### `.ConnectionStrings.VRChatActivityLog`
DBへの接続文字列です。

SQLiteモードのみ、空文字を指定すると`(カレントディレクトリ)\VRChatActivityLog.db`となります。

詳細は各EFCoreのドキュメントをご覧ください。

[MySQL .NET Connection String Options](https://mysqlconnector.net/connection-options/)

[Entity Framework Core > Microsoft.Data.Sqlite > Basic usage](https://github.com/dotnet/efcore?tab=readme-ov-file#basic-usage-1)

#### `.ConnectionStrings.DbKind`
使用するDBの種類です。下記をお使いください。

- `"MariaDB"`
- `"SQLite"`

Note: MySQLでも`"MariaDB"`を指定してください。相違はEFCoreが自動調整します。

#### `.VRChat.LogFileDir`
VRChatのログディレクトリです。

空文字にすると自動的にローカルマシンのアプリケーションデータフォルダとなります。

出力先を変えている方だけご利用ください。

#### 開発時
ローカル環境では`appsettings.json`ではなく`appsettings.Development.json`を使用してください。
`appsettings.Development.json`が存在する場合は`appsettings.json`よりも`appsettings.Development.json`が優先されます。

1. `appsettings.json`をプログラムディレクトリに`appsettings.Development.json`としてコピーする
2. `appsettings.Development.json`を各自環境の設定に書き換える
3. バイナリを実行する

### VRChatActivityLog.db
SQLiteモードでVRChatActivityLogger.exeを実行すると作成されるデータベースファイルです。

中身はSQLite3のデータなので、他のアプリと連携したりもできると思います。

ただし、接続文字列の特性上本ファイルのファイル名は可変です。

## 既知の問題

inviteの送信履歴などから送信先となるユーザ名を表示する事はできません。VRChatのログにユーザ名が記録されない為です。

ネットワーク上のDBへ接続出来るか確認ができません。EFCoreの`CanConnect()`が「DBが存在するかを返す」為です。

## ライセンス

このプログラムにはMITライセンスが適用されます。

# English

## Installation

### Download

You can download it from the following page.

https://github.com/nukora/VRChatActivityTools/releases

You can also download it from Booth.

https://nukora.booth.pm/items/1690568

Or you can compile it by yourself.

### VRChatActivityTools_v.0.0.zip

NET Core 3.1 Runtime must be installed separately to start, but the file size is small and the startup time is short.

The runtime can be found at : 

https://dotnet.microsoft.com/download/dotnet-core/3.1

Install the latest versions of .NET Core Runtime and Desktop Runtime.

If you select Windows x64 for Installers, I think you will be fine.

### VRChatActivityTools_v.0.0_SelfContained.zip

You can start the program without installing the runtime, but the file size is large and it will take a long time to start the first time.

We recommend that you install the runtime if possible.

### Installation

Unzip the downloaded zip file and place it in an appropriate folder.

Do not place it in a write-restricted folder such as Program Files.

This program is for 64bit Windows only.

## How to use :

### VRChatActivityLogger.exe  

Analyzes the VRChat log and creates a database of activity history.

If a database has already been created, it will be added to the database.

It may be useful to use a task scheduler or similar to run it periodically.

If you want to run without displaying the console screen, use the following startup option.

```
VRChatActivityLogger.exe -console false
```

### VRChatActivityLogViewer.exe

Displays the contents of the database in GUI.

If you have not yet created a database, run VRChatActivityLogger.exe first.

You can also run it by clicking the Logger button on the screen.

### appsettings.json

The sample is below.

``` json
{
  "ConnectionStrings": {
    "VRChatActivityLog": "Server=localhost;Database=VRChatActivityLog;Username=usr;Password=pswd;"    
  },
  "DbKind": "MariaDB",

  "VRChat": {
    "LogFileDir": ""
  }
}
```

#### `.ConnectionStrings.VRChatActivityLog`

Connection string to your DB.

In only SQLite mode, if you set empty string, this path set to `(current dir.)\VRChatActivityLog.db`.

For more details, see also each EFCore documents.

[MySQL .NET Connection String Options](https://mysqlconnector.net/connection-options/)

[Entity Framework Core > Microsoft.Data.Sqlite > Basic usage](https://github.com/dotnet/efcore?tab=readme-ov-file#basic-usage-1)

#### `.ConnectionStrings.DbKind`

Kind for your DB. Please use below.

- `"MariaDB"`
- `"SQLite"`

Note: If you want to use MySQL, specify `"MariaDB"`. The deference will be fixed by EFCore.

#### `.VRChat.LogFileDir`

Path for VRChat log directory.

If empty specified, automatically set your local machine's application data folder.

Only who changes log output folder can use this option.

#### Development
In local develop enviroment, you can use `appsettings.Development.json` instead of `appsettings.json`.
If `appsettings.Development.json` exists in the directory, `appsettings.Development.json` will be prioritized than `appsettings.json`.

1. Copy `appsettings.json` as `appsettings.Development.json` to your program directory.
2. Edit `appsettings.Development.json` as your enviroment.
3. Run or debug your binary.
### VRChatActivityLog.db
This is the database file that is created when you run VRChatActivityLogger.exe.

The contents are SQLite3 data, so it can be used in conjunction with other applications.

This file name/path are variable due to the connection string.
##  Known issues

It is not possible to display the name of the user to whom invitations are sent from the invitations sending history, because the user name is not recorded in the VRChat log.

## License  

This software is released under the MIT License, see LICENSE.
