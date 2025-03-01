import { Fragment, useEffect, useMemo, useRef, useState } from "react";
import { BiPlus } from "react-icons/bi";
import { BsFillPeopleFill } from "react-icons/bs";
import { NavLink, Outlet, useLocation, useParams } from "react-router-dom";
import { ConversationResponseDto } from "../Common/Dtos";
import { GetFileUrl } from "../Common/GetMediaUrl";
import Avatar_Icon from "../Components/Shared/Avatar_Icon";
import IconButtonWithText from "../Components/Shared/IconButtonWithText";
import Loading_Spinner from "../Components/Shared/Loading_Spin";
import Sidebar_Navigation from "../Components/Shared/Sidebar_Navigation";
import { useCommonContext } from "../Contexts/Common_Context";
import { useListQuery } from "../Hooks/Querys/use-list-query";
import { useInfiniteScroll } from "../Hooks/use-Infinite-scroll";

function Home_Page() {
  const { conversationId } = useParams<{
    conversationId?: string;
  }>();
  const { dispatch } = useCommonContext();

  const { data, isLoading, ...queryDetails } =
    useListQuery<ConversationResponseDto>({
      queryKey: `Conversations`,
      queryUrl: `/Profile/Conversations`,
      SocketKey: `conversation`,
      around: conversationId,
    });

  const ContainerRef = useRef(null);
  useInfiniteScroll({
    ContainerRef,
    ...queryDetails,
  });

  const location = useLocation();
  const [hasActiveRoute, setHasActiveRoute] = useState(false);

  useEffect(() => {
    setHasActiveRoute(location.pathname !== "/");
  }, [location.pathname]);

  const selecteditem = useMemo(() => {
    if (!conversationId || !data?.pages) return null;

    for (const page of data.pages) {
      const found = page.find((x) => x.id === conversationId);
      if (found) return found;
    }

    return null;
  }, [conversationId, data?.pages]);

  useEffect(() => {
    if (conversationId) {
      const activeItem = document.getElementById(conversationId);
      if (activeItem) {
        activeItem.scrollIntoView({
          behavior: "instant",
          block: "nearest",
        });
      }
    }
    dispatch({ type: "ADD_CONVERSATION", payload: selecteditem });
  }, [conversationId, selecteditem, dispatch]);

  return (
    <div className="flex flex-1 h-full bg-gray-800">
      <Sidebar_Navigation>
        <div className="p-2 h-full">
          {/* <button className="w-full py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-500 transition duration-200 cursor-pointer">
            Join Server By Invite Code
          </button>
          */}

          <NavLink
            to={`/friends`}
            className={({ isActive }) =>
              `m-1 flex items-center gap-3 p-2 cursor-pointer hover:bg-gray-600 rounded-lg transition-colors min-h-12 ${
                isActive ? "bg-blue-600" : ""
              }`
            }
          >
            <IconButtonWithText
              icon={<BsFillPeopleFill className="text-gray-400" />}
              label="Friends"
            />
          </NavLink>

          <div className="w-full h-px bg-gray-600 my-3" />

          <div className="text-gray-300 text-sm font-medium flex justify-between items-center mb-2">
            <span>Direct Messages</span>
            <BiPlus className="text-lg cursor-pointer hover:text-white transition" />
          </div>

          <div
            ref={ContainerRef}
            className="flex flex-1 flex-col py-4 px-2 w-full overflow-y-auto no-scrollbar-hover"
          >
            {isLoading ? (
              <div className="flex h-full justify-center items-center">
                <Loading_Spinner />
              </div>
            ) : (
              data?.pages.map((group, index) => (
                <Fragment key={index}>
                  {group.map((conversation, index) => (
                    <NavLink
                      id={conversation.id}
                      key={index}
                      to={`/conversation/${conversation?.id}/${conversation?.channelId}`}
                      className={({ isActive }) =>
                        `group/button m-1 flex items-center rounded-lg transition
                    ${isActive ? "bg-gray-600" : "hover:bg-gray-600"}`
                      }
                    >
                      <IconButtonWithText
                        icon={
                          <Avatar_Icon
                            Size="h-10 w-10"
                            userName={conversation?.targetProfile?.displayName}
                            avatarUrl={GetFileUrl(
                              conversation?.targetProfile?.imageId
                            )}
                          />
                        }
                        label={conversation?.targetProfile?.displayName}
                      />
                    </NavLink>
                  ))}
                </Fragment>
              ))
            )}
          </div>
        </div>
      </Sidebar_Navigation>

      {hasActiveRoute ? (
        <Outlet />
      ) : (
        <div className="flex-1 flex justify-center items-center text-white text-2xl font-bold select-none">
          Welcome to our site!
        </div>
      )}
    </div>
  );
}

export default Home_Page;
