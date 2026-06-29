> **状态**：已归档（2026-06-30）。未移植项见 [BACKLOG.md](../../BACKLOG.md#discover--扫描后端)。

# SpaceSniffMax × Jvedio 扫描能力对照

## 结论

- **不**跨项目共用 Rust 扫描引擎（域不同：空间分析 vs 媒体入库）
- **在 Jvedio 内**抽象 `IFileDiscoveryBackend`，借鉴 SpaceSniffMax 的 Discover 层模式

## 能力对照

| 维度 | SpaceSniffMax | Jvedio（本迭代后） | 共享？ |
|------|---------------|-------------------|--------|
| 遍历 | jwalk 并行 + MFT | `SimpleDirDiscovery` / `ParallelDirDiscovery` | 模式可借，代码不共享 |
| 产物 | `FileNode` 完整目录树 | `List<string>` 文件路径 | 否 |
| 缓存 | sled + dir_index | SQLite `scan_dir_index` | 思路一致，存储不同 |
| 去重 | size/inode/Magika | `VideoImportClassifier` / VID | 不可替换 |
| 增量 | 顶级子目录 mtime+entry_count | `CachedIncrementalDiscovery` | 对齐 |
| 并行 | rayon | `Parallel.ForEach` | 对齐 |
| 进度 | 分 phase + snapshot | `DiscoveryProgress` + `DiscoveryTiming` | 对齐 |

## 未移植项（有意不做 / 待评估）

- MFT 直读（稳定性 + 管理员权限）
- Magika / 通用去重
- Filter DSL、TreeMap 建树
- sled 全树缓存

## ADR 摘要

| ID | 决策 |
|----|------|
| ADR-008 | 不跨 repo 抽象统一扫描引擎 |
| ADR-009 | Jvedio 仅抽象 `IFileDiscoveryBackend` |
| ADR-010 | 目录指纹缓存用 SQLite |
| ADR-011 | Classify 层保持 VID/Hash 领域规则 |

## 参考

- SpaceSniffMax：`walker.rs`、`incremental.rs`
- Jvedio：`Core/Scan/Discovery/`、`ScanEngine.cs`、`VideoScanPipeline.cs`
