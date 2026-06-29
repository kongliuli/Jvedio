# SCAN-004 — MFT 直读评估结论

**状态**：关闭（不移植）  
**日期**：2026-06-30

## 决策

Jvedio-WPF **不移植** SpaceSniffMax 的 MFT 直读 Discover 路径。

## 理由

1. 现有 `ParallelDirDiscovery` + `CachedIncrementalDiscovery` 已覆盖大库增量场景（SCAN-001）。
2. MFT 直读依赖 NTFS 私有 API，与跨平台/权限模型冲突。
3. 维护成本高，ROI 低于 Discover Pipeline 优化。

## 触发条件（若未来重开）

- 单库 100 万+ 文件且 Parallel 路径 P95 > 30s。
- 用户环境 100% NTFS 且可接受管理员权限。

## 备选

继续优化 `IFileDiscoveryBackend` 实现与 ScanConfig 预检（UI-014），而非引入 MFT。
