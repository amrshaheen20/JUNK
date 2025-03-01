import { FormEvent, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  ChannelResponseDto,
  ChannelType,
  CreateChannelRequestDto,
} from "../../../../Common/Dtos";
import { useDialogContext } from "../../../../Contexts/Dialog_Context";
import { useSendQuery } from "../../../../Hooks/Querys/use-send-query";

const Create_Channel_Dialog = () => {
  const navigate = useNavigate();
  const { isOpened, closeDialog, activeDialog, data } = useDialogContext();

  const [channelName, setChannelName] = useState("");
  const [channelType, setChannelType] = useState(ChannelType.TEXT);
  const [alertMessage, setAlertMessage] = useState("");

  const isActive = isOpened && activeDialog === "CreateChannel";
  const serverId = data?.server?.id;

  const { mutate: mutateCreateChannel, isLoading: isSubmitting } = useSendQuery<
    CreateChannelRequestDto,
    ChannelResponseDto
  >({
    Type: "POST",
    queryKey: "CreateChannel",
    queryUrl: `servers/${serverId}/Channels`,
  });

  useEffect(() => {
    if (isActive) {
      setChannelName("");
      setAlertMessage("");
      setChannelType(ChannelType.TEXT);
    }
  }, [isActive]);

  const validateForm = (): boolean => {
    if (!channelName.trim()) {
      setAlertMessage("Please enter a channel name");
      return false;
    }
    return true;
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!validateForm() || !serverId) return;

    mutateCreateChannel(
      { name: channelName.trim(), channelType },
      {
        onSuccess: (e) => {
          navigate(`/server/${serverId}/${e.id}`);
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
          Create Channel
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

      <div className="mb-6">
        <label className="mb-2 block text-sm font-medium text-gray-900">
          Channel Type
        </label>
        <select
          value={channelType}
          onChange={(e) => setChannelType(e.target.value as ChannelType)}
          className="block w-full rounded-lg border border-gray-300 bg-gray-50 p-2.5 text-sm text-gray-900 focus:border-gray-300"
          disabled={isSubmitting}
        >
          {Object.values(ChannelType)
            .map((type) => (
              <option key={type} value={type}>
                {type.charAt(0) + type.slice(1).toLowerCase()}
              </option>
            ))}
        </select>
      </div>
      <div className="flex flex-col gap-3">
        <button
          type="submit"
          className="w-full rounded-lg bg-blue-500 py-3 text-white hover:bg-blue-600 disabled:opacity-50"
          disabled={isSubmitting}
        >
          {isSubmitting ? "Creating..." : "Create Channel"}
        </button>
        <button
          type="button"
          onClick={closeDialog}
          className="w-full rounded-lg bg-gray-500 py-3 text-white hover:bg-gray-600"
          disabled={isSubmitting}
        >
          Close
        </button>
      </div>
    </form>
  );
};

export default Create_Channel_Dialog;
