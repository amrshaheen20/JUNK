import { NavLink, useNavigate } from "react-router-dom";
import {
  ConversationResponseDto,
  ProfileWithMoreDataResponseDto
} from "../../../../Common/Dtos";
import { GetFileUrl } from "../../../../Common/GetMediaUrl";
import { useCommonContext } from "../../../../Contexts/Common_Context";
import { useDialogContext } from "../../../../Contexts/Dialog_Context";
import { useItemQuery } from "../../../../Hooks/Querys/use-item-query";
import { useSendQuery } from "../../../../Hooks/Querys/use-send-query";
import Avatar_Icon from "../../../Shared/Avatar_Icon";
import Loading_Spinner from "../../../Shared/Loading_Spin";
import MarkdownComponent from "../../../Shared/MarkdownComponent";

function Profile_Dialog() {
  const { isOpened, closeDialog, activeDialog, data } = useDialogContext();
  const isActive = isOpened && activeDialog === "ProfileDialog";
  const { profileId } = data;

  const { globalData } = useCommonContext();
  const { Profile } = globalData;

  const navigate = useNavigate();

  const { data: profileData, isLoading } = useItemQuery<ProfileWithMoreDataResponseDto>({
    queryKey: `/Profile/${profileId}`,
    queryUrl: `/Profile/${profileId}`,
    enabled: !!profileId,
  });



  const { mutate: startConversation } = useSendQuery<
    unknown,
    ConversationResponseDto
  >({
    Type: "POST",
    queryKey: "CreateConversation",
    queryUrl: `/Conversations/${profileId}`,
  });

  if (!isActive) return null;

  if (isLoading) {
    return (
      <div className="fixed inset-0 flex items-center justify-center bg-black/50 z-50">
        <div className="bg-white w-full max-w-lg p-8 rounded-3xl shadow-xl flex justify-center items-center">
          <Loading_Spinner className="!text-black" />
        </div>
      </div>
    );
  }

  if (!profileData) {
    return (
      <div className="fixed inset-0 flex items-center justify-center bg-black/50 z-50">
        <div className="bg-white w-full max-w-lg p-8 rounded-3xl shadow-xl text-center">
          <p className="text-lg text-red-500">Profile not found.</p>
          <button
            type="button"
            onClick={closeDialog}
            className="w-full bg-green-500 hover:bg-green-600 text-white py-3 rounded-lg mt-4"
          >
            Close
          </button>
        </div>
      </div>
    );
  }

  const handleMessageClick = () => {
    startConversation(
      {},
      {
        onSuccess: (conversation) => {
          navigate(`/conversation/${conversation.id}/${conversation.channelId}`);
          closeDialog();
        },
      }
    );
  };

  return (
    <>
      <div className="flex items-center space-x-4 mb-6">
        <Avatar_Icon
          userName={profileData.displayName}
          avatarUrl={GetFileUrl(profileData.imageId)}
          Size="w-24 h-24"
        />
        <div className="flex flex-col">
          <span className="font-semibold text-lg text-gray-800">
            {profileData.displayName}
          </span>
          <span className="text-sm text-gray-500">
            Username: {profileData.userName}
          </span>
          <span className="text-sm text-gray-400">
            Joined: {new Date(profileData.joinTime).toLocaleDateString()}
          </span>
          {profileData.bio && (
            <span className="flex justify-center items-center text-sm mt-2 px-2 rounded-md min-h-10 bg-gray-200">
              <MarkdownComponent>{profileData.bio || ""}</MarkdownComponent>
            </span>
          )}
        </div>
      </div>

      {profileData.id !== Profile?.id && (
        <button
          onClick={handleMessageClick}
          className="w-full bg-blue-500 hover:bg-blue-600 text-white py-3 rounded-lg mt-4"
        >
          Message
        </button>
      )}

      {profileData.mutualServers?.length > 0 && (
        <>
          <h3 className="text-xl font-semibold mb-4 mt-6">Mutual Servers</h3>
          <div className="space-y-4 max-h-60 overflow-y-scroll">
            {profileData.mutualServers.map((server) => (
              <NavLink
                to={`/server/${server.id}`}
                onClick={closeDialog}
                key={server.id}
                className="flex items-center justify-between py-2 px-4 rounded-lg hover:bg-gray-100 transition duration-200 cursor-pointer"
              >
                <div className="flex items-center space-x-2">
                  <Avatar_Icon
                    userName={server.name}
                    avatarUrl={GetFileUrl(server.imageId)}
                  />
                  <span className="text-lg font-medium">{server.name}</span>
                </div>
              </NavLink>
            ))}
          </div>
        </>
      )}

      <button
        type="button"
        onClick={closeDialog}
        className="w-full bg-green-500 hover:bg-green-600 text-white py-3 rounded-lg mt-4"
      >
        Close
      </button>
    </>
  );
}

export default Profile_Dialog;
