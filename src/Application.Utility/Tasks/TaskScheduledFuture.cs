namespace Application.Utility.Tasks
{
    public class TaskScheduledFuture: ScheduledFuture
    {
        public TaskScheduledFuture(string jobId, TimerTaskStatus controller)
        {
            JobId = jobId;
            Controller = controller;
        }

        public string JobId { get; set; }
        public TimerTaskStatus Controller { get; set; }
        bool isCanceled = false;
        public async Task<bool> CancelAsync(bool immediately)
        {
            if (isCanceled)
                return true;

            isCanceled = true;
            if (immediately)
            {
                await Controller.ImmediateCts.CancelAsync();
                Controller.ImmediateCts.Dispose();
            }
            else
            {
                await Controller.GracefulCts.CancelAsync();
                Controller.GracefulCts.Dispose();
            }

            return true;
        }

        public bool cancel(bool immediately)
        {
            return CancelAsync(immediately).Result;
        }
    }
}
