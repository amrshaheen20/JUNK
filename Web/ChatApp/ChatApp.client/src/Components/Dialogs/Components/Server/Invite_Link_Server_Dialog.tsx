import { useEffect, useState } from "react";
import { useDialogContext } from "../../../../Contexts/Dialog_Context";
import CopyButton from "../../../Shared/CopyButton";

function Invite_Link_Server_Dialog() {
  const { isOpened, closeDialog, activeDialog, data } = useDialogContext();
  const [inviteLink, setInviteLink] = useState<string>("");

  const isActive = isOpened && activeDialog === "InviteLinkServer";
  const { server } = data;

  useEffect(() => {
    if (server) {
      setInviteLink(
        `${window.location.origin}/server/${server.inviteCode}/join`
      );
    }
  }, [server, isActive]);

  if (!isActive || !server) return null;

  return (
    <>
      <h2 className="text-3xl font-semibold mb-6 text-center">
        Server Invite Link
      </h2>

      <div className="flex items-center border bg-gray-100 text-gray-700 border-gray-300 rounded-lg overflow-hidden p-2 gap-1">
        <input
          type="text"
          value={inviteLink}
          readOnly
          className="w-full  outline-none"
        />
        <CopyButton text={inviteLink} className=" hover:bg-gray-200  h-full" />
      </div>

      <button
        type="button"
        onClick={closeDialog}
        className="w-full bg-green-500 cursor-pointer hover:bg-green-600 text-white py-3 rounded-lg mt-4"
      >
        Close
      </button>
    </>
  );
}

export default Invite_Link_Server_Dialog;
