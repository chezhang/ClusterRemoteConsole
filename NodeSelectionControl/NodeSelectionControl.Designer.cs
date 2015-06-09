namespace Microsoft.ComputeCluster.Admin
{
    partial class NodeSelectionControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NodeSelectionControl));
            this.nodeListView = new System.Windows.Forms.ListView();
            this.serverContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.serverContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // nodeListView
            // 
            this.nodeListView.ContextMenuStrip = this.serverContextMenuStrip;
            resources.ApplyResources(this.nodeListView, "nodeListView");
            this.nodeListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.nodeListView.HideSelection = false;
            this.nodeListView.MultiSelect = false;
            this.nodeListView.Name = "nodeListView";
            this.nodeListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.nodeListView.UseCompatibleStateImageBehavior = false;
            this.nodeListView.View = System.Windows.Forms.View.Details;
            this.nodeListView.SelectedIndexChanged += new System.EventHandler(this.NodeListView_SelectedIndexChanged);
            // 
            // serverContextMenuStrip
            // 
            this.serverContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disconnectToolStripMenuItem});
            this.serverContextMenuStrip.Name = "serverContextMenuStrip";
            resources.ApplyResources(this.serverContextMenuStrip, "serverContextMenuStrip");
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            resources.ApplyResources(this.disconnectToolStripMenuItem, "disconnectToolStripMenuItem");
            this.disconnectToolStripMenuItem.Click += new System.EventHandler(this.disconnectToolStripMenuItem_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Connected");
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // NodeSelectionControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.nodeListView);
            this.Controls.Add(this.label1);
            this.Name = "NodeSelectionControl";
            this.serverContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView nodeListView;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip serverContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;

    }
}
