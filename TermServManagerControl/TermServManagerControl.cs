using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security;
using System.Windows.Forms;

using Microsoft.ComputeCluster.Management;
using Microsoft.ComputeCluster.Management.ClusterModel;
using System.Diagnostics;

using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.ComputeCluster.Admin
{
    /// <summary>
    /// Control which manages multiple remote connection controls
    /// </summary>
    public partial class TermServManagerControl : UserControl
    {
        #region Private Fields

        /// <summary>
        /// Dictionary of remote connection controls, keyed by the node's NetBiosName
        /// </summary>
        private Dictionary<string, TermServControl> controlLookup;

        private Dictionary<string, Process> linuxClientLookup;
        private Process topMostProcess;
        private Process firstProcess;
        private bool isFirstProcess = true;

        /// <summary>
        /// The currently displayed remote connection control
        /// </summary>
        private TermServControl currentControl;

        /// <summary>
        /// Collection of NetBiosNames for the servers that have open connections
        /// </summary>
        private StringCollection connectedNodeNames;

        /// <summary>
        /// User name (including domain) to use when connecting to any node in the cluster
        /// </summary>
        private string userName;

        /// <summary>
        /// Password to use when connecting to any node in the cluster
        /// </summary>
        private SecureString password;

        /// <summary>
        /// The maximum number of open connections
        /// </summary>
        private int maxOpenConnections = 10;

        /// <summary>
        /// Timer for cleaning up disconnected controls
        /// </summary>
        private System.Timers.Timer cleanupTimer;

        /// <summary>
        /// Number of seconds between cleanups
        /// </summary>
        private double cleanupInterval = 1 * 60;

        #endregion

        #region Constructor

        /// <summary>
        /// Contructor initializes the control
        /// </summary>
        public TermServManagerControl()
        {
            InitializeComponent();
            controlLookup = new Dictionary<string, TermServControl>();
            connectedNodeNames = new StringCollection();
            currentControl = null;

            this.linuxClientLookup = new Dictionary<string, Process>();

            this.serverNameLabel.Text = Resources.NoNodeSelected;

            this.cleanupTimer = new System.Timers.Timer(cleanupInterval * 1000);
            this.cleanupTimer.Elapsed += new System.Timers.ElapsedEventHandler(cleanupTimer_Elapsed);
            this.cleanupTimer.Enabled = true;

            //Disable Windows Aero Shake
            SystemParametersInfo(0x0083, (Convert.ToUInt32(0)), (Convert.ToUInt32(0)), Convert.ToUInt32(1));
        }

        #endregion

        #region Properties

        /// <summary>
        /// User name (including domain) to use when connecting to any node in the cluster
        /// </summary>
        public string UserName
        {
            get
            {
                return this.userName;
            }
            set
            {
                this.userName = value;
            }
        }

        /// <summary>
        /// Password to use when connecting to any node in the cluster
        /// </summary>
        public SecureString Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

        public string CurrentServerName
        {
            get
            {
                return this.currentControl.ServerName;
            }
        }

        public string LinuxClientToolPath { get; set; }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Changes the currently displayed node.
        /// Creates a new remote connection control if neccesary.
        /// Connects the control.
        /// </summary>
        /// <param name="newSelectedNode"></param>
        public void SelectedNodeChanged(object sender, SelectedNodeChangedEventArgs e)
        {
            HideTSControl();

            HideLinuxClientWindows();

            currentControl = null;
            //this.topMostProcess = null;

            if (e.SelectedNodeName == null)
            {
                if (this.firstProcess != null && !this.firstProcess.HasExited)
                {
                    const int SW_SHOW = 5;
                    ShowWindow(this.firstProcess.MainWindowHandle, SW_SHOW);
                    this.firstProcess = null;
                }
                else
                {
                    this.serverNameLabel.Text = Resources.NoNodeSelected;
                }
            }
            else if (e.SelectedNodeName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            {
                this.serverNameLabel.Text = Resources.CannotConnectToLocalMachine;
            }
            else // e.SelectedNodeName != null && not local machine
            {
                if (IsLinuxNode(e.SelectedNodeName))
                {
                    ConnectToLinux(e.SelectedNodeName);
                }
                else
                {
                    ConnectTo(e.SelectedNodeName);
                }
            }
        }

        /// <summary>
        /// Event handler for the TermServControl.Disconnected event
        /// Removes the disconnected node from the list of connected nodes
        /// Raises the ConnectedNodeListChanged event.
        /// </summary>
        /// <param name="sender">the TermServControl object which raised the Disconnected event</param>
        /// <param name="e">not used</param>
        void Control_Disconnected(object sender, EventArgs e)
        {
            this.connectedNodeNames.Remove(((TermServControl)sender).ServerName);
            ConnectedNodeListChangedEventArgs eArgs = new ConnectedNodeListChangedEventArgs(connectedNodeNames);
            OnConnectedNodeListChanged(eArgs);
        }

        void LinuxClinetExited(object sender, EventArgs e)
        {
            string processArguments = ((Process)sender).StartInfo.Arguments;
            var pattern = new System.Text.RegularExpressions.Regex(@"-ssh (?<userName>\S+)@(?<hostName>\S+) \d+ -pw \S+");
            string serverName = pattern.Match(processArguments).Groups["hostName"].Value;
            this.connectedNodeNames.Remove(serverName);
            ConnectedNodeListChangedEventArgs eArgs = new ConnectedNodeListChangedEventArgs(connectedNodeNames);
            OnConnectedNodeListChanged(eArgs);
        }

        /// <summary>
        /// Event Handler for the NodeSelectionControl to disconnect the current server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DisconnectServerEventHandler(object sender, EventArgs e)
        {
            if (this.currentControl != null)
            {
                System.Diagnostics.Debug.Assert((e as DisconnectServerEventArgs).SelectedNode == this.CurrentServerName);
                if (this.currentControl.IsConnected)
                {
                    this.currentControl.Disconnect();
                }
            }
            string serverName = ((DisconnectServerEventArgs)e).SelectedNode;
            if (IsLinuxNode(serverName))
            {
                Process p;
                if (this.linuxClientLookup.TryGetValue(serverName, out p))
                {
                    if (p != null && !p.HasExited)
                    {
                        p.Kill();
                    }
                }
            }
        }

        /// <summary>
        /// Event handler fo the TermServControl.Connected event
        /// Adds the connected node to the list of connected nodes
        /// Raises the ConnectedNodeListChanged event.
        /// </summary>
        /// <param name="sender">the TermServControl object which raised the Connected event</param>
        /// <param name="e">not used</param>
        void Control_Connected(object sender, EventArgs e)
        {
            if (connectedNodeNames.Count >= maxOpenConnections)
            {
                DisconnectOne();
            }

            this.connectedNodeNames.Add(((TermServControl)sender).ServerName);
            ConnectedNodeListChangedEventArgs eArgs = new ConnectedNodeListChangedEventArgs(connectedNodeNames);
            OnConnectedNodeListChanged(eArgs);
        }

        void LinuxClientConnected(string serverName)
        {
            if (connectedNodeNames.Count >= maxOpenConnections)
            {
                DisconnectOne();
            }
            this.connectedNodeNames.Add(serverName);
            ConnectedNodeListChangedEventArgs eArgs = new ConnectedNodeListChangedEventArgs(connectedNodeNames);
            OnConnectedNodeListChanged(eArgs);
        }

        /// <summary>
        /// Event handler for the cleanupTimer.Elapsed event
        /// Disposes TermServControls that are have disconnected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cleanupTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new System.Timers.ElapsedEventHandler(cleanupTimer_Elapsed), sender, e);
                return;
            }

            List<string> keysToDelete = new List<string>();
            List<TermServControl> controlsToDispose = new List<TermServControl>();

            lock (this.controlLookup)
            {
                foreach (string key in this.controlLookup.Keys)
                {
                    TermServControl control = null;
                    this.controlLookup.TryGetValue(key, out control);
                    if (control == null)
                    {
                        continue;
                    }

                    if (!control.IsConnected)
                    {
                        if (this.currentControl == control)
                        {
                            HideTSControl();
                            this.serverNameLabel.Text = Resources.NoNodeSelected;
                            this.currentControl = null;
                        }
                        keysToDelete.Add(key);
                        controlsToDispose.Add(control);

                    }
                }

                foreach (string key in keysToDelete)
                {
                    this.controlLookup.Remove(key);
                }
            }
            foreach (TermServControl control in controlsToDispose)
            {
                control.Dispose();
            }

            List<string> linuxKeysToDelete = new List<string>();
            List<Process> processToDispose = new List<Process>();
            lock (this.linuxClientLookup)
            {
                foreach (string key in this.linuxClientLookup.Keys)
                {
                    Process p;
                    if (this.linuxClientLookup.TryGetValue(key, out p))
                    {
                        if (p != null && p.HasExited)
                        {
                            linuxKeysToDelete.Add(key);
                            processToDispose.Add(p);
                        }
                    }
                }
                foreach (string key in linuxKeysToDelete)
                {
                    this.linuxClientLookup.Remove(key);
                }
            }
            foreach (Process p in processToDispose)
            {
                p.Close();
                p.Dispose();
            }
        }

        #endregion

        #region Public Methods

        public void CloseProcesses()
        {
            foreach (Process p in this.linuxClientLookup.Values)
            {
                if (p != null)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch (InvalidOperationException) { }
                }
            }
        }

        public void ResetTopMostProcess()
        {
            Process p = this.topMostProcess;
            if (p != null && !p.HasExited)
            {
                const int HWND_BOTTOM = 1;
                const int SWP_NOMOVE = 0x0002;
                const int SWP_NOSIZE = 0x0001;
                const int SWP_NOACTIVATE = 0x0010;
                SetWindowPos(p.MainWindowHandle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }
        }

        public void UpdateTopMostProcessPosition()
        {
            Process p = this.topMostProcess;
            if (p != null && !p.HasExited)
            {
                System.Drawing.Point position = this.PointToScreen(this.Location);
                int width = this.Width;
                int height = this.Height;

                const int WS_CAPTION = 0x00C00000;
                const int WS_SYSMENU = 0x00080000;
                const int WS_THICKFRAME = 0x00040000;
                const int WS_MINIMIZE = 0x20000000;
                const int WS_MAXIMIZEBOX = 0x00010000;
                const int GWL_STYLE = -16;

                int style = 0;
                style = GetWindowLong(p.MainWindowHandle, GWL_STYLE);
                style &= ~WS_CAPTION;
                style &= ~WS_SYSMENU;
                style &= ~WS_THICKFRAME;
                style &= ~WS_MINIMIZE;
                style &= ~WS_MAXIMIZEBOX;

                const int HWND_TOPMOST = -1;
                const int SWP_NOMOVE = 0x0002;
                const int SWP_NOSIZE = 0x0001;
                const int SWP_FRAMECHANGED = 0x0020;
                const int SWP_NOACTIVATE = 0x0010;

                //move window
                SetWindowPos(p.MainWindowHandle, HWND_TOPMOST, position.X, position.Y+20, width-4, height-24, SWP_NOACTIVATE);
                //remove window frame
                SetWindowLong(p.MainWindowHandle, GWL_STYLE, style);
                SetWindowLong(p.MainWindowHandle, GWL_STYLE, style);
                SetWindowLong(p.MainWindowHandle, GWL_STYLE, style);
                SetWindowPos(p.MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_NOACTIVATE);
            }
        }

        #endregion

        #region Private Methods

        //move to a extra file
        //use full name (with class name) when being called
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);
        [DllImport("user32.dLL")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        //[DllImport("User32.dll")]
        //static extern bool SystemParametersInfo(uint iAction, uint iParameter, ref uint pParameter, uint iWinIni);
        [DllImport("User32.dll")]
        static extern bool SystemParametersInfo(uint iAction, uint iParameter, uint pParameter, uint iWinIni);
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        /// <summary>
        /// Helper method to connect to a server
        /// Includes creating a new term serv control
        /// and changing display as neede
        /// </summary>
        /// <param name="serverName"></param>
        private void ConnectTo(string serverName)
        {
            this.serverNameLabel.Text = serverName;

            lock (this.controlLookup)
            {
                controlLookup.TryGetValue(serverName, out currentControl);

                if (currentControl == null)
                {
                    currentControl = GetNewTSControl(serverName);
                    controlLookup.Add(serverName, currentControl);
                }

                DisplayTSControl(currentControl);
                string templateName;;
                // We treat azure node different with on-premies node
                if (IsAzureNode(serverName, out templateName))
                {
                    currentControl.ConnectionFailureHandler += this.OnAzureNodeConnectionFailure;
                    if (string.IsNullOrEmpty(templateName))
                    {
                        // The azure node hasn't deployed yet
                        MessageBox.Show(this.ParentForm, Resources.AzureNodeNotDeployed);
                    }
                    else
                    {
                        currentControl.Connect(templateName);
                    }
                }
                else
                {
                    // Retrieve the user/password first if never
                    if (string.IsNullOrEmpty(UserName))
                    {
                        // Only show the credential dialog when neccessary
                        GetCredential getCred = new GetCredential();
                        getCred.Get(((MainForm)this.ParentForm).Cluster.ClusterBinding, this.Handle, false);
                        this.userName = getCred.Username;
                        this.password = getCred.Password;
                    }
                    currentControl.Connect(this.UserName, this.Password);
                }
            }
        }

        private void ConnectToLinux(string serverName)
        {
            this.serverNameLabel.Text = serverName;
            lock (this.linuxClientLookup)
            {
                Process p;
                if (!this.linuxClientLookup.TryGetValue(serverName, out p) || p.HasExited)
                {
                    // Retrieve the user/password first if never
                    if (string.IsNullOrEmpty(this.UserName))
                    {
                        // Only show the credential dialog when neccessary
                        GetCredential getCred = new GetCredential();
                        getCred.Get(((MainForm)this.ParentForm).Cluster.ClusterBinding, this.Handle, false);
                        this.userName = getCred.Username;
                        this.password = getCred.Password;
                    }

                    // Take username and password from Credential
                    // Remove domain name
                    // Retrieve plain password 
                    string linuxUserName = this.userName.Split(new string[] { "\\" }, StringSplitOptions.None).Last();
                    string linuxPassword = new System.Net.NetworkCredential(string.Empty, this.password).Password;
                    
                    // Set linux client tool if it has not been set
                    if (string.IsNullOrEmpty(this.LinuxClientToolPath))
                    {
                        OnLinuxClientToolPathNullOrEmpty(null);
                    }

                    if (!string.IsNullOrEmpty(this.LinuxClientToolPath))
                    {
                        try
                        {
                            p = new Process();
                            string sshPort = "22";
                            p.StartInfo.FileName = this.LinuxClientToolPath;
                            p.StartInfo.Arguments = string.Format(@"-ssh {0}@{1} {2} -pw {3}", linuxUserName, serverName, sshPort, linuxPassword);
                            p.Start();
                            p.EnableRaisingEvents = true;
                            p.Exited += LinuxClinetExited; 

                            this.linuxClientLookup[serverName] = p;
                            LinuxClientConnected(serverName);
                            if (this.isFirstProcess)
                            {
                                this.firstProcess = p;
                                this.isFirstProcess = false;
                            }

                            p.WaitForInputIdle();

                            //hide icon in taskbar
                            const int SW_HIDE = 0x00;
                            const int SW_SHOW = 0x05;
                            const int GWL_EXSTYLE = -0x14;
                            const int WS_EX_TOOLWINDOW = 0x0080;
                            ShowWindow(p.MainWindowHandle, SW_HIDE);
                            SetWindowLong(p.MainWindowHandle, GWL_EXSTYLE, GetWindowLong(p.MainWindowHandle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
                            ShowWindow(p.MainWindowHandle, SW_SHOW);
                        }
                        catch (System.ComponentModel.Win32Exception e)
                        {
                            System.Windows.Forms.MessageBox.Show(e.ToString(), "Can not open linux client tool");
                        }
                    }
                }
                if (p != null && !p.HasExited)
                {
                    const int SW_SHOW = 5;
                    ShowWindow(p.MainWindowHandle, SW_SHOW);
                    this.topMostProcess = p;
                    this.UpdateTopMostProcessPosition();
                }
            }
        }

        private void OnAzureNodeConnectionFailure(object sender, EventArgs e)
        {
            TermServControl control = sender as TermServControl;

            Debug.Assert(control != null);
            if (control != null && 
                !string.IsNullOrEmpty(control.AzureServiceName))
            {
                ClusterManager cluster = ((MainForm)this.ParentForm).Cluster;
                Debug.Assert(cluster != null);

                // We can also delay the failure report by pre-aggregate at the client side if needed
                // the interface is designed to be flexible for this purpose
                cluster.HeadNodeManager.ReportRdpFailure(control.AzureServiceName, new string[]{control.ServerName}, new int[]{1});
            }
        }

        /// <summary>
        /// Check whether the node is Azure node and return the azure template
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        private bool IsAzureNode(string nodeName, out string templateName)
        {
            ClusterManager cluster = ((MainForm)this.ParentForm).Cluster;
            templateName = null;

            using (NodeCollection nodes = cluster.Nodes.GetNodeView(new Filter("SELECT * where NetBiosName=\"" + nodeName + "\"")))
            {
                nodes.Update();
                if (nodes.Count > 0)
                {
                    NodeDescription node = nodes[0];
                    // Check the node role
                    if (node.NodeRole == NodeRole.AzureVMNode || node.NodeRole == NodeRole.AzureWorkerNode)
                    {
                        // The node should be online/offline state
                        if ((node.NodeState == ClusterNodeState.Online) || (node.NodeState == ClusterNodeState.Offline) || (node.NodeState == ClusterNodeState.Draining))
                        {
                            // Return the template name
                            templateName = node["TemplateId"].Value.ToString();
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsLinuxNode(string nodeName)
        {
            ClusterManager cluster = ((MainForm)this.ParentForm).Cluster;

            using (NodeCollection nodes = cluster.Nodes.GetNodeView(new Filter("SELECT * where NetBiosName=\"" + nodeName + "\"")))
            {
                nodes.Update();
                if (nodes.Count > 0)
                {
                    NodeDescription node = nodes[0];
                    foreach (ServiceRoleType role in node.InstalledSvcRoles)
                    {
                        if (role == ServiceRoleType.LinuxNode)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Helper method to disconnect a control to allow another to be connected
        /// while not exceeding the max open connections
        /// </summary>
        private void DisconnectOne()
        {
            if (connectedNodeNames.Count > 0)
            {
                string serverToDisconnect = connectedNodeNames[0];

                if (IsLinuxNode(serverToDisconnect))
                {
                    Process p;
                    lock (this.linuxClientLookup)
                    {
                        linuxClientLookup.TryGetValue(serverToDisconnect, out p);
                        if (p != null && !p.HasExited)
                        {
                            p.Kill();
                        }
                    }
                }
                else
                {
                    TermServControl controlToDisconnect = null;

                    lock (this.controlLookup)
                    {
                        controlLookup.TryGetValue(serverToDisconnect, out controlToDisconnect);

                        if (controlToDisconnect != null)
                        {
                            controlToDisconnect.Disconnect();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to create and initalize a remote connection control for a new node
        /// Adds the new object to controlLookup
        /// </summary>
        /// <param name="serverName">The name of the server the new control should connect to</param>
        /// <returns>A new remote connection control for the server</returns>
        private TermServControl GetNewTSControl(string serverName)
        {
            TermServControl newControl = new TermServControl(serverName);
            
            newControl.Dock = System.Windows.Forms.DockStyle.Fill;
            newControl.Location = new System.Drawing.Point(0, 0);
            newControl.Name = serverName + "TSControl";
            newControl.Size = new System.Drawing.Size(763, 515);
            newControl.TabIndex = 0;

            newControl.Connected += new EventHandler(Control_Connected);
            newControl.Disconnected += new EventHandler(Control_Disconnected);
            
            return newControl;
        }

        /// <summary>
        /// Helper method to display a remote connection control
        /// Also hides any remote connection control already displayed
        /// </summary>
        /// <param name="tsControl">The control to display</param>
        private void DisplayTSControl(TermServControl tsControl)
        {
            HideTSControl();
            this.splitContainer1.Panel2.Controls.Add(tsControl);

            // move the displayed server to the end of the list of connected
            // servers to keep the list order of last viewed.
            if (connectedNodeNames.Contains(tsControl.ServerName))
            {
                connectedNodeNames.Remove(tsControl.ServerName);
                connectedNodeNames.Add(tsControl.ServerName);
            }
        }

        /// <summary>
        /// Helper method to hide all remote connection controls
        /// </summary>
        private void HideTSControl()
        {
            foreach(Control ctl in this.splitContainer1.Panel2.Controls)
            {
                if (ctl is TermServControl)
                {
                    this.splitContainer1.Panel2.Controls.Remove(ctl);
                }
            }
        }

        private void HideLinuxClientWindows()
        {
            const int SW_HIDE = 0; 
            foreach (Process p in this.linuxClientLookup.Values)
            {
                if (p != null && !p.HasExited)
                {
                    ShowWindow(p.MainWindowHandle, SW_HIDE);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the list of nodes which have open connections changes
        /// </summary>
        public event EventHandler<ConnectedNodeListChangedEventArgs> ConnectedNodeListChanged;

        /// <summary>
        /// Raises the ConnectedNodeListChanged event
        /// </summary>
        /// <param name="e">Contains event data</param>
        protected virtual void OnConnectedNodeListChanged(ConnectedNodeListChangedEventArgs e)
        {
            if (ConnectedNodeListChanged != null)
            {
                ConnectedNodeListChanged(this, e);
            }
        }

        public event System.Windows.Forms.LinkLabelLinkClickedEventHandler LinuxClientToolPathNullOrEmpty;
        protected virtual void OnLinuxClientToolPathNullOrEmpty(LinkLabelLinkClickedEventArgs e)
        {
            var temp = this.LinuxClientToolPathNullOrEmpty;
            if (temp != null) temp(this, e);
        }

        #endregion
    }
}