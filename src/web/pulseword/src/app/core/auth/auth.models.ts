export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  username: string;
  isAnonymous: boolean;
  roles: string[];
}

export interface LoginRequest {
  username: string;
  password?: string;
}

export interface AnonymousRequest {
  displayName: string;
}

export interface TokenPayload {
  sub: string;
  exp: number;
  username: string;
  isAnonymous: boolean;
}
