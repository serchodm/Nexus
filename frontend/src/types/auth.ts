export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  avatarUrl?: string;
  role: string;
  isMfaEnabled: boolean;
}

export interface AuthSuccessResponse {
  accessToken: string;
  refreshToken?: string | null;
  expiresIn?: number;
  user: UserProfile;
}

export interface MfaRequiredResponse {
  mfaRequired: true;
  mfaToken: string; // Token temporal con validez de 5 minutos (scope: mfa_pending)
  message?: string;
}

export type AuthResponse = AuthSuccessResponse | MfaRequiredResponse;
