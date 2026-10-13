using System;

namespace WpfApiWrapperExample.Models
{
    /// <summary>
    /// One property change returned by the "{Entity}/History/{id}" endpoints (e.g. /Setups/History/{id}).
    /// The API returns the entries newest first.
    /// </summary>
    public class ApiHistoryEntryModel
    {
        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public string PropertyName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string UpdateType { get; set; }

        /// <summary>
        /// Timestamp shown in the UI: converted to local time when the API flags it as UTC.
        /// </summary>
        public DateTime DisplayTimestamp => Timestamp.Kind == DateTimeKind.Utc ? Timestamp.ToLocalTime() : Timestamp;
    }
}
