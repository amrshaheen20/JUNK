import { Navigate, useLocation } from "react-router-dom";
import { GetAuthInfo } from "../../Auth/Profile";
import { useCommonContext } from "../../Contexts/Common_Context";
import { ReactNode } from "react";

interface Require_Auth_Props {
  children: ReactNode; 
}

export default function Require_Auth({ children }: Require_Auth_Props) {
  const { Auth } = useCommonContext();
  const CurrentUser = GetAuthInfo();

  const location = useLocation();
  if (!CurrentUser && !Auth) {
    return <Navigate to="/login" state={{ path: location.pathname }} />;
  }
  return <>{children}</>;
}