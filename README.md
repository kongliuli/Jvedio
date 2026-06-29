
[中文](README.md) [English](README_EN.md) [日本語](README_JP.md)


<h1 align="center">KVideo</h1>



<div align="center" >
<img src="https://s1.ax1x.com/2022/06/11/XcePQf.png"><h3>本地视频与图片管理</h3>
</div>





---

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-d.svg)](#)
[![Platform](https://img.shields.io/badge/Platform-Win-brightgreen.svg)](#)
[![LICENSE](https://img.shields.io/badge/license-GPL%203.0-blue)](#)
[![Star](https://img.shields.io/github/stars/kongliuli/Jvedio?label=Star%20this%20repo)](https://github.com/kongliuli/Jvedio)
[![Fork](https://img.shields.io/github/forks/kongliuli/Jvedio?label=Fork%20this%20repo)](https://github.com/kongliuli/Jvedio/fork)

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;`KVideo` 是基于 [Jvedio](https://github.com/hitchao/Jvedio) 5.0 架构 fork 演进的 Windows 桌面端本地媒体管理软件，聚焦 **Video + Picture 双库**：视频库保留完整刮削、标签、演员与 FFmpeg 处理能力；图片库提供目录树浏览、相册/单图双视图与 NAS 基础适配。已移除 Game / Comics 库及 4→5 迁移向导，产品更聚焦、更易维护。

仓库：[kongliuli/Jvedio](https://github.com/kongliuli/Jvedio)（分支 `kvdeio`） | 下载：[Releases](https://github.com/kongliuli/Jvedio/releases)（暂无预编译包时请见下方[开发](#开发)章节自行编译）

---

# 双库概览

## Video 库

- 扫描本地视频并建立视频库，提取 **唯一识别码**，自动分类
- 标签 / 演员管理，元数据刮削与翻译
- NFO 识别导入，信息编辑，标记筛选，丰富搜索
- 基于 `FFmpeg` 截图、截取 GIF，影片重命名
- 插件体系（皮肤、同步信息等），多语言界面

## Picture 库

- 按文件夹扫描本地图片，建立图片库
- 侧栏 **仅有图目录** 树，快速定位含图片的文件夹
- **相册视图** / **单图视图** 切换；单图视图默认含子目录，工具栏可关闭
- 保留 Genre / Author 等元数据与侧栏自动分类
- NAS 基础适配：UNC / 网络盘识别，远程图片预览缓存

> 代码与解决方案仍使用 `Jvedio` 命名（如 `Jvedio-WPF/Jvedio.sln`），与 README 中的 KVideo 品牌为同一产品。

---

# 界面预览

以下截图为 Video 库界面（沿用 Jvedio 5.0 视觉，功能在 KVideo 中完整保留）。

[<img src="https://s1.ax1x.com/2022/10/07/x8KbvT.png" alt="主界面" style="zoom:80%;" />](https://imgse.com/i/x8KbvT)

---
[<img src="https://s1.ax1x.com/2022/10/07/x8KOrF.png" alt="列表视图" style="zoom:80%;" />](https://imgse.com/i/x8KOrF)

---
[<img src="https://s1.ax1x.com/2022/10/07/x8MVVH.png" alt="详情视图" style="zoom:80%;" />](https://imgse.com/i/x8MVVH)

---
[<img src="https://s1.ax1x.com/2022/10/07/x8MZad.png" alt="设置" style="zoom:80%;" />](https://imgse.com/i/x8MZad)

# 使用说明

**用户**：Video 库操作与 [Jvedio 5.0 用户文档](https://github.com/hitchao/Jvedio/wiki/02_Beginning) 基本一致；Picture 库详见下方[软件特性](#软件特性)中的 Picture 库章节。项目内文档索引见 [Jvedio-WPF/docs/README.md](Jvedio-WPF/docs/README.md)。

**开发者**：见下方[开发](#开发)章节，或 [Jvedio-WPF/Document/Wiki/5.0/20-Developer.md](Jvedio-WPF/Document/Wiki/5.0/20-Developer.md)。

# 开发

1. 安装 Visual Studio 2022，勾选「.NET 桌面开发」，并在「单个组件」中安装 **.NET Framework 4.7.2 SDK** 和目标包
2. 克隆仓库并切换到 `kvdeio` 分支：

```bash
git clone https://github.com/kongliuli/Jvedio.git
cd Jvedio
git checkout kvdeio
```

3. 使用 Visual Studio 2022 打开 `Jvedio-WPF/Jvedio.sln`
4. 右键解决方案 → 管理 NuGet 程序包，还原依赖
5. 生成 → 重新生成解决方案，点击启动

# 相关项目

KVideo 基于 [hitchao/Jvedio](https://github.com/hitchao/Jvedio) fork，遵循 [GPL 3.0](LICENSE) 许可证。以下为上游 Jvedio 生态：

| 项目 | 网址 |
| -- | -- |
| Jvedio 官方网页 | [JvedioWebPage](https://github.com/hitchao/JvedioWebPage) |
| Chrome（360 极速浏览器）插件 | [Jvedio-Chrome-Extensions](https://github.com/hitchao/Jvedio-Chrome-Extensions) |
| Jvedio 升级的服务器源 | [jvedioupdate](https://github.com/hitchao/jvedioupdate) |
| Gif 控件修改于 | [WpfAnimatedGif](https://github.com/hitchao/WpfAnimatedGif) |

# 路线图

进行中与计划中的功能见 [Jvedio-WPF/docs/BACKLOG.md](Jvedio-WPF/docs/BACKLOG.md)，主要包括：

- **Phase D**：用户自定义相册集合（独立于 Label）
- **NAS 深化**：极空间 WebDAV/API 直连、Picture 列表缩略图异步预取
- **QA**：双库冒烟与 Picture 专项用例完善

# 软件特性

## 通用

### 插件

- 皮肤插件（支持多种皮肤切换）
- 同步信息插件

[<img src="https://s1.ax1x.com/2022/10/07/x8MJaj.png" alt="插件" style="zoom:80%;" />](https://imgse.com/i/x8MJaj)

[<img src="https://s1.ax1x.com/2022/10/07/x8MUGq.png" alt="皮肤" style="zoom:80%;" />](https://imgse.com/i/x8MUGq)

### 语言

支持中文、英语、日语

[<img src="https://s1.ax1x.com/2022/10/07/x8MydJ.png" alt="多语言" style="zoom:80%;" />](https://imgse.com/i/x8MydJ)

## Video 库

### Video + Picture 双库

启动页可选 Video 或 Picture 库，各自独立扫描与管理。

[<img src="https://s1.ax1x.com/2022/10/07/x8KbvT.png" alt="双库" style="zoom:80%;" />](https://imgse.com/i/x8KbvT)

### 支持 NFO 识别导入

[<img src="https://s1.ax1x.com/2022/10/07/x8M5LD.png" alt="NFO" style="zoom:80%;" />](https://imgse.com/i/x8M5LD)

### 支持信息编辑与修改

[<img src="https://s1.ax1x.com/2022/10/07/x8MTdH.png" alt="编辑" style="zoom:80%;" />](https://imgse.com/i/x8MTdH)

### 标记管理 / 筛选

- 支持批量添加 / 修改 / 删除标记
- 根据标记进行筛选

[<img src="https://s1.ax1x.com/2022/10/07/x8MLWt.png" alt="标记" style="zoom:80%;" />](https://imgse.com/i/x8MLWt)

### 丰富的搜索功能

[<img src="https://s1.ax1x.com/2022/10/07/x8MxOS.png" alt="搜索" style="zoom:80%;" />](https://imgse.com/i/x8MxOS)

### 新增演员信息

[<img src="https://s1.ax1x.com/2022/10/07/x8QAS0.png" alt="演员" style="zoom:80%;" />](https://imgse.com/i/x8QAS0)

### 视频处理功能

- 截图
- 截取 GIF

[<img src="https://s1.ax1x.com/2022/10/07/x8QVyT.png" alt="视频处理" style="zoom:80%;" />](https://imgse.com/i/x8QVyT)

### 重命名影片功能

[<img src="https://s1.ax1x.com/2022/10/07/x8Qnw4.png" alt="重命名" style="zoom:80%;" />](https://imgse.com/i/x8Qnw4)

### 其他功能

- 图片展示模式：缩略图、海报图
- 丰富的筛选功能：资源是否存在筛选、图片是否存在筛选、仅显示分段视频、视频类型选择

[<img src="https://s1.ax1x.com/2022/10/07/x8Qr1P.png" alt="筛选" style="zoom:80%;" />](https://imgse.com/i/x8Qr1P)

- 丰富的右键功能

[<img src="https://s1.ax1x.com/2022/10/07/x8Qhhn.png" alt="右键" style="zoom:80%;" />](https://imgse.com/i/x8Qhhn)

- 智能分类

[<img src="https://s1.ax1x.com/2022/10/07/x8QHnU.png" alt="智能分类" style="zoom:80%;" />](https://imgse.com/i/x8QHnU)

## Picture 库

- **目录树侧栏**：仅显示含图片的文件夹，选中节点后列表限定在该子树
- **相册视图**：按含直接图片的子文件夹展示相册条目
- **单图视图**：展开每张图片的明细行，默认包含子目录内图片，可通过工具栏关闭
- **元数据分类**：Genre、Author 等字段与侧栏自动分类保留
- **NAS 提示**：扫描 UNC / 网络盘路径时识别 NAS 环境并建议开启目录指纹缓存；HTTP(S) 远程图片可下载到本地缓存后预览

# 鸣谢

**感谢 [hitchao/Jvedio](https://github.com/hitchao/Jvedio) 原作者及社区**，KVideo 在此基础上 fork 演进。

同时感谢以下网友在 Jvedio 开发中的贡献：

| 板块 | 网友 |
| :--: | :--: |
| UI | 青萍之末, Engine, Erdon, Erik |
| 调试 | Sheldon, SHAWN, dddsG, EEE, Jion 等人 |
| 赞助支持 | 小猪培根 等众多网友 |
