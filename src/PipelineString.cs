using System.Runtime.InteropServices;

using Tokenizers.HuggingFace.Internal;
using Tokenizers.HuggingFace.Internal.PipelineString;

namespace Tokenizers.HuggingFace.PipelineString;

internal static class PipelineStringForeignFunctions
{
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 get_splits(nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern Int32 new_pipeline_string(out nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);

    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_pipeline_string(nuint instance_ptr);
}
/// <summary>
/// A string passed to represent transformations done by: <see cref="Normalizers.Normalizer.Normalize(PipelineString)"/> and <see cref="PreTokenizers.PreTokenizer.PreTokenize(PipelineString)"/>
/// </summary>
public class PipelineString : ForeignInstance
{
    /// <inheritdoc cref="PipelineString"/>
    /// <param name="original">Initial string without any transforamation</param>
    public PipelineString(string original)
    {
        InstancePtr = ForeignFunctions.CreateNewArgsResult(
            PipelineStringForeignFunctions.new_pipeline_string,
            new PipelineStringParams { Content = original }
        );
    }
    /// <summary>
    /// Use this method to see the current state of the <see cref="PipelineString"/> after transforamtions
    /// </summary>
    /// <param name="offsetReferential"></param>
    /// <param name="offsetType"></param>
    /// <param name="includeOffsets">An option to configure if you need the Offset "(UInt64, UInt64)?".<br/>
    /// If set to false this should be a little faster.
    /// </param>
    /// <returns>
    /// A collection of splits (string, (UInt64, UInt64)?), each of them being a slice of the normalized.<br/>
    /// The string represents a part of the original string.<br/>
    /// The (UInt64, UInt64)? represents an optional offset, this will be null if <paramref name="includeOffsets"/> is false
    /// </returns>
    public IEnumerable<(string, (UInt64, UInt64)?)> GetSplits(OffsetReferential offsetReferential, OffsetType offsetType, bool includeOffsets)
    {
        var r = ForeignFunctions.MethodArgsResult(
            PipelineStringForeignFunctions.get_splits,
            this.InstancePtr,
            new SplitParams
            {
                OffsetReferential = offsetReferential,
                OffsetType = offsetType,
                IncludeOffsets = includeOffsets
            },
            SplitResult.Parser
        );
        return includeOffsets
            ? r.Tokens.Zip(r.Offsets, static (token, offset) => (token, ((UInt64, UInt64)?)(offset.Start, offset.End)))
            : r.Tokens.Select(static t => (t, ((UInt64, UInt64)?)null));
    }

    internal override FreeDelegate FreeFunc() =>
        PipelineStringForeignFunctions.free_pipeline_string;
}