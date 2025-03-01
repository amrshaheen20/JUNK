import { Fragment, useEffect, useRef } from "react";
import { FaArrowDownLong } from "react-icons/fa6";
import { useParams } from "react-router-dom";
import {
  MemberResponseDto,
  MemberRole,
  MessageResponseDto,
  ProfileResponseDto,
} from "../../../Common/Dtos";
import { useListQuery } from "../../../Hooks/Querys/use-list-query";
import { useInfiniteScroll } from "../../../Hooks/use-Infinite-scroll";
import { IconButton } from "../IconButton";
import Loading_Spinner from "../Loading_Spin";
import Chat_Message_Card from "./Chat_Message_Card";
import Chat_Message_Input from "./Chat_Message_Input";
import { ChatType } from "./Chat_Message_Title";

interface ChatMessagesViewerProps {
  chatKey: string;
  MessagequeryUrl: string; // get delete edit send
  Type: ChatType;
  Member?: MemberResponseDto;
  Profile?: ProfileResponseDto;
}

function Chat_Messages_Viewer({
  chatKey,
  MessagequeryUrl,
  Type,
  Member,
  Profile,
}: ChatMessagesViewerProps) {
  const isAdmin =
    Member?.role === MemberRole.ADMIN || Member?.role === MemberRole.MODERATOR;

  const { messageId } = useParams<{
    messageId?: string;
  }>();

  const ContainerRef = useRef(null);
  const { data, ...queryDetails } = useListQuery<MessageResponseDto>({
    queryKey: MessagequeryUrl,
    queryUrl: MessagequeryUrl,
    SocketKey: chatKey,
    enabled: !!MessagequeryUrl,
    around: messageId,
  });

  const { scrollToTop, scrollValue } = useInfiniteScroll({
    ContainerRef,
    ...queryDetails,
    Invert: true,
  });

  useEffect(() => {
    if (messageId) {
      const activeItem = document.getElementById(messageId);
      if (activeItem) {
        activeItem.scrollIntoView({
          behavior: "instant",
          block: "nearest",
        });
      }
    }
  }, [messageId, queryDetails.isLoading]);

  const canDelete = (message: MessageResponseDto) =>
    (isAdmin && Type === "Channel") ||
    message?.author.id === Member?.id ||
    message?.author.id === Profile?.id;

  const canEdit = (message: MessageResponseDto) =>
    message?.author.id === Member?.id || message?.author.id === Profile?.id;

  const ScrollToBottom = () => {
    if (!queryDetails.hasPreviousPage) {
      scrollToTop();
      return;
    }
    if (messageId) {
      //Not Implemented
    }
  };

  if (queryDetails.isLoading)
    return (
      <div className="w-full flex flex-1 justify-center items-center">
        <Loading_Spinner />
      </div>
    );

  return (
    <div className="flex-1 flex flex-col overflow-y-auto">
      {data?.pages[0].length || data?.pages[1]?.length ? (
        <>
          <div
            ref={ContainerRef}
            className="flex-1 flex flex-col-reverse overflow-y-scroll bg-gray-800 p-4 w-full"
          >
            {data?.pages.map((group, index) => (
              <Fragment key={index}>
                {group.map((message) => (
                  <Chat_Message_Card
                    id={message.id}
                    key={message?.id}
                    Message={message}
                    MessagequeryUrl={MessagequeryUrl}
                    CanDelete={canDelete(message)}
                    CanEdit={canEdit(message)}
                  />
                ))}
              </Fragment>
            ))}
          </div>
          <div className="relative flex flex-col items-center justify-between">
            {scrollValue < 0 && (
              <IconButton
                className="absolute z-50 top-[-25px] border text-gray-500 border-gray-500 !rounded-full shadow-md hover:text-gray-200 hover:border-gray-200 transition"
                onClick={ScrollToBottom}
              >
                <FaArrowDownLong className="text-sm" />
              </IconButton>
            )}
            <Chat_Message_Input MessagequeryUrl={MessagequeryUrl} />
          </div>
        </>
      ) : (
        <div className="flex flex-1 flex-col items-center justify-center text-gray-400 w-full">
          <h1 className="text-5xl text-gray-300">Hi There!</h1>
          <Chat_Message_Input
            className="w-full p-10"
            MessagequeryUrl={MessagequeryUrl}
          />
        </div>
      )}
    </div>
  );
}

export default Chat_Messages_Viewer;
