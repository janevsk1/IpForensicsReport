import { AuthSession } from "./auth-session.model";

export interface LoginResponse extends AuthSession {
  tokenType: string;
}