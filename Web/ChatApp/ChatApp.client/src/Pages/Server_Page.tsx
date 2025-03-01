import { useEffect } from "react";
import { ChannelType, MemberResponseDto } from "../Common/Dtos";
import Server_SideMenu from "../Components/Server/SideMenu/Server_SideMenu";
import Chat_Message_Title from "../Components/Shared/Chat/Chat_Message_Title";
import Chat_Messages_Viewer from "../Components/Shared/Chat/Chat_Messages_Viewer";
import Loading_Spinner from "../Components/Shared/Loading_Spin";
import VideoChat from "../Components/Shared/VideoChat/VideoChat";
import { useCommonContext } from "../Contexts/Common_Context";
import { useItemQuery } from "../Hooks/Querys/use-item-query";

function Server_Page() {
  const { dispatch, globalData } = useCommonContext();
  const { Server, Channel } = globalData;

  const {
    data: Member,
    isLoading,
    isError,
  } = useItemQuery<MemberResponseDto>({
    queryKey: `/Servers/${Server?.id}/members/@me`,
    queryUrl: `/Servers/${Server?.id}/members/@me`,
    enabled: !!Server?.id,
  });

  useEffect(() => {
    if (Member) {
      dispatch({ type: "ADD_MEMBER", payload: Member });
    }
  }, [dispatch, isLoading, Member]);

  if (isLoading) {
    return (
      <div className="flex flex-1 justify-center items-center h-full bg-gray-800 w-full ">
        <Loading_Spinner />
      </div>
    );
  }

  if (!Server || !Member || isError) {
    return (
      <div className="flex flex-1 justify-center items-center h-full bg-gray-800 w-full ">
        <p>Something went wrong!</p>
      </div>
    );
  }

  return (
    <div className="flex flex-1 bg-gray-800">
      <Server_SideMenu Server={Server} Member={Member} />
      {Channel?.id ? (
        <div className="relative flex flex-1 flex-col">
          <Chat_Message_Title
            ChatName={Channel?.name}
            Channel={Channel}
            Type="Channel"
          />
          {Channel.type === ChannelType.TEXT ? (
            <Chat_Messages_Viewer
              MessagequeryUrl={`/Channels/${Channel?.id}/Messages`}
              chatKey={`${Channel?.id}:message`}
              Type="Channel"
              Member={Member}
            />
          ) : (
            <VideoChat
              channelId={Channel.id}
              isAudioCall={Channel.type === ChannelType.AUDIO}
            />
          )}
        </div>
      ) : (
        <div className="flex-1 flex items-center justify-center text-gray-400">
          <p>Select a channel to view the messages.</p>
        </div>
      )}
    </div>
  );
}

export default Server_Page;
