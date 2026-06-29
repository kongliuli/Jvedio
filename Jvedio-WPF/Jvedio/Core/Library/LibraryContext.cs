using Jvedio.Core.Enums;
using System;
using System.Collections.Generic;

namespace Jvedio.Core.Library
{
    public class LibraryContext
    {
        private static LibraryContext _current;

        public int DBId { get; set; }
        public DataType DataType { get; set; } = DataType.Video;
        public List<string> RootPaths { get; set; } = new List<string>();

        public static event EventHandler CurrentChanged;

        public static LibraryContext Current => _current ?? FromCurrent();

        public static void Apply(LibraryContext context)
        {
            if (context == null) {
                _current = null;
                CurrentChanged?.Invoke(null, EventArgs.Empty);
                return;
            }
            _current = context;
            LibraryRuntime.SetCurrent(context.DataType);
            CurrentChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void Apply(DataType dataType, IEnumerable<string> rootPaths = null, int? dbId = null)
        {
            int resolvedDbId = dbId ?? _current?.DBId ?? 0;
            Apply(new LibraryContext {
                DataType = dataType,
                DBId = resolvedDbId,
                RootPaths = rootPaths != null ? new List<string>(rootPaths) : new List<string>(),
            });
        }

        public static LibraryContext FromCurrent(IEnumerable<string> rootPaths = null, int? dbId = null)
        {
            int resolvedDbId = dbId ?? _current?.DBId ?? 0;
            return new LibraryContext {
                DBId = resolvedDbId,
                DataType = _current?.DataType ?? DataType.Video,
                RootPaths = rootPaths != null ? new List<string>(rootPaths) : new List<string>(),
            };
        }
    }
}
