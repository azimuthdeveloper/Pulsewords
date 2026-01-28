using PulseWord.Core.Entities;

namespace PulseWord.Core.Services;

public interface IWordleService
{
    LetterFeedback[] CalculateFeedback(string guess, string secretWord);
    bool IsValidWord(string word);
    string GetDailyWord(DateOnly date);
}
