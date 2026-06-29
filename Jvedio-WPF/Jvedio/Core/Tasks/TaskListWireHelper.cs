using Jvedio.Core.UserControls.Tasks;
using SuperUtils.Framework.Tasks;
using System;
using System.Threading.Tasks;

namespace Jvedio.Core.Tasks
{
    public static class TaskListWireHelper
    {
        public static void Wire(
            TaskList taskList,
            BaseManager manager,
            Action<TaskList, string> onShowDetail = null,
            Action<string> onRestart = null,
            Func<TaskList> resolveList = null)
        {
            taskList.TaskStatusList = manager.CurrentTasks;
            taskList.onRemoveAll += () => manager.RemoveTask(TaskStatus.Canceled | TaskStatus.RanToCompletion);
            taskList.onRemoveCancel += () => manager.RemoveTask(TaskStatus.Canceled);
            taskList.onRemoveComplete += () => manager.RemoveTask(TaskStatus.RanToCompletion);
            taskList.onCancel += manager.CancelTask;
            taskList.onCancelAll += manager.CancelAll;

            if (onShowDetail != null)
                taskList.onShowDetail += onShowDetail;
            if (onRestart != null)
                taskList.onRestart += onRestart;

            manager.onRunning += () => {
                TaskList list = resolveList?.Invoke() ?? taskList;
                if (list != null)
                    list.AllTaskProgress = manager.Progress;
            };
        }

        public static void WireLogDetail(TaskList taskList, BaseManager manager)
        {
            Wire(taskList, manager, (tList, id) => {
                string logs = manager.GetTaskLogs(id);
                tList.SetLogs(logs);
                tList.ShowLog = true;
            });
        }
    }
}
