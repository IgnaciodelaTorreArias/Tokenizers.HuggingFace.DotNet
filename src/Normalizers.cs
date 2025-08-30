using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Google.Protobuf;

using Tokenizers.HuggingFace.Internal.Normalizers;
using Tokenizers.HuggingFace.Internal;
using Tokenizers.HuggingFace.PipelineString;

namespace Tokenizers.HuggingFace.Normalizers;

internal static class NormalizersForeignFunctions
{
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 normalize(nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 new_normalizer_wrapper(out nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_normalizer_wrapper(nuint instancePtr);
}

/// <summary>
/// Represents a normalizer, this is the base class for all normalizers.<br/>
/// A Normalizer standardizes an input string before it is tokenized.<br/>
/// </summary>
abstract public class Normalizer : ForeignInstance
{
    internal Normalizer(NormalizerWrapperParams param)
    {
        InstancePtr = ForeignFunctions.CreateNewArgsResult(
            NormalizersForeignFunctions.new_normalizer_wrapper,
            param
        );
    }
    /// <summary>
    /// Affects a Pipeline string normalizing it
    /// </summary>
    /// <param name="pipelineString">The <see cref="PipelineString.PipelineString"/> to apply normalization</param>
    /// <exception cref="Errors.NormalizationException"></exception>
    /// <remarks>
    /// It's possible for this method to throw other types of exceptions like (<see cref="Errors.InvalidBufferException"/>, <see cref="Errors.InvalidPointerException"/>, etc)<br/>
    /// This indicates an issue with the library, please open an issue at <see href="https://github.com/IgnaciodelaTorreArias/Tokenizers.HuggingFace.DotNet">GitHub</see>
    /// </remarks>
    public void Normalize(PipelineString.PipelineString pipelineString)
    {
        ForeignFunctions.MethodArgsResult<NormalizeParams>(
            NormalizersForeignFunctions.normalize,
            InstancePtr,
            new NormalizeParams { PipelineString = pipelineString.InstancePtr }
        );
    }
    internal override FreeDelegate FreeFunc() =>
        NormalizersForeignFunctions.free_normalizer_wrapper;
}

/// <summary>
/// Takes care of normalizing raw text before giving it to a Bert model.<br/>
/// This includes cleaning the text, handling accents, chinese chars and lowercasing
/// </summary>
public class Bert : Normalizer
{
    /// <summary>
    /// Whether to do the bert basic cleaning:
    /// <list type="number">
    /// <item>Remove any control</item>
    /// <item>Replace all sorts of whitespace by the classic one</item>
    /// </list>
    /// </summary>
    public readonly bool CleanText;
    /// <summary>
    /// Whether to put spaces around chinese characters so they get split
    /// </summary>
    public readonly bool HandleChineseChars;
    /// <summary>
    /// Whether to strip accents
    /// </summary>
    public readonly bool StripAccents;
    /// <summary>
    /// Whether to lowercase the input
    /// </summary>
    public readonly bool Lowercase;

    /// <inheritdoc cref="Bert"/>
    /// <param name="cleanText">Initializer for: <see cref="CleanText"/></param>
    /// <param name="handleChineseChars">Initializer for: <see cref="HandleChineseChars"/></param>
    /// <param name="stripAccents">Initializer for: <see cref="StripAccents"/></param>
    /// <param name="lowercase">Initializer for: <see cref="Lowercase"/></param>
    public Bert(bool cleanText = true, bool handleChineseChars = true, bool stripAccents = true, bool lowercase = true) :
        base(new NormalizerWrapperParams
        {
            BertNormalizer =
            {
                CleanText = cleanText,
                HandleChineseChars = handleChineseChars,
                StripAccents = stripAccents,
                Lowercase = lowercase
            }
        })
    {
        this.CleanText = cleanText;
        this.HandleChineseChars = handleChineseChars;
        this.StripAccents = stripAccents;
        this.Lowercase = lowercase;
    }
}
/// <summary>
/// NFD unicode normalizer
/// </summary>
public class Nfd : Normalizer
{
    /// <inheritdoc cref="Nfd"/>
    public Nfd() : base(new NormalizerWrapperParams { Nfd = { } }) { }
}
/// <summary>
/// NFKD unicode normalizer
/// </summary>
public class Nfkd : Normalizer
{
    /// <inheritdoc cref="Nfkd"/>
    public Nfkd() : base(new NormalizerWrapperParams { Nfkd = { } }) { }
}
/// <summary>
/// NFC unicode normalizer
/// </summary>
public class Nfc : Normalizer
{
    /// <inheritdoc cref="Nfc"/>
    public Nfc() : base(new NormalizerWrapperParams { Nfc = { } }) { }
}
/// <summary>
/// NFKC unicode normalizer
/// </summary>
public class Nfkc : Normalizer
{
    /// <inheritdoc cref="Nfkc"/>
    public Nfkc() : base(new NormalizerWrapperParams { Nfkc = { } }) { }
}
/// <summary>
/// NMT unicode normalizer
/// </summary>
public class Nmt : Normalizer
{
    /// <inheritdoc cref="Nmt"/>
    public Nmt() : base(new NormalizerWrapperParams { Nmt = { } }) { }
}
/// <summary>
/// Strip whitespaces from the left and/or right of the string.<br/>
/// whitespaces defined by <see href="https://www.unicode.org/reports/tr44/">Unicode Character Database</see> <see href="https://www.unicode.org/Public/UCD/latest/ucd/PropList.txt">PropList.txt</see>
/// </summary>
public class StripNormalizer : Normalizer
{
    /// <summary>
    /// Whether to strip whitespaces from the left
    /// </summary>
    public readonly bool StripLeft;
    /// <summary>
    /// Whether to strip whitespaces from the left
    /// </summary>
    public readonly bool StripRight;

    /// <inheritdoc cref="StripNormalizer"/>
    /// <param name="stripLeft">Initializer for: <see cref="StripLeft"/></param>
    /// <param name="stripRight">Initializer for: <see cref="StripRight"/></param>
    public StripNormalizer(bool stripLeft, bool stripRight) :
        base(new NormalizerWrapperParams
        {
            StripNormalizer =
            {
                StripLeft = stripLeft,
                StripRight = stripRight
            }
        })
    {
        this.StripLeft = stripLeft;
        this.StripRight = stripRight;
    }
}
/// <summary>
/// This normalizer removes combining marks from a normalized string. <br/>
/// Different from unicode as it does not attempt to modify non ascii languages
/// </summary>
public class StripAccents : Normalizer
{
    /// <inheritdoc cref="StripAccents"/>
    public StripAccents() : base(new NormalizerWrapperParams { StripAccents = { } }) { }
}
/// <summary>
/// Allows concatenating multiple other <see cref="Normalizer"/> as a Sequence. All the normalizers are run in sequence in the given order
/// </summary>
public class Sequence : Normalizer
{
    /// <summary>
    /// The normalizers that will be run in sequence
    /// </summary>
    public readonly IEnumerable<Normalizer> Normalizers;
    /// <inheritdoc cref="Sequence"/>
    /// <param name="normalizers">A collection of <see cref="Normalizer"/> instances to initialize <see cref="Normalizers"/>.</param>
    public Sequence(IEnumerable<Normalizer> normalizers) :
        base(new NormalizerWrapperParams
        {
            Sequence = {
                Addresses = { normalizers.Select(static n => (ulong)n.InstancePtr) }
            }
        })
    {
        this.Normalizers = normalizers;
    }
}
/// <summary>
/// Lowercases the input string.<br/>
/// </summary>
public class Lowercase : Normalizer
{
    /// <inheritdoc cref="Lowercase"/>
    public Lowercase() : base(new NormalizerWrapperParams { Lowercase = { } }) { }
}
/// <summary>
/// Prepends a string to the <see cref="PipelineString.PipelineString"/> we want to normalize
/// </summary>
public class Prepend : Normalizer
{
    /// <summary>
    /// The string we want to prepend
    /// </summary>
    public readonly string Prepended;
    /// <inheritdoc cref="Prepend"/>
    /// <param name="prepended">Initializar for: <see cref="Prepended"/></param>
    public Prepend(string prepended) :
        base(new NormalizerWrapperParams
        {
            Prepend = { Prepend_ = prepended }
        })
    {
        this.Prepended = prepended;
    }
}
/// <summary>
/// Takes every occurrence of a regex pattern and replaces it by the given content
/// </summary>
public class Replace : Normalizer
{
    /// <summary>
    /// Pattern to look for, can be a string or a regex
    /// </summary>
    public readonly string Pattern;
    /// <summary>
    /// Content to replace the parts that match the <see cref="Pattern"/> with
    /// </summary>
    public readonly string Content;
    /// <summary>
    /// Use this constructor to replace exact string matches.<br/>
    /// The string will be escaped
    /// </summary>
    /// <param name="pattern">Initializer for: <see cref="Pattern"/></param>
    /// <param name="content">Initializer for: <see cref="Content"/></param>
    public Replace(string pattern, string content) :
        base(new NormalizerWrapperParams
        {
            Replace =
            {
                StringReplacement = pattern,
                Content = content
            }
        })
    {
        this.Pattern = pattern;
        this.Content = content;
    }
    /// <summary>
    /// Use this constructor to replace using a regex pattern for matches
    /// </summary>
    /// <param name="pattern">Initializer for: <see cref="Pattern"/></param>
    /// <param name="content">Initializer for: <see cref="Content"/></param>
    public Replace(Regex pattern, string content) :
        base(new NormalizerWrapperParams
        {
            Replace =
            {
                RegexReplacement = pattern.ToString(),
                Content = content
            }
        })
    {
        this.Pattern = pattern.ToString();
        this.Content = content;
    }
}
/// <summary>
/// Aimed to emulate <see href="https://github.com/google/sentencepiece"/><br/>
/// You probably shouldn't use this
/// </summary>
public class Precompiled : Normalizer
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public readonly byte[] PrecompiledCharsmap;
    public Precompiled(byte[] precompiledCharsmap) :
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        base(new NormalizerWrapperParams
        {
            Precompiled = { PrecompiledCharsmap = ByteString.CopyFrom(precompiledCharsmap) }
        })
    {
        this.PrecompiledCharsmap = precompiledCharsmap;
    }
}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class ByteLevel : Normalizer
{
    public ByteLevel() : base(new NormalizerWrapperParams { ByteLevel = { } }) { }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
