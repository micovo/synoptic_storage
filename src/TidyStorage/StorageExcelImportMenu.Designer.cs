namespace TidyStorage
{
    partial class StorageExcelImportMenu
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.labelSupplierName = new System.Windows.Forms.Label();
            this.labelSupplierNumber = new System.Windows.Forms.Label();
            this.labelStoragePlace = new System.Windows.Forms.Label();
            this.labelStock = new System.Windows.Forms.Label();
            this.labelPartName = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.Location = new System.Drawing.Point(12, 182);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(255, 38);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel Import";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelCount
            // 
            this.labelCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelCount.ForeColor = System.Drawing.SystemColors.Highlight;
            this.labelCount.Location = new System.Drawing.Point(152, 17);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(114, 24);
            this.labelCount.TabIndex = 6;
            this.labelCount.Text = "0 / 0";
            this.labelCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.labelCount.Click += new System.EventHandler(this.labelCount_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Rows To Process:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 37.40157F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 62.59843F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.labelSupplierName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelSupplierNumber, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelStoragePlace, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelStock, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelPartName, 1, 4);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 51);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(254, 123);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Supplier Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Supplier Number";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Storage Place";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Stock";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Part Name";
            // 
            // labelSupplierName
            // 
            this.labelSupplierName.AutoSize = true;
            this.labelSupplierName.Location = new System.Drawing.Point(97, 0);
            this.labelSupplierName.Name = "labelSupplierName";
            this.labelSupplierName.Size = new System.Drawing.Size(35, 13);
            this.labelSupplierName.TabIndex = 5;
            this.labelSupplierName.Text = "label7";
            // 
            // labelSupplierNumber
            // 
            this.labelSupplierNumber.AutoSize = true;
            this.labelSupplierNumber.Location = new System.Drawing.Point(97, 25);
            this.labelSupplierNumber.Name = "labelSupplierNumber";
            this.labelSupplierNumber.Size = new System.Drawing.Size(35, 13);
            this.labelSupplierNumber.TabIndex = 6;
            this.labelSupplierNumber.Text = "label8";
            // 
            // labelStoragePlace
            // 
            this.labelStoragePlace.AutoSize = true;
            this.labelStoragePlace.Location = new System.Drawing.Point(97, 50);
            this.labelStoragePlace.Name = "labelStoragePlace";
            this.labelStoragePlace.Size = new System.Drawing.Size(35, 13);
            this.labelStoragePlace.TabIndex = 7;
            this.labelStoragePlace.Text = "label9";
            // 
            // labelStock
            // 
            this.labelStock.AutoSize = true;
            this.labelStock.Location = new System.Drawing.Point(97, 75);
            this.labelStock.Name = "labelStock";
            this.labelStock.Size = new System.Drawing.Size(41, 13);
            this.labelStock.TabIndex = 8;
            this.labelStock.Text = "label10";
            // 
            // labelPartName
            // 
            this.labelPartName.AutoSize = true;
            this.labelPartName.Location = new System.Drawing.Point(97, 100);
            this.labelPartName.Name = "labelPartName";
            this.labelPartName.Size = new System.Drawing.Size(41, 13);
            this.labelPartName.TabIndex = 9;
            this.labelPartName.Text = "label11";
            // 
            // StorageExcelImportMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(279, 232);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.labelCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StorageExcelImportMenu";
            this.Text = "Excel Import Menu";
            this.TopMost = true;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label labelCount;
        public System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.Label labelSupplierName;
        public System.Windows.Forms.Label labelSupplierNumber;
        public System.Windows.Forms.Label labelStoragePlace;
        public System.Windows.Forms.Label labelStock;
        public System.Windows.Forms.Label labelPartName;
    }
}