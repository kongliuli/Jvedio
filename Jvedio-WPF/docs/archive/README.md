# 文档归档

归档日期：**2026-06-30**

本目录保存 Jvedio 架构演进各阶段的**已完成说明**与**决策记录**。所有**未完成工作**已提取至上级目录 [BACKLOG.md](../BACKLOG.md)，请勿在归档文档中追加新待办。

## 归档清单

| 文件 | 原路径 | 阶段 | 状态 |
|------|--------|------|------|
| [architecture/ui-evolution.md](architecture/ui-evolution.md) | `docs/architecture/ui-evolution.md` | UI Phase A–D 骨架 | 骨架已落地，细节见 BACKLOG |
| [architecture/discovery-engine.md](architecture/discovery-engine.md) | `docs/architecture/discovery-engine.md` | Discover 层工业化 | Video Pipeline 已接入；Game/Picture 待接入 |
| [architecture/spacesniffmax-comparison.md](architecture/spacesniffmax-comparison.md) | `docs/architecture/spacesniffmax-comparison.md` | SpaceSniffMax 对照 | 决策已定，未移植项见 BACKLOG |
| [plans/summary-architecture-evolution.md](plans/summary-architecture-evolution.md) | Cursor plan | 五层架构 Phase 0–5 | Phase 1–5 标记完成，深层缺口见 BACKLOG |
| [plans/summary-ui-evolution.md](plans/summary-ui-evolution.md) | Cursor plan | UI Phase A–D | 首批 todo 已完成，余项见 BACKLOG |
| [plans/summary-industrial-scan.md](plans/summary-industrial-scan.md) | Cursor plan | Discover 工业化 | PR-A/B/C 已完成 |
| [plans/summary-core-scan-decoupling.md](plans/summary-core-scan-decoupling.md) | Cursor plan | Core/Scan 解耦 | 五阶段全部完成 |

## 阅读顺序建议

1. 了解 Discover 层 → `architecture/discovery-engine.md`
2. 了解 UI 策略 → `architecture/ui-evolution.md`
3. 下一步做什么 → [BACKLOG.md](../BACKLOG.md)
