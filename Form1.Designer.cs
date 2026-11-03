namespace PhilippineSMS_MLP
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblTitle, lblAuthors, lblCourse, lblSubtitle;
        private System.Windows.Forms.Label lblData, lblModelStatus, lblResult, lblScore, lblNotes, lblProgress;
        private System.Windows.Forms.Button btnSelect, btnTrain, btnTest, btnExport, btnClassify;

        private System.Windows.Forms.TextBox txtSms, txtOutput;
        private System.Windows.Forms.ProgressBar progressTrain;
        private System.Windows.Forms.Panel leftPanel, rightPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.lblTitle = new System.Windows.Forms.Label();
            this.lblAuthors = new System.Windows.Forms.Label();
            this.lblCourse = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblData = new System.Windows.Forms.Label();
            this.lblModelStatus = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblNotes = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();

            this.btnSelect = new System.Windows.Forms.Button();
            this.btnTrain = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClassify = new System.Windows.Forms.Button();

            this.txtSms = new System.Windows.Forms.TextBox();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.progressTrain = new System.Windows.Forms.ProgressBar();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.rightPanel = new System.Windows.Forms.Panel();

            this.SuspendLayout();

            // WINDOW
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor = System.Drawing.Color.White;
            this.ForeColor = System.Drawing.Color.FromArgb(25, 25, 25);
            this.ClientSize = new System.Drawing.Size(1130, 770);
            this.MinimumSize = new System.Drawing.Size(1130, 770);
            this.MaximumSize = new System.Drawing.Size(1500, 900);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Philippine SMS Spam Detection | Custom C# MLP";

            // HEADER
            this.lblTitle.Location = new System.Drawing.Point(24, 15);
            this.lblTitle.Size = new System.Drawing.Size(1000, 36);
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 19F);
            this.lblTitle.Text = "Philippine SMS Spam Detection";

            this.lblAuthors.Location = new System.Drawing.Point(27, 53);
            this.lblAuthors.Size = new System.Drawing.Size(1020, 21);
            this.lblAuthors.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lblAuthors.Text = "Gabison, Joshua V. and Sabagkit, John Arcel R.";

            this.lblCourse.Location = new System.Drawing.Point(27, 76);
            this.lblCourse.Size = new System.Drawing.Size(850, 19);
            this.lblCourse.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCourse.Text = "BSCS-3 F2 CS345";

            this.lblSubtitle.Location = new System.Drawing.Point(27, 101);
            this.lblSubtitle.Size = new System.Drawing.Size(1000, 20);
            this.lblSubtitle.ForeColor = System.Drawing.Color.DimGray;
            this.lblSubtitle.Text = "Custom 3-layer C# backpropagation neural network  |  Educational prototype";

            // TOOLBAR: Only the four requested buttons remain visible.
            this.btnSelect.Location = new System.Drawing.Point(24, 135);
            this.btnSelect.Size = new System.Drawing.Size(131, 42);
            this.btnSelect.Text = "Select CSV";
            this.btnSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelect.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(175, 175, 175);
            this.btnSelect.BackColor = System.Drawing.Color.White;
            this.btnSelect.ForeColor = System.Drawing.Color.FromArgb(35, 35, 35);
            this.btnSelect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTrain.Location = new System.Drawing.Point(163, 135);
            this.btnTrain.Size = new System.Drawing.Size(147, 42);
            this.btnTrain.Text = "Train / Compare";
            this.btnTrain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTrain.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(175, 175, 175);
            this.btnTrain.BackColor = System.Drawing.Color.White;
            this.btnTrain.ForeColor = System.Drawing.Color.FromArgb(35, 35, 35);
            this.btnTrain.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTest.Location = new System.Drawing.Point(318, 135);
            this.btnTest.Size = new System.Drawing.Size(87, 42);
            this.btnTest.Text = "Test";
            this.btnTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTest.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(175, 175, 175);
            this.btnTest.BackColor = System.Drawing.Color.White;
            this.btnTest.ForeColor = System.Drawing.Color.FromArgb(35, 35, 35);
            this.btnTest.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExport.Location = new System.Drawing.Point(413, 135);
            this.btnExport.Size = new System.Drawing.Size(133, 42);
            this.btnExport.Text = "Export Results";
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(175, 175, 175);
            this.btnExport.BackColor = System.Drawing.Color.White;
            this.btnExport.ForeColor = System.Drawing.Color.FromArgb(35, 35, 35);
            this.btnExport.Cursor = System.Windows.Forms.Cursors.Hand;

            this.btnSelect.Click += new System.EventHandler(this.BtnSelect_Click);
            this.btnTrain.Click += new System.EventHandler(this.BtnTrain_Click);
            this.btnTest.Click += new System.EventHandler(this.BtnTest_Click);
            this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);

            this.lblModelStatus.Location = new System.Drawing.Point(940, 142);
            this.lblModelStatus.Size = new System.Drawing.Size(157, 28);
            this.lblModelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblModelStatus.Text = "Model: not trained";

            // LEFT PANEL: SMS INPUT AND CLASSIFICATION
            this.leftPanel.Location = new System.Drawing.Point(24, 191);
            this.leftPanel.Size = new System.Drawing.Size(512, 516);
            this.leftPanel.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
            this.leftPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            var inputTitle = new System.Windows.Forms.Label();
            inputTitle.Location = new System.Drawing.Point(17, 14);
            inputTitle.Size = new System.Drawing.Size(460, 27);
            inputTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
            inputTitle.Text = "Paste an SMS message";

            this.txtSms.Location = new System.Drawing.Point(18, 52);
            this.txtSms.Size = new System.Drawing.Size(472, 190);
            this.txtSms.Multiline = true;
            this.txtSms.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSms.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSms.Text = "[LUCK9] You won P1000! Claim your bonus now: https://example.com/promo";

            this.btnClassify.Location = new System.Drawing.Point(18, 256);

            this.btnClassify.Size = new System.Drawing.Size(472, 42);

            this.btnClassify.Text = "Classify SMS";

            this.btnClassify.FlatStyle = System.Windows.Forms.FlatStyle.Flat;

            this.btnClassify.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(175, 175, 175);

            this.btnClassify.BackColor = System.Drawing.Color.White;

            this.btnClassify.ForeColor = System.Drawing.Color.FromArgb(35, 35, 35);

            this.btnClassify.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClassify.BackColor = System.Drawing.Color.FromArgb(30, 30, 30);
            this.btnClassify.ForeColor = System.Drawing.Color.White;
            this.btnClassify.FlatAppearance.BorderSize = 0;
            this.btnClassify.Click += new System.EventHandler(this.BtnClassify_Click);

            this.lblResult.Location = new System.Drawing.Point(18, 321);
            this.lblResult.Size = new System.Drawing.Size(472, 34);
            this.lblResult.Font = new System.Drawing.Font("Segoe UI Semibold", 15F);
            this.lblResult.Text = "No classification yet";

            this.lblScore.Location = new System.Drawing.Point(18, 360);
            this.lblScore.Size = new System.Drawing.Size(470, 52);
            this.lblScore.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblScore.Text = "Train a model before classifying.";

            this.lblNotes.Location = new System.Drawing.Point(18, 426);
            this.lblNotes.Size = new System.Drawing.Size(470, 75);
            this.lblNotes.ForeColor = System.Drawing.Color.DimGray;
            this.lblNotes.Text = "Classification is guidance only, not proof that an SMS is safe. Never share OTPs, passwords or PINs based solely on a model result.";

            this.leftPanel.Controls.Add(inputTitle);
            this.leftPanel.Controls.Add(this.txtSms);
            this.leftPanel.Controls.Add(this.btnClassify);
            this.leftPanel.Controls.Add(this.lblResult);
            this.leftPanel.Controls.Add(this.lblScore);
            this.leftPanel.Controls.Add(this.lblNotes);

            // RIGHT PANEL: MODEL EVALUATION, FULL HEIGHT (GRAPH REMOVED)
            this.rightPanel.Location = new System.Drawing.Point(550, 191);
            this.rightPanel.Size = new System.Drawing.Size(554, 516);
            this.rightPanel.BackColor = System.Drawing.Color.FromArgb(249, 249, 249);
            this.rightPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            var reportTitle = new System.Windows.Forms.Label();
            reportTitle.Location = new System.Drawing.Point(17, 14);
            reportTitle.Size = new System.Drawing.Size(490, 27);
            reportTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 12F);
            reportTitle.Text = "Model evaluation";

            this.txtOutput.Location = new System.Drawing.Point(18, 51);
            this.txtOutput.Size = new System.Drawing.Size(514, 442);
            this.txtOutput.Multiline = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtOutput.ReadOnly = true;
            this.txtOutput.WordWrap = true;
            this.txtOutput.BackColor = System.Drawing.Color.White;
            this.txtOutput.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtOutput.Text =
                "1. Select a labeled CSV (label,message).\r\n" +
                "2. Train and compare MLP models.\r\n" +
                "3. Test once on untouched held-out records.\r\n" +
                "4. Paste an SMS to classify.";

            this.rightPanel.Controls.Add(reportTitle);
            this.rightPanel.Controls.Add(this.txtOutput);

            // FOOTER AND STATUS
            this.lblData.Location = new System.Drawing.Point(26, 714);
            this.lblData.Size = new System.Drawing.Size(1075, 20);
            this.lblData.ForeColor = System.Drawing.Color.DimGray;
            this.lblData.Text = "Dataset: not selected";

            this.lblProgress.Location = new System.Drawing.Point(26, 746);
            this.lblProgress.Size = new System.Drawing.Size(1075, 18);
            this.lblProgress.ForeColor = System.Drawing.Color.DimGray;
            this.lblProgress.Text = "Ready";

            this.progressTrain.Location = new System.Drawing.Point(24, 739);
            this.progressTrain.Size = new System.Drawing.Size(1080, 5);
            this.progressTrain.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressTrain.Visible = false;

            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblAuthors);
            this.Controls.Add(this.lblCourse);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnTrain);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.lblModelStatus);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.lblData);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.progressTrain);

            this.ResumeLayout(false);
        }
    }
}
