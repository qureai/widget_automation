using APIClient.Common.Enums;
using System;

namespace APIClient.Common.Models
{
    public class EventLog
    {
        public DateTime? TimeStamp { get; set; }

        public EventTypeEnum EventType { get; set; }

        public string PowerScribeEventName { get; set; }

        public string Message { get; set; }

        public ReportParams ReportParams { get; set; } = new ReportParams();

		public string Time
		{
			get
			{
				return TimeStamp.HasValue ? TimeStamp.Value.ToString("yyyy-MM-dd HH:mm:ss.fff") : string.Empty;
			}
		}

	}
}
