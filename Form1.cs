using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhilippineSMS_MLP
{
    public partial class Form1 : Form
    {
        private List<SmsRecord> records;
        private SmsTrainingResult result;
        private Evaluation heldOutEvaluation;
        private bool trainedUsingCurrentCsv;
        private readonly string outputFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PhilippineSMS-MLP_Results");
        private readonly SmsTrainer trainer = new SmsTrainer();

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetControlState();
            // Auto-load the submitted CSV copied to the output Data folder during build.
            string localCsv = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "filipino_sms.csv");
            if (File.Exists(localCsv)) LoadCsv(localCsv);
        }

        private void SetControlState()
        {
            btnTrain.Enabled = records != null && result == null;
            btnTest.Enabled = result != null && trainedUsingCurrentCsv;
            btnClassify.Enabled = result != null;
            btnExport.Enabled = result != null;
            lblModelStatus.Text = result == null ? "Model: not trained" : "Model: ready";
        }

        private void AppendResult(string text)
        {
            txtOutput.AppendText((txtOutput.TextLength > 0 ? Environment.NewLine : "") + text);
            txtOutput.SelectionStart = txtOutput.TextLength;
            txtOutput.ScrollToCaret();
        }

        private void LoadCsv(string path)
        {
            try
            {
                string audit;
                records = SmsDataset.Load(path, out audit);
                // The supplied CSV already contains the previously added scam examples.
                // There is no extra training CSV or separate supplement.
                result = null;
                heldOutEvaluation = null;
                trainedUsingCurrentCsv = false;
                lblData.Text = "Dataset: " + Path.GetFileName(path) + " | " + audit;
                txtOutput.Text = audit + Environment.NewLine +
                    "Train / Compare learns vocabulary from training records only." + Environment.NewLine +
                    "Notice: Original labels have not all been manually audited.";
                lblProgress.Text = "Dataset loaded successfully.";
                SetControlState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not load labeled CSV:\n" + ex.Message,
                    "Dataset error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "CSV data (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.Title = "Choose CSV with label,message columns";
                if (dialog.ShowDialog(this) == DialogResult.OK) LoadCsv(dialog.FileName);
            }
        }

        private async void BtnTrain_Click(object sender, EventArgs e)
        {
            if (records == null) return;
            if (MessageBox.Show(this,
                "Training and comparison can take a while. Continue?",
                "Train network", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            btnSelect.Enabled = btnTrain.Enabled = btnTest.Enabled = btnExport.Enabled = btnClassify.Enabled = false;
            progressTrain.Visible = true;
            txtOutput.Text = "Training started. Test data will remain untouched until you click Test.";
            var progress = new Progress<string>(s =>
            {
                lblProgress.Text = s;
                if (s.IndexOf("validation F1=", StringComparison.Ordinal) >= 0) AppendResult(s);
            });
            try
            {
                Directory.CreateDirectory(outputFolder);
                SmsTrainingResult newResult = await Task.Run(() => trainer.TrainAndCompare(records, progress, outputFolder));
                result = newResult;
                trainedUsingCurrentCsv = true;
                heldOutEvaluation = null;
                txtOutput.Text = "BEST CONFIGURATION (validation selection)" + Environment.NewLine +
                    result.Config + Environment.NewLine +
                    string.Format(CultureInfo.InvariantCulture, "Decision threshold: {0:F2}\r\n", result.Threshold) +
                    "TRAINING / VALIDATION / TEST: " + result.TrainingCount + " / " +
                    result.ValidationCount + " / " + result.TestCount + Environment.NewLine +
                    "VALIDATION METRICS" + Environment.NewLine + result.Validation.Summary() + Environment.NewLine +
                    Environment.NewLine + "Click TEST for held-out metrics. Test did not affect model selection." + Environment.NewLine +
                    "Training and validation CSV logs saved under Documents\\PhilippineSMS-MLP_Results.";
                lblProgress.Text = "Training complete. You can now click Test or Classify SMS.";
            }
            catch (Exception ex)
            {
                result = null;
                trainedUsingCurrentCsv = false;
                MessageBox.Show(this, "Training failed:\n" + ex, "Training error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblProgress.Text = "Training failed. Check dataset and try again.";
            }
            finally
            {
                btnSelect.Enabled = true;
                progressTrain.Visible = false;
                SetControlState();
            }
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            if (result == null || !trainedUsingCurrentCsv || records == null) return;
            try
            {
                heldOutEvaluation = trainer.TestOnly(result, records);
                AppendResult(Environment.NewLine + "HELD-OUT TEST METRICS (never used for tuning)");
                AppendResult(heldOutEvaluation.Summary());
                AppendResult("Warning: the uploaded data combines Philippine examples and many general English texts; results may not generalize to newer Philippine scams or Cebuano SMS.");
                lblProgress.Text = "Held-out test completed. Do not change settings based on test results.";
                if (heldOutEvaluation.Recall < 0.80 || heldOutEvaluation.F1 < 0.80)
                    AppendResult("RESEARCH QUALITY GATE: recall or F1 is below the suggested 0.80 academic target. Improve data quality and rerun a NEW independent evaluation before recommending real use.");
                else
                    AppendResult("RESEARCH QUALITY GATE: recall and F1 exceed 0.80 on this dataset; independent real-world validation is still required.");
                File.WriteAllText(Path.Combine(outputFolder, "held_out_test.txt"),
                    "Final test results for: " + result.Config + "\r\n" + heldOutEvaluation.Summary(), Encoding.UTF8);
            }
            catch (Exception ex)
            { MessageBox.Show(this, ex.Message, "Test error", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private void BtnClassify_Click(object sender, EventArgs e)
        {
            if (result == null) return;
            string sms = txtSms.Text.Trim();
            if (sms.Length < 5)
            { MessageBox.Show(this, "Please paste a meaningful SMS message."); return; }
            double score = SmsTrainer.ClassifyScore(result.Network, result.Vectorizer, sms);
            double coverage = result.Vectorizer.VocabularyCoverage(sms);
            bool positive = score >= result.Threshold;
            lblResult.Text = positive ? "Likely spam (model)" : "Not flagged as spam";
            lblScore.Text = string.Format(CultureInfo.InvariantCulture,
                "MLP score {0:F3}  |  Threshold {1:F2}\r\nKnown-word coverage {2:P0}", score, result.Threshold, coverage);
            lblNotes.Text = coverage < 0.10
                ? "LOW VOCABULARY COVERAGE: this message differs from training examples, so treat the classification as especially uncertain. Never rely on it to confirm safety."
                : "Educational classification only. A negative result does NOT establish a message is safe. Independently verify links, OTP requests, and payment instructions.";
            lblProgress.Text = "SMS classified. Neural network weights were not changed.";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (result == null) return;
            try
            {
                Directory.CreateDirectory(outputFolder);
                string summary = "Philippine SMS Spam Detection | C# MLP\r\n" +
                    "Model: " + result.Config + "\r\n" +
                    "Validation threshold: " + result.Threshold.ToString("F2", CultureInfo.InvariantCulture) + "\r\n" +
                    (result.Validation == null ? "" : "Validation:\r\n" + result.Validation.Summary() + "\r\n") +
                    (heldOutEvaluation == null ? "Held-out test not evaluated or not available.\r\n" :
                        "Held-out test:\r\n" + heldOutEvaluation.Summary() + "\r\n") +
                    "\r\nData is a mixed corpus: Philippine examples plus a larger English-language SMS collection.\r\n" +
                    "Some labels may be noisy. This model is an educational classifier, not a scam-safety guarantee.\r\n";
                File.WriteAllText(Path.Combine(outputFolder, "summary.txt"), summary, Encoding.UTF8);
                System.Diagnostics.Process.Start("explorer.exe", outputFolder);
                lblProgress.Text = "Opened exported results folder.";
            }
            catch (Exception ex)
            { MessageBox.Show(this, "Export failed:\n" + ex.Message); }
        }

    }
}
