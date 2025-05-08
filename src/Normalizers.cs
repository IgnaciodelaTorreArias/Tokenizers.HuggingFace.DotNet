using System.Runtime.InteropServices;

using Google.Protobuf;
using Messages.Normalizers;

namespace Tokenizers.Normalizers;

internal static class NormalizersForeignFunctions
{
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 normalize(nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 new_normalizer_wrapper(out nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_normalizer_wrapper(nuint instance_ptr);
}
internal interface INormalizer
{
    string Normalize(string text);
}
public class Normalizer : ForeignInstance, INormalizer
{
    internal Normalizer(NormalizerWrapperParams param) {
        InstancePtr = ForeignFunctions.CreateNewArgsResult(
            NormalizersForeignFunctions.new_normalizer_wrapper,
            param
        );
    }
    public string Normalize(string text)
    {
        return ForeignFunctions.MethodArgsResult<NormalizeResult, NormalizeParams>(
            NormalizersForeignFunctions.normalize,
            InstancePtr,
            new NormalizeParams { Original = text },
            NormalizeResult.Parser
        ).Normalized;
    }
    internal override FreeDelegate FreeFunc() => 
        NormalizersForeignFunctions.free_normalizer_wrapper;
}
public class Bert : Normalizer
{
    public readonly bool clean_text;
    public readonly bool handle_chinese_chars;
    public readonly bool strip_accents;
    public readonly bool lowercase;

    public Bert(bool clean_text = true, bool handle_chinese_chars = true, bool strip_accents = true, bool lowercase = true) :
        base(new NormalizerWrapperParams
        {
            BertNormalizer =
            {
                CleanText = clean_text,
                HandleChineseChars = handle_chinese_chars,
                StripAccents = strip_accents,
                Lowercase = lowercase
            }
        })
    {
        this.clean_text = clean_text;
        this.handle_chinese_chars = handle_chinese_chars;
        this.strip_accents = strip_accents;
        this.lowercase = lowercase;
    }
}
public class Nfd : Normalizer
{
    public Nfd() : base(new NormalizerWrapperParams { Nfd = { } }) { }
}
public class Nfkd : Normalizer
{
    public Nfkd() : base(new NormalizerWrapperParams { Nfkd = { } }) { }
}
public class Nfc : Normalizer
{
    public Nfc() : base(new NormalizerWrapperParams { Nfc = { } }) { }
}
public class Nfkc : Normalizer
{
    public Nfkc() : base(new NormalizerWrapperParams { Nfkc = { } }) { }
}
public class Nmt : Normalizer
{
    public Nmt() : base(new NormalizerWrapperParams { Nmt = { } }) { }
}
public class StripNormalizer : Normalizer
{
    public readonly bool strip_left;
    public readonly bool strip_right;
    public StripNormalizer(bool strip_left, bool strip_right) : 
        base(new NormalizerWrapperParams
        {
            StripNormalizer =
            {
                StripLeft = strip_left,
                StripRight = strip_right
            }
        })
    {
        this.strip_left = strip_left;
        this.strip_right = strip_right;
    }
}
public class StripAccents : Normalizer
{
    public StripAccents() : base(new NormalizerWrapperParams { StripAccents = { } }) { }
}
public class Sequence : Normalizer
{
    public readonly IEnumerable<Normalizer> normalizers;
    public Sequence(IEnumerable<Normalizer> normalizers) :
        base(new NormalizerWrapperParams
        {
            Sequence = {
                Addresses = { normalizers.Select(static n => (long)n.InstancePtr) }
            }
        })
    {
        this.normalizers = normalizers;
    }
}
public class Lowercase : Normalizer
{
    public Lowercase() : base(new NormalizerWrapperParams { Lowercase = { } }) { }
}
public class Prepend : Normalizer
{
    public readonly string prepend;
    public Prepend(string prepend) :
        base(new NormalizerWrapperParams
        {
            Prepend = { Prepend_ = prepend }
        })
    {
        this.prepend = prepend;
    }
}
public class Replace : Normalizer
{
    public readonly string pattern;
    public readonly string content;
    public Replace(string pattern, string content) :
        base(new NormalizerWrapperParams
        {
            Replace =
            {
                Pattern = pattern,
                Content = content
            }
        })
    {
        this.pattern = pattern;
        this.content = content;
    }
}
public class Precompiled : Normalizer
{
    public readonly byte[] precompiled_charsmap;
    public Precompiled(byte[] precompiled_charsmap) :
        base(new NormalizerWrapperParams
        {
            Precompiled = { PrecompiledCharsmap = ByteString.CopyFrom(precompiled_charsmap) } 
        })
    {
        this.precompiled_charsmap = precompiled_charsmap;
    }
}
public class ByteLevel : Normalizer
{
    public ByteLevel() : base(new NormalizerWrapperParams { ByteLevel = { } }) { }
}
