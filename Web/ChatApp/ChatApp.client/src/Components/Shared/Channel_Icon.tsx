import { BiHash } from "react-icons/bi";
import { FaVideo } from "react-icons/fa";
import { PiUserSoundFill } from "react-icons/pi";
import { ChannelResponseDto } from "../../Common/Dtos";

interface Channel_Icon_Props {
  Channel: ChannelResponseDto;
  className?: string;
}

const Channel_Icon = ({ Channel, className }: Channel_Icon_Props) => {
  switch (Channel.type) {
    case "TEXT":
      return <BiHash className={className} />;

    case "VIDEO":
      return <FaVideo className={className}  />;

    case "AUDIO":
      return <PiUserSoundFill className={className}  />;
  }
};

export default Channel_Icon;
