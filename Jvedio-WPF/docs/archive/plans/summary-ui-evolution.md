> 归档摘要，来源：Cursor plan「Jvedio UI 演进」（2026-06-30）。首批 11 项 todo 已完成；下列**未完成/部分完成**项见 [BACKLOG.md](../../BACKLOG.md#ui-表现层)。

# UI 演进 — 计划摘要

## 已完成

- Phase A：Discover 三配置、ScanPath→RefreshLibraryWatch、ScanComplete 泛化、DataType 切换 Rebuild
- Phase B：GameSideMenu、ISideNavigation/SideNavigationRouter、Statistic 按 DataType
- Phase C/D 骨架：MediaListMode、TabItemManager 别名、MediaDetailsFactory、SettingsSectionMask、TaskList 徽章、ScanDetail NFO 列隐藏

## 目标接口（原 plan，未完全落地）

```csharp
interface IMediaUIProfile {
    DataType DataType { get; }
    void InitSideMenu(Border container, VieModel_Main vm);
    ISideNavigation CreateSideNavigation(VieModel_Main vm);   // ← 现为全局 Router
    ITabContentFactory CreateTabFactory();                    // ← 未实现
    Type DetailsWindowType { get; }                           // ← 未实现
    SettingsSectionMask SettingsSections { get; }             // ✓
    ScanCompletionPolicy ScanCompletion { get; }             // ← 未实现
}
```

## ADR-UI（仍有效）

- ADR-UI-001：扩展 MediaUIHost
- ADR-UI-002：TabItemManager 单例 + 内容工厂注入
- ADR-UI-003：VideoList Adapter 后拆分
- ADR-UI-004：单设置窗 + 分区
- ADR-UI-005：CurrentDataType 再保留 2 个 UI Phase

详细原文见 `.cursor/plans/jvedio_ui_演进_c5f81216.plan.md`。
