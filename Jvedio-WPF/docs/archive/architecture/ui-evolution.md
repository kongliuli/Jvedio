> **状态**：已归档（2026-06-30）。活跃待办见 [BACKLOG.md](../../BACKLOG.md)。

# Jvedio UI 演进

## 目标

将 WPF 界面从 Video 中心壳演进为按 `DataType` 策略化的多类型媒体库 UI。后端扫描/元数据 Pipeline 已就绪，本模块聚焦表现层。

## 策略中心：MediaUIHost

[`MediaUIHost.cs`](../../Jvedio/Core/UI/MediaUIHost.cs) 注册 `IMediaUIProfile`：

- `InitSideMenu` — 侧栏控件（Video / Picture / Game / Comics）
- `SettingsSections` — 设置页 Tab 可见性（Phase D）
- 导航：`ISideNavigation` / `SideNavigationRouter` 按 DataType 打开 Tab

## Phase 概要

| Phase | 内容 | 归档时状态 |
|-------|------|------------|
| A | Discover 设置项、ScanPath 保存刷新监听、ScanComplete 泛化、DataType 切换重建 UI | 已完成 |
| B | GameSideMenu、SideNavigationRouter、侧栏 Statistic 修复 | 已完成 |
| C | TabItemManager 别名、VideoList `MediaListMode`、`MediaDetailsFactory` | 骨架完成，深度去 Video 化见 BACKLOG |
| D | 设置分区、TaskList DataType 徽章、ScanDetail NFO 列按类型隐藏 | 部分完成，见 BACKLOG |

## Discover 配置（UI）

设置 → 扫描与导入：

- `UseParallelDiscovery` — 并行目录枚举
- `DiscoveryThreadCount` — 线程数（0=自动）
- `EnableDirIndexCache` — SQLite 目录指纹增量

详见 [discovery-engine.md](discovery-engine.md)。

## ADR-UI

- ADR-UI-001：扩展 MediaUIHost，不新建 UI 框架
- ADR-UI-004：单设置窗体 + `SettingsSectionMask` 分区
