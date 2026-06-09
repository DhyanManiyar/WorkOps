using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WorkOps.Models;

namespace WorkOps.Helpers
{
    public class ActivityNotificationService
    {
        private readonly WorkOpsDBEntities _db;
        private readonly HttpRequestBase _request;

        public ActivityNotificationService(WorkOpsDBEntities db, HttpRequestBase request)
        {
            _db = db;
            _request = request;
        }

        public void EnsureSchemaFallback()
        {
            try
            {
                _db.Database.ExecuteSqlCommand(@"
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.ActivityLogs')
      AND name = N'ActivityType'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.ActivityLogs ALTER COLUMN ActivityType NVARCHAR(100) NULL;
END;

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.ActivityLogs')
      AND name = N'Description'
      AND is_nullable = 0
)
BEGIN
    ALTER TABLE dbo.ActivityLogs ALTER COLUMN Description NVARCHAR(MAX) NULL;
END;");
            }
            catch (SqlException)
            {
                // If ALTER is not permitted, explicit non-null values are still provided by app code.
            }
        }

        public void LogActivity(
            int userId,
            string activityType,
            string description,
            string affectedTable = null,
            int? affectedRecordId = null)
        {
            EnsureSchemaFallback();
            _db.ActivityLogs.Add(new ActivityLog
            {
                UserID = userId,
                ActivityType = string.IsNullOrWhiteSpace(activityType) ? "System Activity" : activityType.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? "No description provided." : description.Trim(),
                LogDate = DateTime.Now,
                AffectedTable = affectedTable,
                AffectedRecordID = affectedRecordId,
                IPAddress = _request != null ? _request.UserHostAddress : null,
                UserAgent = _request != null ? _request.UserAgent : null
            });
        }

        public void NotifyUser(int userId, string title, string message, int type = 1, string link = null)
        {
            _db.Notifications.Add(new Notification
            {
                UserID = userId,
                Title = string.IsNullOrWhiteSpace(title) ? "WorkOps Notification" : title.Trim(),
                Message = string.IsNullOrWhiteSpace(message) ? "You have a new update." : message.Trim(),
                NotificationType = type,
                IsRead = false,
                Link = link,
                CreatedDate = DateTime.Now
            });
        }

        public void NotifyEmployeeById(int employeeId, string title, string message, int type = 1, string link = null)
        {
            var userId = _db.Employees.Where(e => e.EmployeeID == employeeId).Select(e => (int?)e.UserID).FirstOrDefault();
            if (userId.HasValue)
                NotifyUser(userId.Value, title, message, type, link);
        }
    }
}
