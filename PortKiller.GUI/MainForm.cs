using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PortKiller.Core;
using PortKiller.SystemAccess.Models;

namespace PortKiller.GUI
{
    public partial class MainForm : Form
    {
        private readonly PortScanner _portScanner;
        private readonly ProcessKiller _processKiller;
        private List<PortInfo> _currentPortInfos;
        private const string PlaceholderText = "e.g. 8080 or 8080,3000,5000";

        public MainForm()
        {
            InitializeComponent();
            _portScanner = new PortScanner();
            _processKiller = new ProcessKiller();
            _currentPortInfos = new List<PortInfo>();
            
            SetupDataGridView();
            UpdateStatus("Ready");
        }

        private void SetupDataGridView()
        {
            dgvProcesses.Columns.Clear();
            dgvProcesses.Columns.Add("Port", "Port");
            dgvProcesses.Columns.Add("PID", "PID");
            dgvProcesses.Columns.Add("ProcessName", "Process");
            dgvProcesses.Columns.Add("Protocol", "Protocol");
            dgvProcesses.Columns.Add("Status", "Status");

            dgvProcesses.Columns["Port"].Width = 80;
            dgvProcesses.Columns["PID"].Width = 80;
            dgvProcesses.Columns["ProcessName"].Width = 150;
            dgvProcesses.Columns["Protocol"].Width = 80;
            dgvProcesses.Columns["Status"].Width = 100;

            dgvProcesses.Columns["PID"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvProcesses.Columns["Port"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvProcesses.Columns["Protocol"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void txtPorts_Enter(object sender, EventArgs e)
        {
            if (txtPorts.Text == PlaceholderText)
            {
                txtPorts.Text = "";
                txtPorts.ForeColor = Color.Black;
            }
        }

        private void txtPorts_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPorts.Text))
            {
                txtPorts.Text = PlaceholderText;
                txtPorts.ForeColor = Color.Gray;
            }
        }

        private async void btnScan_Click(object sender, EventArgs e)
        {
            await PerformScan();
        }

        private async Task PerformScan()
        {
            try
            {
                SetControlsEnabled(false);
                UpdateStatus("Scanning ports...");

                var portInput = txtPorts.Text.Trim();
                
                if (portInput == PlaceholderText || string.IsNullOrWhiteSpace(portInput))
                {
                    await Task.Run(() =>
                    {
                        _currentPortInfos = _portScanner.ScanAllActivePorts();
                    });
                    UpdateStatus($"Found {_currentPortInfos.Count} active ports");
                }
                else
                {
                    if (!_portScanner.IsValidPortInput(portInput))
                    {
                        MessageBox.Show("Invalid port input. Please enter valid port numbers (1-65535) separated by commas.",
                                      "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        UpdateStatus("Invalid port input");
                        return;
                    }

                    await Task.Run(() =>
                    {
                        var ports = _portScanner.ParsePorts(portInput);
                        _currentPortInfos = _portScanner.ScanPorts(ports);
                    });
                    
                    var activeCount = _currentPortInfos.Count(p => p.ProcessId > 0);
                    UpdateStatus($"Scanned {_currentPortInfos.Count} ports, {activeCount} active");
                }

                UpdateDataGridView();
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during scan: {ex.Message}", "Scan Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Scan failed");
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private void UpdateDataGridView()
        {
            dgvProcesses.Rows.Clear();

            foreach (var portInfo in _currentPortInfos)
            {
                var row = new DataGridViewRow();
                row.CreateCells(dgvProcesses);
                
                row.Cells[0].Value = portInfo.Port;
                row.Cells[1].Value = portInfo.ProcessId > 0 ? portInfo.ProcessId.ToString() : "-";
                row.Cells[2].Value = portInfo.ProcessName;
                row.Cells[3].Value = portInfo.Protocol;
                row.Cells[4].Value = portInfo.State;

                if (portInfo.State == "Available")
                {
                    row.DefaultCellStyle.ForeColor = Color.Green;
                }
                else if (portInfo.State == "Error")
                {
                    row.DefaultCellStyle.ForeColor = Color.Red;
                }
                else if (portInfo.ProcessId > 0)
                {
                    if (!_processKiller.CanKillProcess(portInfo.ProcessId))
                    {
                        row.DefaultCellStyle.ForeColor = Color.Orange;
                    }
                }

                dgvProcesses.Rows.Add(row);
            }
        }

        private void dgvProcesses_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            var hasActiveProcesses = _currentPortInfos.Any(p => p.ProcessId > 0 && p.State != "Available");
            var hasSelection = dgvProcesses.SelectedRows.Count > 0;
            var hasKillableSelection = false;

            if (hasSelection)
            {
                foreach (DataGridViewRow selectedRow in dgvProcesses.SelectedRows)
                {
                    var index = selectedRow.Index;
                    if (index >= 0 && index < _currentPortInfos.Count)
                    {
                        var portInfo = _currentPortInfos[index];
                        if (portInfo.ProcessId > 0 && _processKiller.CanKillProcess(portInfo.ProcessId))
                        {
                            hasKillableSelection = true;
                            break;
                        }
                    }
                }
            }

            btnKillSelected.Enabled = hasKillableSelection;
            btnKillAll.Enabled = hasActiveProcesses;
        }

        private async void btnKillSelected_Click(object sender, EventArgs e)
        {
            await KillSelectedProcesses();
        }

        private async Task KillSelectedProcesses()
        {
            var selectedRows = dgvProcesses.SelectedRows.Cast<DataGridViewRow>().ToList();
            
            if (!selectedRows.Any())
            {
                MessageBox.Show("No processes selected.", "No Selection", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var processesToKill = new List<PortInfo>();
            
            foreach (var row in selectedRows)
            {
                var index = row.Index;
                if (index >= 0 && index < _currentPortInfos.Count)
                {
                    var portInfo = _currentPortInfos[index];
                    if (portInfo.ProcessId > 0 && _processKiller.CanKillProcess(portInfo.ProcessId))
                    {
                        processesToKill.Add(portInfo);
                    }
                }
            }

            if (!processesToKill.Any())
            {
                MessageBox.Show("No killable processes selected.", "No Killable Processes", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            await KillProcesses(processesToKill);
        }

        private async void btnKillAll_Click(object sender, EventArgs e)
        {
            await KillAllProcesses();
        }

        private async Task KillAllProcesses()
        {
            var processesToKill = _currentPortInfos
                .Where(p => p.ProcessId > 0 && _processKiller.CanKillProcess(p.ProcessId))
                .ToList();

            if (!processesToKill.Any())
            {
                MessageBox.Show("No killable processes found.", "No Killable Processes", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to kill {processesToKill.Count} process(es)?", 
                "Confirm Kill All", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                await KillProcesses(processesToKill);
            }
        }

        private async Task KillProcesses(List<PortInfo> processesToKill)
        {
            try
            {
                SetControlsEnabled(false);
                UpdateStatus($"Killing {processesToKill.Count} process(es)...");

                var results = await Task.Run(() => 
                    _processKiller.KillProcessesByPorts(processesToKill));

                var successCount = results.Count(r => r.IsSuccess);
                var failureCount = results.Count - successCount;

                var resultMessage = $"Kill operation completed:\n" +
                                  $"• Successful: {successCount}\n" +
                                  $"• Failed: {failureCount}";

                if (failureCount > 0)
                {
                    resultMessage += "\n\nFailed processes:\n" +
                                   string.Join("\n", results.Where(r => !r.IsSuccess)
                                         .Select(r => r.GetDisplayMessage()));
                }

                MessageBox.Show(resultMessage, "Kill Results", 
                              MessageBoxButtons.OK, 
                              failureCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                UpdateStatus($"Kill completed: {successCount} success, {failureCount} failed");

                await Task.Delay(1000);
                await PerformScan();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during kill operation: {ex.Message}", "Kill Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Kill operation failed");
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await PerformScan();
        }

        private void SetControlsEnabled(bool enabled)
        {
            txtPorts.Enabled = enabled;
            btnScan.Enabled = enabled;
            btnRefresh.Enabled = enabled;
            
            if (!enabled)
            {
                btnKillSelected.Enabled = false;
                btnKillAll.Enabled = false;
            }
            else
            {
                UpdateButtonStates();
            }
        }

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateStatus), message);
                return;
            }

            lblStatus.Text = $"Status: {message}";
            Application.DoEvents();
        }
    }
}