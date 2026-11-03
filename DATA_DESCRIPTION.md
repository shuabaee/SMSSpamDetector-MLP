# Data Description

## Source and scope

The dataset is **the CSV supplied for this assignment** (`Data/filipino_sms.csv`). It contains **6,203 data rows** with a `label` field and an SMS `message` field. It also has three extra unnamed columns. In a small number of lines, these contain parts of messages split across columns by legacy CSV quoting. The C# loader recombines such fragments for this particular input format rather than silently dropping message text. The program does not need a separate supplemental CSV.

Its contents are **mixed**, not exclusively Philippine SMS. The first **631 messages** include Philippine-focused local SMS (including previously supplied obfuscated scam examples); the remaining **5,572 messages** are mostly general English-language SMS resembling a commonly used international SMS spam corpus. The CSV itself does not contain authoritative per-row provenance, dates, or independent labeling audits. We therefore cannot claim that model metrics represent performance across the Philippine SMS population.

## Labels and size

| Portion of supplied CSV | Rows | Spam | Ham |
|---|---:|---:|---:|
| Philippine-focused early portion | 631 | 318 | 313 |
| General English-language portion | 5,572 | 747 | 4,825 |
| **Total raw rows** | **6,203** | **1,065** | **5,138** |

The program trims whitespace in class labels, rejects invalid/very short messages, collapses identical texts, and removes texts that have conflicting labels. With these rules, a preliminary audit of this CSV gives **5,773 usable unique records (956 spam / 4,817 ham)**. The app recombines malformed extra columns before checking message text, but the count may vary if the loader rules are changed.

**Imbalance:** Approximately 16.6% of usable unique texts are labeled spam. Predicting every message as ham would achieve roughly 83.4% raw accuracy on these deduplicated records while detecting no spam. Report precision, recall, F1 and the confusion matrix, not just accuracy.

## Features and learning

- Raw text is lowercased; URLs and number patterns are normalized, and common digit-to-letter spam obfuscations are handled.
- 128 binary vocabulary features are selected **only from training messages**.
- 40 hashed character-trigram features recognize patterns even when the exact word is not in the vocabulary.
- 8 structure features summarize URLs, money, message length, number of words, digit/uppercase ratios, exclamation marks and OTP-related terms.
- All **176 model inputs** are in `[0,1]`.
- Binary target: `spam=1` and `ham=0`.
- Split by approximate ratios 65/15/20 (train/validation/held-out test), with similar templates assigned to the same partition.
- Training loss uses mean squared error; performance is assessed using classification accuracy, precision, recall, F1, specificity and the confusion matrix.

## Label quality, bias and limitations

Some labels may be inconsistent (e.g., verification-code or legitimate promotional texts labeled spam). Messages are mixed across geographic contexts and may not reflect recent Philippine SMS scams or Cebuano usage. Accuracy on this combined corpus does not by itself validate use in Cebu or with newer scam types. For meaningful regional claims, collect a newly labeled Philippine-only test set, remove personal identifiers and keep that test set completely outside training/tuning.

**References for source context** (the uploaded CSV may combine data from multiple origins):
- https://github.com/Yissuh/Filipino-Spam-SMS-Detection-Model
- https://www.kaggle.com/datasets/bwandowando/philippine-spam-sms-messages
- https://github.com/AGR-Yes/ScamMessagesPhilippines
- https://github.com/mohitgupta-1O1/Kaggle-SMS-Spam-Collection-Dataset-/blob/master/spam.csv

These links document related Philippine/American datasets; they do not establish independent provenance for every row of the combined uploaded file.
