namespace PulseWord.Core.Entities;

public enum GameStatus
{
    Open,
    Closed
}

public enum GameResult
{
    Win,
    Fail,
    InProgress
}

public enum LetterFeedback
{
    CorrectPosition,
    CorrectLetterWrongPosition,
    Absent
}
