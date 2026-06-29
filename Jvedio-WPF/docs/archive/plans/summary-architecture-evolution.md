> 归档摘要，来源：Cursor plan「Jvedio 演进架构文档」（2026-06-30）。Plan 内 todo 均标记 completed；下列**深层缺口**已转入 [BACKLOG.md](../../BACKLOG.md)。

# 五层架构演进 — 计划摘要

## 已完成（Plan todo）

- Phase 1：ScanFactory 接 UI、Pipeline 生命周期、ScanResult 泛化
- Phase 2：LibraryMonitor、INfoImportService、ScanEngine 门面
- Phase 3：IMetadataProvider 链 + MetadataMerger 骨架
- Phase 4：TaskHub 枚举与 AddTask 路由
- Phase 5：LibraryContext + MediaUIHost 注册

## 目标态（北极星）

`LibraryContext` + `ScanEngine` + `MetadataEngine` + `TaskHub` + `MediaUIHost` 驱动全栈；`Main.CurrentDataType` 降级为兼容层。

## 五层模型（简图）

```
Presentation  → MediaUIHost / TaskPanel / ScanDetail
Orchestration → LibraryContext / TaskHub / LibraryEventBus
Domain        → ScanEngine / MetadataEngine / DedupeEngine / RenameEngine
Infrastructure→ IScanStore / LibraryMonitor / PluginHost
External      → 爬虫 DLL / Java Sidecar / JvedioLib
```

## 分阶段路线图（原 plan）

| 阶段 | 主题 | Plan 状态 |
|------|------|-----------|
| Phase 0 | Core/Scan 解耦 | 完成 |
| Phase 1 | 接线与一致性 | 完成 |
| Phase 2 | 扫描增强 | 完成 |
| Phase 3 | MetadataEngine | 骨架完成 |
| Phase 4 | TaskHub 与插件 | 部分完成 |
| Phase 5 | 多类型 UI | 骨架完成 |

详细原文见工作区 `.cursor/plans/jvedio_演进架构文档_9202d5d5.plan.md`（不纳入 git 归档）。
