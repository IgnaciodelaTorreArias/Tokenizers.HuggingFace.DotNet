using Google.Protobuf.Collections;

using Messages.Processors;

namespace DotNet.HuggingFace.Tokenizers.Processors;

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
public class  ByteLevel : Processor
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
public class Template : Processor
{
    public readonly string single;
    public readonly string pair;
    public readonly UInt64 added_single;
    public readonly UInt64 added_pair;
    public readonly IEnumerable<(string, UInt32)> special_tokens;
    internal static MapField<string, UInt32> GetSpecialtokens(IEnumerable<(string, UInt32)>? special_tokens)
    {
        if (special_tokens == null)
            return new MapField<string, UInt32>();
        MapField<string, UInt32> m = new();
        foreach (var token in special_tokens)
        {
            m.Add(token.Item1, token.Item2);
        }
        return m;
    }
    public Template(string single = "$0", string pair = "$A:0 $B:1", UInt64 added_single = 0, UInt64 added_pair = 0, IEnumerable<(string, UInt32)>? special_tokens = null) :
        base(new Params
        {
            TemplateProcessing =
            {
                Single = single,
                Pair = pair,
                AddedSingle = added_single,
                AddedPair = added_pair,
                SpecialTokens = { GetSpecialtokens(special_tokens) }
            }
        })
    {
        this.single = single;
        this.pair = pair;
        this.added_single = added_single;
        this.added_pair = added_pair;
        this.special_tokens = special_tokens ?? Enumerable.Empty<(string, UInt32)>();
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