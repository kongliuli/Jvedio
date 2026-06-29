# UI-016 — 列表控件策略（Adapter 方案 A）

**状态**：关闭（暂不拆控件）  
**日期**：2026-06-30

## 决策

Picture / Game **不**单独拆 `PictureList` / `GameList` 控件，继续使用 `VideoList` + `MediaListMode` + `ApplyListModeColumns()`。

## 理由

- `ProfileTabContentFactory` 已按 Profile 注入 Tab 内容。
- `IMediaListTab` 抽象已覆盖列策略差异。
- 拆控件仅增加 XAML/code-behind 重复，无独立交互模型。

## 重开条件

某 DataType 需要与 Video 完全不同的右键菜单、拖拽行为或多选逻辑，且 Adapter 分支 > 5 处。
