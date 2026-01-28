export enum LetterFeedback {
  None = 0,
  Incorrect = 1,
  WrongLocation = 2,
  Correct = 3
}

export interface UserProfileDto {
  id: string;
  userName: string;
  displayName: string;
  avatarUrl?: string;
  isAnonymous: boolean;
  totalGames: number;
  wins: number;
  applauseCount: number;
}

export interface GuessResponseDto {
  guess: string;
  feedback: LetterFeedback[];
  isCorrect: boolean;
}

export interface PlayerGameDto {
  id: string;
  userId: string;
  username: string;
  avatarUrl?: string;
  guesses: GuessResponseDto[];
  timeTakenMs: number;
  score: number;
  completedAt: string;
  applauseCount: number;
  rank?: number;
}

export interface LeaderboardEntryDto {
  rank: number;
  playerId: string;
  username: string;
  avatarUrl?: string;
  score: number;
  timeTakenMs: number;
  guessCount: number;
  applauseCount: number;
  isCurrentUser: boolean;
}

export interface DailyGameDto {
  id: string;
  date: string;
  wordLength: number;
  maxGuesses: number;
  leaderboard: LeaderboardEntryDto[];
}
