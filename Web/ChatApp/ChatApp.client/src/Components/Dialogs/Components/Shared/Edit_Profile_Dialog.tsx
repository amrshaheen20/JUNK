import { FormEvent, useEffect, useState } from "react";
import { ProfileResponseDto } from "../../../../Common/Dtos";
import { useCommonContext } from "../../../../Contexts/Common_Context";
import { useDialogContext } from "../../../../Contexts/Dialog_Context";
import { useSendQuery } from "../../../../Hooks/Querys/use-send-query";
import { checkAvailability } from "../../../../Pages/Auth/Register";
import Image_Upload, { UploadedFileProps } from "../../../Shared/Image_Upload";

function Edit_Profile_Dialog() {
  const { isOpened, closeDialog, activeDialog } = useDialogContext();
  const { globalData, dispatch } = useCommonContext();
  const [displayName, setDisplayName] = useState("");
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [bio, setBio] = useState("");

  const [photo, setPhoto] = useState<UploadedFileProps>({
    File: null,
    FileId: null,
  });

  const [alertMessage, setAlertMessage] = useState<string>("");

  const { mutate, isLoading: isSubmitting } = useSendQuery<
    FormData,
    ProfileResponseDto
  >({
    Type: "PATCH",
    queryKey: "EditProfile",
    queryUrl: `/Profile`,
    Config: {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    },
  });

  const isActive = isOpened && activeDialog === "EditProfile";

  useEffect(() => {
    if (globalData.Profile && isActive) {
      setPhoto({ File: null, FileId: globalData.Profile.imageId });
      setDisplayName(globalData.Profile.displayName);
      setUserName(globalData.Profile.userName);
      setEmail(globalData.Profile.email);
      setBio(globalData.Profile.bio || "");
    }
  }, [globalData?.Profile, isActive]);

  if (!isActive || !globalData.Profile) return null;

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const isUserNameAvailable = await checkAvailability("userName", userName);
    const isEmailAvailable = await checkAvailability("email", email);

    if (!isUserNameAvailable && globalData.Profile?.userName !== userName) {
      setAlertMessage("Username is already taken.");
      return;
    }

    if (!isEmailAvailable && globalData.Profile?.email !== email) {
      setAlertMessage("Email is already taken.");
      return;
    }

    const formData = new FormData();
    formData.append("displayName", displayName);
    formData.append("userName", userName);
    formData.append("email", email);
    formData.append("bio", bio);
    if (photo.File) {
      formData.append("Image", photo.File);
    }

    mutate(formData, {
      onSuccess: (e) => {
        dispatch({ type: "ADD_PROFILE", payload: e });
        closeDialog();
      },
      onError: () => {
        setAlertMessage("Something went wrong! Please try again.");
      },
    });
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2 className="text-3xl font-semibold mb-6 text-center">Edit Profile</h2>

      <Image_Upload File={photo} onFileChange={setPhoto} />

      {alertMessage && (
        <div className="bg-red-500 text-white text-sm p-3 mb-4 rounded-lg">
          {alertMessage}
        </div>
      )}

      <div className="mb-4">
        <label htmlFor="displayName" className="block mb-2 text-sm">
          Display Name
        </label>
        <input
          type="text"
          id="displayName"
          value={displayName}
          autoComplete="name"
          onChange={(e) => setDisplayName(e.target.value)}
          className="w-full p-2 bg-gray-100 text-gray-800 rounded-lg focus:ring-2 focus:ring-blue-500"
          placeholder="Display Name"
          required
          disabled={isSubmitting}
        />
      </div>

      <div className="mb-4">
        <label htmlFor="userName" className="block mb-2 text-sm">
          Username
        </label>
        <input
          type="text"
          id="userName"
          value={userName}
          autoComplete="username"
          onChange={(e) => setUserName(e.target.value)}
          className="w-full p-2 bg-gray-100 text-gray-800 rounded-lg focus:ring-2 focus:ring-blue-500"
          placeholder="Username"
          required
          disabled={isSubmitting}
        />
      </div>

      <div className="mb-4">
        <label htmlFor="email" className="block mb-2 text-sm">
          Email Address
        </label>
        <input
          type="email"
          id="email"
          value={email}
          autoComplete="email"
          onChange={(e) => setEmail(e.target.value)}
          className="w-full p-2 bg-gray-100 text-gray-800 rounded-lg focus:ring-2 focus:ring-blue-500"
          placeholder="Email address"
          required
          disabled={isSubmitting}
        />
      </div>

      <div className="mb-4">
        <label htmlFor="email" className="block mb-2 text-sm">
          Bio
        </label>
        <input
          type="text"
          id="bio"
          value={bio}
          onChange={(e) => setBio(e.target.value)}
          className="w-full p-2 bg-gray-100 text-gray-800 rounded-lg focus:ring-2 focus:ring-blue-500"
          placeholder="Email address"
          disabled={isSubmitting}
        />
      </div>

      <button
        type="submit"
        className="w-full bg-blue-500 hover:bg-blue-600 text-white py-3 rounded-lg mb-4"
        disabled={isSubmitting}
      >
        {isSubmitting ? "Editing..." : "Edit Profile"}
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

export default Edit_Profile_Dialog;
