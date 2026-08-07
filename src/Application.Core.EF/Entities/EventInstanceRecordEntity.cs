using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.EF.Entities
{
    /// <summary>
    /// FB 记录
    /// </summary>
    public class EventInstanceRecordEntity
    {
        private EventInstanceRecordEntity() { }
        public int Id { get; set; }
        public string EventName { get; set; }
        public string EventInstanceId { get; set; }
        public int CharacterId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset FinishTime { get; set; }
        /// <summary>
        /// 是否通关
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}
