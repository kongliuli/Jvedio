# TASK-001 TaskHub 合并评估

> 日期：2026-06-30 · 状态：Phase B/C 部分完成（Wave 15 去双重限流）

## 现状

`TaskHub.AddTask` 经 `EnqueueUnified` 路由到三个 Manager。UI（TaskList）通过 `TaskType` 分 Tab 展示。

**Wave 12–15 已做：**
- `UnifiedTaskWorkerPool`：仅 **Scan** 走全局槽（避免与 Download/ScreenShot 的 `TaskDispatcher` 双重限流）
- Download/ScreenShot 仍用各自 `TaskDispatcher` 并发控制
- `TaskHub.TotalRunningCount` / `TaskHubBinding` 供主界面徽章
- `TaskListWireHelper` 统一 TaskList 接线

## 未做（真 Phase C）

单队列 + `TaskKind` 优先级调度；Manager 仅作 UI 分 Tab 视图。

## 结论

当前为 **混合模式**：Scan 全局槽 + Download/Generate 各自 Dispatcher。真单队列待需求明确后再开。
