import { FormEvent, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { ServerResponseDto } from "../../../../Common/Dtos";
import { useDialogContext } from "../../../../Contexts/Dialog_Context";
import { useSendQuery } from "../../../../Hooks/Querys/use-send-query";
import Image_Upload, { UploadedFileProps } from "../../../Shared/Image_Upload";

function Edit_Server_Dialog() {
  const { isOpened, closeDialog, activeDialog, data } = useDialogContext();
  const [ServerName, setServerName] = useState<string>("");
  const [photo, setPhoto] = useState<UploadedFileProps>({
    File: null,
    FileId: null,
  });
  const [alertMessage, setAlertMessage] = useState<string>("");
  const navigate = useNavigate();

  const { server } = data;

  const { mutate: mutateEditServer, isLoading: isSubmitting } = useSendQuery<
    FormData,
    ServerResponseDto
  >({
    Type: "PATCH",
    queryKey: "EditServer",
    queryUrl: `/Servers/${server?.id}`,
    Config: {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    },
  });

  const isActive = isOpened && activeDialog === "EditServer";

  useEffect(() => {
    if (server && isActive) {
      setPhoto({ File: null, FileId: server.imageId });
      setServerName(server.name);
    }
  }, [server, isActive]);

  if (!isActive || !server) return null;

  const validateForm = (): boolean => {
    if (!ServerName.trim()) {
      setAlertMessage("Please enter a server name.");
      return false;
    }
    return true;
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!validateForm()) return;

    const formData = new FormData();
    if (photo?.File) {
      formData.append("image", photo.File);
    }
    formData.append("name", ServerName);

    mutateEditServer(formData, {
      onSuccess: (response) => {
        closeDialog();
        navigate(`/server/${response.id}`);
      },
      onError: () => {
        setAlertMessage("An error occurred. Please try again.");
      },
    });
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2 className="text-3xl font-semibold mb-6 text-center">Edit Server</h2>

      <Image_Upload File={photo} onFileChange={setPhoto} />

      {alertMessage && (
        <div className="bg-red-500 text-white text-sm p-3 mb-4 rounded-lg">
          {alertMessage}
        </div>
      )}

      <div className="mb-6">
        <label className="block text-sm text-gray-700 mb-2">Server Name</label>
        <input
          type="text"
          value={ServerName}
          onChange={(e) => setServerName(e.target.value)}
          className="w-full p-3 bg-gray-100 text-gray-800 rounded-lg"
          placeholder="Enter server name"
          disabled={isSubmitting}
          maxLength={127}
          minLength={1}
        />
      </div>

      <button
        type="submit"
        className="w-full bg-blue-500 hover:bg-blue-600 text-white py-3 rounded-lg mb-4"
        disabled={isSubmitting}
      >
        {isSubmitting ? "Editing..." : "Edit Server"}
      </button>
      <button
        type="button"
        onClick={closeDialog}
        className="w-full bg-gray-500 hover:bg-gray-600 text-white py-3 rounded-lg"
        disabled={isSubmitting}
      >
        Close
      </button>
    </form>
  );
}

export default Edit_Server_Dialog;
