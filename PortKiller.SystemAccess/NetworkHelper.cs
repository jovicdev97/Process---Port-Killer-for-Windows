using System.Net.NetworkInformation;
using System.Net;
using System.Diagnostics;
using PortKiller.SystemAccess.Models;

namespace PortKiller.SystemAccess
{
    public class NetworkHelper
    {
        private static string? _cachedNetstatOutput;
        private static DateTime _cacheTime = DateTime.MinValue;
        private static readonly TimeSpan CacheTimeout = TimeSpan.FromSeconds(2);

        public List<ConnectionInfo> GetActiveConnections()
        {
            var connections = new List<ConnectionInfo>();
            var properties = IPGlobalProperties.GetIPGlobalProperties();

            var netstatData = GetNetstatData();

            var tcpConnections = properties.GetActiveTcpConnections();
            foreach (var connection in tcpConnections)
            {
                var connInfo = new ConnectionInfo
                {
                    LocalAddress = connection.LocalEndPoint.Address.ToString(),
                    LocalPort = connection.LocalEndPoint.Port,
                    RemoteAddress = connection.RemoteEndPoint.Address.ToString(),
                    RemotePort = connection.RemoteEndPoint.Port,
                    Protocol = "TCP",
                    State = connection.State.ToString()
                };

                connInfo.ProcessId = GetProcessIdFromNetstat(connection.LocalEndPoint.Port, "TCP", netstatData);
                connections.Add(connInfo);
            }

            var tcpListeners = properties.GetActiveTcpListeners();
            foreach (var listener in tcpListeners)
            {
                var connInfo = new ConnectionInfo
                {
                    LocalAddress = listener.Address.ToString(),
                    LocalPort = listener.Port,
                    RemoteAddress = "0.0.0.0",
                    RemotePort = 0,
                    Protocol = "TCP",
                    State = "LISTENING"
                };

                connInfo.ProcessId = GetProcessIdFromNetstat(listener.Port, "TCP", netstatData);
                connections.Add(connInfo);
            }

            var udpListeners = properties.GetActiveUdpListeners();
            foreach (var listener in udpListeners)
            {
                var connInfo = new ConnectionInfo
                {
                    LocalAddress = listener.Address.ToString(),
                    LocalPort = listener.Port,
                    RemoteAddress = "0.0.0.0",
                    RemotePort = 0,
                    Protocol = "UDP",
                    State = "LISTENING"
                };

                connInfo.ProcessId = GetProcessIdFromNetstat(listener.Port, "UDP", netstatData);
                connections.Add(connInfo);
            }

            return connections;
        }

        public List<ProcessInfo> GetProcessesByPort(int port)
        {
            var processes = new List<ProcessInfo>();
            var connections = GetActiveConnections();

            foreach (var connection in connections.Where(c => c.LocalPort == port))
            {
                if (connection.ProcessId > 0)
                {
                    try
                    {
                        var process = Process.GetProcessById(connection.ProcessId);
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
                }
            }

            return processes.DistinctBy(p => p.ProcessId).ToList();
        }

        private string GetNetstatData()
        {
            if (_cachedNetstatOutput != null && DateTime.Now - _cacheTime < CacheTimeout)
            {
                return _cachedNetstatOutput;
            }

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-ano",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processStartInfo);
                if (process == null) return string.Empty;

                var output = process.StandardOutput.ReadToEnd();
                if (!process.WaitForExit(5000))
                {
                    process.Kill();
                    return string.Empty;
                }

                _cachedNetstatOutput = output;
                _cacheTime = DateTime.Now;
                return output;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private int GetProcessIdFromNetstat(int port, string protocol, string netstatOutput)
        {
            if (string.IsNullOrEmpty(netstatOutput))
                return 0;

            try
            {
                var lines = netstatOutput.Split('\n');
                foreach (var line in lines)
                {
                    if (line.Contains($":{port} ") && line.Contains(protocol))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (protocol == "TCP" && parts.Length >= 5 && int.TryParse(parts[4], out int tcpPid))
                        {
                            return tcpPid;
                        }
                        else if (protocol == "UDP" && parts.Length >= 4 && int.TryParse(parts[3], out int udpPid))
                        {
                            return udpPid;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return 0;
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

        private bool IsSystemProcess(Process process)
        {
            try
            {
                var systemProcessNames = new[] { "System", "svchost", "csrss", "winlogon", "services", "lsass" };
                return systemProcessNames.Contains(process.ProcessName, StringComparer.OrdinalIgnoreCase) ||
                       process.SessionId == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}