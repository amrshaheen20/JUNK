import { useEffect, useMemo, useRef } from "react";
import { IoMdSettings } from "react-icons/io";
import { NavLink, useParams } from "react-router-dom";
import {
  ChannelResponseDto,
  MemberResponseDto,
  MemberRole,
  ServerResponseDto,
} from "../../../Common/Dtos";
import { useCommonContext } from "../../../Contexts/Common_Context";
import { useDialogContext } from "../../../Contexts/Dialog_Context";
import { useListQuery } from "../../../Hooks/Querys/use-list-query";
import { useInfiniteScroll } from "../../../Hooks/use-Infinite-scroll";
import Channel_Icon from "../../Shared/Channel_Icon";
import IconButtonWithText from "../../Shared/IconButtonWithText";
import Loading_Spinner from "../../Shared/Loading_Spin";

interface Server_SideMenu_Channels_Props {
  Server: ServerResponseDto;
  Member: MemberResponseDto;
}

function Server_SideMenu_Channels({
  Server,
  Member,
}: Server_SideMenu_Channels_Props) {
  const { serverId, channelId } = useParams<{
    serverId?: string;
    channelId?: string;
  }>();

  const { openDialog } = useDialogContext();
  const { dispatch } = useCommonContext();

  const IsAdmin =
    Member.role === MemberRole.ADMIN || Member.role === MemberRole.MODERATOR;

  const { data, isLoading, ...queryDetails } = useListQuery<ChannelResponseDto>({
    queryKey: `/servers/${serverId}/channels`,
    queryUrl: `/servers/${serverId}/channels`,
    SocketKey: `${serverId}:channel`,
    enabled: !!serverId,
    around: channelId,
  });

  const ContainerRef = useRef(null);

  useInfiniteScroll({
    ContainerRef,
    ...queryDetails,
  });

  const selecteditem = useMemo(() => {
    if (!channelId || !data?.pages) return null;

    for (const page of data.pages) {
      const found = page.find((x) => x.id === channelId);
      if (found) return found;
    }

    return null;
  }, [channelId, data?.pages]);

  useEffect(() => {
    if (channelId) {
      const activeItem = document.getElementById(channelId);
      if (activeItem) {
        activeItem.scrollIntoView({
          behavior: "instant",
          block: "nearest",
        });
      }
    }
    dispatch({ type: "ADD_CHANNEL", payload: selecteditem });
  }, [channelId, dispatch, selecteditem]);

  if (isLoading)
    return (
      <div className="w-full flex flex-1 justify-center items-center">
        <Loading_Spinner />
      </div>
    );

  return (
    <div
      ref={ContainerRef}
      className="flex flex-1 flex-col py-4 px-2 w-full overflow-y-auto no-scrollbar-hover"
    >
      {data?.pages.flatMap((group) =>
        group.map((channel) => (
          <div key={channel.id} className="w-full relative" id={channel.id}>
            <NavLink
              to={`/server/${serverId}/${channel.id}`}
              className={({ isActive }) =>
                `group/button m-1 flex items-center rounded-lg transition
            ${isActive ? "bg-gray-600" : "hover:bg-gray-600"}`
              }
            >
              <IconButtonWithText
                icon={
                  <Channel_Icon Channel={channel} className="text-gray-400" />
                }
                label={channel.name}
              >
                <div
                  hidden={!IsAdmin}
                  onClick={() =>
                    openDialog("EditChannel", { channel, server: Server })
                  }
                  className="text-xl md:opacity-0 group-hover/button:opacity-100"
                >
                  <IoMdSettings />
                </div>
              </IconButtonWithText>
            </NavLink>
          </div>
        ))
      )}
    </div>
  );
}

export default Server_SideMenu_Channels;
