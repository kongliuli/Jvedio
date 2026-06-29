# QA-001 — 四 DataType 手工冒烟清单

## 自动化执行（推荐每次发版前跑）

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" `
  "Jvedio-WPF\Jvedio.Test\bin\Debug\Jvedio.Test.dll" `
  /TestCaseFilter:"TestCategory=QA-001"
```

| 测试类 | 覆盖 |
|--------|------|
| `Qa001SmokeChecklistTest` | 本清单逐步骤 1–6 + Video/非 Video 额外项 |
| `FourDataTypeSmokeTest` | Profile / Tab / Scan / TaskHub |
| `Wave12OpenItemsTest` / `Wave13OpenItemsTest` | 单池、Context、PLG-002、Http 站点 |

可选实机 UI（需 WinAppDriver）：`FourDataTypeAppiumSmokeTest`（默认 `[Ignore]`）

---

## 通用步骤（每种库各跑一遍）

| # | 步骤 | 预期 | 自动化 |
|---|------|------|--------|
| 1 | 启动页选择库类型 → 进入主窗 | 侧栏为对应 Profile；工具栏随 `LibraryContextBinding` 更新 | `Checklist_Step1_*` |
| 2 | 侧栏：全部 → 收藏 → 最近 → 标签 | 列表刷新无异常 | `Checklist_Step2_*` + 实机 |
| 3 | 主列表 Tab | 列与 `MediaListMode` 一致 | `Checklist_Step3_*` |
| 4 | 设置 → 扫描 | 可见区块符合 `SettingsSectionMask` | `Checklist_Step4_*` |
| 5 | 触发扫描（小目录） | TaskList 任务 + 子进度 `n/m` | `Checklist_Step5_*` + 实机 |
| 6 | TaskList 取消/清除 | 无崩溃 | `Checklist_Step6_*` + 实机 |

## Video 额外

| 项 | 自动化 |
|----|--------|
| 「刷新元数据」→ `TaskKind.Scrape` | `Checklist_Video_ScrapeUsesDownloadManager` |
| NFO / 爬虫设置 Tab 可见 | `Checklist_Video_CrawlerAndNfoSections` |
| PLG-002 签名开关 | `Checklist_Video_Plg002ToggleExistsInConfig` + 实机插件 Tab |

## Game / Picture / Comics 额外

| 项 | 自动化 |
|----|--------|
| 无爬虫 Tab | `Checklist_NonVideo_NoCrawlerTab` |
| Picture/Comics：PicturePaths | `Checklist_PictureComics_PicturePathsVisible` |
| Game WebUrl → og:title | `Checklist_Game_WebMetadataSiteRule` |

---

## 实机勾选记录

| 日期 | 测试人 | Video | Picture | Game | Comics | 备注 |
|------|--------|-------|---------|------|--------|------|
| 2026-06-30 | QA-001 自动化 | ✅ | ✅ | ✅ | ✅ | `TestCategory=QA-001` 全通过 |
| | （可选实机） | [ ] | [ ] | [ ] | [ ] | WinAppDriver 或下方快速路径 |

---

## 实机快速路径（约 15 min/库）

1. 启动 `Jvedio.exe` → 启动页选库 → 进入主窗，确认侧栏 Profile 正确
2. 设置 → 扫描：确认 Tab 可见性与库类型一致
3. 对小目录触发扫描 → TaskList 见子进度 `n/m` → 取消/清除无崩溃
4. **Video**：设置 → 插件 Tab 可见 PLG-002 开关；试勾选签名强制
5. **Game/Picture/Comics**：确认无爬虫 Tab

---

## 最近一次自动化结果

> 由 Agent 执行：`Qa001SmokeChecklistTest` + 关联 Smoke 套件。  
> 实机 UI 交互（点击侧栏、目视子进度）仍建议发版前人工抽测一轮。
