using PortKiller.SystemAccess;
using PortKiller.SystemAccess.Models;

namespace PortKiller.Core
{
    public class PortScanner
    {
        private readonly NetworkHelper _networkHelper;

        public PortScanner()
        {
            _networkHelper = new NetworkHelper();
        }

        public List<int> ParsePorts(string input)
        {
            var ports = new List<int>();

            if (string.IsNullOrWhiteSpace(input))
            {
                return ports;
            }

            try
            {
                var portStrings = input.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var portString in portStrings)
                {
                    var trimmed = portString.Trim();

                    if (trimmed.Contains('-'))
                    {
                        var rangeParts = trimmed.Split('-');
                        if (rangeParts.Length == 2 &&
                            int.TryParse(rangeParts[0].Trim(), out int startPort) &&
                            int.TryParse(rangeParts[1].Trim(), out int endPort))
                        {
                            if (ValidatePort(startPort) && ValidatePort(endPort) && startPort <= endPort)
                            {
                                for (int port = startPort; port <= endPort; port++)
                                {
                                    if (!ports.Contains(port))
                                    {
                                        ports.Add(port);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (int.TryParse(trimmed, out int port) && ValidatePort(port))
                        {
                            if (!ports.Contains(port))
                            {
                                ports.Add(port);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return new List<int>();
            }

            return ports.OrderBy(p => p).ToList();
        }

        public bool ValidatePort(int port)
        {
            return port >= 1 && port <= 65535;
        }

        public List<PortInfo> ScanPorts(List<int> ports)
        {
            var results = new List<PortInfo>();

            if (ports == null || !ports.Any())
            {
                return results;
            }

            try
            {
                var connections = _networkHelper.GetActiveConnections();

                foreach (int port in ports)
                {
                    var portConnections = connections.Where(c => c.LocalPort == port).ToList();

                    if (portConnections.Any())
                    {
                        foreach (var connection in portConnections)
                        {
                            var processName = GetProcessName(connection.ProcessId);
                            
                            var portInfo = new PortInfo(
                                port,
                                connection.ProcessId,
                                processName,
                                connection.Protocol,
                                connection.State
                            );

                            results.Add(portInfo);
                        }
                    }
                    else
                    {
                        var portInfo = new PortInfo(
                            port,
                            0,
                            "Not in use",
                            "",
                            "Available"
                        );

                        results.Add(portInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                foreach (int port in ports)
                {
                    var errorInfo = new PortInfo(
                        port,
                        0,
                        $"Error: {ex.Message}",
                        "",
                        "Error"
                    );
                    results.Add(errorInfo);
                }
            }

            return results.OrderBy(r => r.Port).ToList();
        }

        public List<PortInfo> ScanAllActivePorts()
        {
            var results = new List<PortInfo>();

            try
            {
                var connections = _networkHelper.GetActiveConnections();

                foreach (var connection in connections)
                {
                    var processName = GetProcessName(connection.ProcessId);
                    
                    var portInfo = new PortInfo(
                        connection.LocalPort,
                        connection.ProcessId,
                        processName,
                        connection.Protocol,
                        connection.State
                    );

                    results.Add(portInfo);
                }
            }
            catch (Exception ex)
            {
                var errorInfo = new PortInfo(
                    0,
                    0,
                    $"Scan failed: {ex.Message}",
                    "",
                    "Error"
                );
                results.Add(errorInfo);
            }

            return results.OrderBy(r => r.Port).ToList();
        }

        private string GetProcessName(int processId)
        {
            if (processId <= 0)
            {
                return "Unknown";
            }

            try
            {
                var processHelper = new ProcessHelper();
                var process = processHelper.GetProcessById(processId);
                return process?.ProcessName ?? "Unknown";
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        public bool IsValidPortInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var ports = ParsePorts(input);
            return ports.Any();
        }
    }
}