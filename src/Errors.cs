using static System.Net.WebRequestMethods;

namespace Tokenizers.HuggingFace.Errors;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public sealed class InvalidBufferException : Exception { }
public sealed class InvalidPointerException : Exception {
    internal InvalidPointerException(string details) : base(details) { }
}
public sealed class NormalizationException : Exception
{
    internal NormalizationException(string details) : base(details) { }
}
public sealed class PreTokenizationException : Exception
{
    internal PreTokenizationException(string details) : base(details) { }
}
public sealed class TokenizerBuildException : Exception
{
    internal TokenizerBuildException(string details) : base(details) { }
}
public sealed class TokenizerTrainingException : Exception
{
    internal TokenizerTrainingException(string details) : base(details) { }
}
public sealed class TokenizerSaveException : Exception
{
    internal TokenizerSaveException(string details) : base(details) { }
}
public sealed class TokenizerLoadFileException : Exception
{
    internal TokenizerLoadFileException(string details) : base(details) { }
}
public sealed class TokenizerEncodingException : Exception
{
    internal TokenizerEncodingException(string details) : base(details) { }
}
public sealed class TokenizerDecodingException : Exception
{
    internal TokenizerDecodingException(string details) : base(details) { }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member