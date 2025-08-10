using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Messages.PreTokenizers;

namespace Tokenizers.HuggingFace.PreTokenizers;

internal static class PreTokenizersForeignFunctions
{
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 pre_tokenize(nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 new_pre_tokenizer_wrapper(out nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_pre_tokenizer_wrapper(nuint instance_ptr);
}
internal interface IPreTokenizer
{
    IEnumerable<(string, (UInt64, UInt64)?)> PreTokenize(string text, OffsetReferential or, OffsetType ot, bool include_offsets = true);
}
public class PreTokenizer : ForeignInstance, IPreTokenizer
{
    internal PreTokenizer(PreTokenizerWrapperParams param)
    {
        InstancePtr = ForeignFunctions.CreateNewArgsResult(
            PreTokenizersForeignFunctions.new_pre_tokenizer_wrapper,
            param
        );
    }
    public IEnumerable<(string, (UInt64, UInt64)?)> PreTokenize(string text, OffsetReferential or, OffsetType ot, bool include_offsets = true)
    {
        var r = ForeignFunctions.MethodArgsResult<PreTokenizeResult, PreTokenizeParams>(
            PreTokenizersForeignFunctions.pre_tokenize,
            InstancePtr,
            new PreTokenizeParams { Normalized = text, OffsetReferential = or, OffsetType = ot, IncludeOffsets = include_offsets },
            PreTokenizeResult.Parser
        );
        return include_offsets
            ? r.Tokens.Zip(r.Offsets, static (token, offset) => (token, ((UInt64, UInt64)?)(offset.Start, offset.End)))
            : r.Tokens.Select(static t => (t, ((UInt64, UInt64)?)null)); ;
    }
    internal override FreeDelegate FreeFunc() =>
        PreTokenizersForeignFunctions.free_pre_tokenizer_wrapper;
}
public class Bert : PreTokenizer
{
    public Bert() : base(new PreTokenizerWrapperParams { BertPreTokenizer = { } }) { }
}
public class ByteLeve : PreTokenizer
{
    public readonly bool add_prefix_space;
    public readonly bool trim_offsets;
    public readonly bool use_regex;

    public ByteLeve(bool add_prefix_space = true, bool trim_offsets = true, bool use_regex = true) :
        base(new PreTokenizerWrapperParams
        {
            ByteLevel =
            {
                AddPrefixSpace = add_prefix_space,
                TrimOffsets = trim_offsets,
                UseRegex = use_regex,
            }
        })
    {
        this.add_prefix_space = add_prefix_space;
        this.trim_offsets = trim_offsets;
        this.use_regex = use_regex;
    }
}
public class Metaspace : PreTokenizer
{
    public readonly char replacement_char;
    public readonly PrependScheme prepend_scheme;
    public readonly bool split;

    public Metaspace(char replacement_char, PrependScheme prepend_scheme, bool split) :
        base(new PreTokenizerWrapperParams
        {
            Metaspace =
            {
                ReplacementChar = replacement_char.ToString(),
                PrependScheme = prepend_scheme,
                Split = split
            }
        })
    {
        this.replacement_char = replacement_char;
        this.prepend_scheme = prepend_scheme;
        this.split = split;
    }
}
public class Whitespace : PreTokenizer
{
    public Whitespace() :
        base(new PreTokenizerWrapperParams { Whitespace = { } })
    { }
}
public class WhitespaceSplit : PreTokenizer
{
    public WhitespaceSplit() :
        base(new PreTokenizerWrapperParams { WhitespaceSplit = { } })
    { }
}
public class Delimiter : PreTokenizer
{
    public readonly char delimiter_char;

    public Delimiter(char delimiter_char) :
        base(new PreTokenizerWrapperParams
        {
            Delimiter =
            {
                Char = delimiter_char.ToString()
            }
        })
    {
        this.delimiter_char = delimiter_char;
    }
}
public class Squence : PreTokenizer
{
    public readonly IEnumerable<PreTokenizer> pre_tokenizers;
    public Squence(IEnumerable<PreTokenizer> pre_tokenizers) :
        base(new PreTokenizerWrapperParams
        {
            Sequence =
            {
                Addresses = { pre_tokenizers.Select(static n => (long)n.InstancePtr) }
            }
        })
    {
        this.pre_tokenizers = pre_tokenizers;
    }
}
public class Split : PreTokenizer
{
    public readonly string pattern;
    public readonly SplitDelimiterBehavior behavior;
    public readonly bool invert;

    public Split(string pattern, SplitDelimiterBehavior behavior, bool invert) :
        base(new PreTokenizerWrapperParams
        {
            Split =
            {
                StringSplit = pattern,
                Behavior = behavior,
                Invert = invert
            }
        })
    {
        this.pattern = pattern;
        this.behavior = behavior;
        this.invert = invert;
    }
    public Split(Regex pattern, SplitDelimiterBehavior behavior, bool invert) :
        base(new PreTokenizerWrapperParams
        {
            Split =
            {
                RegexSplit = pattern.ToString(),
                Behavior = behavior,
                Invert = invert
            }
        })
    {
        this.pattern = pattern.ToString();
        this.behavior = behavior;
        this.invert = invert;
    }
}
public class Punctuation : PreTokenizer
{
    public readonly SplitDelimiterBehavior behavior;

    public Punctuation(SplitDelimiterBehavior behavior = SplitDelimiterBehavior.Isolated) :
        base(new PreTokenizerWrapperParams
        {
            Punctuation =
            {
                Behavior = behavior
            }
        })
    {
        this.behavior = behavior;
    }
}
public class Digits : PreTokenizer
{
    public readonly bool individual_digits;

    public Digits(bool individual_digits) :
        base(new PreTokenizerWrapperParams
        {
            Digits =
            {
                IndividualDigits = individual_digits
            }
        })
    {
        this.individual_digits = individual_digits;
    }
}
public class UnicodeScripts : PreTokenizer
{
    public UnicodeScripts() : base(new PreTokenizerWrapperParams { UnicodeScripts = { } }) { }
}
public class FixedLength : PreTokenizer
{
    public readonly UInt64 length;

    public FixedLength(UInt64 length = 5) :
        base(new PreTokenizerWrapperParams
        {
            FixedLength =
            {
                Length = length
            }
        })
    {
        this.length = length;
    }
}