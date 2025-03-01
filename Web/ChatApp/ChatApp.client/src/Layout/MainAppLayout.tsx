import { useEffect } from "react";
import { CiServer } from "react-icons/ci";
import { Outlet } from "react-router-dom";
import { ProfileResponseDto } from "../Common/Dtos";
import DialogsProvider from "../Components/Dialogs/Dialogs_Provider";
import Servers_Navigation_Menu from "../Components/Server/Servers_Navigation_Menu";
import User_Account from "../Components/Shared/User_Account";
import { useCommonContext } from "../Contexts/Common_Context";
import { useItemQuery } from "../Hooks/Querys/use-item-query";

function MainAppLayout() {
  const { IsAppServerDown, dispatch } = useCommonContext();

  const { data, isLoading } = useItemQuery<ProfileResponseDto>({
    queryKey: `/Profile/@me`,
    queryUrl: `/Profile/@me`,
  });

  useEffect(() => {
    if (data) {
      dispatch({ type: "ADD_PROFILE", payload: data });
    }
  }, [dispatch, isLoading, data]);

  return (
    <div className="flex flex-1 h-screen w-full bg-gray-800 text-gray-200 select-none">
      {!IsAppServerDown || isLoading ? (
        <>
          <DialogsProvider />
          <User_Account />
          <Servers_Navigation_Menu />
          <Outlet />
        </>
      ) : (
        <div className="flex items-center justify-center w-full h-full text-xl flex-col space-y-4">
          <CiServer className="text-red-500 text-4xl animate-bounce" />
          <span className="text-white animate-pulse text-center">
            Trying to connect to the server, please wait...
          </span>
        </div>
      )}
    </div>
  );
}

export default MainAppLayout;
