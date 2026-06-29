# Jvedio-WPF 待办清单（BACKLOG）

> 更新日期：2026-06-30  
> 原则：新工作只在本文件追加；归档文档只读。  
> 产品方向：**Video + Picture 双库**；收敛为聚焦产品，非砍功能清单。

---

## Open 项

### CONV-A — Phase A：删 Game/Comics + 删 4→5 迁移 `done`

- [x] 移除 `DataType.Game / Comics` 及全部代码路径
- [x] 删除 `Jvedio4ToJvedio5`，`MoveOldFiles` 仅保留旧文件搬移
- [x] 启动页只保留 Video / Picture；老库 `DataType=2/3` 不展示（`ResolveDataType` 回退侧栏索引）
- [x] 测试改为双库冒烟（QA-001 19/19 通过）

### CONV-B — Phase B：Picture 存储（目录树 + 单图） `done`

- [x] 启用/扩展 `metadata_picture_file`（单图明细，`RelativePath` 相对 ScanRoot）
- [x] 新增 `picture_folder_node`（仅有图目录入树）
- [x] 重写 `PictureScanPipeline`：按有图文件夹 upsert FolderAlbum + 单图 file 行
- [x] `PicPaths` 仍写入文件名列表（兼容旧 UI；新数据以 file 表为准）

**DDL 草案（`single_db_picture.sql` 追加）：**

```sql
-- 单图明细
create table if not exists metadata_picture_file (
    FID INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER NOT NULL,
    RelativePath TEXT NOT NULL,
    FileName TEXT,
    Size INTEGER DEFAULT 0,
    Hash VARCHAR(32),
    Width INT DEFAULT 0,
    Height INT DEFAULT 0,
    ExtraInfo TEXT,
    unique(DataID, RelativePath)
);
CREATE INDEX IF NOT EXISTS metadata_picture_file_idx_DataID ON metadata_picture_file (DataID);

-- 仅有图目录节点（侧栏目录树）
create table if not exists picture_folder_node (
    NodeID INTEGER PRIMARY KEY autoincrement,
    DBId INTEGER NOT NULL,
    ScanRoot TEXT NOT NULL,
    FullPath TEXT NOT NULL,
    ParentPath TEXT,
    Name TEXT,
    Depth INT DEFAULT 0,
    HasImages INT DEFAULT 1,
    unique(DBId, FullPath)
);
CREATE INDEX IF NOT EXISTS picture_folder_node_idx_DBId_Parent ON picture_folder_node (DBId, ParentPath);
```

### CONV-C — Phase C：Picture UI（目录树 + 双视图） `done`

- [x] 侧栏「目录」：`picture_folder_node` 树（**仅有图目录**）
- [x] 工具栏视图切换：**相册视图**（子文件夹） / **单图视图**（含子目录，默认；可关）
- [x] 选中目录节点 → 两种视图均限定该子树
- [x] 保留 Genre / Author 侧栏「自动分类」及 `metadata_picture` 字段

### CONV-F — NAS / 极空间适配（Phase C+ 设计 + 基础钩子）

- [x] `NasPathHelper`：UNC/网络盘识别、极空间路径启发式、扫描建议
- [x] 扫描入口（ScanNas/选路径）：检测 NAS 路径并提示开启目录指纹缓存
- [x] `PictureRemoteImageService`：HTTP(S) 图片下载到本地 `Cache/PictureRemote`（远程资源预览基础）
- [ ] 极空间 WebDAV/API 直连（需设备 API 文档）
- [ ] Picture 列表 NAS 缩略图异步预取队列
- [ ] 远程相册 URL 写入 `ExtraInfo` 后的批量下载任务

### CONV-D — Phase D：用户相册集合

```sql
create table if not exists picture_collection (
    CollectionID INTEGER PRIMARY KEY autoincrement,
    DBId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    SortOrder INT DEFAULT 0,
    CoverPath TEXT,
    CreateDate VARCHAR(30),
    unique(DBId, Name)
);

create table if not exists picture_collection_item (
    id INTEGER PRIMARY KEY autoincrement,
    CollectionID INTEGER NOT NULL,
    ItemType VARCHAR(20) NOT NULL,  -- folder | file | album
    RefPath TEXT,
    RefDataID INTEGER,
    RefFID INTEGER,
    SortOrder INT DEFAULT 0,
    unique(CollectionID, ItemType, RefPath, RefDataID, RefFID)
);
```

- [ ] 侧栏「我的相册」独立于 Label
- [ ] 右键加入/移出集合；同一项可进多集合

### CONV-E — 测试与 QA

- [ ] 双库冒烟 + Picture 目录/视图/集合专项用例
- [ ] 发版前 `/TestCaseFilter:"TestCategory=QA-001"`

---

## 已确认产品决策（2026-06-30）

| 项 | 决定 |
|----|------|
| 库类型 | 仅 Video + Picture；Game/Comics 删除，Comics 能力并入 Picture |
| 目录树 | **仅有图目录**显示 |
| 单图视图 | 默认**含所有子文件夹**；工具栏可关 |
| 元数据 | 保留 Genre/Author 等 + 现有侧栏分类 |
| 自定义分组 | **独立相册集合**（非 Label） |
| 迁移 | 不要 4→5；其余 Video 能力保留 |
| Monorepo | Vue / 插件 / FFmpeg / 多库等保留 |

---

## Phase A 文件级改动清单

**删除文件：**

- `Core/Scan/GameScan.cs`, `GameScanPipeline.cs`, `IGameScanStore.cs`, `ComicScan.cs`
- `Core/UserControls/GameSideMenu.xaml(.cs)`
- `Entity/Data/Game.cs`, `Comic.cs`
- `Mapper/Common/GameMapper.cs`, `ComicMapper.cs`
- `Core/Enums/ComicType.cs`
- `Data/Sql/single_db_game.sql`, `single_db_comic.sql`
- `Upgrade/Jvedio4ToJvedio5.cs`

**修改要点：**

- `DataType.cs` → 仅 Video, Picture
- `StartupLibraryMapping` → 双项映射；`ResolveDataType` 忽略 legacy 2/3
- `ScanFactory`, `MediaUIHost`, `SideNavigations`, `SettingsSectionMask`, `MediaFeatureMask`, `TabTypeExtensions`, `MediaListMode`, `MetaDataDeleteStrategy`, `NonVideoMetadataEngine`, `IPictureScanStore`, `Picture.cs`, `MapperManager`, `AppDatabases`, `ScanDiscoveryHelper`, `WindowStartUp.xaml(.cs)`, `Jvedio.csproj`
- 测试：`FourDataTypeSmokeTest`, `ScanFactoryIntegrationTest`, `MediaUIHostTest`, `StartupLibraryMappingTest`, `Qa001SmokeChecklistTest`, Wave7–13 等

---

## 十、已完成（勿重复开项）

<details>
<summary>点击展开已完成清单</summary>

**Wave 1–13** — TASK/CTX/META/PLG/DOM/QA 见历史

**2026-06-30 QA-001** — `Qa001SmokeChecklistTest` 映射清单

**2026-06-30 Git** — 初始提交推送到 `origin/dev-5.0`

</details>

---

## 变更记录

| 日期 | 说明 |
|------|------|
| 2026-06-30 | Phase C：Picture 目录树侧栏 + 相册/单图双视图 + NAS 基础适配 |
| 2026-06-30 | Phase B：Picture 目录树/单图表 + 扫描管线重写 |
| 2026-06-30 | Phase A 完成：双库收敛（删 Game/Comics/4→5 迁移），QA-001 19/19 |
