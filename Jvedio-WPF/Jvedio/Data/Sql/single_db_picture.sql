

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
