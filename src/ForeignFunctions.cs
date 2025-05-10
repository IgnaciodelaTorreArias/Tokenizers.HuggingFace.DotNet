using System.Runtime.InteropServices;

using Google.Protobuf;

using Messages;

namespace Tokenizers.HuggingFace;

internal sealed class InvalidProtocolBufferException : IOException { }

internal sealed class NormalizationException : Exception
{
    internal NormalizationException(string details) : base(details) { }
}
internal sealed class PreTokenizationException : Exception
{
    internal PreTokenizationException(string details) : base(details) { }
}
internal sealed class TokenizerBuildException : Exception
{
    internal TokenizerBuildException(string details) : base(details) { }
}
internal sealed class TokenizerTrainingException : Exception
{
    internal TokenizerTrainingException(string details) : base(details) { }
}
internal sealed class TokenizerSaveException : Exception
{
    internal TokenizerSaveException(string details) : base(details) { }
}
internal sealed class TokenizerLoadFileException : Exception
{
    internal TokenizerLoadFileException(string details) : base(details) { }
}
internal sealed class TokenizerEncodingException : Exception
{
    internal TokenizerEncodingException(string details) : base(details) { }
}
internal sealed class TokenizerDecodingException : Exception
{
    internal TokenizerDecodingException(string details) : base(details) { }
}

internal static partial class ForeignFunctions
{
    [DllImport("tokenizers_proto", CallingConvention = CallingConvention.Cdecl)]
    static extern void free_buffer(nuint ptr, nuint len);
    /// <summary>
    /// Filters foreign function errors and throws the corresponding C# exception.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="buf"></param>
    /// <param name="ptr"></param>
    /// <param name="len"></param>
    /// <exception cref="InvalidProtocolBufferException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NormalizationException"></exception>
    /// <exception cref="PreTokenizationException"></exception>
    /// <exception cref="TokenizerBuildException"></exception>
    /// <exception cref="TokenizerTrainingException"></exception>
    /// <exception cref="TokenizerSaveException"></exception>
    /// <exception cref="TokenizerLoadFileException"></exception>
    /// <exception cref="TokenizerEncodingException"></exception>
    /// <exception cref="TokenizerDecodingException"></exception>
    /// <exception cref="Exception"></exception>
    static void ThrowErrors(Int32 status, Span<byte> buf, nuint ptr, nuint len)
    {
        var call_status = (CallStatus)status;
        var details = "";
        if (status > 0)
        {
            Error err = new Error();
            if (len > 0)
            {
                err = Error.Parser.ParseFrom(buf);
                free_buffer(ptr, len);
            }
            details = err.Details;
        }
        switch (call_status)
        {
            case CallStatus.Ok:
                return;
            case CallStatus.DecodeError:
                throw new InvalidProtocolBufferException();
            case CallStatus.InvalidArgumentsDetails:
                throw new ArgumentException(details);
            case CallStatus.InvalidArguments:
                throw new ArgumentException();
            case CallStatus.UnknownEnumValue:
                throw new ArgumentOutOfRangeException("Not a valid Enum Value");
            case CallStatus.EmptyParams:
                throw new ArgumentNullException("A requiered field is not present");
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

    public delegate Int32 _CreateNewArgsResultDelegate(out nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    public static nuint CreateNewArgsResult<I>(_CreateNewArgsResultDelegate func, I request)
        where I : IMessage<I>
    {
        byte[] buf = request.ToByteArray();
        unsafe
        {
            fixed (byte* ptr = buf)
            {
                Int32 result = func(
                    out nuint instance_ptr,
                    (nuint)ptr,
                    (nuint)buf.Length,
                    out nuint outPtr,
                    out nuint outLen
                );
                Span<byte> span = new((void*)outPtr, (int)outLen);
                ThrowErrors(result, span, outPtr, outLen);
                // We don't free the outPtr here, because it's only used to pass error details, managed by ThrowErrors
                return instance_ptr;
            }
        }
    }

    public delegate Int32 _MethodFuncArgsResultDelegate(nuint instance_ptr, nuint ptr, nuint len, out nuint outPtr, out nuint outLen);
    public static R MethodArgsResult<R, I>(_MethodFuncArgsResultDelegate func, nuint instance_ptr, I request, MessageParser<R> Parser)
        where I : IMessage<I>
        where R : IMessage<R>
    {
        byte[] buf = request.ToByteArray();
        unsafe
        {
            fixed (byte* ptr = buf)
            {
                Int32 result = func(
                    instance_ptr,
                    (nuint)ptr,
                    (nuint)buf.Length,
                    out nuint outPtr,
                    out nuint outLen
                );
                Span<byte> span = new((void*)outPtr, (int)outLen);
                ThrowErrors(result, span, outPtr, outLen);
                var res = Parser.ParseFrom(span);
                free_buffer(outPtr, outLen);
                return res;
            }
        }
    }
}

public abstract class ForeignInstance
{
    internal nuint InstancePtr;
    internal delegate void FreeDelegate(nuint instance_ptr);
    internal abstract FreeDelegate FreeFunc();
    internal ForeignInstance()
    {
        InstancePtr = 0;
    }
    ~ForeignInstance()
    {
        FreeFunc()(InstancePtr);
    }
}