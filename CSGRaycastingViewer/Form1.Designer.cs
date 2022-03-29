
namespace CSGRaycastingViewer
{
    partial class Form1
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lightDirectionGroupBox = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.psiLightDirectionTrackBar = new System.Windows.Forms.TrackBar();
            this.psiLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.phiLightDirectionTrackBar = new System.Windows.Forms.TrackBar();
            this.phiLabel = new System.Windows.Forms.Label();
            this.cameraRotationGroupBox = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.rotationXTrackBar = new System.Windows.Forms.TrackBar();
            this.rotationZLabel = new System.Windows.Forms.Label();
            this.rotationYTrackBar = new System.Windows.Forms.TrackBar();
            this.rotationYLabel = new System.Windows.Forms.Label();
            this.rotationZTrackBar = new System.Windows.Forms.TrackBar();
            this.rotationXLabel = new System.Windows.Forms.Label();
            this.lightParamsGroupBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.kaTrackBar = new System.Windows.Forms.TrackBar();
            this.kaLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.kdTrackBar = new System.Windows.Forms.TrackBar();
            this.kdLabel = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.ksTrackBar = new System.Windows.Forms.TrackBar();
            this.ksLabel = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.mTrackBar = new System.Windows.Forms.TrackBar();
            this.mLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.lightDirectionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.psiLightDirectionTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.phiLightDirectionTrackBar)).BeginInit();
            this.cameraRotationGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rotationXTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotationYTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotationZTrackBar)).BeginInit();
            this.lightParamsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kaTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kdTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ksTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.SizeChanged += new System.EventHandler(this.splitContainer1_Panel1_SizeChanged);
            this.splitContainer1.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lightParamsGroupBox);
            this.splitContainer1.Panel2.Controls.Add(this.lightDirectionGroupBox);
            this.splitContainer1.Panel2.Controls.Add(this.cameraRotationGroupBox);
            this.splitContainer1.Size = new System.Drawing.Size(928, 543);
            this.splitContainer1.SplitterDistance = 698;
            this.splitContainer1.TabIndex = 0;
            // 
            // lightDirectionGroupBox
            // 
            this.lightDirectionGroupBox.Controls.Add(this.label6);
            this.lightDirectionGroupBox.Controls.Add(this.psiLightDirectionTrackBar);
            this.lightDirectionGroupBox.Controls.Add(this.psiLabel);
            this.lightDirectionGroupBox.Controls.Add(this.label4);
            this.lightDirectionGroupBox.Controls.Add(this.phiLightDirectionTrackBar);
            this.lightDirectionGroupBox.Controls.Add(this.phiLabel);
            this.lightDirectionGroupBox.Location = new System.Drawing.Point(3, 194);
            this.lightDirectionGroupBox.Name = "lightDirectionGroupBox";
            this.lightDirectionGroupBox.Size = new System.Drawing.Size(220, 119);
            this.lightDirectionGroupBox.TabIndex = 7;
            this.lightDirectionGroupBox.TabStop = false;
            this.lightDirectionGroupBox.Text = "Light Direction (spherical)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 80);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "psi";
            // 
            // psiLightDirectionTrackBar
            // 
            this.psiLightDirectionTrackBar.Location = new System.Drawing.Point(25, 70);
            this.psiLightDirectionTrackBar.Maximum = 90;
            this.psiLightDirectionTrackBar.Minimum = -90;
            this.psiLightDirectionTrackBar.Name = "psiLightDirectionTrackBar";
            this.psiLightDirectionTrackBar.Size = new System.Drawing.Size(167, 45);
            this.psiLightDirectionTrackBar.TabIndex = 10;
            this.psiLightDirectionTrackBar.Scroll += new System.EventHandler(this.lightDirectionTrackBar_Scroll);
            // 
            // psiLabel
            // 
            this.psiLabel.AutoSize = true;
            this.psiLabel.Location = new System.Drawing.Point(198, 80);
            this.psiLabel.Name = "psiLabel";
            this.psiLabel.Size = new System.Drawing.Size(13, 13);
            this.psiLabel.TabIndex = 11;
            this.psiLabel.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "phi";
            // 
            // phiLightDirectionTrackBar
            // 
            this.phiLightDirectionTrackBar.Location = new System.Drawing.Point(25, 19);
            this.phiLightDirectionTrackBar.Maximum = 180;
            this.phiLightDirectionTrackBar.Minimum = -180;
            this.phiLightDirectionTrackBar.Name = "phiLightDirectionTrackBar";
            this.phiLightDirectionTrackBar.Size = new System.Drawing.Size(167, 45);
            this.phiLightDirectionTrackBar.TabIndex = 7;
            this.phiLightDirectionTrackBar.Scroll += new System.EventHandler(this.lightDirectionTrackBar_Scroll);
            // 
            // phiLabel
            // 
            this.phiLabel.AutoSize = true;
            this.phiLabel.Location = new System.Drawing.Point(198, 29);
            this.phiLabel.Name = "phiLabel";
            this.phiLabel.Size = new System.Drawing.Size(13, 13);
            this.phiLabel.TabIndex = 8;
            this.phiLabel.Text = "0";
            // 
            // cameraRotationGroupBox
            // 
            this.cameraRotationGroupBox.Controls.Add(this.label3);
            this.cameraRotationGroupBox.Controls.Add(this.label2);
            this.cameraRotationGroupBox.Controls.Add(this.label1);
            this.cameraRotationGroupBox.Controls.Add(this.rotationXTrackBar);
            this.cameraRotationGroupBox.Controls.Add(this.rotationZLabel);
            this.cameraRotationGroupBox.Controls.Add(this.rotationYTrackBar);
            this.cameraRotationGroupBox.Controls.Add(this.rotationYLabel);
            this.cameraRotationGroupBox.Controls.Add(this.rotationZTrackBar);
            this.cameraRotationGroupBox.Controls.Add(this.rotationXLabel);
            this.cameraRotationGroupBox.Location = new System.Drawing.Point(3, 3);
            this.cameraRotationGroupBox.Name = "cameraRotationGroupBox";
            this.cameraRotationGroupBox.Size = new System.Drawing.Size(220, 185);
            this.cameraRotationGroupBox.TabIndex = 6;
            this.cameraRotationGroupBox.TabStop = false;
            this.cameraRotationGroupBox.Text = "Camera Rotation";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Z";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(14, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Y";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "X";
            // 
            // rotationXTrackBar
            // 
            this.rotationXTrackBar.Location = new System.Drawing.Point(25, 19);
            this.rotationXTrackBar.Maximum = 180;
            this.rotationXTrackBar.Minimum = -180;
            this.rotationXTrackBar.Name = "rotationXTrackBar";
            this.rotationXTrackBar.Size = new System.Drawing.Size(167, 45);
            this.rotationXTrackBar.TabIndex = 0;
            this.rotationXTrackBar.Scroll += new System.EventHandler(this.trackBar_Scroll);
            // 
            // rotationZLabel
            // 
            this.rotationZLabel.AutoSize = true;
            this.rotationZLabel.Location = new System.Drawing.Point(198, 134);
            this.rotationZLabel.Name = "rotationZLabel";
            this.rotationZLabel.Size = new System.Drawing.Size(13, 13);
            this.rotationZLabel.TabIndex = 5;
            this.rotationZLabel.Text = "0";
            // 
            // rotationYTrackBar
            // 
            this.rotationYTrackBar.Location = new System.Drawing.Point(25, 70);
            this.rotationYTrackBar.Maximum = 180;
            this.rotationYTrackBar.Minimum = -180;
            this.rotationYTrackBar.Name = "rotationYTrackBar";
            this.rotationYTrackBar.Size = new System.Drawing.Size(167, 45);
            this.rotationYTrackBar.TabIndex = 1;
            this.rotationYTrackBar.Scroll += new System.EventHandler(this.trackBar_Scroll);
            // 
            // rotationYLabel
            // 
            this.rotationYLabel.AutoSize = true;
            this.rotationYLabel.Location = new System.Drawing.Point(198, 80);
            this.rotationYLabel.Name = "rotationYLabel";
            this.rotationYLabel.Size = new System.Drawing.Size(13, 13);
            this.rotationYLabel.TabIndex = 4;
            this.rotationYLabel.Text = "0";
            // 
            // rotationZTrackBar
            // 
            this.rotationZTrackBar.Location = new System.Drawing.Point(25, 121);
            this.rotationZTrackBar.Maximum = 180;
            this.rotationZTrackBar.Minimum = -180;
            this.rotationZTrackBar.Name = "rotationZTrackBar";
            this.rotationZTrackBar.Size = new System.Drawing.Size(167, 45);
            this.rotationZTrackBar.TabIndex = 2;
            this.rotationZTrackBar.Scroll += new System.EventHandler(this.trackBar_Scroll);
            // 
            // rotationXLabel
            // 
            this.rotationXLabel.AutoSize = true;
            this.rotationXLabel.Location = new System.Drawing.Point(198, 29);
            this.rotationXLabel.Name = "rotationXLabel";
            this.rotationXLabel.Size = new System.Drawing.Size(13, 13);
            this.rotationXLabel.TabIndex = 3;
            this.rotationXLabel.Text = "0";
            // 
            // lightParamsGroupBox
            // 
            this.lightParamsGroupBox.Controls.Add(this.label12);
            this.lightParamsGroupBox.Controls.Add(this.mTrackBar);
            this.lightParamsGroupBox.Controls.Add(this.mLabel);
            this.lightParamsGroupBox.Controls.Add(this.label10);
            this.lightParamsGroupBox.Controls.Add(this.ksTrackBar);
            this.lightParamsGroupBox.Controls.Add(this.ksLabel);
            this.lightParamsGroupBox.Controls.Add(this.label8);
            this.lightParamsGroupBox.Controls.Add(this.kdTrackBar);
            this.lightParamsGroupBox.Controls.Add(this.kdLabel);
            this.lightParamsGroupBox.Controls.Add(this.label5);
            this.lightParamsGroupBox.Controls.Add(this.kaTrackBar);
            this.lightParamsGroupBox.Controls.Add(this.kaLabel);
            this.lightParamsGroupBox.Location = new System.Drawing.Point(3, 315);
            this.lightParamsGroupBox.Name = "lightParamsGroupBox";
            this.lightParamsGroupBox.Size = new System.Drawing.Size(220, 223);
            this.lightParamsGroupBox.TabIndex = 8;
            this.lightParamsGroupBox.TabStop = false;
            this.lightParamsGroupBox.Text = "Light Parameters";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(19, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "ka";
            // 
            // kaTrackBar
            // 
            this.kaTrackBar.Location = new System.Drawing.Point(25, 19);
            this.kaTrackBar.Maximum = 100;
            this.kaTrackBar.Name = "kaTrackBar";
            this.kaTrackBar.Size = new System.Drawing.Size(167, 45);
            this.kaTrackBar.TabIndex = 7;
            this.kaTrackBar.Value = 10;
            this.kaTrackBar.Scroll += new System.EventHandler(this.lightParamtersTrackBar_Scroll);
            // 
            // kaLabel
            // 
            this.kaLabel.AutoSize = true;
            this.kaLabel.Location = new System.Drawing.Point(198, 29);
            this.kaLabel.Name = "kaLabel";
            this.kaLabel.Size = new System.Drawing.Size(13, 13);
            this.kaLabel.TabIndex = 8;
            this.kaLabel.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 80);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(19, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "kd";
            // 
            // kdTrackBar
            // 
            this.kdTrackBar.Location = new System.Drawing.Point(25, 70);
            this.kdTrackBar.Maximum = 100;
            this.kdTrackBar.Name = "kdTrackBar";
            this.kdTrackBar.Size = new System.Drawing.Size(167, 45);
            this.kdTrackBar.TabIndex = 10;
            this.kdTrackBar.Value = 45;
            this.kdTrackBar.Scroll += new System.EventHandler(this.lightParamtersTrackBar_Scroll);
            // 
            // kdLabel
            // 
            this.kdLabel.AutoSize = true;
            this.kdLabel.Location = new System.Drawing.Point(198, 80);
            this.kdLabel.Name = "kdLabel";
            this.kdLabel.Size = new System.Drawing.Size(13, 13);
            this.kdLabel.TabIndex = 11;
            this.kdLabel.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 130);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(18, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "ks";
            // 
            // ksTrackBar
            // 
            this.ksTrackBar.Location = new System.Drawing.Point(25, 120);
            this.ksTrackBar.Maximum = 100;
            this.ksTrackBar.Name = "ksTrackBar";
            this.ksTrackBar.Size = new System.Drawing.Size(167, 45);
            this.ksTrackBar.TabIndex = 13;
            this.ksTrackBar.Value = 45;
            this.ksTrackBar.Scroll += new System.EventHandler(this.lightParamtersTrackBar_Scroll);
            // 
            // ksLabel
            // 
            this.ksLabel.AutoSize = true;
            this.ksLabel.Location = new System.Drawing.Point(198, 130);
            this.ksLabel.Name = "ksLabel";
            this.ksLabel.Size = new System.Drawing.Size(13, 13);
            this.ksLabel.TabIndex = 14;
            this.ksLabel.Text = "0";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 181);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(15, 13);
            this.label12.TabIndex = 18;
            this.label12.Text = "m";
            // 
            // mTrackBar
            // 
            this.mTrackBar.Location = new System.Drawing.Point(25, 171);
            this.mTrackBar.Maximum = 100;
            this.mTrackBar.Minimum = 1;
            this.mTrackBar.Name = "mTrackBar";
            this.mTrackBar.Size = new System.Drawing.Size(167, 45);
            this.mTrackBar.TabIndex = 16;
            this.mTrackBar.Value = 30;
            this.mTrackBar.Scroll += new System.EventHandler(this.lightParamtersTrackBar_Scroll);
            // 
            // mLabel
            // 
            this.mLabel.AutoSize = true;
            this.mLabel.Location = new System.Drawing.Point(198, 181);
            this.mLabel.Name = "mLabel";
            this.mLabel.Size = new System.Drawing.Size(13, 13);
            this.mLabel.TabIndex = 17;
            this.mLabel.Text = "0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(928, 543);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.lightDirectionGroupBox.ResumeLayout(false);
            this.lightDirectionGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.psiLightDirectionTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.phiLightDirectionTrackBar)).EndInit();
            this.cameraRotationGroupBox.ResumeLayout(false);
            this.cameraRotationGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rotationXTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotationYTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rotationZTrackBar)).EndInit();
            this.lightParamsGroupBox.ResumeLayout(false);
            this.lightParamsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kaTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kdTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ksTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TrackBar rotationZTrackBar;
        private System.Windows.Forms.TrackBar rotationYTrackBar;
        private System.Windows.Forms.TrackBar rotationXTrackBar;
        private System.Windows.Forms.Label rotationZLabel;
        private System.Windows.Forms.Label rotationYLabel;
        private System.Windows.Forms.Label rotationXLabel;
        private System.Windows.Forms.GroupBox lightDirectionGroupBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar psiLightDirectionTrackBar;
        private System.Windows.Forms.Label psiLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar phiLightDirectionTrackBar;
        private System.Windows.Forms.Label phiLabel;
        private System.Windows.Forms.GroupBox cameraRotationGroupBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox lightParamsGroupBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TrackBar mTrackBar;
        private System.Windows.Forms.Label mLabel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TrackBar ksTrackBar;
        private System.Windows.Forms.Label ksLabel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TrackBar kdTrackBar;
        private System.Windows.Forms.Label kdLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar kaTrackBar;
        private System.Windows.Forms.Label kaLabel;
    }
}

