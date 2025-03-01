import { MemberResponseDto, ServerResponseDto } from "../../../Common/Dtos";
import Sidebar_Navigation from "../../Shared/Sidebar_Navigation";
import Server_SideMenu_Channels from "./Server_SideMenu_Channels";
import Server_SideMenu_Members from "./Server_SideMenu_Members";
import Server_SideMenu_Title from "./Server_SideMenu_Title";

interface Server_SideMenu_Props {
  Server: ServerResponseDto;
  Member: MemberResponseDto;
}

const Server_SideMenu = ({ Server, Member }: Server_SideMenu_Props) => {
  return (
    <Sidebar_Navigation>
        <Server_SideMenu_Title Server={Server} Member={Member} />
        <Server_SideMenu_Members server={Server} />
        <Server_SideMenu_Channels Server={Server} Member={Member} />
    </Sidebar_Navigation>
  );
};

export default Server_SideMenu;
