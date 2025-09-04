using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Tokenizers.HuggingFace.Decoders;
using Tokenizers.HuggingFace.Internal;
using Tokenizers.HuggingFace.Internal.PreTokenizers;
using Tokenizers.HuggingFace.Normalizers;

namespace Tokenizers.HuggingFace.PreTokenizers;

internal static class PreTokenizersForeignFunctions
{
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 pre_tokenize(nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 new_pre_tokenizer_wrapper(out nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_pre_tokenizer_wrapper(nuint instancePtr);
}
/// <summary>
/// Represents a pre-tokenizer, which is used to split text into smaller pieces.<br/>
/// For example the most common pre-tokenizer is the Whitespace, which splits text on 'whitespace characters' like spaces.
/// </summary>
public class PreTokenizer : ForeignInstance
{
    internal PreTokenizer(PreTokenizerWrapper param)
    {
        InstancePtr = ForeignFunctions.CreateNewArgsResult(
            PreTokenizersForeignFunctions.new_pre_tokenizer_wrapper,
            param
        );
    }
    /// <summary>
    /// Splits the given string in multiple substrings, keeping track of the offsets of said substrings
    /// </summary>
    /// <param name="pipelineString">The <see cref="PipelineString.PipelineString"/> to pre-tokenize</param>
    /// <exception cref="Errors.PreTokenizationException"></exception>
    /// <remarks>
    /// It's possible for this method to throw other types of exceptions like (<see cref="Errors.InvalidBufferException"/>, <see cref="Errors.InvalidPointerException"/>, etc)<br/>
    /// This indicates an issue with the library, please open an issue at <see href="https://github.com/IgnaciodelaTorreArias/Tokenizers.HuggingFace.DotNet">GitHub</see>
    /// </remarks>
    public void PreTokenize(PipelineString.PipelineString pipelineString)
    {
        if (InstancePtr == 0)
            throw new ObjectDisposedException("PreTokenizer");
        ForeignFunctions.MethodArgsResult<PreTokenizeParams>(
            PreTokenizersForeignFunctions.pre_tokenize,
            InstancePtr,
            new PreTokenizeParams { PipelineString = pipelineString.InstancePtr }
        );
    }
    internal override FreeDelegate FreeFunc() =>
        PreTokenizersForeignFunctions.free_pre_tokenizer_wrapper;
}
/// <summary>
/// Splits on whitespaces and punctuation
/// </summary>
public class Bert : PreTokenizer
{
    /// <inheritdoc cref="Bert"/>
    public Bert() : base(new PreTokenizerWrapper { BertPreTokenizer = new() { } }) { }
}
/// <summary>
/// Provides all the necessary steps to handle the BPE tokenization at the byte-level.<br/>
/// Takes care of all the required processing steps to transform a UTF-8 string as needed before and after the BPE model does its job
/// </summary>
public class ByteLevel : PreTokenizer
{
    /// <summary>
    /// Whether to add a leading space to the first word. This allows to treat the leading word just as any other word
    /// </summary>
    public readonly bool AddPrefixSpace;
    /// <summary>
    /// Whether the post processing step should trim offsets to avoid including whitespaces
    /// </summary>
    public readonly bool TrimOffsets;
    /// <summary>
    /// Whether to use the standard GPT2 regex for whitespace splitting Set it to False if you want to use your own splitting
    /// </summary>
    public readonly bool UseRegex;
    /// <inheritdoc cref="ByteLevel"/>
    /// <param name="addPrefixSpace">Initializer for: <see cref="AddPrefixSpace"/></param>
    /// <param name="trimOffsets">Initializer for: <see cref="TrimOffsets"/></param>
    /// <param name="useRegex">Initializer for: <see cref="UseRegex"/></param>
    public ByteLevel(bool addPrefixSpace = true, bool trimOffsets = true, bool useRegex = true) :
        base(new PreTokenizerWrapper
        {
            ByteLevel = new()
            {
                AddPrefixSpace = addPrefixSpace,
                TrimOffsets = trimOffsets,
                UseRegex = useRegex,
            }
        })
    {
        this.AddPrefixSpace = addPrefixSpace;
        this.TrimOffsets = trimOffsets;
        this.UseRegex = useRegex;
    }
}
/// <summary>
/// Replaces all the spaces with the povided meta character and then splits on this character
/// </summary>
public class Metaspace : PreTokenizer
{
    /// <summary>
    /// The meta character
    /// </summary>
    public readonly char ReplacementChar;
    /// <inheritdoc cref="Decoders.PrependScheme"/>
    public readonly PrependScheme PrependScheme;
    /// <summary>
    /// Whether spliting at the end of the replacement
    /// </summary>
    public readonly bool Split;
    /// <inheritdoc cref="Metaspace"/>
    /// <param name="replacementChar">Initializer for: <see cref="ReplacementChar"/></param>
    /// <param name="prependScheme">Initializer for: <see cref="PrependScheme"/></param>
    /// <param name="split">Initializer for: <see cref="Split"/></param>
    public Metaspace(char replacementChar, PrependScheme prependScheme, bool split) :
        base(new PreTokenizerWrapper
        {
            Metaspace = new()
            {
                ReplacementChar = replacementChar.ToString(),
                PrependScheme = prependScheme,
                Split = split
            }
        })
    {
        this.ReplacementChar = replacementChar;
        this.PrependScheme = prependScheme;
        this.Split = split;
    }
}
/// <summary>
/// Uses the inverted regex expression: "\w+|[^\w\s]+" for distinguishing whitespaces
/// </summary>
public class Whitespace : PreTokenizer
{
    /// <inheritdoc cref="Whitespace"/>
    public Whitespace() :
        base(new PreTokenizerWrapper { Whitespace = new() { } })
    { }
}
/// <summary>
/// Whitespaces defined by <see href="https://www.unicode.org/reports/tr44/">Unicode Character Database</see> <see href="https://www.unicode.org/Public/UCD/latest/ucd/PropList.txt">PropList.txt</see>
/// </summary>
public class WhitespaceSplit : PreTokenizer
{
    /// <inheritdoc cref="WhitespaceSplit"/>
    public WhitespaceSplit() :
        base(new PreTokenizerWrapper { WhitespaceSplit = new() { } })
    { }
}
/// <summary>
/// Splits on the <see cref="DelimiterChar"/>
/// </summary>
public class Delimiter : PreTokenizer
{
    /// <summary>
    /// The char to use as delimiter
    /// </summary>
    public readonly char DelimiterChar;
    /// <inheritdoc cref="Delimiter"/>
    /// <param name="delimiterChar">Initializer for: <see cref="DelimiterChar"/></param>
    public Delimiter(char delimiterChar) :
        base(new PreTokenizerWrapper
        {
            Delimiter = new()
            {
                Char = delimiterChar.ToString()
            }
        })
    {
        this.DelimiterChar = delimiterChar;
    }
}
/// <summary>
/// Allows concatenating multiple other <see cref="PreTokenizer"/> as a Sequence. All the pre-tokenizers are run in sequence in the given order
/// </summary>
public class Squence : PreTokenizer
{
    /// <summary>
    /// The pre-tokenizers that will be run in sequence
    /// </summary>
    public readonly IEnumerable<PreTokenizer> PreTokenizers;
    /// <inheritdoc cref="Tokenizers.HuggingFace.PreTokenizers.Squence"/>
    /// <param name="preTokenizers">A collection of <see cref="PreTokenizer"/> instances to initialize <see cref="PreTokenizers"/>.</param>
    public Squence(IEnumerable<PreTokenizer> preTokenizers) :
        base(new PreTokenizerWrapper
        {
            Sequence = ForeignFunctions.ToSequence(preTokenizers)
        })
    {
        this.PreTokenizers = preTokenizers;
    }
    internal override void Dispose(bool disposing)
    {
        foreach (var pt in PreTokenizers)
            pt.Dispose();
        base.Dispose(disposing);
    }
}
/// <summary>
/// Gives control on spliting behavior by using regex to determine the spliting points
/// </summary>
public class Split : PreTokenizer
{
    /// <summary>
    /// The pattern used to find spliting points <br/>
    /// Even though here is a string, internally is managed as regex pattern
    /// </summary>
    public readonly string Pattern;
    /// <inheritdoc cref="SplitDelimiterBehavior"/>
    public readonly SplitDelimiterBehavior Behavior;
    /// <summary>
    /// Whether to invert the <see cref="Pattern"/><br/>
    /// Usefull whe you use <see cref="Split.Split(Regex, SplitDelimiterBehavior, bool)"/>
    /// </summary>
    public readonly bool Invert;
    /// <summary>
    /// Scapes the <paramref name="pattern"/> and uses regex to the spliting
    /// </summary>
    /// <param name="pattern">Initializer for: <see cref="Pattern"/></param>
    /// <param name="behavior">Initializer for: <see cref="Behavior"/></param>
    /// <param name="invert">Initializer for: <see cref="Invert"/></param>
    public Split(string pattern, SplitDelimiterBehavior behavior, bool invert) :
        base(new PreTokenizerWrapper
        {
            Split = new()
            {
                StringSplit = pattern,
                Behavior = behavior,
                Invert = invert
            }
        })
    {
        this.Pattern = pattern;
        this.Behavior = behavior;
        this.Invert = invert;
    }
    /// <summary>
    /// Uses the regex as you specify it
    /// </summary>
    /// <param name="pattern">Initializer for: <see cref="Pattern"/></param>
    /// <param name="behavior">Initializer for: <see cref="Behavior"/></param>
    /// <param name="invert">Initializer for: <see cref="Invert"/></param>
    public Split(Regex pattern, SplitDelimiterBehavior behavior, bool invert) :
        base(new PreTokenizerWrapper
        {
            Split = new()
            {
                RegexSplit = pattern.ToString(),
                Behavior = behavior,
                Invert = invert
            }
        })
    {
        this.Pattern = pattern.ToString();
        this.Behavior = behavior;
        this.Invert = invert;
    }
}
/// <summary>
/// Splits on ASCII punctuation characters and (Pc, Pd, Pe, Pf, Pi, Po, or Ps) Unicode categories
/// </summary>
public class Punctuation : PreTokenizer
{
    /// <inheritdoc cref="SplitDelimiterBehavior"/>
    public readonly SplitDelimiterBehavior Behavior;
    /// <inheritdoc cref="Punctuation"/>
    /// <param name="behavior">Initializer for: <see cref="Behavior"/></param>
    public Punctuation(SplitDelimiterBehavior behavior = SplitDelimiterBehavior.Isolated) :
        base(new PreTokenizerWrapper
        {
            Punctuation = new()
            {
                Behavior = behavior
            }
        })
    {
        this.Behavior = behavior;
    }
}
/// <summary>
/// Pre tokenizes the numbers into single tokens
/// </summary>
public class Digits : PreTokenizer
{
    /// <summary>
    /// If set to true all digits are splitted into individual tokens
    /// </summary>
    public readonly bool IndividualDigits;
    /// <inheritdoc cref="Digits"/>
    /// <param name="individualDigits">Initializer for: <see cref="IndividualDigits"/></param>
    public Digits(bool individualDigits) :
        base(new PreTokenizerWrapper
        {
            Digits = new()
            {
                IndividualDigits = individualDigits
            }
        })
    {
        this.IndividualDigits = individualDigits;
    }
}
/// <summary>
/// This pre-tokenizer splits on characters that belong to different language family.<br/>
/// It roughly follows <see href="https://github.com/google/sentencepiece/blob/master/data/Scripts.txt">Scripts.txt</see>.<br/>
/// Actually Hiragana and Katakana are fused with Han, and 0x30FC is Han too. This mimicks SentencePiece Unigram implementation.
/// </summary>
public class UnicodeScripts : PreTokenizer
{
    /// <inheritdoc cref="UnicodeScripts"/>
    public UnicodeScripts() : base(new PreTokenizerWrapper { UnicodeScripts = new() { } }) { }
}
/// <summary>
/// Creates fixed length splits
/// </summary>
public class FixedLength : PreTokenizer
{
    /// <summary>
    /// The length of the splits
    /// </summary>
    public readonly UInt64 Length;
    /// <inheritdoc cref="FixedLength"/>
    /// <param name="length">Initializer for: <see cref="Length"/></param>
    public FixedLength(UInt64 length = 5) :
        base(new PreTokenizerWrapper
        {
            FixedLength = new()
            {
                Length = length
            }
        })
    {
        this.Length = length;
    }
}