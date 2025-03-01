import { ReactNode, useEffect } from "react";
import { IoIosArrowForward } from "react-icons/io";
import { IoClose } from "react-icons/io5";
import { useParams } from "react-router-dom";
import { useCommonContext } from "../../Contexts/Common_Context";

interface Sidebar_Navigation_Props {
  children: ReactNode;
}

function Sidebar_Navigation({ children }: Sidebar_Navigation_Props) {
  const { channelId } = useParams<{
    serverId?: string;
    channelId?: string;
  }>();
  const { isSideBarOpened, setIsSideBarOpened } = useCommonContext();

  useEffect(() => {
    setIsSideBarOpened(channelId == undefined );

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [channelId]);

  return (
    <>
      <div
        className={`fixed md:relative top-0 left-0 bg-gray-700  flex flex-col h-full space-y-2 transition-transform duration-300 z-50
      ${isSideBarOpened ? "translate-x-0 left-16" : "-translate-x-full"}
      w-64 min-w-64 md:translate-x-0  md:left-0`}
      >
        {isSideBarOpened && (
          <button
            onClick={() => setIsSideBarOpened(false)}
            className="fixed top-1/2 right-[-40px] md:hidden bg-gray-900 text-white p-2 rounded-full shadow-md cursor-pointer"
          >
            <IoClose className="text-2xl" />
          </button>
        )}
        {children}
      </div>

      {!isSideBarOpened && !channelId && (
        <button
          onClick={() => setIsSideBarOpened(true)}
          className="absolute top-1/2  md:hidden hover:bg-gray-700 hover:rounded-full z-50 cursor-pointer"
        >
          <IoIosArrowForward className="text-4xl" />
        </button>
      )}
    </>
  );
}

export default Sidebar_Navigation;
