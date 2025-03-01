import { ComponentProps, useEffect, useRef, useState } from "react";
import { AiOutlineClose } from "react-icons/ai";
import { FaRegFile } from "react-icons/fa";
import { GrAttachment } from "react-icons/gr";
import { IoMdRocket } from "react-icons/io";
import { MessageResponseDto } from "../../../Common/Dtos";
import { MicroTextReplacer } from "../../../Common/MicroTextReplacer";
import { useSendQuery } from "../../../Hooks/Querys/use-send-query";
import EmojiPicker from "../EmojiPicker";
import { IconButton } from "../IconButton";
import { BsEmojiSmile } from "react-icons/bs";

interface ChatMessageBoxProps extends ComponentProps<"div"> {
  MessagequeryUrl: string; //delete | edit | send
}

const Chat_Message_Input = ({
  MessagequeryUrl,
  ...props
}: ChatMessageBoxProps) => {
  const [filePreviews, setFilePreviews] = useState<File[]>([]);
  const [message, setMessage] = useState<string>("");
  const [isFocused, setIsFocused] = useState<boolean>(false);
  const [CanSend, setCanSend] = useState<boolean>(true);
  const [showEmojiPicker, setShowEmojiPicker] = useState<boolean>(false);

  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const InputRef = useRef<HTMLTextAreaElement | null>(null);
  const EmojiPickerRef = useRef<HTMLDivElement | null>(null);

  const MAX_HEIGHT = 300;

  const { mutate: mutateSendMessage, isLoading: isSubmitting } = useSendQuery<
    FormData,
    MessageResponseDto
  >({
    Type: "POST",
    queryKey: "SendMessage",
    queryUrl: MessagequeryUrl,
    Config: {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    },
  });

  useEffect(() => {
    setCanSend(message.trim().length > 0 || filePreviews.length > 0);
  }, [message, filePreviews]);

  const resetForm = () => {
    setMessage("");
    setFilePreviews([]);
    setIsFocused(false);

    if (InputRef.current) {
      InputRef.current.style.height = "auto";
    }
  };

  const handleFilesUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files) return;

    const newFiles = Array.from(e.target.files);
    setFilePreviews((prev) => [...prev, ...newFiles]);

    e.target.value = "";
  };

  const removeFilePreview = (index: number) => {
    if (isSubmitting) return;
    setFilePreviews((prev) => prev.filter((_, i) => i !== index));
  };

  const handleInput = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const textarea = e.target;
    const replacedText = MicroTextReplacer(textarea.value);
    setMessage(replacedText);

    textarea.style.height = "auto";
    textarea.style.height = `${Math.min(textarea.scrollHeight, MAX_HEIGHT)}px`;
  };

  const handleMessageSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!CanSend) return;
    const formData = new FormData();
    if (filePreviews.length > 0) {
      Array.from(filePreviews).forEach((file) => {
        formData.append("files", file);
      });
    }

    formData.append("content", message.trim());
    mutateSendMessage(formData, {
      onSuccess: resetForm,
    });
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleMessageSubmit(e as React.FormEvent);
    }
  };

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        EmojiPickerRef.current &&
        !EmojiPickerRef.current.contains(event.target as Node)
      ) {
        setShowEmojiPicker(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  const handleEmojiSelect = (emoji: string) => {
    setMessage((prev) => prev + emoji);
  };

  return (
    <div className="flex w-full mx-5 p-4 rounded-2xl  content-center" {...props}>
      <form className="flex flex-col w-full" onSubmit={handleMessageSubmit}>
        <div className="relative">
          {showEmojiPicker && (
            <div ref={EmojiPickerRef} className="absolute bottom-0 left-0 z-50">
              <EmojiPicker onSelect={handleEmojiSelect} />
            </div>
          )}
          {filePreviews.length > 0 && (
            <div className="w-full bg-gray-700 rounded-lg flex flex-wrap gap-3 p-4 mb-4 max-h-60 overflow-y-auto">
              {filePreviews.map((file, index) => (
                <div
                  key={index}
                  className="flex flex-col items-center justify-between bg-gray-800 p-3 rounded-lg w-44 max-h-48 min-h-48 flex-shrink-0 relative"
                >
                  {file.type.startsWith("image") ? (
                    <img
                      src={URL.createObjectURL(file)}
                      alt="Preview"
                      className="w-full max-h-32 min-h-32 object-cover rounded-lg "
                    />
                  ) : (
                    <div className="flex flex-col items-center justify-center h-32 w-full">
                      <FaRegFile className="text-white text-3xl mb-2" />
                    </div>
                  )}
                  <p
                    className="text-white text-sm w-full text-center truncate px-2 mt-2"
                    title={file.name}
                  >
                    {file.name}
                  </p>
                  <button
                    type="button"
                    onClick={() => removeFilePreview(index)}
                    className="absolute top-1 right-1 text-gray-400 cursor-pointer hover:text-white text-lg rounded-full  hover:scale-110 bg-gray-800 p-1"
                    disabled={isSubmitting}
                  >
                    <AiOutlineClose />
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        <div
          className={`flex w-full flex-col bg-gray-700 p-2 rounded-lg transition-all relative shadow-lg ${
            isFocused ? "outline-2 outline-blue-400" : "outline-none"
          }`}
        >
          <textarea
            className="w-full p-2 overflow-y-auto text-white border-transparent focus:border-transparent focus:ring-0 focus:outline-none resize-none"
            placeholder="Type a message..."
            value={message || ""}
            onChange={handleInput}
            onInput={handleInput}
            onFocus={() => setIsFocused(true)}
            onBlur={() => setIsFocused(false)}
            style={{ maxHeight: MAX_HEIGHT }}
            onKeyDown={handleKeyDown}
            disabled={isSubmitting}
            ref={InputRef}
          />
          <div className="flex justify-between w-full px-1 bg-gray-700">
            <div className="flex">
              <IconButton
                type="button"
                onClick={() => fileInputRef.current?.click()}
                disabled={isSubmitting}
                className={`text-gray-400 ${
                  isSubmitting ? "" : "hover:text-white"
                }`}
              >
                <GrAttachment />
              </IconButton>

              <IconButton
                type="button"
                onClick={() => setShowEmojiPicker((prev) => !prev)}
                className="text-gray-400 hover:text-white"
                disabled={isSubmitting}
              >
                <BsEmojiSmile />
              </IconButton>
            </div>
            <IconButton
              type="submit"
              disabled={isSubmitting || !CanSend}
              className={`p-3 !rounded-full text-gray-400 ${
                isSubmitting || !CanSend ? "" : "text-white hover:bg-gray-600"
              }`}
            >
              <span className={isSubmitting ? "flame-rocket" : ""}>
                <IoMdRocket />
              </span>
            </IconButton>
          </div>

          <style>
            {`
  @keyframes flameEffect {
    0% { transform: translate(0px, 0px) rotate(0deg); }
    25% { transform: translate(-2px, -2px) rotate(-2deg); }
    50% { transform: translate(2px, -4px) rotate(2deg); }
    75% { transform: translate(-1px, -3px) rotate(-1deg); }
    100% { transform: translate(1px, -5px) rotate(1deg); }
  }

  .flame-rocket {
    display: inline-block;
    animation: flameEffect 0.2s ease-in-out infinite alternate;
  }
`}
          </style>

          <input
            type="file"
            ref={fileInputRef}
            className="hidden"
            multiple
            onChange={handleFilesUpload}
            disabled={isSubmitting}
          />
        </div>
      </form>
    </div>
  );
};

export default Chat_Message_Input;
