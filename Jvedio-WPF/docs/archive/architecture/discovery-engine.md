> **状态**：已归档（2026-06-30）。活跃待办见 [BACKLOG.md](../../BACKLOG.md)。

# Discover 层：IFileDiscoveryBackend

## 定位

Jvedio 扫描 Pipeline 分为 Discover → Parse → Classify → Persist。本模块仅升级 **Discover**（文件枚举），Parse 及之后阶段不变。

## 接口

```csharp
interface IFileDiscoveryBackend {
    DiscoveryResult Discover(DiscoveryRequest request, IProgress<DiscoveryProgress> progress, CancellationToken ct);
}
```

### DiscoveryRequest

| 字段 | 说明 |
|------|------|
| `RootPaths` | 待递归扫描的根目录 |
| `FilePaths` | 单文件直扫（可选） |
| `ExtensionFilter` | 扩展名过滤（可选） |
| `Mode` | `Full` / `Incremental` |
| `VolumeKey` | 目录指纹缓存分区键，通常为 `DBId` |

### DiscoveryResult

| 字段 | 说明 |
|------|------|
| `FilePaths` | 枚举到的文件路径 |
| `Timing` | 分阶段耗时（对标 SpaceSniffMax `ScanTiming`） |
| `FromCache` | 增量模式下是否跳过磁盘遍历 |

## 实现

| 类 | 场景 |
|----|------|
| `SimpleDirDiscovery` | 默认；包装现有 `DirHelper.GetFileList` |
| `ParallelDirDiscovery` | 多根路径、大目录；`Parallel.ForEach` |
| `CachedIncrementalDiscovery` | 启动扫描 / NAS；SQLite `scan_dir_index` 目录指纹 |

工厂：`DiscoveryBackendFactory.Create()` 按 `ScanConfig` 选择后端。

## 配置（ScanConfig）

- `UseParallelDiscovery` — 启用并行遍历
- `DiscoveryThreadCount` — 线程数（0 = `ProcessorCount × 2`）
- `EnableDirIndexCache` — 启用目录指纹缓存

UI 入口：设置 → 扫描与导入。详见 [ui-evolution.md](ui-evolution.md#discover-配置ui)。

## 与 ScanEngine 的关系

- `ScanEngine.Discover(library, options)` — 仅执行 Discover，供测试或预检
- `ScanEngine.StartWatching` — `LibraryMonitor` 触发增量 job
- `VideoScanPipeline.DiscoverPaths` — 通过 `DiscoveryBackendFactory` 注入 backend

> **归档注（2026-06-30）**：Game/Picture Pipeline 已接入 `GameScanPipeline` / `PictureScanPipeline`；Discover 后端工厂仍以 Video 路径为主，Game/Picture 可按需扩展 `DiscoveryBackendFactory`。

## 目录指纹（scan_dir_index）

```
scan_dir_index(db_id, dir_path, mtime, entry_count, children_checksum, last_scan_utc)
```

增量逻辑：对比各根目录**顶级子目录**指纹；未变更子树不重扫，与 `ExistVideoIndex`（库内去重）分工。
