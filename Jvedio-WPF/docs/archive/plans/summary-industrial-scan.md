> 归档摘要，来源：Cursor plan「工业扫描引擎评估」（2026-06-30）。Plan todo 全部 completed。

# Discover 工业化 — 计划摘要

## 已完成

- PR-A：`IFileDiscoveryBackend` + `SimpleDirDiscovery` + `DiscoveryTiming`
- PR-B：`ParallelDirDiscovery` + ScanConfig 开关 + `StartWatching` UI 接线
- PR-C：SQLite `scan_dir_index` + `CachedIncrementalDiscovery`
- 文档：`discovery-engine.md`、`spacesniffmax-comparison.md`

## 有意不移植

MFT 直读、Magika 去重、Filter DSL、TreeMap、sled 全树缓存 — 见 `spacesniffmax-comparison.md`。

## 遗留（非 plan todo，实现缺口）

- Game/Picture/Comics Pipeline 未走 `DiscoveryBackendFactory`（仅 Video）
- Discover 预检 UI（`ScanEngine.Discover` 按钮）未做
- TaskList/ScanDetail 未展示 Discover 分阶段耗时（仅 Logs 字符串）

详细原文见 `.cursor/plans/工业扫描引擎评估_75b5b97f.plan.md`。
