using System.Diagnostics;
using System.Security.Principal;
using PortKiller.SystemAccess.Models;

namespace PortKiller.SystemAccess
{
    public class ProcessHelper
    {
        public Process? GetProcessById(int pid)
        {
            try
            {
                return Process.GetProcessById(pid);
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public KillResult KillProcessWithElevation(int pid)
        {
            try
            {
                var process = GetProcessById(pid);
                if (process == null)
                {
                    return KillResult.ProcessNotFound;
                }

                if (IsSystemProcess(process))
                {
                    return KillResult.SystemProcess;
                }

                try
                {
                    process.Kill();
                    process.WaitForExit(5000);
                    return KillResult.Success;
                }
                catch (UnauthorizedAccessException)
                {
                    return KillProcessWithTaskkill(pid);
                }
                catch (Exception)
                {
                    return KillResult.UnknownError;
                }
            }
            catch (Exception)
            {
                return KillResult.UnknownError;
            }
        }

        public bool IsElevationRequired(int pid)
        {
            try
            {
                var process = GetProcessById(pid);
                if (process == null) return false;

                if (IsSystemProcess(process))
                {
                    return true;
                }

                try
                {
                    _ = process.MainModule?.FileName;
                    return false;
                }
                catch (UnauthorizedAccessException)
                {
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsCurrentUserAdministrator()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsSystemProcess(Process process)
        {
            try
            {
                var systemProcessNames = new[]
                {
                    "System", "svchost", "csrss", "winlogon", "services", "lsass",
                    "smss", "wininit", "explorer", "dwm", "spoolsv", "conhost"
                };

                if (systemProcessNames.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (process.SessionId == 0 && process.ProcessName != "explorer")
                {
                    return true;
                }

                if (process.Id <= 4)
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private KillResult KillProcessWithTaskkill(int pid)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/PID {pid} /F",
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using var process = Process.Start(processStartInfo);
                if (process == null)
                {
                    return KillResult.AccessDenied;
                }

                process.WaitForExit();

                return process.ExitCode == 0 ? KillResult.Success : KillResult.UnknownError;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1223)
                {
                    return KillResult.AccessDenied;
                }
                return KillResult.UnknownError;
            }
            catch (Exception)
            {
                return KillResult.UnknownError;
            }
        }

        public List<ProcessInfo> GetAllProcesses()
        {
            var processes = new List<ProcessInfo>();

            try
            {
                var systemProcesses = Process.GetProcesses();
                foreach (var process in systemProcesses)
                {
                    try
                    {
                        var processInfo = new ProcessInfo(
                            process.Id,
                            process.ProcessName,
                            GetProcessExecutablePath(process),
                            IsSystemProcess(process)
                        );

                        processes.Add(processInfo);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception)
            {
            }

            return processes;
        }

        private string GetProcessExecutablePath(Process process)
        {
            try
            {
                return process.MainModule?.FileName ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}