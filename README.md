VRChatActivityTools
====

VRChatのログを解析し、joinしたワールドや会った人の履歴などをデータベースに保存します。

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

This software is released under the MIT License, see LICENSE.txt.

