namespace Application.Core.OpenApi
{
    public static class TaskService
    {
        public static List<string> GetTaskList()
        {
            return SchedulerManager.TaskScheduler.Keys.ToList();
        }
    }
}
