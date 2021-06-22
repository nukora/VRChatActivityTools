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

```
VRChatActivityLogger.exe -console false
```

### VRChatActivityLogViewer.exe
データベースの内容をGUIで表示します。

データベースをまだ作成していない場合は、先にVRChatActivityLogger.exeを実行してください。

画面上のLoggerボタンをクリックする事でも実行する事ができます。

### VRChatActivityLog.db
VRChatActivityLogger.exeを実行すると作成されるデータベースファイルです。

中身はSQLite3のデータなので、他のアプリと連携したりもできると思います。

## 既知の問題

inviteの送信履歴などから送信先となるユーザ名を表示する事はできません。VRChatのログにユーザ名が記録されない為です。

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

### VRChatActivityLog.db
This is the database file that is created when you run VRChatActivityLogger.exe.

The contents are SQLite3 data, so it can be used in conjunction with other applications.

##  Known issues

It is not possible to display the name of the user to whom invitations are sent from the invitations sending history, because the user name is not recorded in the VRChat log.

## License  

This software is released under the MIT License, see LICENSE.
