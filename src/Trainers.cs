using Google.Protobuf;

using Messages.Trainers;

namespace Tokenizers.Trainers;

public abstract class Trainer {
    internal TrainerParams.TrainerOneofCase trainer_type;
    internal IMessage message;
    internal Trainer(TrainerParams.TrainerOneofCase trainer_type, IMessage message)
    {
        this.trainer_type = trainer_type;
        this.message = message;
    }
}
public class Bpe : Trainer
{
    public readonly UInt64 min_frequency;
    public readonly UInt64 vocab_size;
    public readonly bool show_progress;
    public readonly IEnumerable<Messages.AddedToken> special_tokens;
    public readonly UInt64? limit_alphabet;
    public readonly string initial_alphabet;
    public readonly string? continuing_subword_prefix;
    public readonly string? end_of_word_suffix;
    public readonly UInt64? max_token_length;
    internal static BpeTrainer GetBpeTrainerParams(UInt64 min_frequency, UInt64 vocab_size, bool show_progress, IEnumerable<Messages.AddedToken>? special_tokens, UInt64? limit_alphabet, string initial_alphabet, string? continuing_subword_prefix, string? end_of_word_suffix, UInt64? max_token_length)
    {
        var r = new BpeTrainer()
        {
            MinFrequency = min_frequency,
            VocabSize = vocab_size,
            ShowProgress = show_progress,
            SpecialTokens = { special_tokens },
            InitialAlphabet = initial_alphabet,
        };
        if (limit_alphabet.HasValue)
            r.LimitAlphabet = limit_alphabet.Value;
        if (continuing_subword_prefix != null)
            r.ContinuingSubwordPrefix = continuing_subword_prefix;
        if (end_of_word_suffix != null)
            r.EndOfWordSuffix = end_of_word_suffix;
        if (max_token_length.HasValue)
            r.MaxTokenLength = max_token_length.Value;
        return r;
    }
    public Bpe(UInt64 min_frequency = 0, UInt64 vocab_size = 30_000, bool show_progress = true, IEnumerable<Messages.AddedToken>? special_tokens = null, UInt64? limit_alphabet = null, string initial_alphabet = "", string? continuing_subword_prefix = null, string? end_of_word_suffix = null, UInt64? max_token_length = null) :
        base(TrainerParams.TrainerOneofCase.BpeTrainer, GetBpeTrainerParams(min_frequency, vocab_size, show_progress, special_tokens, limit_alphabet, initial_alphabet, continuing_subword_prefix, end_of_word_suffix, max_token_length))
    {
        this.min_frequency = min_frequency;
        this.vocab_size = vocab_size;
        this.show_progress = show_progress;
        this.special_tokens = special_tokens ??  Enumerable.Empty<Messages.AddedToken>();
        this.limit_alphabet = limit_alphabet;
        this.initial_alphabet = initial_alphabet;
        this.continuing_subword_prefix = continuing_subword_prefix;
        this.end_of_word_suffix = end_of_word_suffix;
        this.max_token_length = max_token_length;
    }

}
public class WordPiece : Trainer
{
    public readonly UInt64 min_frequency;
    public readonly UInt64 vocab_size;
    public readonly bool show_progress;
    public readonly IEnumerable<Messages.AddedToken> special_tokens;
    public readonly UInt64? limit_alphabet;
    public readonly string initial_alphabet;
    public readonly string? continuing_subword_prefix;
    public readonly string? end_of_word_suffix;
    internal static WordPieceTrainer GetWordPieceTrainerParams(UInt64 min_frequency, UInt64 vocab_size, bool show_progress, IEnumerable<Messages.AddedToken>? special_tokens, UInt64? limit_alphabet, string initial_alphabet, string? continuing_subword_prefix, string? end_of_word_suffix)
    {
        var r = new WordPieceTrainer()
        {
            MinFrequency = min_frequency,
            VocabSize = vocab_size,
            ShowProgress = show_progress,
            SpecialTokens = { special_tokens },
            InitialAlphabet = initial_alphabet,
        };
        if (limit_alphabet.HasValue)
            r.LimitAlphabet = limit_alphabet.Value;
        if (continuing_subword_prefix != null)
            r.ContinuingSubwordPrefix = continuing_subword_prefix;
        if (end_of_word_suffix != null)
            r.EndOfWordSuffix = end_of_word_suffix;
        return r;
    }
    public WordPiece(UInt64 min_frequency = 0, UInt64 vocab_size = 30_000, bool show_progress = true, IEnumerable<Messages.AddedToken>? special_tokens = null, UInt64? limit_alphabet = null, string initial_alphabet = "", string? continuing_subword_prefix = null, string? end_of_word_suffix = null) :
        base(TrainerParams.TrainerOneofCase.WordPieceTrainer, GetWordPieceTrainerParams(min_frequency, vocab_size, show_progress, special_tokens, limit_alphabet, initial_alphabet, continuing_subword_prefix, end_of_word_suffix))
    {
        this.min_frequency = min_frequency;
        this.vocab_size = vocab_size;
        this.show_progress = show_progress;
        this.special_tokens = special_tokens ?? Enumerable.Empty<Messages.AddedToken>();
        this.limit_alphabet = limit_alphabet;
        this.initial_alphabet = initial_alphabet;
        this.continuing_subword_prefix = continuing_subword_prefix;
        this.end_of_word_suffix = end_of_word_suffix;
    }
}
public class WordLevel : Trainer
{
    public readonly UInt64 min_frequency;
    public readonly UInt64 vocab_size;
    public readonly bool show_progress;
    public readonly IEnumerable<Messages.AddedToken> special_tokens;
    public WordLevel(UInt64 min_frequency = 0, UInt64 vocab_size = 30_000, bool show_progress = true, IEnumerable<Messages.AddedToken>? special_tokens = null) :
        base(TrainerParams.TrainerOneofCase.WordLevelTrainer, new WordLevelTrainer()
        {
            MinFrequency = min_frequency,
            VocabSize = vocab_size,
            ShowProgress = show_progress,
            SpecialTokens = { special_tokens }
        })
    {
        this.min_frequency = min_frequency;
        this.vocab_size = vocab_size;
        this.show_progress = show_progress;
        this.special_tokens = special_tokens ?? Enumerable.Empty<Messages.AddedToken>();
    }
}
public class Unigram : Trainer
{
    public readonly bool show_progress;
    public readonly UInt32 vocab_size;
    public readonly UInt32 n_sub_iterations;
    public readonly double shrinking_factor;
    public readonly IEnumerable<Messages.AddedToken> special_tokens;
    public readonly string initial_alphabet;
    public readonly string? unk_token;
    public readonly UInt64 max_piece_length;
    internal static UnigramTrainer GetUnigramTrainerParams(bool show_progress, UInt32 vocab_size, UInt32 n_sub_iterations, double shrinking_factor, IEnumerable<Messages.AddedToken>? special_tokens, string initial_alphabet, string? unk_token, UInt64 max_piece_length)
    {
        var r = new UnigramTrainer()
        {
            ShowProgress = show_progress,
            VocabSize = vocab_size,
            NSubIterations = n_sub_iterations,
            ShrinkingFactor = shrinking_factor,
            SpecialTokens = { special_tokens },
            InitialAlphabet = initial_alphabet,
            MaxPieceLength = max_piece_length,
        };
        if (unk_token != null)
            r.UnkToken = unk_token;
        return r;
    }
    public Unigram(bool show_progress = true, UInt32 vocab_size = 8_000, UInt32 n_sub_iterations = 2, double shrinking_factor = 0.75, IEnumerable<Messages.AddedToken>? special_tokens = null, string initial_alphabet = "", string? unk_token = null, UInt64 max_piece_length = 16) :
        base(TrainerParams.TrainerOneofCase.UnigramTrainer, GetUnigramTrainerParams(show_progress, vocab_size, n_sub_iterations, shrinking_factor, special_tokens, initial_alphabet, unk_token, max_piece_length))
    {
        this.show_progress = show_progress;
        this.vocab_size = vocab_size;
        this.n_sub_iterations = n_sub_iterations;
        this.shrinking_factor = shrinking_factor;
        this.special_tokens = special_tokens ?? Enumerable.Empty<Messages.AddedToken>();
        this.initial_alphabet = initial_alphabet;
        this.unk_token = unk_token;
        this.max_piece_length = max_piece_length;
    }
}

