using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Microsoft.ComputeCluster.Management;
using Microsoft.ComputeCluster.Management.ClusterModel;

namespace Microsoft.ComputeCluster.Admin
{
    /// <summary>
    /// Control which displays a list of nodes and maintains the selected node.
    /// </summary>
    public partial class NodeSelectionControl : UserControl
    {
        #region Private Fields
        /// <summary>
        /// Key used to identify the Connected icon
        /// </summary>
        private const string ImageKeyConnected = "Connected";

        /// <summary>
        /// The NetBiosName of the selected node that this object controls
        /// </summary>
        private string selectedNodeName;

        /// <summary>
        /// The list of nodes to be displayed
        /// </summary>
        private List<string> nodeNames;

        /// <summary>
        /// The names of the nodes that have open connections
        /// </summary>
        private StringCollection connectedNodeNames;

        /// <summary>
        /// Cache of ListViewItems keyed by node name
        /// </summary>
        private Dictionary<string, ListViewItem> itemLookup;

        /// <summary>
        /// Whether or not the list view is currently updating its item list
        /// </summary>
        private bool updating;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor initializes the control
        /// </summary>
        public NodeSelectionControl()
        {
            InitializeComponent();

            updating = false;
            nodeNames = null;
            selectedNodeName = null;
            connectedNodeNames = new StringCollection();
            itemLookup = new Dictionary<string,ListViewItem>();

            OnSelectedNodeChanged(new SelectedNodeChangedEventArgs(selectedNodeName));

            this.nodeListView.Columns.Add("Node", -2, HorizontalAlignment.Left);
            this.nodeListView.SmallImageList = this.imageList1;
        }

        #endregion

        #region Properties

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler fo the SelectedIndexChanged event on NodeListView
        /// Updates the selectedNode and notifies observers
        /// </summary>
        /// <param name="sender">not used</param>
        /// <param name="e">not used</param>
        private void NodeListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            string newSelectedNodeName = null;

            if (this.nodeListView.SelectedItems.Count == 0)
            {
                newSelectedNodeName = null;
            }
            else
            {
                newSelectedNodeName = this.nodeListView.SelectedItems[0].Text;
            }
            System.IO.File.AppendAllText("c:\\sshLog.txt", string.Format("{0}: NodeListView_SelectedIndexChanged, {1}, updating={2}\r\n", System.DateTime.Now.Ticks, newSelectedNodeName, updating));
            if (!updating && newSelectedNodeName != selectedNodeName)
            {
                selectedNodeName = newSelectedNodeName;
                OnSelectedNodeChanged(new SelectedNodeChangedEventArgs(selectedNodeName));
            }
        }

        /// <summary>
        /// Event handler for the NodeListChanged event.
        /// Updates the NodeListView.
        /// </summary>
        /// <param name="nodes"></param>
        public void NodeListChanged(object sender, NodeListChangedEventArgs e)
        {
            this.nodeNames = e.NodeNameList;
            UpdateNodeListView();
        }


        /// <summary>
        /// Event handler for the ConnectedNodeListChanged event.
        /// Updates the NodeListView.
        /// </summary>
        /// <param name="nodes"></param>
        public void ConnectedNodeListChanged(object sender, ConnectedNodeListChangedEventArgs e)
        {
            this.connectedNodeNames = e.ConnectedNodeNames;
            UpdateNodeListView();
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DisconnectServer(sender, new DisconnectServerEventArgs(selectedNodeName));
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        /// <summary>
        /// Displays the node collection
        /// Also updates nodeLookup
        /// </summary>
        private void UpdateNodeListView()
        {
            List<ListViewItem> items = new List<ListViewItem>();

            if (nodeNames != null)
            {
                for (int i = 0; i < nodeNames.Count; i++) 
                {
                    string name = nodeNames[i];
                    ListViewItem item = null;

                    if (!itemLookup.TryGetValue(name, out item))
                    {
                        item = new ListViewItem(name);
                        itemLookup.Add(name, item);
                    }

                    if (connectedNodeNames.Contains(name))
                    {
                        item.ImageKey = ImageKeyConnected;
                    }
                    else
                    {
                        item.ImageIndex = -1;
                    }

                    // Prevent the duplicate node name in which scenario: same netbios name but different domain
                    if (!items.Contains(item))
                    {
                        items.Add(item);
                    }
                }
            }

            this.nodeListView.BeginUpdate();

            updating = true;

            this.nodeListView.Items.Clear();
            nodeListView.Items.AddRange(items.ToArray());

            updating = false;

            this.nodeListView.EndUpdate();
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the selected node changes.
        /// </summary>
        public event EventHandler<SelectedNodeChangedEventArgs> SelectedNodeChanged;

        /// <summary>
        /// Raises the SelectedNodeChanged event
        /// </summary>
        /// <param name="e">Contains event data</param>
        protected virtual void OnSelectedNodeChanged(SelectedNodeChangedEventArgs e)
        {
            if (SelectedNodeChanged != null)
            {
                SelectedNodeChanged(this, e);
            }
        }

        /// <summary>
        /// Occurs when the user right clicks on a server name and selects disconnect.
        /// </summary>
        public event EventHandler<DisconnectServerEventArgs> DisconnectServer;

        /// <summary>
        /// Raises the DisconnectServer event
        /// </summary>
        /// <param name="e">Contains event data</param>
        protected virtual void OnDisconnectServer(DisconnectServerEventArgs e)
        {
            if (DisconnectServer != null)
            {
                DisconnectServer(this, e);
            }
        }

        #endregion

    }
}