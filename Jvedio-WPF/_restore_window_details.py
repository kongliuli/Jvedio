"""Restore Window_Details.xaml.cs from upstream snapshot + local Wave patches."""
import re
from pathlib import Path

SRC = Path(r"C:\Users\19808\.cursor\projects\c-Code-Jvedio-master\agent-tools\af020a0a-b705-4990-9c15-9d4cf9adb113.txt")
DST = Path(__file__).parent / "Jvedio" / "Windows" / "Window_Details.xaml.cs"


def reindent(text: str) -> str:
    out = []
    depth = 0
    for raw in text.splitlines():
        stripped = raw.strip()
        if not stripped:
            out.append("")
            continue
        if stripped.startswith("//"):
            out.append("    " * depth + stripped)
            continue
        line = stripped
        close = 0
        while line.startswith("}"):
            close += 1
            line = line[1:].lstrip()
        depth = max(0, depth - close)
        out.append("    " * depth + stripped)
        code = stripped.split("//")[0]
        depth += code.count("{") - code.count("}")
        depth = max(0, depth)
    return "\n".join(out) + "\n"


def fix_generics(text: str) -> str:
    reps = [
        ("using Jvedio.Core.FFmpeg;\n", "using Jvedio.Core.FFmpeg;\nusing Jvedio.Core.Library;\n"),
        ("public Action onViewAssoData", "public Action<long> onViewAssoData"),
        ("WrapperEventArg CurrentWrapperArg", "WrapperEventArg<Video> CurrentWrapperArg"),
        ("WrapperEventArg arg)", "WrapperEventArg<Video> arg)"),
        ("private List DataIDs", "private List<long> DataIDs"),
        ("DataIDs = new List ();", "DataIDs = new List<long>();"),
        ("= new List ();", "= new List<long>();"),
        ("DataIDs = new List<long> ();", "DataIDs = new List<long>();"),
        ("SelectWrapper wrapper", "SelectWrapper<Video> wrapper"),
        ("new SelectWrapper ().Eq", "new SelectWrapper<Video>().Eq"),
        ("List<Dictionary<string, object>>", "List<Dictionary<string, object>>"),  # noop guard
        ("List<Dictionary<string, object> >", "List<Dictionary<string, object>>"),
        ("List<Dictionary<string, object>> list = videoMapper", "List<Dictionary<string, object>> list = videoMapper"),
        ("windowMain?.DeleteID(new List { vieModel", "windowMain?.DeleteID(new List<Video> { vieModel"),
        ("List files = new List ();", "List<string> files = new List<string>();"),
        ("List filepaths = new List ();", "List<string> filepaths = new List<string>();"),
        ("List imagePathList = new List ();", "List<string> imagePathList = new List<string>();"),
        ("List imagePathList = new List<string> ();", "List<string> imagePathList = new List<string>();"),
        ("List imageList = await", "List<string> imageList = await"),
        ("List screenShotList = await", "List<string> screenShotList = await"),
        ("List imagePathList = new List<string> ();", "List<string> imagePathList = new List<string>();"),
        ("Task<List > GetImageList", "Task<List<string>> GetImageList"),
        ("List imagePathList = new List ();", "List<string> imagePathList = new List<string>();"),
        ("List list = new List ();", "List<string> list = new List<string>();"),
        ("FindParentOfType (\"rootGrid\")", "FindParentOfType<Grid>(\"rootGrid\")"),
        ("FindParentOfType ();", "FindParentOfType<ItemsControl>();"),
        ("ObservableCollection tagStamp =", "ObservableCollection<TagStamp> tagStamp ="),
        ("ObservableCollection tagStamps =", "ObservableCollection<TagStamp> tagStamps ="),
        ("vieModel.CurrentActorList = new ObservableCollection ();", "vieModel.CurrentActorList = new ObservableCollection<ActorInfo>();"),
        ("PreviewImagePathList = new ObservableCollection ();", "PreviewImagePathList = new ObservableCollection<string>();"),
        ("PreviewImageList = new ObservableCollection ();", "PreviewImageList = new ObservableCollection<BitmapSource>();"),
        ("video.PreviewImageList = new ObservableCollection ();", "video.PreviewImageList = new ObservableCollection<BitmapSource>();"),
        ("video.PreviewImagePathList = new ObservableCollection ();", "video.PreviewImagePathList = new ObservableCollection<string>();"),
        ("ScanTask.PICTURE_EXTENSIONS_LIST", "ScanExtensions.PICTURE_EXTENSIONS_LIST"),
    ]
    for old, new in reps:
        text = text.replace(old, new)
    # leftover List () in mapper select
    text = re.sub(
        r"List<Dictionary<string, object>> list = videoMapper\.Select\(sql\);",
        "List<Dictionary<string, object>> list = videoMapper.Select(sql);",
        text,
    )
    text = re.sub(
        r"List imagePathList = new List\(\);",
        "List<string> imagePathList = new List<string>();",
        text,
    )
    text = re.sub(
        r"List imagePathList = new List<string>\(\);",
        "List<string> imagePathList = new List<string>();",
        text,
    )
    text = re.sub(
        r"private async Task<List > GetImageList",
        "private async Task<List<string>> GetImageList",
        text,
    )
    text = re.sub(
        r"List imagePathList = new List<string>\(\);",
        "List<string> imagePathList = new List<string>();",
        text,
    )
    text = re.sub(
        r"List imagePathList = new List\(\);",
        "List<string> imagePathList = new List<string>();",
        text,
    )
    text = re.sub(
        r"List imagePathList = new List<string>\(\);",
        "List<string> imagePathList = new List<string>();",
        text,
    )
    return text


def apply_wave_patches(text: str) -> str:
    text = text.replace(
        "DownLoadTask.onDownloadSuccess += onDownloadSuccess;",
        "LibraryEventBus.DownloadCompleted += (s, e) => onDownloadSuccess(e.Task);",
    )
    text = text.replace(
        "ScreenShotTask.onScreenShotCompleted += onScreenShotCompleted;",
        "LibraryEventBus.ScreenShotCompleted += (s, e) => onScreenShotCompleted(e.Success, e.DataId);",
    )
    text = text.replace(
        "Filter.onTagStampDelete += RemoveTag;",
        "LibraryEventBus.TagStampDeleted += (s, e) => RemoveTag(e.TagId);",
    )
    text = text.replace(
        "Filter.onTagStampRefresh += onRefreshTagStemp;",
        "LibraryEventBus.TagStampFilterRefresh += (s, e) => onRefreshTagStemp(e.TagId);",
    )
    return text


def main():
    raw = SRC.read_text(encoding="utf-8")
    text = reindent(raw)
    text = fix_generics(text)
    text = apply_wave_patches(text)
    DST.write_text(text, encoding="utf-8-sig")
    print(f"Restored {DST} ({len(text.splitlines())} lines)")


if __name__ == "__main__":
    main()
