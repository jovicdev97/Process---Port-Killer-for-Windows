namespace PortKiller.SystemAccess.Models
{
    public class PortInfo
    {
        public int Port { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public DateTime ScanTime { get; set; }

        public PortInfo()
        {
            ScanTime = DateTime.Now;
        }

        public PortInfo(int port, int processId, string processName, string protocol, string state)
        {
            Port = port;
            ProcessId = processId;
            ProcessName = processName;
            Protocol = protocol;
            State = state;
            ScanTime = DateTime.Now;
        }
    }

    public class ConnectionInfo
    {
        public int LocalPort { get; set; }
        public string LocalAddress { get; set; } = string.Empty;
        public int RemotePort { get; set; }
        public string RemoteAddress { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public int ProcessId { get; set; }

        public ConnectionInfo()
        {
        }
    }

    public class ProcessInfo
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public bool IsSystemProcess { get; set; }
        public DateTime StartTime { get; set; }

        public ProcessInfo()
        {
        }

        public ProcessInfo(int processId, string processName, string executablePath = "", bool isSystemProcess = false)
        {
            ProcessId = processId;
            ProcessName = processName;
            ExecutablePath = executablePath;
            IsSystemProcess = isSystemProcess;
            StartTime = DateTime.Now;
        }
    }

    public enum KillResult
    {
        Success,
        AccessDenied,
        ProcessNotFound,
        SystemProcess,
        UnknownError
    }

    public class KillProcessResult
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public KillResult Result { get; set; }
        public DateTime Timestamp { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsSuccess => Result == KillResult.Success;

        public string GetDisplayMessage()
        {
            var baseMessage = $"PID {ProcessId} ({ProcessName}): {GetKillResultMessage(Result)}";
            
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                baseMessage += $" - {ErrorMessage}";
            }

            return baseMessage;
        }

        private string GetKillResultMessage(KillResult result)
        {
            return result switch
            {
                KillResult.Success => "Process terminated successfully",
                KillResult.AccessDenied => "Access denied - Administrator privileges required",
                KillResult.ProcessNotFound => "Process not found or already terminated",
                KillResult.SystemProcess => "Cannot terminate system process",
                KillResult.UnknownError => "Unknown error occurred",
                _ => "Unknown result"
            };
        }
    }
}