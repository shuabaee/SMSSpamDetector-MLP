# PhilippineSMS-MLP

**Philippine SMS Spam Detection Using a Custom Multi-Layer Perceptron in C#**  
Authors: **Gabison, Joshua V. and Sabagkit, John Arcel R.**  
Class: **BSCS-3 F2 CS345**  
Track: **Binary Classification (supervised learning)**

## Model explanation
- Binary target: `spam=1`, `ham=0`; one sigmoid output.
- Inputs: 128 word-presence features, 40 hashed character-trigram features and 8 structural features, each within `[0,1]` (176 total).
- Hidden layer: 8, 12 or 16 neurons. Five trials vary the hidden layer size, epoch count and the learning rates of the two weight layers.
- Learning: original C# `Backprop.NeuralNet` forward/backpropagation implementation from the previous activity. `BackNeural.cs` contains documented small extensions to control learning rates, reproducibly initialize weights, and copy the validation-best checkpoint. No external ML engine.
- Approximate split: 65% training, 15% validation, 20% held-out testing; related message templates are grouped before splitting. The word vocabulary is learned only from the training split.
- Model and threshold selected on **validation F1**. The held-out test is not used for model selection. Because data are imbalanced, assess spam **recall and precision**, not accuracy alone.
- The Windows Forms interface intentionally has **no on-screen graph, Save Model, Load Model, or External Test controls**. Training-loss CSVs still provide the required logs, and the confusion matrix visualizes classification performance in text.
- The project uses `filipino_sms.csv` as its SMS dataset. The CSV file is located inside the `Data` directory of the project.

```text
SMSSpamDetector-MLP/
└── Data/
    └── filipino_sms.csv
```

## Important limitations
- The supplied CSV **is not exclusively Philippine SMS**. It combines a Philippine-focused portion and a substantially larger general English-language SMS collection. See [DATA_DESCRIPTION.md](DATA_DESCRIPTION.md).
- Labels are not comprehensively audited and some legitimate-looking messages may be mislabeled. The data may include repeated or similar text and inconsistent quoting; the loader skips invalid labels and groups similar templates to limit leakage.
- This is an **educational model**, not a guarantee that an SMS is safe. In particular, a negative classification is not security clearance for links, payments or OTPs. Real Philippine deployment would require a new, representative, independently labeled external test set.
- No tested accuracy is claimed in this repository before running the actual C# training and held-out evaluation.
