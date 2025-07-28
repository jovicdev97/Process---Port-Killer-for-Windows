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
        private static readonly TimeSpan CacheTimeout = TimeSpan.FromSeconds(1);

        public List<ConnectionInfo> GetActiveConnections()
        {
            var connections = new List<ConnectionInfo>();
            var netstatData = GetNetstatData();

            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();

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

                connections.AddRange(GetAdditionalNetstatConnections(netstatData));
            }
            catch (Exception)
            {
                connections.AddRange(GetNetstatOnlyConnections(netstatData));
            }

            return connections.DistinctBy(c => new { c.LocalPort, c.Protocol, c.State }).ToList();
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
                if (!process.WaitForExit(8000))
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

        private List<ConnectionInfo> GetAdditionalNetstatConnections(string netstatOutput)
        {
            var connections = new List<ConnectionInfo>();
            
            if (string.IsNullOrEmpty(netstatOutput))
                return connections;

            try
            {
                var lines = netstatOutput.Split('\n');
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || 
                        !trimmedLine.Contains("LISTENING") && !trimmedLine.Contains("ESTABLISHED"))
                        continue;

                    var parts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 4) continue;

                    var protocol = parts[0].ToUpper();
                    if (protocol != "TCP" && protocol != "UDP") continue;

                    var localAddress = parts[1];
                    var colonIndex = localAddress.LastIndexOf(':');
                    if (colonIndex == -1) continue;

                    if (!int.TryParse(localAddress.Substring(colonIndex + 1), out int port))
                        continue;

                    var state = protocol == "UDP" ? "LISTENING" : (parts.Length > 3 ? parts[3] : "UNKNOWN");
                    var pidIndex = protocol == "TCP" ? 4 : 3;
                    int processId = 0;
                    
                    if (parts.Length > pidIndex && int.TryParse(parts[pidIndex], out int pid))
                        processId = pid;

                    var connInfo = new ConnectionInfo
                    {
                        LocalAddress = localAddress.Substring(0, colonIndex),
                        LocalPort = port,
                        RemoteAddress = "0.0.0.0",
                        RemotePort = 0,
                        Protocol = protocol,
                        State = state,
                        ProcessId = processId
                    };

                    connections.Add(connInfo);
                }
            }
            catch (Exception)
            {
            }

            return connections;
        }

        private List<ConnectionInfo> GetNetstatOnlyConnections(string netstatOutput)
        {
            var connections = new List<ConnectionInfo>();
            
            if (string.IsNullOrEmpty(netstatOutput))
                return connections;

            try
            {
                var lines = netstatOutput.Split('\n');
                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;

                    var parts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 4) continue;

                    var protocol = parts[0].ToUpper();
                    if (protocol != "TCP" && protocol != "UDP") continue;

                    var localAddress = parts[1];
                    var colonIndex = localAddress.LastIndexOf(':');
                    if (colonIndex == -1) continue;

                    if (!int.TryParse(localAddress.Substring(colonIndex + 1), out int port))
                        continue;

                    var remoteAddress = parts.Length > 2 ? parts[2] : "0.0.0.0:0";
                    var remoteColonIndex = remoteAddress.LastIndexOf(':');
                    var remoteIp = remoteColonIndex > 0 ? remoteAddress.Substring(0, remoteColonIndex) : "0.0.0.0";
                    int.TryParse(remoteColonIndex > 0 ? remoteAddress.Substring(remoteColonIndex + 1) : "0", out int remotePort);

                    var state = "UNKNOWN";
                    var pidIndex = -1;

                    if (protocol == "TCP" && parts.Length >= 4)
                    {
                        state = parts[3];
                        pidIndex = 4;
                    }
                    else if (protocol == "UDP")
                    {
                        state = "LISTENING";
                        pidIndex = 3;
                    }

                    int processId = 0;
                    if (pidIndex > 0 && parts.Length > pidIndex && int.TryParse(parts[pidIndex], out int pid))
                        processId = pid;

                    var connInfo = new ConnectionInfo
                    {
                        LocalAddress = localAddress.Substring(0, colonIndex),
                        LocalPort = port,
                        RemoteAddress = remoteIp,
                        RemotePort = remotePort,
                        Protocol = protocol,
                        State = state,
                        ProcessId = processId
                    };

                    connections.Add(connInfo);
                }
            }
            catch (Exception)
            {
            }

            return connections;
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