import { logError } from "../Common/Logger";

export interface AuthInfo {
  token: string;
}

export const SetAuthInfo = (authInfo: AuthInfo): void => {
  try {
    localStorage.setItem("AuthInfo", JSON.stringify(authInfo));
  } catch (err) {
    logError(err);
  }
};

export const GetAuthInfo = (): AuthInfo | null => {
  try {
    const item = localStorage.getItem("AuthInfo");
    return item ? (JSON.parse(item) as AuthInfo) : null;
  } catch (error) {
    logError(error);
    return null;
  }
};

export const RemoveAuthInfo = (): void => {
  localStorage.removeItem("AuthInfo");
};

export const RemoveAuth = (): void => {
  RemoveAuthInfo();
  window.location.href = "/login";
};

export const IsLoggedIn = (): boolean => {
  return GetAuthInfo() !== null;
};

export const GetAuthHeader = (): Record<string, string> => {
  const authInfo = GetAuthInfo();
  return authInfo ? { Authorization: `Bearer ${authInfo.token}` } : {};
};
