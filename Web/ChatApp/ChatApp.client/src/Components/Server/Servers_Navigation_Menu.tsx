import { Fragment, useEffect, useMemo, useRef } from "react";
import { BiPlus } from "react-icons/bi";
import { TbMessageChatbot } from "react-icons/tb";
import { NavLink, useParams } from "react-router-dom";
import { ServerResponseDto } from "../../Common/Dtos";
import { GetFileUrl } from "../../Common/GetMediaUrl";
import { useCommonContext } from "../../Contexts/Common_Context";
import { useDialogContext } from "../../Contexts/Dialog_Context";
import { useListQuery } from "../../Hooks/Querys/use-list-query";
import { useInfiniteScroll } from "../../Hooks/use-Infinite-scroll";
import Avatar_Icon from "../Shared/Avatar_Icon";
import Loading_Spinner from "../Shared/Loading_Spin";

function Servers_Navigation_Menu() {
  const { serverId } = useParams<{
    serverId?: string;
  }>();

  const { dispatch, isSideBarOpened } = useCommonContext();
  const { openDialog } = useDialogContext();
  const { data, isLoading, ...queryDetails } = useListQuery<ServerResponseDto>({
    queryKey: `/Profile/Servers`,
    queryUrl: `/Profile/Servers`,
    SocketKey: `server`,
    around: serverId,
  });
  const ContainerRef = useRef(null);
  useInfiniteScroll({
    ContainerRef,
    ...queryDetails,
  });

  const selecteditem = useMemo(() => {
    if (!serverId || !data?.pages) return null;

    for (const page of data.pages) {
      const found = page.find((x) => x.id === serverId);
      if (found) return found;
    }

    return null;
  }, [serverId, data?.pages]);

  useEffect(() => {
    if (serverId) {
      const activeItem = document.getElementById(serverId);

      if (activeItem) {
        activeItem.scrollIntoView({
          behavior: "instant",
          block: "nearest",
        });
      }
    }
    dispatch({ type: "ADD_SERVER", payload: selecteditem });

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [serverId, selecteditem]);

  useEffect(() => {
    if (!serverId) return;

    const activeItem = document.getElementById(serverId);

    if (activeItem) {
      activeItem.scrollIntoView({
        behavior: "instant",
        block: "nearest",
      });
    }
  }, [isSideBarOpened, serverId]);

  return (
    <div
      className={`  ${
        isSideBarOpened ? "flex" : "hidden  md:flex"
      }  flex-col items-center w-16 bg-gray-900 py-4 space-y-4 h-full `}
    >
      <NavLink
        to={"/"}
        title="Direct Messages"
        className="w-12 bg-blue-500 rounded-2xl hover:bg-blue-400 cursor-pointer relative flex justify-center items-center text-white"
      >
        <TbMessageChatbot className="text-white text-2xl h-12" />
      </NavLink>

      <div className="w-12 h-px bg-gray-700" />

      <div
        ref={ContainerRef}
        className="flex flex-col items-center space-y-4 overflow-y-auto no-scrollbar h-full w-full"
      >
        {isLoading ? (
          <Loading_Spinner />
        ) : (
          data?.pages.map((group, index) => (
            <Fragment key={index}>
              {group.map((server, index) => (
                <div className="w-full" key={index} id={server.id}>
                  <NavLink
                    to={`/server/${server?.id}`}
                    className={({ isActive }) =>
                      `relative flex items-center justify-center px-2 transition-all ${
                        isActive &&
                        `
                       before:content-['']
                       before:absolute 
                       before:left-0
                       before:top-1/2 
                       before:-translate-y-1/2
                       before:w-1 
                       before:h-10 
                       before:bg-blue-500 
                       before:rounded-full`
                      }`
                    }
                  >
                    <Avatar_Icon
                      userName={server?.name}
                      avatarUrl={GetFileUrl(server?.imageId)}
                    />
                  </NavLink>
                </div>
              ))}
            </Fragment>
          ))
        )}

        <div className="w-12 flex bg-blue-500 rounded-full items-center justify-center hover:bg-blue-400 cursor-pointer relative">
          <BiPlus
            className="text-white text-2xl h-12"
            title="Add Server"
            onClick={() => openDialog("CreateServer")}
          />
        </div>
      </div>
    </div>
  );
}

export default Servers_Navigation_Menu;
