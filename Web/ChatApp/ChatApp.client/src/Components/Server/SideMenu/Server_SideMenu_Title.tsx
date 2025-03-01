import { FaLink } from "react-icons/fa";
import { FaTrash } from "react-icons/fa6";
import { FiEdit } from "react-icons/fi";
import { IoIosArrowDown, IoMdClose, IoMdLogOut } from "react-icons/io";
import { PiPlusBold } from "react-icons/pi";
import {
  MemberResponseDto,
  MemberRole,
  ServerResponseDto,
} from "../../../Common/Dtos";
import { useDialogContext } from "../../../Contexts/Dialog_Context";
import Dropdown_Menu from "../../Shared/Dropdown_Menu";
import IconButtonWithText from "../../Shared/IconButtonWithText";

interface Server_SideMenu_Title_Props {
  Server: ServerResponseDto;
  Member: MemberResponseDto;
}

function Server_SideMenu_Title({
  Server,
  Member,
}: Server_SideMenu_Title_Props) {
  const { openDialog } = useDialogContext();

  return (
    <div className="w-full border-b-1 border-gray-600 select-none">
      <Dropdown_Menu
        className="absolute left-1/2 transform -translate-x-1/2 w-48 bg-gray-900"
        dropdown_Button={({ open }) => (
          <div className="relative p-4 flex justify-between items-center hover:bg-gray-800 hover:rounded-sm cursor-pointer transition">
            <h2
              className="text-xl font-semibold text-white truncate w-full text-left"
              title={Server?.name}
            >
              {Server?.name}
            </h2>

            <div className="ml-1.5 text-white text-2xl w-10 cursor-pointer">
              <IoIosArrowDown className={`${open && "hidden"}`} />
              <IoMdClose className={`${!open && "hidden"}`} />
            </div>
          </div>
        )}
      >
        <IconButtonWithText
          className="hover:!bg-gray-800"
          onClick={() => openDialog("InviteLinkServer", { server: Server })}
          icon={<FaLink />}
          label="Invite Link"
        />

        {(Member.role === MemberRole.ADMIN ||
          Member.role === MemberRole.MODERATOR) && (
          <>
            <IconButtonWithText
              className="hover:!bg-gray-800"
              onClick={() => openDialog("EditServer", { server: Server })}
              icon={<FiEdit />}
              label="Edit"
            />

            <IconButtonWithText
              className="hover:!bg-gray-800"
              onClick={() => openDialog("CreateChannel", { server: Server })}
              icon={<PiPlusBold />}
              label="Create Channel"
            />
          </>
        )}

        {Member.role !== MemberRole.ADMIN && (
          <IconButtonWithText
            onClick={() =>
              openDialog("LeaveServer", {
                queryUrl: `/Servers/${Server.id}/Leave`,
              })
            }
            icon={<IoMdLogOut />}
            label="Leave server"
            className="text-red-500 hover:!bg-red-500 hover:!text-white"
          />
        )}

        {Member.role === MemberRole.ADMIN && (
          <IconButtonWithText
            onClick={() =>
              openDialog("DeleteServer", { queryUrl: `/Servers/${Server.id}` })
            }
            icon={<FaTrash />}
            label="Delete server"
            className="text-red-500 hover:!bg-red-500 hover:!text-white"
          />
        )}
      </Dropdown_Menu>
    </div>
  );
}

export default Server_SideMenu_Title;
