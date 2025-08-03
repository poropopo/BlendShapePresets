# BlendShape Presets

Unity用のBlendShapeの値をエクスポート・インポートするエディタツールです。

## 概要

BlendShape Presetsは、UnityのSkinnedMeshRendererコンポーネントに設定されているBlendShapeの値を保存・復元するためのツールです。キャラクターの表情やアニメーションの設定を簡単に共有したり、バックアップを取ったりすることができます。

## 機能

- **エクスポート機能**
  - BlendShapeの値をJSONファイルとして保存
  - BlendShapeの値をクリップボードにコピー
  - 子オブジェクトを含めた一括エクスポート

- **インポート機能**
  - JSONファイルからBlendShapeの値を読み込み
  - クリップボードからBlendShapeの値を貼り付け
  - 子オブジェクトを含めた一括インポート

## 動作環境

- Unity 2022.3以降
- VRChat SDK 2022.1.1以降（VRChatアバター用途の場合）

## インストール方法

(VPM)[https://vpm.porop.dev/]からインストールしてください。

## 使用方法

### ツールウィンドウを開く

1. Unityのメニューバーから「Tools > BlendShape Presets」を選択します
2. BlendShape Presetsウィンドウが開きます

### エクスポート（書き出し）

1. **オブジェクトを選択**: HierarchyでBlendShapeを含むGameObjectを選択します
2. **設定を調整**: 
   - 「Include Child Objects」にチェックを入れると、子オブジェクトのBlendShapeも含めてエクスポートします
3. **エクスポート方法を選択**:
   - **「Export to File」**: JSONファイルとして保存します
   - **「Copy to Clipboard」**: クリップボードにコピーします

#### エクスポートされるデータ

- オブジェクト名とパス
- 各BlendShapeの名前、インデックス、重み値
- 複数のSkinnedMeshRendererがある場合は、すべてのデータが含まれます

### インポート（読み込み）

1. **オブジェクトを選択**: BlendShapeを適用したいGameObjectを選択します
2. **設定を調整**:
   - 「Include Child Objects」にチェックを入れると、子オブジェクトのBlendShapeにも適用します
3. **インポート方法を選択**:
   - **「Import from File」**: JSONファイルから読み込みます
   - **「Paste from Clipboard」**: クリップボードから貼り付けます

#### インポート時の動作

- オブジェクト名またはパスが一致するSkinnedMeshRendererを自動的に検索します
- 一致するBlendShape名が見つかった場合、その値を適用します
- 一致しないBlendShapeは無視されます

## データ形式

エクスポートされるJSONファイルの構造：

```json
{
  "rootObjectName": "ルートオブジェクト名",
  "meshDataList": [
    {
      "objectName": "オブジェクト名",
      "objectPath": "階層パス",
      "blendShapes": [
        {
          "name": "BlendShape名",
          "index": 0,
          "weight": 100.0
        }
      ]
    }
  ]
}
```

## 使用例

### 表情プリセットの作成

1. キャラクターの表情を手動で調整します
2. 表情が完成したら「Export to File」でプリセットを保存します
3. 後で「Import from File」で同じ表情を復元できます

## 注意事項

- BlendShapeが設定されていないオブジェクトではツールは動作しません
- インポート時は、BlendShape名が完全に一致する必要があります
- 異なるメッシュ間でのBlendShapeの互換性は保証されません

## トラブルシューティング

### 「No SkinnedMeshRenderer found」エラー

- 選択したオブジェクトにSkinnedMeshRendererコンポーネントがありません
- 子オブジェクトを含める場合は「Include Child Objects」にチェックを入れてください

### 「No blend shapes found」エラー

- 選択したオブジェクトのメッシュにBlendShapeが設定されていません
- 正しいメッシュが設定されているか確認してください

### インポート時に値が適用されない

- オブジェクト名またはBlendShape名が一致していない可能性があります
- エクスポート元とインポート先で同じメッシュ構造になっているか確認してください

## ライセンス

MIT License

Copyright (c) 2024 poropopo

詳細は[LICENSE](LICENSE)ファイルをご覧ください。
