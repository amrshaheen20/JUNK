import { FunctionComponent, useEffect, useRef, useState } from "react";

import Avatar_Icon from "../Avatar_Icon";
import { GetFileUrl } from "../../../Common/GetMediaUrl";
import { PeerProps } from "../../../Hooks/use-video-chat";
import { CiMicrophoneOff } from "react-icons/ci";

interface VideoChat_VideoCall_Props {
  peer: PeerProps;
  className?: string;
  Tag?: number;
  MuteVideo?: boolean;
}

const VideoChat_VideoCall: FunctionComponent<VideoChat_VideoCall_Props> = ({
  peer,
  MuteVideo = false,
  Tag = 0,
  className,
  ...props
}) => {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [isSpeaking] = useState(false);

  useEffect(() => {
    if (videoRef.current && peer.mediaStream) {
      videoRef.current.srcObject = peer.mediaStream;
    }
  }, [peer.mediaStream]);

  const Label = () => {
    return (
      <span className="text-sm flex items-center gap-1 text-white">
        {peer.profile?.displayName}
        <div className="flex items-center">
          {peer.state?.isMuted && (
            <CiMicrophoneOff className="w-4 h-4 text-red-500" />
          )}
          {/* {isVideoOff && <CiVideoOff className="w-4 h-4 text-red-500" />} */}
        </div>
      </span>
    );
  };

  return (
    <div
      {...props}
      className={`relative w-full h-full overflow-hidden rounded-lg ${className}`}
    >
      {peer.state?.isVideoOff && (
        <div className="absolute inset-0 flex items-center justify-center">
          <Avatar_Icon
            className={`shadow-lg rounded-full hover:cursor-pointer animate- ${
              isSpeaking ? "animate-shake border-2 border-white" : "border-none"
            }`}
            userName={peer.profile?.displayName}
            avatarUrl={GetFileUrl(peer.profile?.imageId)}
          />
        </div>
      )}

      {peer.mediaStream  && (
        <video
          ref={videoRef}
          className={`w-full h-full  aspect-video ${
            Tag === 1 ? "object-contain" : "object-cover"
          }`}
          autoPlay
          playsInline
          muted={MuteVideo}
        />
      )}

      {Tag === 1 && (
        <div className="absolute bottom-4 left-4 bg-gray-800/80 px-3 py-1 rounded-lg">
          {Label()}
        </div>
      )}

      {Tag === 2 && (
        <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/80 to-transparent p-2">
          {Label()}
        </div>
      )}
    </div>
  );
};

export default VideoChat_VideoCall;
