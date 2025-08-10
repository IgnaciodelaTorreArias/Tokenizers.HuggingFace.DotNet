using Google.Protobuf.Collections;

using Messages.Processors;
using System.Collections.Immutable;

namespace Tokenizers.HuggingFace.Processors;

public abstract class Processor
{
    public static implicit operator ProcessorWrapperParams(Processor p) => new ProcessorWrapperParams
    {
        Params = { p._params }
    };
    internal readonly Params _params;
    internal Processor(Params param)
    {
        _params = param;
    }
}
public class Roberta : Processor
{
    public readonly (string, UInt32) sep;
    public readonly (string, UInt32) cls;
    public readonly bool trim_offsets;
    public readonly bool add_prefix_space;
    public Roberta(string sep_str = "</s>", UInt32 sep_id = 2, string cls_str = "<s>", UInt32 cls_id = 0, bool trim_offsets = true, bool add_prefix_space = true) :
        base(new Params
        {
            RobertaProcessing =
            {
                SepStr = sep_str,
                SepId = sep_id,
                ClsStr = cls_str,
                ClsId = cls_id,
                TrimOffsets = trim_offsets,
                AddPrefixSpace = add_prefix_space
            }
        })
    {
        sep = (sep_str, sep_id);
        cls = (cls_str, cls_id);
        this.trim_offsets = trim_offsets;
        this.add_prefix_space = add_prefix_space;
    }
}
public class Bert : Processor
{
    public readonly (string, UInt32) sep;
    public readonly (string, UInt32) cls;
    public Bert(string sep_str = "[SEP]", UInt32 sep_id = 102, string cls_str = "[CLS]", UInt32 cls_id = 101) :
        base(new Params
        {
            BertProcessing =
            {
                SepStr = sep_str,
                SepId = sep_id,
                ClsStr = cls_str,
                ClsId = cls_id
            }
        })
    {
        sep = (sep_str, sep_id);
        cls = (cls_str, cls_id);
    }
}
public class ByteLevel : Processor
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
public class Token
{
    public readonly string token;
    public ImmutableArray<UInt32> ids;
    public ImmutableArray<string> tokens;
    internal readonly Messages.Processors.Token _token;

    public Token(UInt32 id, string token)
    {
        this.token = token;
        this.ids = new ImmutableArray<UInt32>() { id };
        this.tokens = new ImmutableArray<string>() { token };
        this._token = new Messages.Processors.Token
        {
            TokenPair =
            {
                Token = token,
                TokenId = id
            }
        };
    }
    public Token(string token, IEnumerable<UInt32> ids, IEnumerable<string> tokens)
    {
        this.token = token;
        this.ids = ids.ToImmutableArray();
        this.tokens = tokens.ToImmutableArray();
        if (this.ids.Length != this.tokens.Length)
            throw new ArgumentException("ids and tokens must be the same Length");
        this._token = new Messages.Processors.Token
        {
            SpecialToken =
            {
                Token = token,
                Ids = { ids },
                Tokens = { tokens }
            }
        };
    }
}
public class Template : Processor
{
    public readonly string single;
    public readonly string pair;
    public readonly UInt64 added_single;
    public readonly UInt64 added_pair;
    public readonly ImmutableDictionary<string, Token>? special_tokens_dict;
    public readonly ImmutableArray<Token>? special_tokens;

    public Template(string single = "$0", string pair = "$A:0 $B:1", UInt64 added_single = 0, UInt64 added_pair = 0, IDictionary<string, Token>? special_tokens = null) :
        base(new Params
        {
            TemplateProcessing =
            {
                Single = single,
                Pair = pair,
                AddedSingle = added_single,
                AddedPair = added_pair,
            }
        })
    {
        this.single = single;
        this.pair = pair;
        this.added_single = added_single;
        this.added_pair = added_pair;
        if (special_tokens != null)
        {
            this._params.TemplateProcessing.TokensMap.Tokens.Add(special_tokens.ToDictionary(kv => kv.Key, kv => kv.Value._token));
            this.special_tokens_dict = special_tokens.ToDictionary(kv => kv.Key, kv => kv.Value).ToImmutableDictionary();
        }
    }
    public Template(string single = "$0", string pair = "$A:0 $B:1", UInt64 added_single = 0, UInt64 added_pair = 0, IEnumerable<Token>? special_tokens = null) :
        base(new Params
        {
            TemplateProcessing =
            {
                Single = single,
                Pair = pair,
                AddedSingle = added_single,
                AddedPair = added_pair,
            }
        })
    {
        this.single = single;
        this.pair = pair;
        this.added_single = added_single;
        this.added_pair = added_pair;
        if (special_tokens != null)
        {
            this._params.TemplateProcessing.Tokens.Tokens_.Add(special_tokens.Select(t => t._token));
            this.special_tokens = special_tokens.ToImmutableArray();
        }
    }
}
public class Sequence
{
    public static implicit operator ProcessorWrapperParams(Sequence p) => p._params;
    internal readonly ProcessorWrapperParams _params;
    public readonly IEnumerable<Processor> processors;
    public Sequence(IEnumerable<Processor> processors)
    {
        this.processors = processors;
        _params = new ProcessorWrapperParams
        {
            Params = { processors.Select(p => p._params) }
        };
    }
}