using System.Runtime.InteropServices;

using Tokenizers.HuggingFace.Internal;
using Normalizer = Tokenizers.HuggingFace.Normalizers.Normalizer;
using PreTokenizer = Tokenizers.HuggingFace.PreTokenizers.PreTokenizer;

namespace Tokenizers.HuggingFace.Tokenizer;

internal static class TokenizerForeignFunctions
{
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 encode(nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 decode(nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 tokenizer_from_file(out nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 tokenizer_from_train(out nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_tokenizer(nuint instancePtr);
}
/// <summary>
/// A class to manage a Tokenizer
/// </summary>
public class Tokenizer : ForeignInstance
{
    internal Tokenizer(nuint addr)
    {
        this.InstancePtr = addr;
    }
    /// <summary>
    /// Loads a tokenizer from a JSON file
    /// </summary>
    /// <param name="path">A path to a local JSON file representing a previously serialized Tokenizer</param>
    /// <returns>The tokenizer</returns>
    /// <exception cref="Errors.TokenizerLoadFileException"></exception>
    public static Tokenizer FromFile(string path)
    {
        return new(ForeignFunctions.CreateNewArgsResult(
            TokenizerForeignFunctions.tokenizer_from_file,
            new Internal.Tokenizer.TokenizerFromFile { File = path }
        ));
    }
    /// <summary>
    /// Trains a Tokenizer and saves it as a JSON file
    /// </summary>
    /// <param name="files">A collection of paths to local files to use as training data</param>
    /// <param name="savePath">The path in which the JSON file should be saved</param>
    /// <param name="model">Trained algorithm that defines how text is split into tokens</param>
    /// <param name="trainer">A trainer has the responsibility to train a model. We feed it with lines/sentences and then it can train the given Model</param>
    /// <param name="pretty">Whether the JSON file should be saved with a pretty more human readable format</param>
    /// <param name="normalizer">Takes care of pre-processing strings.</param>
    /// <param name="preTokenizer">The pre tokenizer is in charge of doing the pre-segmentation step. It splits the given string in multiple substrings, keeping track of the offsets of said substrings</param>
    /// <param name="processors">A post processor has the responsibility to post process an encoded output of the Tokenizer. It adds any special tokens that a language model would require</param>
    /// <param name="decoders">A decoder changes the raw tokens into its more readable form</param>
    /// <param name="truncation">Truncation shortens input sequences that exceed a specified max length. This is crucial because most models have a fixed maximum input size.</param>
    /// <param name="padding">If <paramref name="truncation"/> takes care of cases when an input sequence exceeds a max length, padding takes care when the input sequence is shorter</param>
    /// <returns>The trained tokenizer</returns>
    /// <exception cref="ArgumentException"></exception>"
    /// <exception cref="Errors.TokenizerBuildException"></exception>
    /// <exception cref="Errors.TokenizerTrainingException"></exception>
    /// <exception cref="Errors.TokenizerSaveException"></exception>
    /// <remarks>
    /// It's possible for this method to throw other types of exceptions like (<see cref="Errors.InvalidBufferException"/>, <see cref="Errors.InvalidPointerException"/>, etc)<br/>
    /// This indicates an issue with the library, please open an issue at <see href="https://github.com/IgnaciodelaTorreArias/Tokenizers.HuggingFace.DotNet">GitHub</see>
    /// </remarks>
    public static Tokenizer FromTrain(IEnumerable<string> files, string savePath, Models.ModelWrapper model, Trainers.TrainerWrapper trainer, bool pretty = false, Normalizer? normalizer = null, PreTokenizer? preTokenizer = null, IEnumerable<Processors.PostProcessorWrapper>? processors = null, IEnumerable<Decoders.DecoderWrapper>? decoders = null, Truncation.TruncationParams? truncation = null, Padding.PaddingParams? padding = null)
    {
        var trainingParams = new Internal.Tokenizer.TokenizerFromTrain
        {
            Files = { files },
            SavePath = savePath,
            Pretty = pretty
        };
        trainingParams.Model = model;
        trainingParams.Trainer = trainer;
        if (normalizer != null)
            trainingParams.Normalizer = normalizer.InstancePtr;
        if (preTokenizer != null)
            trainingParams.PreTokenizer = preTokenizer.InstancePtr;
        if (processors != null)
        {
            trainingParams.Processor = new();
            trainingParams.Processor.Params.AddRange(processors);
        }
        if (decoders != null)
        {
            trainingParams.Decoder = new();
            trainingParams.Decoder.Params.AddRange(decoders);
        }
        if (truncation != null)
            trainingParams.Truncation = truncation;
        if (padding != null)
            trainingParams.Padding = padding;
        return new(ForeignFunctions.CreateNewArgsResult(
            TokenizerForeignFunctions.tokenizer_from_train,
            trainingParams
        ));
    }
    /// <summary>
    /// Decodes a collection of token ids'
    /// </summary>
    /// <param name="ids">The token ids'</param>
    /// <param name="skipSpecialTokens"></param>
    /// <returns>The string result of the decoding</returns>
    /// <exception cref="Errors.TokenizerDecodingException"></exception>"
    /// <remarks>
    /// It's possible for this method to throw other types of exceptions like (<see cref="Errors.InvalidBufferException"/>, <see cref="Errors.InvalidPointerException"/>, etc)<br/>
    /// This indicates an issue with the library, please open an issue at <see href="https://github.com/IgnaciodelaTorreArias/Tokenizers.HuggingFace.DotNet">GitHub</see>
    /// </remarks>
    public string Decode(IEnumerable<uint> ids, bool skipSpecialTokens)
    {
        if (InstancePtr == 0)
            throw new ObjectDisposedException("Tokenizer");
        return ForeignFunctions.MethodArgsResult<Internal.Tokenizer.DecodeResult, Internal.Tokenizer.DecodeParams>(
            TokenizerForeignFunctions.decode,
            InstancePtr,
            new Internal.Tokenizer.DecodeParams
            {
                Ids = { ids },
                SkipSpecialTokens = skipSpecialTokens
            },
            Internal.Tokenizer.DecodeResult.Parser
        ).Decoded;
    }
    /// <summary>
    /// Encodes a string into the corresponding token ids'
    /// </summary>
    /// <param name="input">The string to encode</param>
    /// <param name="addSpecialTokens">Whether to add special tokens</param>
    /// <param name="input2">An optional 2nd input for dual sequences</param>
    /// <param name="includeTypeIds">Type of the IDs. <see cref="Encoding.TypeIds"/><br/>
    /// When false (default) this helps improve performance since unused data is not serialized/deserialized preventing over fetching
    /// </param>
    /// <param name="includeTokens">Tokens (string) associated to each ID. <see cref="Encoding.Tokens"/><br/>
    /// When false (default) this helps improve performance since unused data is not serialized/deserialized preventing over fetching
    /// </param>
    /// <param name="includeWords">Indices of the word associated to each token/ID. <see cref="Encoding.Words"/><br/>
    /// When false (default) this helps improve performance since unused data is not serialized/deserialized preventing over fetching
    /// </param>
    /// <param name="includeOffsets">Offsets of the token/ID. <see cref="Encoding.Offsets"/> <see cref="Offsets"/><br/>
    /// If false this will internally use encode_fast
    /// When false (default) this helps improve performance since unused data is not serialized/deserialized preventing over fetching
    /// </param>
    /// <param name="includeSpecialTokensMask">Mask identifying special tokens. <see cref="Encoding.SpecialTokensMask"/><br/>
    /// When false (default) this helps improve performance since unused data is not serialized/deserialized preventing over fetching
    /// </param>
    /// <param name="includeAttentionMask">Mask identifying padding tokens for the attention mechanism. <see cref="Encoding.AttentionMask"/><br/>
    /// When false (default) this helps improve performance since unused data is not serialized/deserialized preventing over fetching
    /// </param>
    /// <param name="includeOverflowing">
    /// When false (default) this helps improve performance since unused data is not serialized/deserialized preventing over fetching.
    /// If false the return will only contain 1 <see cref="Encoding"/>
    /// </param>
    /// <returns>A collection of Encodings with the contents as specified by the parameters</returns>
    /// <exception cref="Errors.TokenizerEncodingException"></exception>"
    /// <remarks>
    /// It's possible for this method to throw other types of exceptions like (<see cref="Errors.InvalidBufferException"/>, <see cref="Errors.InvalidPointerException"/>, etc)<br/>
    /// This indicates an issue with the library, please open an issue at <see href="https://github.com/IgnaciodelaTorreArias/Tokenizers.HuggingFace.DotNet">GitHub</see>
    /// </remarks>
    public IEnumerable<Encoding> Encode(string input, bool addSpecialTokens, string? input2 = null, bool includeTypeIds = false, bool includeTokens = false, bool includeWords = false, bool includeOffsets = false, bool includeSpecialTokensMask = false, bool includeAttentionMask = false, bool includeOverflowing = false)
    {
        if (InstancePtr == 0)
            throw new ObjectDisposedException("Tokenizer");
        var encodeParams = new Internal.Tokenizer.EncodeParams
        {
            Input = input,
            AddSpecialTokens = addSpecialTokens,
            IncludeTypeIds = includeTypeIds,
            IncludeTokens = includeTokens,
            IncludeWords = includeWords,
            IncludeOffsets = includeOffsets,
            IncludeSpecialTokensMask = includeSpecialTokensMask,
            IncludeAttentionMask = includeAttentionMask,
            IncludeOverflowing = includeOverflowing
        };
        if (input2 != null)
            encodeParams.Input2 = input2;
        return ForeignFunctions.MethodArgsResult<Internal.Tokenizer.EncodeResult, Internal.Tokenizer.EncodeParams>(
            TokenizerForeignFunctions.encode,
            InstancePtr,
            encodeParams,
            Internal.Tokenizer.EncodeResult.Parser
        ).Encodings.AsEnumerable();
    }

    internal override FreeDelegate FreeFunc() =>
        TokenizerForeignFunctions.free_tokenizer;
}