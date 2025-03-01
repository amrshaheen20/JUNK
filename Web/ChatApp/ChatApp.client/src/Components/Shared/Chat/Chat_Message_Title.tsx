import { HiOutlineMenu } from "react-icons/hi";
import { ChannelResponseDto, ProfileResponseDto } from "../../../Common/Dtos";
import { useCommonContext } from "../../../Contexts/Common_Context";
import Avatar_Icon from "../Avatar_Icon";
import Channel_Icon from "../Channel_Icon";
import { useDialogContext } from "../../../Contexts/Dialog_Context";

export type ChatType = "Channel" | "DirectMessage";

interface Chat_Message_Title_Props {
  ChatName?: string;
  AvatarUrl?: string;
  Channel?: ChannelResponseDto;
  Profile?: ProfileResponseDto;
  Type: ChatType;
}

function Chat_Message_Title({
  ChatName,
  AvatarUrl,
  Type,
  Channel,
  Profile,
}: Chat_Message_Title_Props) {
  const { setIsSideBarOpened } = useCommonContext();
  const { openDialog } = useDialogContext();

  return (
    <div className="w-full h-15 flex items-center bg-gray-700 border-b border-gray-600 shadow-md px-1 py-1">
      <button
        className="p-2 mr-1 rounded-md transition hover:bg-gray-800 md:hidden"
        onClick={() => setIsSideBarOpened((prev) => !prev)}
      >
        <HiOutlineMenu className="text-3xl text-white" />
      </button>

      <div className="flex flex-1 w-0 items-center justify-between rounded-md select-none overflow-hidden">
        <div
          className={`flex flex-1 overflow-hidden items-center gap-3 ${
            Type === "DirectMessage" && "cursor-pointer"
          }`}
          onClick={() => {
            if (Type === "DirectMessage") {
              openDialog("ProfileDialog", {
                profileId: Profile?.id,
              });
            }
          }}
        >
          <span className="flex-shrink-0 text-gray-400">
            {Type === "Channel" && Channel && (
              <Channel_Icon
                Channel={Channel}
                className="text-2xl font-semibold"
              />
            )}
            {Type === "DirectMessage" && (
              <Avatar_Icon
                userName={ChatName}
                avatarUrl={AvatarUrl}
                Size="w-10 h-10"
              />
            )}
          </span>

            <p className="truncate text-lg font-semibold text-white pr-15">
              {ChatName}
            </p>
      
        </div>
      </div>
    </div>
  );
}

export default Chat_Message_Title;
