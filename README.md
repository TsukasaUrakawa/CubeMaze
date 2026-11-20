# Cube Maze

コントローラーを傾けて立体迷路を回転させ、ボールをゴールまで導く3Dアクションゲームです。
ジャイロ操作を生かし、迷路の向きやボールの動きを見ながら進みます。

- ポートフォリオ: https://tsukasaurakawa.github.io/
- プレイ映像: https://youtu.be/9qpwIKrR8LE
- 制作期間: 2026年7月31日〜 制作中
- 体制: 個人制作
- 担当: 企画・実装

## 遊び方

対応コントローラー: Flydigi Vader 4 Pro / DualSense
対応環境: Windows

- コントローラーを傾ける: 迷路を回転させ、ボールを転がす
- 十字ボタン: ピッチ方向・ロール方向に90°回転
- Nボタン: 迷路の傾きを基準姿勢に戻し、観察モードに切り替え
- L・Rボタン: 観察モードでカメラを90°ずつ旋回
- Wボタン: 観察した向きを操作の基準にして、通常操作へ戻る

実行ファイルの配布は現在行っていません。ソースコードの公開のみです。

## 技術

- エンジン: Unity 6000.0.75f1
- 言語: C#
- 開発環境: Visual Studio
- ライブラリ: JoyShockLibrary / DOTween / Cinemachine
- 迷路の作成: ProBuilder

## 実装のポイント

### ジャイロ入力の調整

コントローラーの姿勢をそのまま迷路に反映すると、意図しないY軸方向の回転が操作に混ざっていました。
キャリブレーション時の基準姿勢との差から、Y軸まわりのねじれと傾きを分離し、必要な傾きだけを反映する構成にしています。

- `Assets/Project/Scripts/GyroController.cs` — ジャイロ制御

### 接続状態の管理

コントローラーの検索・選択・キャリブレーション・使用中を状態として分け、切断時は接続準備へ戻る構成です。接続状態の変更はイベントでUIへ通知します。

- `Assets/Project/Scripts/DeviceConnectManager.cs` — 接続管理

### カメラと操作方向の連携

Cinemachineを使い、観察モードではボールの周囲を90度ずつ見回せるようにしています。観察後はカメラの水平な向きを操作の基準にし、見ている方向と手元の傾きが対応する構成です。
回転は補間して滑らかにし、迷路への反映には `FixedUpdate` と `Rigidbody.MoveRotation` を使用しています。

- `Assets/Project/Scripts/CameraOrbitController.cs` — カメラ制御

## 主なスクリプト

`Assets/Project/Scripts/` 配下の主なファイルです。

- `GyroController.cs` — ジャイロ入力と迷路の回転
- `DeviceConnectManager.cs` — コントローラーの接続・状態管理
- `CameraOrbitController.cs` — 観察モードのカメラ制御
- `GoalTriggerController.cs` — ゴール判定
- `MazeCutOutController.cs` — 迷路の切り欠き・演出まわり
- `Respawn.cs` — リスポーン処理
- `PersistentSystem.cs` — 永続化まわり
- `TitleTextController.cs` — タイトル表示まわり

## 開き方

1. このリポジトリをクローンします。
2. Unity 6000.0.75f1 でプロジェクトを開きます。
3. 対応コントローラーを接続してプレイします。

## クレジット

- 迷路の作成・配置: ProBuilder
- 使用ライブラリ: JoyShockLibrary / DOTween / Cinemachine
