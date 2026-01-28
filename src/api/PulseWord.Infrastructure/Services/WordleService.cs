using System;
using System.Collections.Generic;
using System.Linq;
using PulseWord.Core.Entities;
using PulseWord.Core.Services;
using PulseWord.Infrastructure.Data;

namespace PulseWord.Infrastructure.Services;

public class WordleService : IWordleService
{
    public LetterFeedback[] CalculateFeedback(string guess, string secretWord)
    {
        if (guess.Length != 5 || secretWord.Length != 5)
        {
            throw new ArgumentException("Words must be 5 letters long.");
        }

        guess = guess.ToUpperInvariant();
        secretWord = secretWord.ToUpperInvariant();

        var feedback = new LetterFeedback[5];
        var secretLetterCounts = new Dictionary<char, int>();

        // Initialize feedback with Absent and count letters in secret word
        for (int i = 0; i < 5; i++)
        {
            feedback[i] = LetterFeedback.Absent;
            char s = secretWord[i];
            secretLetterCounts[s] = secretLetterCounts.GetValueOrDefault(s) + 1;
        }

        // First pass: Correct positions
        for (int i = 0; i < 5; i++)
        {
            if (guess[i] == secretWord[i])
            {
                feedback[i] = LetterFeedback.CorrectPosition;
                secretLetterCounts[guess[i]]--;
            }
        }

        // Second pass: Correct letters in wrong positions
        for (int i = 0; i < 5; i++)
        {
            if (feedback[i] != LetterFeedback.CorrectPosition)
            {
                char g = guess[i];
                if (secretLetterCounts.GetValueOrDefault(g) > 0)
                {
                    feedback[i] = LetterFeedback.CorrectLetterWrongPosition;
                    secretLetterCounts[g]--;
                }
            }
        }

        return feedback;
    }

    public bool IsValidWord(string word)
    {
        return WordList.IsValidWord(word);
    }

    public string GetDailyWord(DateOnly date)
    {
        // Use a deterministic seed based on the date
        int seed = date.Year * 10000 + date.Month * 100 + date.Day;
        return GetWordBySeed(seed);
    }

    private string GetWordBySeed(int seed)
    {
        return WordList.GetWordBySeed(seed);
    }
}
