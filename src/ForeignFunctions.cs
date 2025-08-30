using System.Runtime.InteropServices;

using Google.Protobuf;

using Tokenizers.HuggingFace.Errors;
using Tokenizers.HuggingFace.Internal.Errors;

namespace Tokenizers.HuggingFace.Internal;

internal static partial class ForeignFunctions
{
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    static extern void free_buffer(nuint ptr, nuint len);

    static void ThrowErrors(Int32 status, Span<byte> buf, nuint ptr, nuint len)
    {
        var callStatus = (CallStatus)status;
        var details = "";
        if (status > 0)
        {
            Error err = new();
            err = Error.Parser.ParseFrom(buf);
            free_buffer(ptr, len);
            details = err.Details;
        }
        switch (callStatus)
        {
            case CallStatus.Ok:
                return;
            case CallStatus.DecodeError:
                throw new InvalidBufferException();
            case CallStatus.InvalidArgumentsDetails:
                throw new ArgumentException(details);
            case CallStatus.InvalidArguments:
                throw new ArgumentException();
            case CallStatus.UnknownEnumValue:
                throw new ArgumentOutOfRangeException("Not a valid Enum Value");
            case CallStatus.EmptyParams:
                throw new ArgumentNullException("A requiered field is not present");
            case CallStatus.InvalidPointerDetails:
                throw new InvalidPointerException(details);
            case CallStatus.NormalizationErrorDetails:
                throw new NormalizationException(details);
            case CallStatus.PreTokenizationErrorDetails:
                throw new PreTokenizationException(details);
            case CallStatus.TokenizerBuildErrorDetails:
                throw new TokenizerBuildException(details);
            case CallStatus.TokenizerTrainingErrorDetails:
                throw new TokenizerTrainingException(details);
            case CallStatus.TokenizerSaveErrorDetails:
                throw new TokenizerSaveException(details);
            case CallStatus.TokenizerLoadFileErrorDetails:
                throw new TokenizerLoadFileException(details);
            case CallStatus.TokenizerEncodingErrorDetails:
                throw new TokenizerEncodingException(details);
            case CallStatus.TokenizerDecodingErrorDetails:
                throw new TokenizerDecodingException(details);
        }
        if (status > 0)
            throw new Exception($"Unknown error occurred, code: {status}, details: {details}");
        else
            throw new Exception($"Unknown error occurred, code: {status}");
    }

    internal delegate Int32 _CreateNewArgsResultDelegate(out nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    internal static nuint CreateNewArgsResult<I>(_CreateNewArgsResultDelegate func, I request)
        where I : IMessage<I>
    {
        byte[] buf = request.ToByteArray();
        unsafe
        {
            fixed (byte* ptr = buf)
            {
                Int32 result = func(
                    out nuint instancePtr,
                    (nuint)ptr,
                    (nuint)buf.Length,
                    out nuint outPtr,
                    out nuint outLen
                );
                Span<byte> span = new((void*)outPtr, (int)outLen);
                ThrowErrors(result, span, outPtr, outLen);
                // We don't free the outPtr here, because it's only used to pass error details, managed by ThrowErrors
                return instancePtr;
            }
        }
    }

    internal delegate Int32 _MethodFuncArgsResultDelegate(nuint instancePtr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    internal static R MethodArgsResult<R, I>(_MethodFuncArgsResultDelegate func, nuint instancePtr, I request, MessageParser<R> parser)
        where I : IMessage<I>
        where R : IMessage<R>
    {
        byte[] buf = request.ToByteArray();
        unsafe
        {
            fixed (byte* ptr = buf)
            {
                Int32 result = func(
                    instancePtr,
                    (nuint)ptr,
                    (nuint)buf.Length,
                    out nuint outPtr,
                    out nuint outLen
                );
                Span<byte> span = new((void*)outPtr, (int)outLen);
                ThrowErrors(result, span, outPtr, outLen);
                var res = parser.ParseFrom(span);
                free_buffer(outPtr, outLen);
                return res;
            }
        }
    }
    internal static void MethodArgsResult<I>(_MethodFuncArgsResultDelegate func, nuint instancePtr, I request)
        where I : IMessage<I>
    {
        byte[] buf = request.ToByteArray();
        unsafe
        {
            fixed (byte* ptr = buf)
            {
                Int32 result = func(
                    instancePtr,
                    (nuint)ptr,
                    (nuint)buf.Length,
                    out nuint outPtr,
                    out nuint outLen
                );
                Span<byte> span = new((void*)outPtr, (int)outLen);
                ThrowErrors(result, span, outPtr, outLen);
                // We don't free the outPtr here, because it's only used to pass error details, managed by ThrowErrors
            }
        }
    }
}
/// <summary>
/// The base class for "managed instances". Which internally are managed by our rust dynamic library.
/// Current managed instances are:
/// <list type="bullet">
/// <item><see cref="HuggingFace.Normalizers.Normalizer"/></item>
/// <item><see cref="HuggingFace.PreTokenizers.PreTokenizer"/></item>
/// <item><see cref="HuggingFace.PipelineString.PipelineString"/></item>
/// <item><see cref="HuggingFace.Tokenizer.Tokenizer"/></item>
/// </list>
/// </summary>
public abstract class ForeignInstance
{
    internal nuint InstancePtr;
    internal delegate void FreeDelegate(nuint instancePtr);
    internal abstract FreeDelegate FreeFunc();
    internal ForeignInstance()
    {
        InstancePtr = 0;
    }
    /// <summary>
    /// Every managed instance must implement <see cref="FreeFunc"/> this destructure uses the callback function to free the instance internally.
    /// </summary>
    ~ForeignInstance()
    {
        FreeFunc()(InstancePtr);
    }
}