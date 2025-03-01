import { ReactNode, useEffect } from "react";

import { AxiosError } from "axios";
import { Api } from "../../Common/axios";
import { RemoveAuth } from "../../Auth/Profile";

interface Axios_Error_Handler_Props {
  children?: ReactNode;
}

const Axios_Error_Handler = ({ children }: Axios_Error_Handler_Props) => {
  useEffect(() => {
    const responseInterceptor = Api.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        if (error) {
          if (error.status === 401) {
            RemoveAuth();
          }
        } 
      }
    );

    return () => {
      Api.interceptors.response.eject(responseInterceptor);
    };
  }, []);

  return <>{children}</>;
};

export default Axios_Error_Handler;
