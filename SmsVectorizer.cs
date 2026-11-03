using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PhilippineSMS_MLP
{
    // Convert raw SMS to fixed-size [0,1] features. Vocabulary learned on TRAIN ONLY.
    // Hybrid: binary word presence + hashed character trigrams + numerical message features.
    public sealed class SmsVectorizer
    {
        public const int WordFeatures = 128;
        public const int CharFeatures = 40;
        public const int ExtraFeatures = 8;
        public int InputSize { get { return WordFeatures + CharFeatures + ExtraFeatures; } }
        private readonly List<string> vocabulary = new List<string>();
        private readonly Dictionary<string, int> lookup = new Dictionary<string, int>(StringComparer.Ordinal);
        public IList<string> Vocabulary { get { return vocabulary.AsReadOnly(); } }

        public static string ReplaceLinksAndNumbers(string text)
        {
            string s = text.ToLowerInvariant();
            s = Regex.Replace(s, @"(?:https?://|www\.)\S+|[\w.-]+\.(?:com|ph|net|org|io|xyz|app|top)\b\S*", " urltoken ");
            s = Regex.Replace(s, @"\b\d{2,}\b", " numtoken ");
            // Decode common letter/digit substitutions only in mixed tokens with
            // at least three letters. Keep 30k, 9k, OTP numbers and prices intact.
            s = Regex.Replace(s, @"[\p{L}\p{N}]+", match =>
            {
                string token = match.Value;
                if (token.Count(char.IsLetter) < 3 || !token.Any(char.IsDigit)) return token;
                return token.Replace('0', 'o').Replace('1', 'i').Replace('3', 'e')
                            .Replace('4', 'a').Replace('5', 's').Replace('7', 't');
            });
            return s;
        }
        public static List<string> Words(string text)
        {
            return Regex.Matches(ReplaceLinksAndNumbers(text), @"[\p{L}]{2,}")
                .Cast<Match>().Select(x => x.Value).ToList();
        }
        public void Fit(IEnumerable<SmsRecord> training)
        {
            vocabulary.Clear(); lookup.Clear();
            var frequencies = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (SmsRecord r in training)
                foreach (string word in Words(r.Message).Distinct(StringComparer.Ordinal))
                {
                    int count;
                    frequencies.TryGetValue(word, out count);
                    frequencies[word] = count + 1;
                }
            foreach (var entry in frequencies.Where(x => x.Value >= 2)
                         .OrderByDescending(x => x.Value).ThenBy(x => x.Key).Take(WordFeatures))
                AddWord(entry.Key);
        }
        private void AddWord(string word)
        {
            if (!lookup.ContainsKey(word) && vocabulary.Count < WordFeatures)
            { lookup[word] = vocabulary.Count; vocabulary.Add(word); }
        }
        public void SaveVocabulary(TextWriter writer)
        {
            writer.WriteLine("SMS_VECTOR_V1");
            writer.WriteLine(vocabulary.Count);
            foreach (string w in vocabulary) writer.WriteLine(w);
        }
        public static SmsVectorizer LoadVocabulary(TextReader reader)
        {
            if (reader.ReadLine() != "SMS_VECTOR_V1") throw new InvalidDataException("Unknown vectorizer version.");
            int n = int.Parse(reader.ReadLine());
            if (n < 1 || n > WordFeatures) throw new InvalidDataException("Invalid vocabulary size.");
            var v = new SmsVectorizer();
            for (int i = 0; i < n; i++)
            {
                string w = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(w)) throw new InvalidDataException("Missing vocabulary item.");
                v.AddWord(w);
            }
            return v;
        }
        // Stable FNV-1a hash, not .NET string.GetHashCode (which changes between processes).
        private static uint StableHash(string s)
        {
            uint hash = 2166136261u;
            foreach (char ch in s) { hash ^= ch; hash *= 16777619u; }
            return hash;
        }
        public double VocabularyCoverage(string text)
        {
            var words = Words(text).Distinct().ToList();
            if (words.Count == 0) return 0;
            return words.Count(x => lookup.ContainsKey(x)) / (double)words.Count;
        }
        public double[] Transform(string text)
        {
            double[] x = new double[InputSize];
            var words = Words(text);
            foreach (string word in words)
            {
                int index;
                if (lookup.TryGetValue(word, out index)) x[index] = 1.0;
            }
            string characters = Regex.Replace(ReplaceLinksAndNumbers(text), @"\s+", " ").Trim();
            int charCount = 0;
            for (int i = 0; i + 3 <= characters.Length; i++)
            {
                string gram = characters.Substring(i, 3);
                int bucket = (int)(StableHash(gram) % CharFeatures);
                x[WordFeatures + bucket] += 1.0;
                charCount++;
            }
            if (charCount > 0)
            {
                for (int i = 0; i < CharFeatures; i++)
                    x[WordFeatures + i] = Math.Min(1.0, x[WordFeatures + i] / Math.Max(1.0, charCount * 0.2));
            }
            int offset = WordFeatures + CharFeatures;
            x[offset] = Regex.IsMatch(text, @"(?:https?://|www\.)\S+|[\w.-]+\.(?:com|ph|net|org|io|xyz|app|top)\b", RegexOptions.IgnoreCase) ? 1 : 0;
            x[offset + 1] = Regex.IsMatch(text, @"(?:₱|php\s*|p\s*\d|\d+\s*pesos?)", RegexOptions.IgnoreCase) ? 1 : 0;
            x[offset + 2] = Math.Min(1, text.Length / 300.0);
            x[offset + 3] = Math.Min(1, words.Count / 55.0);
            x[offset + 4] = Math.Min(1, text.Count(char.IsDigit) / Math.Max(1.0, text.Length) * 4);
            x[offset + 5] = Math.Min(1, text.Count(char.IsUpper) / Math.Max(1.0, text.Count(char.IsLetter)) * 2);
            x[offset + 6] = text.Contains("!") ? 1 : 0;
            x[offset + 7] = Regex.IsMatch(text, @"\b(?:otp|verification|one.time.password|code)\b", RegexOptions.IgnoreCase) ? 1 : 0;
            return x;
        }
    }
}
