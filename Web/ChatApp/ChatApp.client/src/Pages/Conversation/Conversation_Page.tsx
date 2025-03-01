import { GetFileUrl } from "../../Common/GetMediaUrl";
import Chat_Message_Title from "../../Components/Shared/Chat/Chat_Message_Title";
import Chat_Messages_Viewer from "../../Components/Shared/Chat/Chat_Messages_Viewer";
import { useCommonContext } from "../../Contexts/Common_Context";

function Conversation_Page() {
  const { globalData } = useCommonContext();
  const { Conversation, Profile } = globalData;

  return (
    <div className="flex flex-col flex-1 ">
      {Conversation && (
        <>
          <Chat_Message_Title
            ChatName={Conversation?.targetProfile?.displayName}
            AvatarUrl={GetFileUrl(Conversation?.targetProfile?.imageId)}
            Type="DirectMessage"
            Profile={Conversation?.targetProfile}
          />
          <Chat_Messages_Viewer
            chatKey={`${Conversation?.channelId}:message`}
            MessagequeryUrl={`/Channels/${Conversation?.channelId}/Messages`}
            Type="DirectMessage"
            Profile={Profile}
          />
        </>
      )}
    </div>
  );
}

export default Conversation_Page;
