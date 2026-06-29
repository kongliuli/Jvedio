# NAS-001：极空间（ZSpace）访问方式调研

> 日期：2026-06-30  
> 状态：Spike 完成（本机探测 + 社区资料对照）  
> 关联：BACKLOG CONV-F

## 用户现状

- 仅在 **极空间 PC 客户端** 内浏览图片，**未**映射到 Windows 资源管理器。
- Jvedio 现有扫描依赖 `FileHelper.SelectPath` / 本地路径 → **无法直接扫客户端内部虚拟路径**。

## Step 0：本机端口探测（127.0.0.1:13579）

| 项 | 结果 |
|----|------|
| 命令 | `Test-NetConnection 127.0.0.1 -Port 13579` |
| TcpTestSucceeded | **False**（探测时客户端可能未运行或未暴露代理） |

**解读**：社区项目 [zspace-cli](https://github.com/skyzhao1223/zspace-cli) 称极空间桌面客户端登录后会在 `127.0.0.1:13579` 提供与 Web UI 相同的 HTTP 代理。本机本次未连通，需在 **客户端已登录且保持运行** 时复测。

复测命令：

```powershell
Test-NetConnection 127.0.0.1 -Port 13579
# 若 TcpTestSucceeded = True，再试（需参考 zspace-cli 携带 Cookie/Header）：
# curl -X POST http://127.0.0.1:13579/v2/file/list -d "path=/..."
```

## Step 1：社区逆向 API 摘要（非官方）

来源：zspace-cli README

| 端点 | 方法 | 关键参数 |
|------|------|----------|
| `/v2/file/list` | POST | `path`, `show_hidden` |
| `/v2/file/info` | POST | `path` |
| `/v2/file/newdir` | POST | `parent`, `name` |
| `/v2/file/move` | POST | `paths[]`, `to` |
| `/v2/file/copy` | POST | `paths[]`, `to` |
| `/v2/file/remove` | POST | `paths[]` |

**风险**：无官方文档；客户端升级可能导致接口变更；认证依赖客户端会话，不适合在 Jvedio 内长期存储 NAS 账号密码。

## Step 2：标准协议路线（推荐优先验证）

极空间系统设置 → **文件及共享服务** 可开启：

| 协议 | 适用场景 | Jvedio 集成成本 |
|------|----------|-----------------|
| **SMB (Samba)** | 局域网；映射为 `\\NAS\share` 或盘符 | **零代码**（现有扫描 + `NasPathHelper` 提示） |
| **WebDAV** | 远程；RaiDrive 等填账号密码挂载 | **零代码**（挂载后同 SMB） |
| SFTP / FTP / NFS | 特殊工具链 | 高，暂不推荐 |

### 路线 A（推荐）：SMB 一次性配置

1. NAS 开启 SMB，创建有读权限的共享目录。
2. Windows「映射网络驱动器」→ 例如 `Z:`。
3. Jvedio → 扫描 `Z:\photos`。
4. 设置 → 扫描 → 开启 **目录指纹增量缓存**（NAS 大库必备）。

**优点**：稳定、官方支持、与 Phase D 相册集合无耦合。  
**缺点**：需用户自行挂载；纯客户端浏览习惯需改变。

### 路线 B：客户端本地代理（待复测通过后评估）

- 前提：`13579` 可连通 + 能 list 目录。
- 实现：`ZSpaceDiscoveryBackend : IFileDiscoveryBackend`，Discover 阶段走 HTTP list，Persist 仍用现有 Picture 管线。
- **不做**：在 Jvedio 配置里存极空间账号密码（优先依赖「客户端已登录」）。

### 路线 C：WebDAV 自研客户端

- 仅当用户无法 SMB 且不愿用 RaiDrive 时考虑；工作量大，优先级低于 A/B。

## Step 3：SMB 对照实验（待用户本机执行）

- [ ] 极空间开启 SMB
- [ ] Windows 映射驱动器
- [ ] Jvedio Picture 库扫描测试目录
- [ ] 确认 `picture_folder_node` / 相册 / 单图视图正常

## 结论与建议

| 优先级 | 行动 |
|--------|------|
| **P0** | 引导用户 **SMB 挂载**（路线 A）；Jvedio 已有 `NasPathHelper` + 扫描提示 |
| **P1** | 客户端运行时 **复测 13579**；若通，开 spike `ZSpaceDiscoveryBackend` 只读 list |
| **P2** | NAS 列表缩略图异步预取（CONV-F 未做项） |
| **Defer** | 极空间官方 API（不存在）；Jvedio 内嵌账号密码 WebDAV |

## Jvedio 已有钩子

- [NasPathHelper.cs](../Jvedio/Core/Scan/NasPathHelper.cs) — UNC/网络盘/极空间路径启发式
- [Window_Main.Menu.partial.cs](../Jvedio/Windows/Window_Main.Menu.partial.cs) — 选 NAS 路径时提示增量缓存
- [PictureRemoteImageService.cs](../Jvedio/Core/Media/PictureRemoteImageService.cs) — HTTP 图片落盘缓存（远程 URL 用）

## 变更记录

| 日期 | 说明 |
|------|------|
| 2026-06-30 | 初版：13579 不通；推荐 SMB；记录 zspace-cli API 摘要 |
