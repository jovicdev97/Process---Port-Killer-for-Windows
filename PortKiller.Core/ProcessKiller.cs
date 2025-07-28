using PortKiller.SystemAccess;
using PortKiller.SystemAccess.Models;

namespace PortKiller.Core
{
    public class ProcessKiller
    {
        private readonly ProcessHelper _processHelper;

        public ProcessKiller()
        {
            _processHelper = new ProcessHelper();
        }

        public KillResult KillProcess(int pid)
        {
            if (pid <= 0)
            {
                return KillResult.ProcessNotFound;
            }

            try
            {
                return _processHelper.KillProcessWithElevation(pid);
            }
            catch (Exception)
            {
                return KillResult.UnknownError;
            }
        }

        public List<KillProcessResult> KillMultipleProcesses(List<int> pids)
        {
            var results = new List<KillProcessResult>();

            if (pids == null || !pids.Any())
            {
                return results;
            }

            foreach (int pid in pids.Distinct())
            {
                var processName = GetProcessName(pid);
                var result = KillProcess(pid);
                
                results.Add(new KillProcessResult
                {
                    ProcessId = pid,
                    ProcessName = processName,
                    Result = result,
                    Timestamp = DateTime.Now
                });
            }

            return results;
        }

        public List<KillProcessResult> KillProcessesByPorts(List<PortInfo> portInfos)
        {
            var results = new List<KillProcessResult>();

            if (portInfos == null || !portInfos.Any())
            {
                return results;
            }

            var processIds = portInfos
                .Where(p => p.ProcessId > 0 && p.State != "Available")
                .Select(p => p.ProcessId)
                .Distinct()
                .ToList();

            return KillMultipleProcesses(processIds);
        }

        public bool CanKillProcess(int pid)
        {
            if (pid <= 0)
            {
                return false;
            }

            try
            {
                var process = _processHelper.GetProcessById(pid);
                if (process == null)
                {
                    return false;
                }

                var systemProcessNames = new[]
                {
                    "System", "svchost", "csrss", "winlogon", "services", "lsass",
                    "smss", "wininit", "dwm", "spoolsv", "conhost"
                };

                if (systemProcessNames.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (pid <= 4)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RequiresElevation(int pid)
        {
            return _processHelper.IsElevationRequired(pid);
        }

        public bool IsCurrentUserAdministrator()
        {
            return _processHelper.IsCurrentUserAdministrator();
        }

        private string GetProcessName(int processId)
        {
            if (processId <= 0)
            {
                return "Unknown";
            }

            try
            {
                var process = _processHelper.GetProcessById(processId);
                return process?.ProcessName ?? "Unknown";
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        public string GetKillResultMessage(KillResult result)
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