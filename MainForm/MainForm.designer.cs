namespace Microsoft.ComputeCluster.Admin
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.SetLinuxClientToolLinkLabel1 = new System.Windows.Forms.LinkLabel();
            this.changePasswordLinkLabel = new System.Windows.Forms.LinkLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.filterControl1 = new Microsoft.ComputeCluster.Admin.FilterControl();
            this.nodeSelectionControl1 = new Microsoft.ComputeCluster.Admin.NodeSelectionControl();
            this.termServManagerControl1 = new Microsoft.ComputeCluster.Admin.TermServManagerControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.filterControl1);
            resources.ApplyResources(this.splitContainer2.Panel1, "splitContainer2.Panel1");
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.TabStop = false;
            // 
            // splitContainer3
            // 
            resources.ApplyResources(this.splitContainer3, "splitContainer3");
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.nodeSelectionControl1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.SetLinuxClientToolLinkLabel1);
            this.splitContainer3.Panel2.Controls.Add(this.changePasswordLinkLabel);
            this.splitContainer3.TabStop = false;
            // 
            // SetLinuxClientToolLinkLabel1
            // 
            resources.ApplyResources(this.SetLinuxClientToolLinkLabel1, "SetLinuxClientToolLinkLabel1");
            this.SetLinuxClientToolLinkLabel1.Name = "SetLinuxClientToolLinkLabel1";
            this.SetLinuxClientToolLinkLabel1.TabStop = true;
            this.SetLinuxClientToolLinkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SetLinuxClientToolLinkLabel_LinkClicked);
            // 
            // changePasswordLinkLabel
            // 
            resources.ApplyResources(this.changePasswordLinkLabel, "changePasswordLinkLabel");
            this.changePasswordLinkLabel.Name = "changePasswordLinkLabel";
            this.changePasswordLinkLabel.TabStop = true;
            this.changePasswordLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ChangePasswordLinkLabel_LinkClicked);
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.termServManagerControl1);
            this.splitContainer1.TabStop = false;
            // 
            // filterControl1
            // 
            resources.ApplyResources(this.filterControl1, "filterControl1");
            this.filterControl1.Name = "filterControl1";
            this.filterControl1.UsePreSelectedNodes = false;
            // 
            // nodeSelectionControl1
            // 
            resources.ApplyResources(this.nodeSelectionControl1, "nodeSelectionControl1");
            this.nodeSelectionControl1.Name = "nodeSelectionControl1";
            // 
            // termServManagerControl1
            // 
            this.termServManagerControl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.termServManagerControl1, "termServManagerControl1");
            this.termServManagerControl1.LinuxClientToolPath = null;
            this.termServManagerControl1.Name = "termServManagerControl1";
            this.termServManagerControl1.Password = null;
            this.termServManagerControl1.TabStop = false;
            this.termServManagerControl1.UserName = null;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "MainForm";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Move += new System.EventHandler(this.MainForm_Move);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TermServManagerControl termServManagerControl1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private FilterControl filterControl1;
        private NodeSelectionControl nodeSelectionControl1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.LinkLabel changePasswordLinkLabel;
        private System.Windows.Forms.LinkLabel SetLinuxClientToolLinkLabel1;
    }
}

