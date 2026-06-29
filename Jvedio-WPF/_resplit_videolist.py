"""Re-split VideoList.FileOps.partial.cs by fixed line ranges (string-safe)."""
from pathlib import Path

ROOT = Path(__file__).parent / "Jvedio" / "Core" / "UserControls"
MAIN = ROOT / "VideoList.xaml.cs"
FILE_OPS = ROOT / "VideoList.FileOps.partial.cs"
DELETE = ROOT / "VideoList.Delete.partial.cs"
TASKS = ROOT / "VideoList.Tasks.partial.cs"

USINGS = open(ROOT / "VideoList.Delete.partial.cs", encoding="utf-8").read().split("public partial class VideoList")[0]

def wrap(body: str, tab: str) -> str:
    return USINGS + f"    public partial class VideoList\n    {{\n        // Tab: {tab}\n" + body + "\n    }\n}\n"

def slice_lines(path, ranges):
    lines = path.read_text(encoding="utf-8").splitlines(keepends=True)
    out = []
    for a, b in ranges:
        out.append("".join(lines[a - 1 : b]))
    return out

def main():
    # 1) AsyncDeleteFile from main (lines 420-454) -> Delete.partial before DeleteFile
    main_lines = MAIN.read_text(encoding="utf-8").splitlines(keepends=True)
    async_body = "".join(main_lines[419:454])
    main_lines = main_lines[:419] + main_lines[454:]

    delete_text = DELETE.read_text(encoding="utf-8")
    insert_at = delete_text.rfind("        public async void DeleteFile")
    DELETE.write_text(delete_text[:insert_at] + async_body + delete_text[insert_at:], encoding="utf-8")

    fo = FILE_OPS
    chunks = {
        "FileOps": [(50, 343), (1288, 1300)],
        "Tasks": [(473, 755), (1080, 1098)],
        "Tags": [(812, 871), (1117, 1120)],
        "Search": [(610, 640), (1135, 1235)],
        "ContextMenu": [(436, 472), (756, 811), (908, 1127)],
    }
    main_from_fo = [(344, 435), (487, 609), (718, 730), (890, 907), (1129, 1133), (1237, 1334)]

    # merge new Tasks chunks into existing Tasks.partial
    tasks_body = "".join(slice_lines(fo, chunks["Tasks"]))
    tasks_text = TASKS.read_text(encoding="utf-8")
    tasks_text = tasks_text.replace("\n    }\n}\n", "\n" + tasks_body + "\n    }\n}\n")
    TASKS.write_text(tasks_text, encoding="utf-8")
    del chunks["Tasks"]

    for name, ranges in chunks.items():
        body = "".join(slice_lines(fo, ranges))
        out = ROOT / f"VideoList.{name}.partial.cs"
        out.write_text(wrap(body, name), encoding="utf-8")
        print(f"{out.name}: {body.count(chr(10))} lines")

    main_append = "".join(slice_lines(fo, main_from_fo))
    main_text = "".join(main_lines).rstrip()
    if not main_text.endswith("}"):
        main_text += "\n"
    # drop broken trailing close if present
    main_text = main_text.removesuffix("\n}\n").removesuffix("\n    }\n")
    main_text += "\n" + main_append + "\n    }\n}\n"
    MAIN.write_text(main_text, encoding="utf-8")

    FILE_OPS.unlink()
    print(f"VideoList.xaml.cs: {main_text.count(chr(10))} lines")
    print("removed VideoList.FileOps.partial.cs (replaced)")


if __name__ == "__main__":
    main()
