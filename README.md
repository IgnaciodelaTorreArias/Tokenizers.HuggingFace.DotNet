# Tokenizers.HuggingFace

.NET bindings for [huggingface/tokenizers](https://github.com/huggingface/tokenizers) using protobufs for comunication and C-ABI.

## How to install

```ps
dotnet add package Tokenizers.HuggingFace
```

## Supported targets

- win-x64
- linux-x64
- osx-x64
- osx-arm64
- win-arm64
- linux-arm64

## Usage

Casses:

- Normalization
- PreTokenization
- Tokenizer (Encode, Decode, Load From File, Train)

### Examples

#### Sentence Similarity with [sentence-transformers/all-MiniLM-L6-v2](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2)

Steps:

- Create Console app

```ps
dotnet new console --name Sentences
```

- Download [onnx/model.onnx](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/blob/main/onnx/model.onnx).
- Download [tokenizer.json](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/blob/main/tokenizer.json).
optional: Remove the padding and truncation.
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
        var encodings = tk.Encode(text, true, include_type_ids: true, include_attention_mask: true).Encodings[0];
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

If you know the target target you are building yout project for use:

```ps
dotnet build .\YourProject.csproj -c Release -r [target]
```

This way you avoid including all native libraries.
