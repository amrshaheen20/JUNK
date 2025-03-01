import { useDialogContext } from "../../Contexts/Dialog_Context";
import Create_Channel_Dialog from "./Components/Channel/Create_Channel_Dialog";
import Delete_Channel_Dialog from "./Components/Channel/Delete_Channel_Dialog";
import Edit_Channel_Dialog from "./Components/Channel/Edit_Channel_Dialog";
import Delete_Message_Dialog from "./Components/Message/Delete_Message_Dialog";
import Create_Server_Dialog from "./Components/Server/Create_Server_Dialog";
import Delete_Server_Dialog from "./Components/Server/Delete_Server_Dialog";
import Edit_Server_Dialog from "./Components/Server/Edit_Server_Dialog";
import Invite_Link_Server_Dialog from "./Components/Server/Invite_Link_Server_Dialog";
import Leave_Server_Dialog from "./Components/Server/Leave_Server_Dialog";
import Delete_Member_Dialog from "./Components/Member/Delete_Member_Dialog";
import Members_Server_Dialog from "./Components/Server/members/Members_Server_Dialog";
import Profile_Dialog from "./Components/Shared/Profile_Dialog";
import Edit_Profile_Dialog from "./Components/Shared/Edit_Profile_Dialog";

function DialogsProvider() {
  const { isOpened, activeDialog, closeDialog } = useDialogContext();

  if (!isOpened) return null;

  return (
    <div
      className="fixed inset-0 z-1000 flex items-center justify-center bg-black/50 backdrop-blur-sm "
      onClick={() => closeDialog()}
    >
      <div
        className="w-full max-w-md rounded-3xl bg-white text-gray-600 p-8 shadow-xl m-5"
        onClick={(e) => e.stopPropagation()}
      >
        {activeDialog === "CreateServer" && <Create_Server_Dialog />}
        {activeDialog === "DeleteServer" && <Delete_Server_Dialog />}
        {activeDialog === "EditServer" && <Edit_Server_Dialog />}
        {activeDialog === "InviteLinkServer" && <Invite_Link_Server_Dialog />}
        {activeDialog === "LeaveServer" && <Leave_Server_Dialog />}
        {activeDialog === "CreateChannel" && <Create_Channel_Dialog />}
        {activeDialog === "DeleteChannel" && <Delete_Channel_Dialog />}
        {activeDialog === "EditChannel" && <Edit_Channel_Dialog />}
        {activeDialog === "DeleteMessage" && <Delete_Message_Dialog />}
        {activeDialog === "MembersServer" && <Members_Server_Dialog />}
        {activeDialog === "ProfileDialog" && <Profile_Dialog />}
        {activeDialog === "DeleteMember" && <Delete_Member_Dialog />}
        {activeDialog === "EditProfile" && <Edit_Profile_Dialog />}
      </div>
    </div>
  );
}

export default DialogsProvider;
