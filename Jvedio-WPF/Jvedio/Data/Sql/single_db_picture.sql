

-- 图片信息表
-- VID 识别出来的标识符
-- PreviewImages 预览图路径
-- ImageUrls: {"actress":[],"smallimage":"","bigimage":"","extraimages":[]}
-- web_type : 所属网址 => [db,library,bus]
-- WebUrl : 对应的网址
-- SubSection: 分段视频位置
drop table if exists metadata_picture;
BEGIN;
create table metadata_picture(
    PID INTEGER PRIMARY KEY autoincrement,
    DataID INTEGER,
    Director VARCHAR(100),
    Studio TEXT,
    Publisher TEXT,
    Plot TEXT,
    Outline TEXT,
    PicCount INTEGER DEFAULT 0,
    PicPaths TEXT,
    VideoPaths TEXT,
    ExtraInfo TEXT,
    
    unique(DataID,PID)
);
CREATE INDEX metadata_picture_idx_DataID_PID ON metadata_picture (DataID,PID);
COMMIT;

-- 单图明细（RelativePath 相对 ScanRoot）
drop table if exists metadata_picture_file;
BEGIN;
create table metadata_picture_file(
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
CREATE INDEX metadata_picture_file_idx_DataID ON metadata_picture_file (DataID);
COMMIT;

-- 仅有图目录的文件夹树（Phase B）
drop table if exists picture_folder_node;
BEGIN;
create table picture_folder_node(
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
CREATE INDEX picture_folder_node_idx_DBId_Parent ON picture_folder_node (DBId, ParentPath);
COMMIT;

-- 用户自定义相册集合（Phase D）
drop table if exists picture_collection_item;
drop table if exists picture_collection;
BEGIN;
create table picture_collection(
    CollectionID INTEGER PRIMARY KEY autoincrement,
    DBId INTEGER NOT NULL,
    Name TEXT NOT NULL,
    SortOrder INT DEFAULT 0,
    CoverPath TEXT,
    CreateDate VARCHAR(30),
    unique(DBId, Name)
);
COMMIT;

BEGIN;
create table picture_collection_item(
    id INTEGER PRIMARY KEY autoincrement,
    CollectionID INTEGER NOT NULL,
    ItemType VARCHAR(20) NOT NULL,
    RefPath TEXT,
    RefDataID INTEGER DEFAULT 0,
    RefFID INTEGER DEFAULT 0,
    SortOrder INT DEFAULT 0,
    unique(CollectionID, ItemType, RefPath, RefDataID, RefFID)
);
CREATE INDEX picture_collection_item_idx_CollectionID ON picture_collection_item (CollectionID);
COMMIT;
