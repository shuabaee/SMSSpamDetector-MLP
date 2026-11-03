using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Backprop;

namespace PhilippineSMS_MLP
{
    public sealed class EpochPoint
    {
        public int Epoch;
        public double TrainingMse;
        public double ValidationMse;
    }
    public sealed class Evaluation
    {
        public int TP, FP, TN, FN;
        public double Mse;
        public int Count { get { return TP + FP + TN + FN; } }
        public double Accuracy { get { return (TP + TN) / (double)Math.Max(1, Count); } }
        public double Precision { get { return TP / (double)Math.Max(1, TP + FP); } }
        public double Recall { get { return TP / (double)Math.Max(1, TP + FN); } }
        public double Specificity { get { return TN / (double)Math.Max(1, TN + FP); } }
        public double F1 { get { return Precision + Recall == 0 ? 0 : 2 * Precision * Recall / (Precision + Recall); } }
        public string Summary()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "N={0} | Accuracy={1:P2} Precision={2:P2} Recall={3:P2} F1={4:F3} Specificity={5:P2} MSE={6:F4}\r\n" +
                "Confusion matrix (actual rows / predicted columns):\r\n" +
                "                   Spam      Ham\r\n" +
                "Actual spam    TP={7,-6} FN={8,-6}\r\n" +
                "Actual ham     FP={9,-6} TN={10,-6}",
                Count, Accuracy, Precision, Recall, F1, Specificity, Mse, TP, FN, FP, TN);
        }
    }
    public sealed class SmsTrainingResult
    {
        public NeuralNet Network;
        public SmsVectorizer Vectorizer;
        public double Threshold;
        public int Hidden;
        public string Config;
        public Evaluation Validation;
        public List<EpochPoint> Loss = new List<EpochPoint>();
        public List<string> Experiments = new List<string>();
        public int TrainingCount, ValidationCount, TestCount;
    }
    public sealed class SmsTrainer
    {
        private struct TrainingExample
        {
            public double[] X;
            public int Y;
        }
        private sealed class Configuration
        {
            public int Hidden, Epochs;
            public double InputRate, OutputRate;
            public string Name
            {
                get { return string.Format(CultureInfo.InvariantCulture, "Hidden={0}, epochs={1}, LR(input)={2:0.00}, LR(output)={3:0.00}", Hidden, Epochs, InputRate, OutputRate); }
            }
        }
        private static List<TrainingExample> MakeExamples(IEnumerable<SmsRecord> records, SmsVectorizer v)
        {
            return records.Select(r => new TrainingExample { X = v.Transform(r.Message), Y = r.Label }).ToList();
        }
        private static double Predict(NeuralNet network, double[] x)
        {
            for (int i = 0; i < x.Length; i++) network.setInputs(i, x[i]);
            network.run();
            return network.getOuputData(0);
        }
        public static double ClassifyScore(NeuralNet network, SmsVectorizer vectorizer, string message)
        {
            return Predict(network, vectorizer.Transform(message));
        }
        private static Evaluation Evaluate(NeuralNet model, IList<TrainingExample> rows, double threshold)
        {
            var e = new Evaluation();
            foreach (var row in rows)
            {
                double score = Predict(model, row.X);
                double err = score - row.Y;
                e.Mse += err * err;
                bool positive = score >= threshold;
                if (positive && row.Y == 1) e.TP++;
                else if (positive) e.FP++;
                else if (row.Y == 1) e.FN++;
                else e.TN++;
            }
            e.Mse /= Math.Max(1, rows.Count);
            return e;
        }
        private static Evaluation BestValidationThreshold(NeuralNet model, IList<TrainingExample> rows, out double chosen)
        {
            chosen = 0.5;
            Evaluation best = null;
            for (int t = 25; t <= 80; t += 5)
            {
                double threshold = t / 100.0;
                var eval = Evaluate(model, rows, threshold);
                if (best == null || eval.F1 > best.F1 + 1e-10 ||
                    (Math.Abs(eval.F1 - best.F1) <= 1e-10 && eval.Recall > best.Recall + 1e-10))
                { best = eval; chosen = threshold; }
            }
            return best;
        }
        public SmsTrainingResult TrainAndCompare(List<SmsRecord> records, IProgress<string> progress,
            string resultsFolder)
        {
            List<SmsRecord> training, validation, testing;
            // Split and group messages before fitting features: no test/validation leakage.
            SmsDataset.StratifiedGroupSplit(records, out training, out validation, out testing);
            var vectorizer = new SmsVectorizer();
            vectorizer.Fit(training);
            var train = MakeExamples(training, vectorizer);
            var val = MakeExamples(validation, vectorizer);
            var configurations = new[]
            {
                new Configuration { Hidden=8, Epochs=70, InputRate=0.08, OutputRate=0.12 },
                new Configuration { Hidden=12, Epochs=110, InputRate=0.12, OutputRate=0.18 },
                new Configuration { Hidden=16, Epochs=140, InputRate=0.12, OutputRate=0.18 },
                new Configuration { Hidden=12, Epochs=150, InputRate=0.05, OutputRate=0.10 },
                new Configuration { Hidden=16, Epochs=180, InputRate=0.15, OutputRate=0.20 }
            };
            SmsTrainingResult best = null;
            var allExperimentLines = new List<string>();
            allExperimentLines.Add("config,validation_threshold,validation_accuracy,validation_precision,validation_recall,validation_f1,validation_mse");
            Directory.CreateDirectory(resultsFolder);
            for (int c = 0; c < configurations.Length; c++)
            {
                Configuration cfg = configurations[c];
                if (progress != null) progress.Report(string.Format("Experiment {0}/{1}: {2}", c + 1, configurations.Length, cfg.Name));
                var net = new NeuralNet(vectorizer.InputSize, cfg.Hidden, 1);
                net.ResetWeights(2600 + c);
                net.SetLearningRates(cfg.InputRate, cfg.OutputRate);
                var log = new List<EpochPoint>();
                NeuralNet checkpoint = null;
                double lowestValidationMse = double.PositiveInfinity;
                int[] order = Enumerable.Range(0, train.Count).ToArray();
                var rng = new Random(9001 + c);
                for (int epoch = 1; epoch <= cfg.Epochs; epoch++)
                {
                    SmsDataset.Shuffle(order, rng);
                    foreach (int i in order)
                    {
                        TrainingExample sample = train[i];
                        for (int f = 0; f < sample.X.Length; f++) net.setInputs(f, sample.X[f]);
                        net.setDesiredOutput(0, sample.Y);
                        net.learn();
                    }
                    if (epoch == 1 || epoch % 10 == 0 || epoch == cfg.Epochs)
                    {
                        var trainEval = Evaluate(net, train, 0.5);
                        var validationEval = Evaluate(net, val, 0.5);
                        log.Add(new EpochPoint { Epoch=epoch, TrainingMse=trainEval.Mse, ValidationMse=validationEval.Mse });
                        if (validationEval.Mse < lowestValidationMse)
                        {
                            lowestValidationMse = validationEval.Mse;
                            checkpoint = net.CloneTrainedNetwork();
                        }
                        if (progress != null && epoch % 30 == 0)
                            progress.Report(string.Format(CultureInfo.InvariantCulture,
                                "Experiment {0}/{1}, epoch {2}/{3}, training MSE {4:F4}, validation MSE {5:F4}",
                                c + 1, configurations.Length, epoch, cfg.Epochs, trainEval.Mse, validationEval.Mse));
                    }
                }
                // Only the checkpoint with best validation MSE is scored for this experiment.
                net = checkpoint ?? net;
                double threshold;
                Evaluation metrics = BestValidationThreshold(net, val, out threshold);
                allExperimentLines.Add(string.Format(CultureInfo.InvariantCulture,
                    "\"{0}\",{1:F2},{2:F4},{3:F4},{4:F4},{5:F4},{6:F6}",
                    cfg.Name, threshold, metrics.Accuracy, metrics.Precision, metrics.Recall, metrics.F1, metrics.Mse));
                var csv = new List<string> { "epoch,training_mse,validation_mse" };
                csv.AddRange(log.Select(p => string.Format(CultureInfo.InvariantCulture, "{0},{1:F6},{2:F6}", p.Epoch, p.TrainingMse, p.ValidationMse)));
                File.WriteAllLines(Path.Combine(resultsFolder, "loss_experiment_" + (c + 1) + ".csv"), csv, System.Text.Encoding.UTF8);
                if (progress != null) progress.Report(string.Format(CultureInfo.InvariantCulture,
                    "Experiment {0}/{1}: validation F1={2:F3}, recall={3:P1}, precision={4:P1}, threshold={5:F2}",
                    c + 1, configurations.Length, metrics.F1, metrics.Recall, metrics.Precision, threshold));

                // Model selection based ONLY on validation F1. Test examples remain untouched.
                if (best == null || metrics.F1 > best.Validation.F1 + 0.00001 ||
                    (Math.Abs(metrics.F1 - best.Validation.F1) < 0.00001 && metrics.Recall > best.Validation.Recall))
                {
                    best = new SmsTrainingResult
                    {
                        Network = net, Vectorizer = vectorizer, Hidden=cfg.Hidden, Threshold=threshold,
                        Validation=metrics, Loss=log, Config=cfg.Name,
                        TrainingCount=train.Count, ValidationCount=val.Count, TestCount=testing.Count
                    };
                }
            }
            best.Experiments = allExperimentLines;
            File.WriteAllLines(Path.Combine(resultsFolder, "validation_experiments.csv"), allExperimentLines, System.Text.Encoding.UTF8);
            File.WriteAllLines(Path.Combine(resultsFolder, "dataset_split_summary.txt"), new[]
            {
                "Original records after duplicate and conflict removal: " + records.Count,
                "Training: " + train.Count + ", spam=" + train.Count(x=>x.Y==1) + ", ham=" + train.Count(x=>x.Y==0),
                "Validation: " + val.Count + ", spam=" + val.Count(x=>x.Y==1) + ", ham=" + val.Count(x=>x.Y==0),
                "Held-out test: " + testing.Count + ", spam=" + testing.Count(x=>x.Label==1) + ", ham=" + testing.Count(x=>x.Label==0),
                "Random seed: 42 for grouping/split. Vocabulary fitted using training only.",
                "Model and threshold chosen on validation only; test is not used to tune."
            });
            return best;
        }
        public Evaluation TestOnly(SmsTrainingResult result, List<SmsRecord> records)
        {
            List<SmsRecord> train, val, test;
            SmsDataset.StratifiedGroupSplit(records, out train, out val, out test);
            return Evaluate(result.Network, MakeExamples(test, result.Vectorizer), result.Threshold);
        }
    }
}
