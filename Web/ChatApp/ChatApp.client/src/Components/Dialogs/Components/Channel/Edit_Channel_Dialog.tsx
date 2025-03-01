import { FormEvent, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  ChannelResponseDto,
  CreateChannelRequestDto,
} from "../../../../Common/Dtos";
import { useDialogContext } from "../../../../Contexts/Dialog_Context";
import { useSendQuery } from "../../../../Hooks/Querys/use-send-query";

const Edit_Channel_Dialog = () => {
  const navigate = useNavigate();
  const { isOpened, closeDialog, activeDialog, data, openDialog } =
    useDialogContext();
  const [channelName, setChannelName] = useState("");
  const [alertMessage, setAlertMessage] = useState("");

  const isActive = isOpened && activeDialog === "EditChannel";
  const server = data?.server;
  const channel = data.channel;

  const { mutate: mutateEditChannel, isLoading: isSubmitting } = useSendQuery<
    CreateChannelRequestDto,
    ChannelResponseDto
  >({
    Type: "PATCH",
    queryKey: "EditChannel",
    queryUrl: `servers/${server?.id}/Channels/${channel?.id}`,
  });

  useEffect(() => {
    if (isActive) {
      setChannelName(channel?.name || "");
      setAlertMessage("");
    }
  }, [isActive, channel]);

  const validateForm = (): boolean => {
    if (!channelName.trim()) {
      setAlertMessage("Please enter a channel name");
      return false;
    }
    return true;
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!validateForm() || !server?.id) return;
    mutateEditChannel(
      { name: channelName.trim() },
      {
        onSuccess: (e) => {
          navigate(`/server/${server?.id}/${e.id}`);
          closeDialog();
        },
        onError: () => {
          setAlertMessage("Failed to create channel");
        },
      }
    );
  };

  if (!isActive) return null;

  return (
    <form onSubmit={handleSubmit}>
      <header className="mb-6 text-center">
        <h2 id="channel-dialog-title" className="text-3xl font-semibold">
          Edit Channel
        </h2>
      </header>

      {alertMessage && (
        <div className="mb-4 rounded-lg bg-red-500 p-3 text-sm text-white">
          {alertMessage}
        </div>
      )}

      <div className="mb-6">
        <label className="mb-2 block text-sm text-gray-700">Channel Name</label>
        <input
          type="text"
          value={channelName}
          onChange={(e) => setChannelName(e.target.value)}
          className="w-full rounded-lg bg-gray-100 p-3 text-gray-800"
          placeholder="Enter channel name"
          disabled={isSubmitting}
          maxLength={127}
          minLength={1}
        />
      </div>

      <div className="flex flex-col gap-3">
        <button
          onClick={() => {
            openDialog("DeleteChannel", {
              queryUrl: `/Servers/${server?.id}/Channels/${channel?.id}`,
              server: server,
            });
          }}
          className="w-full rounded-lg py-3 text-white bg-red-500 hover:bg-red-600 transition-all duration-200"
          disabled={isSubmitting}
        >
          Delete Channel
        </button>
        <div className="border-t border-gray-300 my-4"></div>

        <button
          type="submit"
          className="w-full rounded-lg py-3 text-white bg-blue-500 hover:bg-blue-600 transition-all duration-200 disabled:opacity-50"
          disabled={isSubmitting}
        >
          {isSubmitting ? "Editing..." : "Edit Channel"}
        </button>

        <button
          type="button"
          onClick={closeDialog}
          className="w-full rounded-lg py-3 text-white bg-gray-500 hover:bg-gray-600 transition-all duration-200"
          disabled={isSubmitting}
        >
          Close
        </button>
      </div>
    </form>
  );
};

export default Edit_Channel_Dialog;
