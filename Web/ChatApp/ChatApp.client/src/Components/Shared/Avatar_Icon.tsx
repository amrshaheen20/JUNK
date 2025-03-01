import { ComponentProps } from "react";
import { getAvatarColor } from "../../Common/AvatarColor";

interface Avatar_Icon_Props extends ComponentProps<"div"> {
  avatarUrl?: string;
  userName?: string;
  Size?: string;
  className?: string;
  isOnline?: boolean; 
  showStatus?: boolean; 
}

const Avatar_Icon = ({
  avatarUrl,
  userName,
  Size = "w-12 h-12",
  className,
  isOnline = false, 
  showStatus = false, 
  ...props
}: Avatar_Icon_Props) => {
  return (
    <div
      {...props}
      className={`group relative flex flex-row items-center min-w-fit min-h-fit  ${className}`}
    >
      <div title={userName} className="relative group">
        {avatarUrl ? (
          <img
            src={avatarUrl}
            className={`${Size} rounded-full border-2 border-gray-700 shadow-md`}
            loading="lazy"
          />
        ) : (
          <div
            className={`${Size} border-2 border-gray-700 shadow-md rounded-full flex items-center justify-center text-white font-semibold ${className}`}
            style={{
              backgroundColor: getAvatarColor(userName),
            }}
            aria-label={userName}
          >
            {userName?.charAt(0)?.toUpperCase() ?? ""}
          </div>
        )}

        {showStatus && (
          <div
            className={`absolute bottom-0 right-0 w-4 h-4 rounded-full border-2 border-white ${
              isOnline ? "bg-green-500" : "bg-gray-400"
            }`}
          />
        )}
      </div>
    </div>
  );
};

export default Avatar_Icon;
