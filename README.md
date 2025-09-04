# Tokenizers.HuggingFace

.NET bindings for [huggingface/tokenizers](https://github.com/huggingface/tokenizers) using protobufs for communication and C-ABI.

## How to install

```ps
dotnet add package Tokenizers.HuggingFace
```

## Supported targets

- linux-musl-arm64
- linux-musl-x64
- linux-arm64
- linux-x64
- osx-arm64
- osx-x64
- win-x64
- win-arm64

## Usage

Cases:

- Normalization
- PreTokenization
- Tokenizer (Encode, Decode, Load From File, Train)

### Examples

#### Basic Tokenization from file

```csharp
using Tokenizers.HuggingFace.Tokenizer;

var tk = Tokenizer.FromFile("./tokenizer.json");
var encodings = tk.Encode("Hello, World!", true).First();
Console.WriteLine($"{string.Join(",", encodings.Ids)}");
// Optionally dispose the tokenizer if no longer needed
// If not disposed, it will be cleaned up by the finalizer
tk.Dispose();
```

#### Test Pipeline with normalization and pretokenization

```csharp
var lowerCase = new Tokenizers.HuggingFace.Normalizers.Lowercase();
Tokenizers.HuggingFace.Normalizers.Sequence normalizer = new([
    new Tokenizers.HuggingFace.Normalizers.Nfd(),
    lowerCase,
    new Tokenizers.HuggingFace.Normalizers.StripAccents()
]);
// Optionally dispose the normalizer if no longer needed
// If not disposed, it will be cleaned up by the finalizer
// Disposing this won't affect the sequence we created
lowerCase.Dispose();
Tokenizers.HuggingFace.PreTokenizers.Bert bert = new();
var testString = new Tokenizers.HuggingFace.PipelineString.PipelineString("H�llo,  W�rld!");
normalizer.Normalize(testString);
bert.PreTokenize(testString);
var splits = testString.GetSplits(
    Tokenizers.HuggingFace.PipelineString.OffsetReferential.Original,
    Tokenizers.HuggingFace.PipelineString.OffsetType.Char,
    includeOffsets: true
);
Console.WriteLine($"Tokens: [{string.Join(",", splits.Select(split=> $"'{split.Item1}'"))}]");
Console.WriteLine($"Offsets: [{string.Join(",", splits.Select(split => split.Item2))}]");
bert.Dispose();
normalizer.Dispose();
testString.Dispose();
```

#### Train a [all-together-a-bert-tokenizer-from-scratch](https://huggingface.co/docs/tokenizers/pipeline#all-together-a-bert-tokenizer-from-scratch)

```csharp
var normalizer = new Tokenizers.HuggingFace.Normalizers.Sequence([
    new Tokenizers.HuggingFace.Normalizers.Nfd(),
    new Tokenizers.HuggingFace.Normalizers.Lowercase(),
    new Tokenizers.HuggingFace.Normalizers.StripAccents(),
]);
var preTokenizer = new Tokenizers.HuggingFace.PreTokenizers.Whitespace();
Tokenizers.HuggingFace.Processors.Token[] tokensProcessor = [
    new() { TokenPair = new() { Token = "[CLS]", TokenId = 1 } },
    new() { TokenPair = new() { Token = "[SEP]", TokenId = 2 } },
];
var processor = new Tokenizers.HuggingFace.Processors.TemplateProcessing()
{
    Single = "[CLS] $A [SEP]",
    Pair = "[CLS] $A [SEP] $B:1 [SEP]:1",
};
processor.Tokens = new();
processor.Tokens.Tokens_.AddRange(tokensProcessor);
Tokenizers.HuggingFace.Trainers.AddedToken[] tokensTrainer = [
    new() { Content = "[UNK]", Special = true },
    new() { Content = "[CLS]", Special = true },
    new() { Content = "[SEP]", Special = true },
    new() { Content = "[PAD]", Special = true },
    new() { Content = "[MASK]", Special = true },
];
var trainer = new Tokenizers.HuggingFace.Trainers.WordPieceTrainer() { VocabSize = 30522 };
trainer.SpecialTokens.AddRange(tokensTrainer);
var tk = Tokenizers.HuggingFace.Tokenizer.Tokenizer.FromTrain(
    files: ["corpus.txt"],
    savePath: "my_tokenizer.json",
    model: new() { WordPiece = new() }, // by default uses [UNK]
    trainer: new() { WordPiece = trainer },
    // From here on are optional parameters
    //normalizer: normalizer,
    //preTokenizer: preTokenizer,
    processors: [new() { TemplateProcessing = processor }]
);
```

#### Sentence Similarity with [sentence-transformers/all-MiniLM-L6-v2](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2)

Steps:

- Create Console app

```ps
dotnet new console --name Sentences
```

- Download [onnx/model.onnx](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/blob/main/onnx/model.onnx).
- Download [tokenizer.json](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/blob/main/tokenizer.json).
- Install onnxruntime and Tokenizers.HuggingFace

```ps
dotnet add package Microsoft.ML.OnnxRuntime
dotnet add package Tokenizers.HuggingFace
```

- Add the following code to Program.cs

```csharp
using System.Numerics.Tensors;

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

using Tokenizers.HuggingFace.Tokenizer;


var a = SentenceSimilarityModel.GetEmbeddings("Hello, world");
var b = SentenceSimilarityModel.GetEmbeddings("Hello, world, good to be here");

Console.WriteLine($"E: {string.Join(',', a)}");
Console.WriteLine($"a-b: {TensorPrimitives.CosineSimilarity(a, b)}");

static class SentenceSimilarityModel
{
    static readonly Tokenizer tk = Tokenizer.FromFile("./tokenizer.json");
    static readonly InferenceSession session = new InferenceSession("./model.onnx");
    static (int, NamedOnnxValue[]) PrepareInputs(string text)
    {
        var encodings = tk.Encode(text, true, includeTypeIds: true, includeAttentionMask: true).First();
        var sequenceLenght = encodings.Ids.Count;
        var input_ids = new DenseTensor<long>(encodings.Ids.Select(t => (long)t).ToArray(), [1, sequenceLenght]);
        var type_ids = new DenseTensor<long>(encodings.TypeIds.Select(t => (long)t).ToArray(), [1, sequenceLenght]);
        var attention_mask = new DenseTensor<long>(encodings.AttentionMask.Select(t => (long)t).ToArray(), [1, sequenceLenght]);

        return (sequenceLenght, [
            NamedOnnxValue.CreateFromTensor("input_ids", input_ids),
            NamedOnnxValue.CreateFromTensor("token_type_ids", type_ids),
            NamedOnnxValue.CreateFromTensor("attention_mask", attention_mask)
        ]);
    }
    static public float[] GetEmbeddings(string text)
    {
        var (sequenceLenght, inputs) = PrepareInputs(text);
        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
        var outputTensor = results.First().AsEnumerable<float>().ToArray();
        float[] result = new float[384];
        for (int i = 0; i < sequenceLenght; i++)
        {
            ReadOnlySpan<float> floats = new ReadOnlySpan<float>(outputTensor, i*384, 384);
            TensorPrimitives.Add(floats, result, result);
        }
        TensorPrimitives.Divide(result, sequenceLenght, result);
        return result;
    }
}

```

## Releasing

If you know the target target you are building your project for use:

```ps
dotnet build .\YourProject.csproj -c Release -r [target]
```

This way you avoid including all native libraries.
