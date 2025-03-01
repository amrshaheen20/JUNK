import { ComponentProps, useState } from "react";
import { CiEdit } from "react-icons/ci";
import { MdDelete } from "react-icons/md";
import { getAvatarColor } from "../../../Common/AvatarColor";
import { MessageResponseDto } from "../../../Common/Dtos";
import { GetFileUrl } from "../../../Common/GetMediaUrl";
import { useDialogContext } from "../../../Contexts/Dialog_Context";
import { useSendQuery } from "../../../Hooks/Querys/use-send-query";
import Avatar_Icon from "../Avatar_Icon";
import Chat_Message_Attachments from "./Chat_Message_Attachments";
import MarkdownComponent from "../MarkdownComponent";
import CopyButton from "../CopyButton";
import { IconButton } from "../IconButton";

interface Chat_Message_Card_Props extends ComponentProps<"div"> {
  Message?: MessageResponseDto | null;
  ShowAuthor?: boolean;
  CanEdit: boolean;
  CanDelete: boolean;
  MessagequeryUrl: string; //delete edit send
}

const Chat_Message_Card = ({
  Message,
  ShowAuthor,
  CanEdit,
  CanDelete,
  MessagequeryUrl,
  ...props
}: Chat_Message_Card_Props) => {
  const [IsEdited, setIsEdited] = useState<boolean>(false);
  const [MessageContent, setMessageContent] = useState<string>("");
  const { openDialog } = useDialogContext();

  const { mutate: mutateEdit, isLoading: isLoadingEdit } = useSendQuery<
    FormData,
    MessageResponseDto
  >({
    Type: "PATCH",
    queryKey: "EditMessage",
    queryUrl: `${MessagequeryUrl}/${Message?.id}`,
    Config: {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    },
  });

  const handleMessageSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const formData = new FormData();
    formData.append("content", MessageContent);
    mutateEdit(formData, {
      onSuccess: () => setIsEdited(false),
    });
  };

  return (
    <div
      {...props}
      className={`group flex w-full  ${
        ShowAuthor ? "ml-15" : "items-start space-x-3 mt-5"
      }  hover:bg-blue-400/10 p-2 rounded-lg relative`}
    >
      {!ShowAuthor && (
        <Avatar_Icon
          userName={Message?.author.userName}
          avatarUrl={GetFileUrl(Message?.author.imageId)}
          onClick={() => {
            openDialog("ProfileDialog", {
              profileId: Message?.author?.id,
            });
          }}
        />
      )}

      <div className={`flex flex-col w-full`}>
        {!ShowAuthor && (
          <div className="flex flex-wrap items-start gap-x-2 gap-y-1">
            <strong
              className="text-sm md:text-base"
              style={{ color: getAvatarColor(Message?.author.userName) }}
            >
              {Message?.author.userName}
            </strong>
            <p className="text-xs md:text-sm text-gray-400">
              {new Date(Message?.createdAt || "").toLocaleString()}
            </p>
          </div>
        )}

        {IsEdited ? (
          <form
            className="bg-blue-400/30 p-2 rounded-2xl"
            onSubmit={handleMessageSubmit}
          >
            <textarea
              className="border-transparent h-40 focus:ring-0 focus:outline-none resize-none w-full bg-transparent"
              defaultValue={Message?.content}
              onChange={(e) => setMessageContent(e.target.value)}
              disabled={isLoadingEdit}
            />

            <div className="w-full flex justify-end gap-2 mt-2">
              <button
                className="bg-gray-800 text-white px-3 py-1 rounded-4xl cursor-pointer"
                onClick={() => setIsEdited(false)}
                disabled={isLoadingEdit}
              >
                Close
              </button>
              <button
                type="submit"
                className="bg-white text-black px-3 py-1 rounded-4xl cursor-pointer"
                disabled={isLoadingEdit}
              >
                Send
              </button>
            </div>
          </form>
        ) : (
          <div className="prose prose-invert max-w-full w-full">
            {/* <p className="text-sm text-gray-300 ">{Message?.content}</p> */}

            <MarkdownComponent>{Message?.content || ""}</MarkdownComponent>
            {Message?.isEdited && (
              <span className="text-xs text-gray-400 italic">(Edited)</span>
            )}
          </div>
        )}

        <Chat_Message_Attachments attachments={Message?.attachments} />

        {!IsEdited && (
          <div className="group-hover:opacity-100  opacity-0  absolute top-[-20px] right-2 flex items-center gap-2 bg-gray-900/70  p-1 rounded-lg shadow-md backdrop-blur-md">
            {CanEdit && (
              <IconButton
                className="text-blue-500 hover:text-blue-600 hover:bg-gray-800"
                onClick={() => setIsEdited(true)}
              >
                <CiEdit />
              </IconButton>
            )}

            {CanDelete && (
              <IconButton
                className="text-red-500 hover:text-red-600 hover:bg-gray-800"
                onClick={() => {
                  openDialog("DeleteMessage", {
                    queryUrl: `${MessagequeryUrl}/${Message?.id}`,
                  });
                }}
              >
                <MdDelete />
              </IconButton>
            )}

            <CopyButton
              text={Message?.content || ""}
              className="hover:bg-gray-800"
            />
          </div>
        )}
      </div>
    </div>
  );
};

export default Chat_Message_Card;
