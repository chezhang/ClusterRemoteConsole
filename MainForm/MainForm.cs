using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;

using Microsoft.ComputeCluster.Management;

using System.Linq;

namespace Microsoft.ComputeCluster.Admin
{
    /// <summary>
    /// The main form for the TermServTool
    /// </summary>
    public partial class MainForm : Form
    {
        #region Private Fields
        /// <summary>
        /// ClusterManager for the open cluster
        /// </summary>
        private ClusterManager cluster;

        private static readonly string executingAssemblyPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static readonly string linuxClientToolPathStoreFileName = "ClusterRemoteConsoleLinuxClientToolPath";
        private static readonly string linuxClientToolPathStoreFile = string.Format("{0}\\{1}", executingAssemblyPath, linuxClientToolPathStoreFileName);

        #endregion

        #region Constructor

        /// <summary>
        /// Constuctor initializes the form with a clusterName and serverNames
        /// Any serverNames provided are the pre-selected servers.
        /// If serverNames are provided, opens to pre-selected servers view.
        /// Otherwise defaults to filtered nodes view.
        /// </summary>
        /// <param name="clusterName">Name of the cluster to open</param>
        /// <param name="serverNames">Names of pre-selected servers</param>
        public MainForm(string clusterName, StringCollection serverNames)
        {
            Initialize(clusterName, serverNames);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The current open cluster
        /// </summary>
        internal ClusterManager Cluster
        {
            get
            {
                return this.cluster;
            }
            set
            {
                this.cluster = value;
                this.Text = this.cluster != null ? this.cluster.ClusterBinding : Resources.DefaultTitleBar;
                OnClusterChanged(new ClusterChangedEventArgs(this.cluster));
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler for the FilterControl's HeightChanged event
        /// Changes the height of the panel containing the filter control 
        /// to the height specified by the control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterControl1_HeightChanged(object sender, HeightChangedEventArgs e)
        {
            this.splitContainer2.SplitterDistance = e.Height;
        }

        /// <summary>
        /// Event handler for the click event on the ChangePassword link label.
        /// Changes the stored credentials (including stored in credman if requested).
        /// Changes the cred used for future new connections.
        /// Does not change the cred used for existing connections.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangePasswordLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GetCredential getCred = new GetCredential();
            getCred.Get(this.Cluster.ClusterBinding, this.Handle, true);
            termServManagerControl1.UserName = getCred.Username;
            termServManagerControl1.Password = getCred.Password;
        }

        /// <summary>
        /// Event handler for the click event on the SetLinuxClientTool link label.
        /// Open a file choosing dialog for user to set a linux client tool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetLinuxClientToolLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var openFile = new System.Windows.Forms.OpenFileDialog();
            openFile.Title = "Select Linux Client Tool";
            openFile.Filter = "All files (*.*)|*.*|exe files (*.exe)|*.exe";
            openFile.FilterIndex = 2;
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.termServManagerControl1.LinuxClientToolPath = openFile.FileName;
                if (!string.Equals(openFile.FileName.Split(new char[] { '\\' }).Last(), "putty.exe"))
                {
                    System.Windows.Forms.MessageBox.Show("Only a putty client is supported to remote to the linux node.\nYou can click \"Set Linux Client Tool\" to set it again.", "Warm Tip");
                }
                try
                {
                    System.IO.File.WriteAllText(MainForm.linuxClientToolPathStoreFile, openFile.FileName);
                }
                catch (System.IO.IOException) { }
                catch (System.ArgumentNullException) { }
                catch (System.UnauthorizedAccessException) { }
                catch (System.NotSupportedException) { }
            }
        }
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.termServManagerControl1.CloseProcesses();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the form with a clusterName and serverNames
        /// If clusterName is not null or empty, cluster is opened on startup.
        /// If serverNames are listed and clusterName is specified, specified servers are listed in the NodeSelectionControl 
        /// and the filter panel is hidden.
        /// If serverNames are specified but clusterName is not, serverNames are ignored
        /// </summary>
        /// <param name="clusterName">Name of the cluster to open automatically. Open Cluster menu item enabled if null or empty.</param>
        /// <param name="serverNames">Names of servers to list automatically. Show Filters menu item enabled if null or empty, or if clusterName is not specified.</param>
        private void Initialize(string clusterName, StringCollection serverNames)
        {
            InitializeComponent();
            this.ClusterChanged += new EventHandler<ClusterChangedEventArgs>(this.filterControl1.ClusterChanged);
            this.filterControl1.NodeListChanged += new EventHandler<NodeListChangedEventArgs>(this.nodeSelectionControl1.NodeListChanged);
            this.nodeSelectionControl1.SelectedNodeChanged += new EventHandler<SelectedNodeChangedEventArgs>(this.termServManagerControl1.SelectedNodeChanged);
            this.nodeSelectionControl1.DisconnectServer += new EventHandler<DisconnectServerEventArgs>(this.termServManagerControl1.DisconnectServerEventHandler);
            this.termServManagerControl1.ConnectedNodeListChanged += new EventHandler<ConnectedNodeListChangedEventArgs>(this.nodeSelectionControl1.ConnectedNodeListChanged);
            this.filterControl1.HeightChanged += new EventHandler<HeightChangedEventArgs>(FilterControl1_HeightChanged);

            this.termServManagerControl1.LinuxClientToolPathNullOrEmpty += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SetLinuxClientToolLinkLabel_LinkClicked);

            Cluster = null;

            if (String.IsNullOrEmpty(clusterName))
            {
                throw new ArgumentNullException(Resources.ClusterNameArgException);
            }

            Cluster = OpenCluster(clusterName);

            if (Cluster == null)
            {
                throw new ArgumentException(String.Format(Resources.ClusterNameInvalid, clusterName));
            }

            if (serverNames != null && serverNames.Count > 0)
            {
                this.filterControl1.SetPreSelectedNodeList(serverNames);
            }

            SetLinuxClientToolPath();
        }

        /// <summary>
        /// Opens and returns a ClusterManager for the named cluster.
        /// Displays a message box with any errors encountered.
        /// </summary>
        /// <param name="clusterName">The name of the cluster to open</param>
        private ClusterManager OpenCluster(string clusterName)
        {
            ClusterManager clMan = null;
            try
            {
                clMan = new ClusterManager(clusterName);
                clMan.Connect();
            }
            catch (Exception ex)
            {
                clMan = null;

                string message = String.Format(Resources.ClusterConnectionError, clusterName, ex.Message);
                string caption = Resources.ConnectionFailedDialogCaption;
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return clMan;
        }

        private void SetLinuxClientToolPath()
        {
            try
            {
                this.termServManagerControl1.LinuxClientToolPath = System.IO.File.ReadAllText(MainForm.linuxClientToolPathStoreFile);
            }
            catch (System.IO.IOException) { }
            catch (System.UnauthorizedAccessException) { }
            catch (System.NotSupportedException) { }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the open cluster changes
        /// </summary>
        public event EventHandler<ClusterChangedEventArgs> ClusterChanged;

        /// <summary>
        /// Raises the ClusterChanged event
        /// </summary>
        /// <param name="e">Contains event data</param>
        protected virtual void OnClusterChanged(ClusterChangedEventArgs e)
        {
            if (ClusterChanged != null)
            {
                ClusterChanged(this, e);
            }
        }

        #endregion

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            this.termServManagerControl1.ResetTopMostProcess();
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            this.termServManagerControl1.UpdateTopMostProcessPosition();
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
            this.termServManagerControl1.UpdateTopMostProcessPosition();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            this.termServManagerControl1.UpdateTopMostProcessPosition();
        }

    }
}