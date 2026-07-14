import { AuthenticatedUserResponse } from "./authenticated-user-response.model";

export interface AuthSession {
  accessToken: string;
  expiresAtUtc: string;
  user: AuthenticatedUserResponse;
}