using System.Drawing;

namespace chessair_client
{
    partial class chess
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
            //base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(chess));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.oponent = new System.Windows.Forms.Label();
            this.me = new System.Windows.Forms.Label();
            this.play = new System.Windows.Forms.Button();
            this.outcome_tx = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "_green..png");
            this.imageList1.Images.SetKeyName(1, "_red..png");
            this.imageList1.Images.SetKeyName(2, "_white..png");
            this.imageList1.Images.SetKeyName(3, "tiny_green..png");
            this.imageList1.Images.SetKeyName(4, "tiny_red..png");
            this.imageList1.Images.SetKeyName(5, "chessairboard.png");
            // 
            // oponent
            // 
            this.oponent.AutoSize = true;
            this.oponent.BackColor = System.Drawing.Color.Transparent;
            this.oponent.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.oponent.ForeColor = System.Drawing.Color.DodgerBlue;
            this.oponent.Location = new System.Drawing.Point(3, 3);
            this.oponent.Name = "oponent";
            this.oponent.Size = new System.Drawing.Size(83, 25);
            this.oponent.TabIndex = 0;
            this.oponent.Text = "oponent";
            this.oponent.Visible = false;
            // 
            // me
            // 
            this.me.AutoSize = true;
            this.me.BackColor = System.Drawing.Color.Transparent;
            this.me.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.me.ForeColor = System.Drawing.Color.DodgerBlue;
            this.me.Location = new System.Drawing.Point(3, 432);
            this.me.Name = "me";
            this.me.Size = new System.Drawing.Size(39, 25);
            this.me.TabIndex = 1;
            this.me.Text = "me";
            this.me.Visible = false;
            // 
            // play
            // 
            this.play.AutoEllipsis = true;
            this.play.BackColor = System.Drawing.Color.ForestGreen;
            this.play.Cursor = System.Windows.Forms.Cursors.Hand;
            this.play.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F);
            this.play.Location = new System.Drawing.Point(542, 160);
            this.play.Name = "play";
            this.play.Size = new System.Drawing.Size(160, 120);
            this.play.TabIndex = 2;
            this.play.Text = "play";
            this.play.UseVisualStyleBackColor = false;
            this.play.Click += new System.EventHandler(this.play_Click);
            // 
            // outcome_tx
            // 
            this.outcome_tx.AutoSize = true;
            this.outcome_tx.BackColor = System.Drawing.Color.Transparent;
            this.outcome_tx.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Underline);
            this.outcome_tx.ForeColor = System.Drawing.Color.Green;
            this.outcome_tx.Location = new System.Drawing.Point(536, 138);
            this.outcome_tx.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.outcome_tx.Name = "outcome_tx";
            this.outcome_tx.Size = new System.Drawing.Size(0, 31);
            this.outcome_tx.TabIndex = 3;
            // 
            // chess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(758, 460);
            this.Controls.Add(this.outcome_tx);
            this.Controls.Add(this.play);
            this.Controls.Add(this.me);
            this.Controls.Add(this.oponent);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Name = "chess";
            this.Text = "chessair";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label oponent;
        private System.Windows.Forms.Label me;
        private System.Windows.Forms.Button play;
        private System.Windows.Forms.Label outcome_tx;
    }
}

