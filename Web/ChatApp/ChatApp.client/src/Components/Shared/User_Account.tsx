import { FaUserEdit } from "react-icons/fa";
import { IoMdLogOut } from "react-icons/io";
import { RemoveAuth } from "../../Auth/Profile";
import { GetFileUrl } from "../../Common/GetMediaUrl";
import { useCommonContext } from "../../Contexts/Common_Context";
import Avatar_Icon from "./Avatar_Icon";
import Dropdown_Menu from "./Dropdown_Menu";
import IconButtonWithText from "./IconButtonWithText";
import { useDialogContext } from "../../Contexts/Dialog_Context";

function User_Account() {
  const { openDialog } = useDialogContext();

  const { globalData } = useCommonContext();
  const { Profile } = globalData;

  return (
    <div className="fixed right-0 m-1 flex flex-col items-end z-50 select-none">
      <Dropdown_Menu
        className="right-0 mt-2 w-48 bg-gray-900"
        dropdown_Button={
          <Avatar_Icon
            className="shadow-lg rounded-full hover:cursor-pointer"
            userName={Profile?.displayName}
            avatarUrl={GetFileUrl(Profile?.imageId)}
          />
        }
      >
        <p
          className="relative w-full text-center font-semibold text-white 
             py-2 px-4 truncate opacity-100 hover:!bg-transparent
             after:content-[''] after:block after:h-[1px] after:bg-gray-700 after:mt-2 "
        >
          {Profile?.displayName}
        </p>

        <IconButtonWithText
          onClick={() => {
            openDialog("EditProfile");
          }}
          className="hover:!bg-gray-800"
          label="Edit Profile"
          icon={<FaUserEdit />}
        />
        <IconButtonWithText
          onClick={RemoveAuth}
          className="text-red-500 hover:!bg-red-500 hover:!text-white"
          label="Log Out"
          icon={<IoMdLogOut />}
        />
      </Dropdown_Menu>
    </div>
  );
}

export default User_Account;
