import { MemberResponseDto, ServerResponseDto } from "../../../Common/Dtos";
import { GetFileUrl } from "../../../Common/GetMediaUrl";
import { useDialogContext } from "../../../Contexts/Dialog_Context";
import { useListQuery } from "../../../Hooks/Querys/use-list-query";
import Avatar_Icon from "../../Shared/Avatar_Icon";

interface Server_SideMenu_Members_Props {
  server: ServerResponseDto;
}

function Server_SideMenu_Members({ server }: Server_SideMenu_Members_Props) {
  const { openDialog } = useDialogContext();

  const MAX_MEMBERS = 3;

  const { data } = useListQuery<MemberResponseDto>({
    queryKey: `/Servers/${server.id}/members`,
    queryUrl: `/Servers/${server.id}/members`,
    limit: MAX_MEMBERS + 1,
    SocketKey: `${server.id}:member`,
    enabled: !!server,
  });

  const members = data?.pages.flatMap((page) => page) || [];

  return (
    <div className="flex flex-col items-center space-y-3 py-3">
      <span className="text-sm font-semibold text-gray-300">Members</span>

      <div
        className="group flex items-center justify-center space-x-2"
        onClick={() => {
          openDialog("MembersServer", { server: server });
        }}
      >
        <div className="flex -space-x-2">
          {members.slice(0, MAX_MEMBERS).map((member, index) => (
            <Avatar_Icon
              key={index}
              userName={member?.userName}
              avatarUrl={GetFileUrl(member?.imageId)}
            />
          ))}
          {server.membersCount > MAX_MEMBERS && (
            <span className="flex h-12 w-12 items-center justify-center rounded-full border-2 border-gray-700 shadow-md bg-blue-500 text-white text-xs font-medium transition duration-200 cursor-pointer">
              +{server.membersCount - MAX_MEMBERS}
            </span>
          )}
        </div>
      </div>
      <div className="w-4/5 h-px bg-gray-500 opacity-50" />
    </div>
  );
}

export default Server_SideMenu_Members;
