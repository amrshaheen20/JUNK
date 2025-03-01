import { FaRegFile } from "react-icons/fa";
import { AttachmentResponseDto } from "../../../Common/Dtos";
import { GetFileUrl } from "../../../Common/GetMediaUrl";

const Chat_Message_Attachments = ({ attachments }: { attachments?: AttachmentResponseDto[] }) => {
  if (!attachments?.length) return null;

  const renderAttachment = (attachment: AttachmentResponseDto) => {
    const fileUrl = GetFileUrl(attachment.id);

    if (attachment.contentType.includes("image")) {
      return (
        <img
          src={fileUrl}
          alt="Preview"
          className="w-full max-w-full h-auto max-h-[60vh] md:max-h-80 object-contain rounded-lg"
          loading="lazy"
        />
      );
    }

    if (attachment.contentType.includes("video")) {
      return (
        <video
          src={fileUrl}
          className="w-full max-w-full h-auto max-h-[60vh] md:max-h-80 object-contain rounded-lg"
          controls
          preload="metadata"
        />
      );
    }

    return (
      <div className="flex items-center p-4 rounded-md bg-gray-700 max-w-full">
      <FaRegFile className="h-10 w-10 fill-white stroke-white" />
      <a
        href={fileUrl}
        target="_blank"
        rel="noopener noreferrer"
        className="ml-3 text-lg text-blue-400 hover:underline truncate overflow-hidden whitespace-nowrap max-w-[200px] sm:max-w-[300px] md:max-w-[400px]"
        title={attachment.name} 
      >
        {attachment.name}
      </a>
    </div>
    
    );
  };

  return (
    <div className="flex flex-col items-start w-full">
      {attachments.map((attachment) => (
        <div
          key={attachment.id}
          className="rounded-md mt-2 overflow-hidden flex items-center bg-black max-w-full"
        >
          {renderAttachment(attachment)}
        </div>
      ))}
    </div>
  );
};

export default Chat_Message_Attachments;
