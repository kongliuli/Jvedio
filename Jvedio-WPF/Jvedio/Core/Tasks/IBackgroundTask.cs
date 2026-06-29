using SuperUtils.Framework.Tasks;
using System.Collections.Generic;

namespace Jvedio.Core.Tasks
{
    /// <summary>复合任务子进度（TASK-004 首版）。</summary>
    public interface IBackgroundTask
    {
        IReadOnlyList<AbstractTask> Children { get; }
    }
}
