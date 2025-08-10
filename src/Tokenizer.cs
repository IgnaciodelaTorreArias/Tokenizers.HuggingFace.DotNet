using System.Runtime.InteropServices;

using Messages.Tokenizer;
using Messages.Trainers;
using ProcessorWrapperParams = Messages.Processors.ProcessorWrapperParams;
using DecoderWrapperParams = Messages.Decoders.DecoderWrapperParams;
using Normalizer = Tokenizers.HuggingFace.Normalizers.Normalizer;
using PreTokenizer = Tokenizers.HuggingFace.PreTokenizers.PreTokenizer;
using Trainer = Tokenizers.HuggingFace.Trainers.Trainer;

namespace Tokenizers.HuggingFace.Tokenizer;

internal static class TokenizerForeignFunctions
{
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 encode(nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 decode(nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 tokenizer_from_file(out nuint instance_ptr, nuint ptr, nuint len, out nuint out_ptr, out nuint outLen);
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 tokenizer_from_train(out nuint instance_ptr, nuint ptr, nuint len, out nuint out_ptr, out nuint outLen);
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_tokenizer(nuint instance_ptr);
}
internal interface ITokenizerEncode
{
    EncodeResult Encode(
        string input,
        bool add_special_tokens,
        string? input2 = null,
        bool include_type_ids = false,
        bool include_tokens = false,
        bool include_words = false,
        bool include_offsets = false,
        bool include_special_tokens_mask = false,
        bool include_attention_mask = false,
        bool include_overflowing = false
    );
}
internal interface ITokenizerDecode
{
    string Decode(IEnumerable<UInt32> ids, bool skip_special_tokens);
}
public class Tokenizer : ForeignInstance, ITokenizerEncode, ITokenizerDecode
{
    public static Tokenizer FromFile(string path)
    {
        var instance = new Tokenizer();
        instance.InstancePtr = ForeignFunctions.CreateNewArgsResult(
            TokenizerForeignFunctions.tokenizer_from_file,
            new TokenizerFromFile { File = path }
        );
        return instance;
    }

    public static Tokenizer FromTrain(IEnumerable<string> files, string save_path, Trainer trainer, bool pretty = false, Normalizer? normalizer = null, PreTokenizer? pre_tokenizer = null, ProcessorWrapperParams? processor = null, DecoderWrapperParams? decoder = null, TruncationParams? truncation = null, PaddingParams? padding = null)
    {
        var trainer_params = new TrainerParams
        {
            Files = { files },
            SavePath = save_path,
            Pretty = pretty
        };
        switch (trainer.trainer_type)
        {
            case TrainerParams.TrainerOneofCase.BpeTrainer:
                trainer_params.BpeTrainer = (BpeTrainer)trainer.message;
                break;
            case TrainerParams.TrainerOneofCase.WordPieceTrainer:
                trainer_params.WordPieceTrainer = (WordPieceTrainer)trainer.message;
                break;
            case TrainerParams.TrainerOneofCase.WordLevelTrainer:
                trainer_params.WordLevelTrainer = (WordLevelTrainer)trainer.message;
                break;
            case TrainerParams.TrainerOneofCase.UnigramTrainer:
                trainer_params.UnigramTrainer = (UnigramTrainer)trainer.message;
                break;
            default:
                throw new ArgumentNullException("A requiered field is not present");
        }
        if (normalizer != null)
            trainer_params.Normalizer = normalizer.InstancePtr;
        if (pre_tokenizer != null)
            trainer_params.PreTokenizer = pre_tokenizer.InstancePtr;
        if (processor != null)
            trainer_params.Processor = processor;
        if (decoder != null)
            trainer_params.Decoder = decoder;
        if (truncation != null)
            trainer_params.Truncation = truncation;
        if (padding != null)
            trainer_params.Padding = padding;
        var instance = new Tokenizer();
        instance.InstancePtr = ForeignFunctions.CreateNewArgsResult(
            TokenizerForeignFunctions.tokenizer_from_train,
            trainer_params
        );
        return instance;
    }

    public string Decode(IEnumerable<uint> ids, bool skip_special_tokens)
    {
        return ForeignFunctions.MethodArgsResult<DecodeResult, DecodeParams>(
            TokenizerForeignFunctions.decode,
            InstancePtr,
            new DecodeParams
            {
                Ids = { ids },
                SkipSpecialTokens = skip_special_tokens
            },
            DecodeResult.Parser
        ).Decoded;
    }

    public EncodeResult Encode(string input, bool add_special_tokens, string? input2 = null, bool include_type_ids = false, bool include_tokens = false, bool include_words = false, bool include_offsets = false, bool include_special_tokens_mask = false, bool include_attention_mask = false, bool include_overflowing = false)
    {
        var encode_params = new EncodeParams
        {
            Input = input,
            AddSpecialTokens = add_special_tokens,
            IncludeTypeIds = include_type_ids,
            IncludeTokens = include_tokens,
            IncludeWords = include_words,
            IncludeOffsets = include_offsets,
            IncludeSpecialTokensMask = include_special_tokens_mask,
            IncludeAttentionMask = include_attention_mask,
            IncludeOverflowing = include_overflowing
        };
        if (input2 != null)
            encode_params.Input2 = input2;
        return ForeignFunctions.MethodArgsResult<EncodeResult, EncodeParams>(
            TokenizerForeignFunctions.encode,
            InstancePtr,
            encode_params,
            EncodeResult.Parser
        );
    }

    internal override FreeDelegate FreeFunc() =>
        TokenizerForeignFunctions.free_tokenizer;
}