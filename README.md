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

TODO!

## Releasing

If you know the target target you are building yout project for use:

```ps
dotnet build .\YourProject.csproj -c Release -r [target]
```

This way you avoid including all native libraries.
