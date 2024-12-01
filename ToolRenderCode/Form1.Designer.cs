namespace ToolRenderCode
{
    partial class RenderForm
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
            this.txtClass = new System.Windows.Forms.RichTextBox();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.btnFolder = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnTaoFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtClass
            // 
            this.txtClass.Location = new System.Drawing.Point(59, 152);
            this.txtClass.Name = "txtClass";
            this.txtClass.Size = new System.Drawing.Size(546, 261);
            this.txtClass.TabIndex = 0;
            this.txtClass.Text = "";
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(59, 79);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(546, 20);
            this.txtFolder.TabIndex = 1;
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(699, 152);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFile.TabIndex = 2;
            this.btnSelectFile.Text = "Chọn File";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // btnFolder
            // 
            this.btnFolder.Location = new System.Drawing.Point(699, 79);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(75, 23);
            this.btnFolder.TabIndex = 3;
            this.btnFolder.Text = "Chọn Folder";
            this.btnFolder.UseVisualStyleBackColor = true;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(59, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Đường dẫn project";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(62, 120);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Text Class Model";
            // 
            // btnTaoFile
            // 
            this.btnTaoFile.Location = new System.Drawing.Point(699, 205);
            this.btnTaoFile.Name = "btnTaoFile";
            this.btnTaoFile.Size = new System.Drawing.Size(75, 23);
            this.btnTaoFile.TabIndex = 6;
            this.btnTaoFile.Text = "Tạo File";
            this.btnTaoFile.UseVisualStyleBackColor = true;
            this.btnTaoFile.Click += new System.EventHandler(this.btnTaoFile_Click);
            // 
            // RenderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnTaoFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnFolder);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.txtClass);
            this.Name = "RenderForm";
            this.Text = "ToolRenderCode";
            this.Load += new System.EventHandler(this.RenderForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtClass;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnTaoFile;
    }
}

