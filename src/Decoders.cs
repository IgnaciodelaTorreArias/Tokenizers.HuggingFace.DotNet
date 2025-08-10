using Messages.Decoders;
using Messages.Normalizers;
using System.Text.RegularExpressions;
using PrependScheme = Messages.PreTokenizers.PrependScheme;

namespace Tokenizers.HuggingFace.Decoders;

public abstract class Decoder
{
    public static implicit operator DecoderWrapperParams(Decoder p) => new DecoderWrapperParams
    {
        Params = { p._params }
    };
    internal readonly Params _params;
    internal Decoder(Params param)
    {
        _params = param;
    }
}
public class Bpe : Decoder
{
    public readonly string suffix;
    public Bpe(string suffix = "</w>") :
        base(new Params
        {
            BpeDecoder =
            {
                Suffix = suffix
            }
        })
    {
        this.suffix = suffix;
    }
}
public class ByteLevel : Decoder
{
    public readonly bool add_prefix_space;
    public readonly bool trim_offsets;
    public readonly bool use_regex;
    public ByteLevel(bool add_prefix_space = true, bool trim_offsets = true, bool use_regex = true) :
        base(new Params
        {
            ByteLevel =
            {
                AddPrefixSpace = add_prefix_space,
                TrimOffsets = trim_offsets,
                UseRegex = use_regex
            }
        })
    {
        this.add_prefix_space = add_prefix_space;
        this.trim_offsets = trim_offsets;
        this.use_regex = use_regex;
    }
}
public class WordPiece : Decoder
{
    public readonly string prefix;
    public readonly bool cleanup;
    public WordPiece(string prefix = "##", bool cleanup = true) :
        base(new Params
        {
            WordPiece =
            {
                Prefix = prefix,
                Cleanup = cleanup
            }
        })
    {
        this.prefix = prefix;
        this.cleanup = cleanup;
    }
}
public class Metaspace : Decoder
{
    public readonly char replacement_char;
    public readonly PrependScheme prepend_scheme;
    public readonly bool split;
    public Metaspace(char replacement_char, PrependScheme prepend_scheme, bool split) :
        base(new Params
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
public class Ctc : Decoder
{
    public readonly string pad_token;
    public readonly string word_delimiter_token;
    public readonly bool cleanup;
    public Ctc(string pad_token = "<pad>", string word_delimiter_token = "|", bool cleanup = true) :
        base(new Params
        {
            Ctc =
            {
                PadToken = pad_token,
                WordDelimiterToken = word_delimiter_token,
                Cleanup = cleanup
            }
        })
    {
        this.pad_token = pad_token;
        this.word_delimiter_token = word_delimiter_token;
        this.cleanup = cleanup;
    }
}
public class Replace : Decoder
{
    public readonly string pattern;
    public readonly string content;
    public Replace(string pattern, string content) :
        base(new Params
        {
            Replace =
            {
                StringReplacement = pattern,
                Content = content
            }
        })
    {
        this.pattern = pattern;
        this.content = content;
    }
    public Replace(Regex pattern, string content) :
        base(new Params
        {
            Replace =
            {
                RegexReplacement = pattern.ToString(),
                Content = content
            }
        })
    {
        this.pattern = pattern.ToString();
        this.content = content;
    }
}
public class Fuse : Decoder
{
    public Fuse() : base(new Params { Fuse = { } }) { }
}
public class Strip : Decoder
{
    public readonly char content;
    public readonly UInt64 start;
    public readonly UInt64 stop;
    public Strip(char content, UInt64 start, UInt64 stop) :
        base(new Params
        {
            Strip =
            {
                Content = content.ToString(),
                Start = start,
                Stop = stop
            }
        })
    {
        this.content = content;
        this.start = start;
        this.stop = stop;
    }
}
public class ByteFallback : Decoder
{
    public ByteFallback() : base(new Params { ByteFallback = { } }) { }
}
public class Sequence
{
    public static implicit operator DecoderWrapperParams(Sequence p) => p._params;
    internal readonly DecoderWrapperParams _params;
    public readonly IEnumerable<Decoder> decoders;
    public Sequence(IEnumerable<Decoder> decoders)
    {
        this.decoders = decoders;
        _params = new DecoderWrapperParams
        {
            Params = { decoders.Select(p => p._params) }
        };
    }
}
