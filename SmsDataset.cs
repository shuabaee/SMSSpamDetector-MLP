using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PhilippineSMS_MLP
{
    public sealed class SmsRecord
    {
        public string Message;
        public int Label; // 1 = spam; 0 = ham
        public string GroupKey;
    }

    public static class SmsDataset
    {
        // Correctly handles UTF-8 BOM, quoted fields, escaped quotes, commas and embedded line breaks.
        public static List<string[]> ReadCsv(string path)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);
            var rows = new List<string[]>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (ch == '\uFEFF' && i == 0) continue;
                if (ch == '"')
                {
                    if (quoted && i + 1 < text.Length && text[i + 1] == '"')
                    { field.Append('"'); i++; }
                    else quoted = !quoted;
                }
                else if (!quoted && ch == ',')
                { row.Add(field.ToString()); field.Clear(); }
                else if (!quoted && (ch == '\n' || ch == '\r'))
                {
                    if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    row.Add(field.ToString()); field.Clear();
                    if (row.Any(x => !string.IsNullOrWhiteSpace(x))) rows.Add(row.ToArray());
                    row.Clear();
                }
                else field.Append(ch);
            }
            if (quoted) throw new InvalidDataException("CSV ends inside a quoted field.");
            row.Add(field.ToString());
            if (row.Any(x => !string.IsNullOrWhiteSpace(x))) rows.Add(row.ToArray());
            return rows;
        }

        public static List<SmsRecord> Load(string path, out string audit, int minPerClass = 15)
        {
            var rows = ReadCsv(path);
            if (rows.Count < 3) throw new InvalidDataException("CSV requires a header and labeled messages.");
            string[] head = rows[0].Select(x => x.Trim().ToLowerInvariant()).ToArray();
            int labelIdx = Array.FindIndex(head, x => x == "label" || x == "class" || x == "category");
            int messageIdx = Array.FindIndex(head, x => x == "message" || x == "text" || x == "sms" || x == "v2");
            if (labelIdx < 0 || messageIdx < 0)
                throw new InvalidDataException("Expected CSV headers: label,message (or class,text). Unlabeled data cannot be used for supervised training.");

            var records = new Dictionary<string, SmsRecord>(StringComparer.Ordinal);
            var conflicts = new HashSet<string>(StringComparer.Ordinal);
            // The user-supplied CSV has three unnamed columns after 'message'. Some
            // malformed legacy rows spill message fragments across them. Recover
            // those fragments instead of discarding parts of the original SMS.
            bool unnamedTailColumns = head.Skip(messageIdx + 1).All(string.IsNullOrWhiteSpace);
            int invalid = 0, duplicates = 0;
            for (int i = 1; i < rows.Count; i++)
            {
                string[] cells = rows[i];
                if (cells.Length <= Math.Max(labelIdx, messageIdx)) { invalid++; continue; }
                string label = cells[labelIdx].Trim().ToLowerInvariant();
                int y;
                if (label == "spam" || label == "scam" || label == "smishing" || label == "1") y = 1;
                else if (label == "ham" || label == "legitimate" || label == "not spam" || label == "0") y = 0;
                else { invalid++; continue; }
                string msg = cells[messageIdx].Trim();
                if (unnamedTailColumns && cells.Length > messageIdx + 1)
                {
                    int last = Array.FindLastIndex(cells, c => !string.IsNullOrWhiteSpace(c));
                    if (last > messageIdx)
                        msg = string.Join(",", cells.Skip(messageIdx).Take(last - messageIdx + 1)).Trim();
                }
                if (msg.Length < 5) { invalid++; continue; }
                string key = Regex.Replace(msg.ToLowerInvariant(), @"\s+", " ").Trim();
                if (records.ContainsKey(key))
                {
                    duplicates++;
                    if (records[key].Label != y) conflicts.Add(key);
                    continue;
                }
                records[key] = new SmsRecord { Message = msg, Label = y, GroupKey = MakeGroupKey(msg) };
            }
            foreach (string key in conflicts) records.Remove(key); // Don't guess a ground-truth label.
            var result = records.Values.ToList();
            int spam = result.Count(x => x.Label == 1), ham = result.Count - spam;
            audit = "Loaded " + result.Count + " unique labeled messages (spam=" + spam + ", ham=" + ham +
                    "). Removed " + duplicates + " repeated rows, " + conflicts.Count +
                    " conflicting-label messages and " + invalid + " invalid/unknown rows.";
            if (spam < minPerClass || ham < minPerClass) throw new InvalidDataException("Not enough labeled spam AND ham messages for this evaluation.");
            return result;
        }

        public static string MakeGroupKey(string text)
        {
            // Use the same obfuscation normalization as the vectorizer for grouping.
            string s = SmsVectorizer.ReplaceLinksAndNumbers(text);
            s = Regex.Replace(s, @"urltoken", " URL ");
            s = Regex.Replace(s, @"\d+", " NUM ");
            s = Regex.Replace(s, @"[^\p{L}\s]", " ");
            return Regex.Replace(s, @"\s+", " ").Trim();
        }

        public static void StratifiedGroupSplit(List<SmsRecord> data, out List<SmsRecord> train,
            out List<SmsRecord> validation, out List<SmsRecord> test)
        {
            train = new List<SmsRecord>(); validation = new List<SmsRecord>(); test = new List<SmsRecord>();
            // Group ALL messages (across both labels) by normalized template first,
            // so variants of the same SMS cannot appear in different partitions.
            var groups = data.GroupBy(x => x.GroupKey).Select(g => g.ToList()).ToList();
            var random = new Random(42);
            for (int j = groups.Count - 1; j > 0; j--)
            {
                int k = random.Next(j + 1);
                var tmp = groups[j]; groups[j] = groups[k]; groups[k] = tmp;
            }
            int[] totals = { data.Count(x => x.Label == 0), data.Count(x => x.Label == 1) };
            int[] trainTarget = totals.Select(x => (int)Math.Round(x * 0.65)).ToArray();
            int[] valTarget = totals.Select(x => (int)Math.Round(x * 0.15)).ToArray();
            int[] trainCount = new int[2], valCount = new int[2];
            foreach (var group in groups)
            {
                int major = group.Count(x => x.Label == 1) > group.Count / 2 ? 1 : 0;
                int[] groupCounts = { group.Count(x => x.Label == 0), group.Count(x => x.Label == 1) };
                if (trainCount[major] < trainTarget[major])
                {
                    train.AddRange(group);
                    trainCount[0] += groupCounts[0]; trainCount[1] += groupCounts[1];
                }
                else if (valCount[major] < valTarget[major])
                {
                    validation.AddRange(group);
                    valCount[0] += groupCounts[0]; valCount[1] += groupCounts[1];
                }
                else test.AddRange(group);
            }
            Shuffle(train, new Random(15)); Shuffle(validation, new Random(16)); Shuffle(test, new Random(17));
            if (!train.Any(x => x.Label == 1) || !train.Any(x => x.Label == 0)
             || !validation.Any(x => x.Label == 1) || !validation.Any(x => x.Label == 0)
             || !test.Any(x => x.Label == 1) || !test.Any(x => x.Label == 0))
                throw new InvalidDataException("Unable to make a stratified split with both classes. Check the dataset.");
        }

        public static void Shuffle<T>(IList<T> items, Random rng)
        {
            for (int i = items.Count - 1; i > 0; i--)
            { int j = rng.Next(i + 1); T tmp = items[i]; items[i] = items[j]; items[j] = tmp; }
        }
    }
}
