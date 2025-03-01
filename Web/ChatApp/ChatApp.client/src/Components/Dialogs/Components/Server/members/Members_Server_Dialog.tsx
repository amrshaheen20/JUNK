import { Fragment, MouseEvent, useRef } from "react";
import { MemberResponseDto, MemberRole } from "../../../../../Common/Dtos";
import { GetFileUrl } from "../../../../../Common/GetMediaUrl";
import { useCommonContext } from "../../../../../Contexts/Common_Context";
import { useDialogContext } from "../../../../../Contexts/Dialog_Context";
import { useListQuery } from "../../../../../Hooks/Querys/use-list-query";
import { useInfiniteScroll } from "../../../../../Hooks/use-Infinite-scroll";
import Avatar_Icon from "../../../../Shared/Avatar_Icon";
import Loading_Spinner from "../../../../Shared/Loading_Spin";

const Members_Server_Dialog = () => {
  const { isOpened, closeDialog, activeDialog, data, openDialog } =
    useDialogContext();

  const isActive = isOpened && activeDialog === "MembersServer";
  const { server } = data;

  const { Member } = useCommonContext().globalData;

  const {
    data: Pagesdata,
    isLoading,
    isError,
    ...queryDetails
  } = useListQuery<MemberResponseDto>({
    queryKey: `/servers/${server?.id}/members`,
    queryUrl: `/servers/${server?.id}/members`,
    limit: 50,
    SocketKey: `${server?.id}:member`,
    enabled: !!server?.id,
  });

  const ContainerRef = useRef(null);

  useInfiniteScroll({
    ContainerRef,
    ...queryDetails,
  });

  if (!isActive || !server || !Pagesdata) return null;

  const handleRemoveMember = (
    e: MouseEvent<HTMLButtonElement>,
    memberId?: string
  ) => {
    e.stopPropagation();

    openDialog("DeleteMember", {
      queryUrl: `/servers/${server.id}/Members/${memberId}`,
      server: server,
    });
  };

  const handleProfileOpen = (
    e: MouseEvent<HTMLDivElement>,
    member?: MemberResponseDto
  ) => {
    e.preventDefault();
    openDialog("ProfileDialog", {
      profileId: member?.id,
    });
  };

  if (isLoading) {
    return <Loading_Spinner className="text-red-400 w-full" />;
  }

  if (isError) {
    return (
      <div className="flex flex-1 justify-center items-center h-full bg-gray-800 w-full ">
        <p>Something went wrong!</p>
        <button
          type="button"
          onClick={closeDialog}
          className="w-full bg-green-500 cursor-pointer hover:bg-green-600 text-white py-3 rounded-lg mt-4"
          aria-label="Close members dialog"
        >
          Close
        </button>
      </div>
    );
  }

  return (
    <>
      <h2 className="text-3xl font-semibold mb-6 text-center">
        Members of {server.name}
      </h2>

      <div
        ref={ContainerRef}
        className="space-y-4 overflow-y-auto max-h-[400px]"
      >
        {Pagesdata?.pages.map((group, index) => (
          <Fragment key={index}>
            {group.map((member) => (
              <div
                key={member?.id}
                className="flex items-center justify-between py-4 px-6 rounded-lg hover:bg-gray-100 transition duration-200 cursor-pointer"
                onClick={(e) => handleProfileOpen(e, member || undefined)}
              >
                <div className="flex items-center space-x-4">
                  <Avatar_Icon
                    userName={member?.userName}
                    avatarUrl={GetFileUrl(member?.imageId)}
                    Size="w-14 h-14"
                  />
                  <div className="flex flex-col">
                    <span className="font-semibold text-lg text-gray-800">
                      {member?.userName}
                    </span>
                    <span className="text-sm text-gray-500">
                      {member?.role}
                    </span>
                  </div>
                </div>

                {(Member?.role == MemberRole.ADMIN ||
                  Member?.role == MemberRole.MODERATOR) &&
                  member?.role !== MemberRole.ADMIN &&
                  member?.role !== MemberRole.MODERATOR && (
                    <button
                      onClick={(e) => handleRemoveMember(e, member?.id)}
                      className="p-2 text-red-500 hover:text-white hover:bg-red-500 rounded-md transition duration-200 cursor-pointer"
                    >
                      Remove
                    </button>
                  )}
              </div>
            ))}
          </Fragment>
        ))}
      </div>

      <button
        type="button"
        onClick={closeDialog}
        className="w-full bg-green-500 cursor-pointer hover:bg-green-600 text-white py-3 rounded-lg mt-4"
        aria-label="Close members dialog"
      >
        Close
      </button>
    </>
  );
};

export default Members_Server_Dialog;
