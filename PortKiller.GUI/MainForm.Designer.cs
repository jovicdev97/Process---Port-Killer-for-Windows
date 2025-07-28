namespace PortKiller.GUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtPorts;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.DataGridView dgvProcesses;
        private System.Windows.Forms.Button btnKillSelected;
        private System.Windows.Forms.Button btnKillAll;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblPortsLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtPorts = new System.Windows.Forms.TextBox();
            this.btnScan = new System.Windows.Forms.Button();
            this.dgvProcesses = new System.Windows.Forms.DataGridView();
            this.btnKillSelected = new System.Windows.Forms.Button();
            this.btnKillAll = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblPortsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcesses)).BeginInit();
            this.SuspendLayout();
            
            this.lblPortsLabel.AutoSize = true;
            this.lblPortsLabel.Location = new System.Drawing.Point(12, 15);
            this.lblPortsLabel.Name = "lblPortsLabel";
            this.lblPortsLabel.Size = new System.Drawing.Size(45, 13);
            this.lblPortsLabel.TabIndex = 0;
            this.lblPortsLabel.Text = "Port(s):";
            
            this.txtPorts.Location = new System.Drawing.Point(63, 12);
            this.txtPorts.Name = "txtPorts";
            this.txtPorts.Size = new System.Drawing.Size(300, 20);
            this.txtPorts.TabIndex = 1;
            this.txtPorts.Text = "e.g. 8080 or 8080,3000,5000";
            this.txtPorts.ForeColor = System.Drawing.Color.Gray;
            this.txtPorts.Enter += new System.EventHandler(this.txtPorts_Enter);
            this.txtPorts.Leave += new System.EventHandler(this.txtPorts_Leave);
            
            this.btnScan.Location = new System.Drawing.Point(375, 10);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(75, 23);
            this.btnScan.TabIndex = 2;
            this.btnScan.Text = "Scan";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            
            this.dgvProcesses.AllowUserToAddRows = false;
            this.dgvProcesses.AllowUserToDeleteRows = false;
            this.dgvProcesses.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProcesses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProcesses.Location = new System.Drawing.Point(12, 45);
            this.dgvProcesses.Name = "dgvProcesses";
            this.dgvProcesses.ReadOnly = true;
            this.dgvProcesses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProcesses.Size = new System.Drawing.Size(560, 280);
            this.dgvProcesses.TabIndex = 3;
            this.dgvProcesses.MultiSelect = true;
            this.dgvProcesses.SelectionChanged += new System.EventHandler(this.dgvProcesses_SelectionChanged);
            
            this.btnKillSelected.Location = new System.Drawing.Point(12, 335);
            this.btnKillSelected.Name = "btnKillSelected";
            this.btnKillSelected.Size = new System.Drawing.Size(100, 30);
            this.btnKillSelected.TabIndex = 4;
            this.btnKillSelected.Text = "Kill Selected";
            this.btnKillSelected.UseVisualStyleBackColor = true;
            this.btnKillSelected.Enabled = false;
            this.btnKillSelected.Click += new System.EventHandler(this.btnKillSelected_Click);
            
            this.btnKillAll.Location = new System.Drawing.Point(125, 335);
            this.btnKillAll.Name = "btnKillAll";
            this.btnKillAll.Size = new System.Drawing.Size(75, 30);
            this.btnKillAll.TabIndex = 5;
            this.btnKillAll.Text = "Kill All";
            this.btnKillAll.UseVisualStyleBackColor = true;
            this.btnKillAll.Enabled = false;
            this.btnKillAll.Click += new System.EventHandler(this.btnKillAll_Click);
            
            this.btnRefresh.Location = new System.Drawing.Point(215, 335);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 30);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 375);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "Ready";
            
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 401);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnKillAll);
            this.Controls.Add(this.btnKillSelected);
            this.Controls.Add(this.dgvProcesses);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.txtPorts);
            this.Controls.Add(this.lblPortsLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Port Killer";
            ((System.ComponentModel.ISupportInitialize)(this.dgvProcesses)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}