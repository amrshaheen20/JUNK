import { FormEvent, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  ServerResponseDto
} from "../../../../Common/Dtos";
import { useDialogContext } from "../../../../Contexts/Dialog_Context";
import { useSendQuery } from "../../../../Hooks/Querys/use-send-query";
import Image_Upload, { UploadedFileProps } from "../../../Shared/Image_Upload";

const Create_Server_Dialog = () => {
  const navigate = useNavigate();
  const { isOpened, closeDialog, activeDialog } = useDialogContext();
  const [serverName, setServerName] = useState<string>("");
  const [photo, setPhoto] = useState<UploadedFileProps>({
    File: null,
    FileId: null,
  });
  const [alertMessage, setAlertMessage] = useState<string>("");

  const { mutate: mutateCreateServer, isLoading: isSubmitting } = useSendQuery<
    FormData,
    ServerResponseDto
  >({
    Type: "POST",
    queryKey: "CreateServer",
    queryUrl: "/Servers",
    Config: {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    },
  });

  const isActive = isOpened && activeDialog === "CreateServer";

  useEffect(() => {
    if (isActive) {
      setPhoto({ File: null, FileId: null });
      setServerName("");
      setAlertMessage("");
    }
  }, [isActive]);

  if (!isActive) return null;

  const validateForm = (): boolean => {
    if (!serverName.trim()) {
      setAlertMessage("Please enter a server name.");
      return false;
    }
    if (!photo) {
      setAlertMessage("Please upload a server photo.");
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
    formData.append("name", serverName);

    mutateCreateServer(formData, {
      onSuccess: (e) => {
        closeDialog();
        navigate(`/server/${e.id}`);
      },
      onError: () => {
        setAlertMessage("An error occurred. Please try again.");
      },
    });
  };

  return (
        <form onSubmit={handleSubmit}>
          <h2 className="text-3xl font-semibold mb-6 text-center">
            Create Server
          </h2>

          <Image_Upload File={photo} onFileChange={setPhoto} />

          {alertMessage && (
            <div className="bg-red-500 text-white text-sm p-3 mb-4 rounded-lg">
              {alertMessage}
            </div>
          )}

          <div className="mb-6">
            <label className="block text-sm text-gray-700 mb-2">
              Server Name
            </label>
            <input
              type="text"
              value={serverName}
              onChange={(e) => setServerName(e.target.value)}
              className="w-full p-3 bg-gray-100 text-gray-800 rounded-lg"
              placeholder="Enter server name"
              disabled={isSubmitting}
              maxLength={127}
              minLength={1}
            />
          </div>

          <div className="flex flex-col gap-3">
            <button
              type="submit"
              className="w-full bg-blue-500 hover:bg-blue-600 text-white py-3 rounded-lg disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Creating..." : "Create Server"}
            </button>
            <button
              type="button"
              onClick={closeDialog}
              className="w-full bg-gray-500 hover:bg-gray-600 text-white py-3 rounded-lg"
              disabled={isSubmitting}
            >
              Close
            </button>
          </div>
        </form>
  );
};

export default Create_Server_Dialog;
