> 归档摘要，来源：Cursor plan「Core Scan 模块解耦」（2026-06-30）。五阶段 todo 全部 completed。

# Core/Scan 解耦 — 计划摘要

## 已完成

1. `ScanExtensions` 独立 + `HashPathIndex` 基类 + Picture 索引化
2. `VideoImportClassifier` 规则链 + `HashPathImportClassifier`
3. `VideoScanPipeline`，`ScanTask.DoWork` 委托 Pipeline
4. Game/Picture/Comic Pipeline 化 + `ScanFactory` 注册表
5. `IScanStore` + Mapper 默认实现

## 后续扩展点（原 plan 提及，未单独排期）

- `IVideoImportRule` 插件式注册（规则链已内聚，未暴露注册 API）
- `ScanTask` 完全退化为薄壳（仍保留 NFO/视频专用方法）
- `ComicScan` 独立 Factory 注册（仍委托 Picture Pipeline）

详细原文见 `.cursor/plans/core_scan_模块解耦_000fdad2.plan.md`。
