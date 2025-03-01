import React from "react";
import {
  CiMicrophoneOff,
  CiMicrophoneOn,
  CiVideoOff,
  CiVideoOn,
} from "react-icons/ci";
import { LuScreenShare, LuScreenShareOff } from "react-icons/lu";
import { SlCallEnd } from "react-icons/sl";

interface VideoChat_ControlPanel_Props {
  screenPlay: () => void;
  handleEndCall: () => void;
  isSharingScreen: boolean;
  isAudioOn: boolean;
  toggleMute: () => void;
  isVideoOn: boolean;
  toggleVideo: () => void;
  isAudioCall: boolean;
}

const VideoChat_ControlPanel = ({
  isAudioOn,
  toggleMute,
  isVideoOn,
  toggleVideo,
  screenPlay,
  isSharingScreen,
  handleEndCall,
  isAudioCall,
}: VideoChat_ControlPanel_Props) => {
  return (
    <div className="flex justify-center m-2">
      <div className="flex gap-4 p-4 bg-gray-800/80 backdrop-blur-sm rounded-full shadow-lg">
        <Control_Button
          icon={isAudioOn ? <CiMicrophoneOn /> : <CiMicrophoneOff />}
          label={isAudioOn ? "Mute" : "Unmute"}
          active={isAudioOn}
          onClick={toggleMute}
        />

        {!isAudioCall && (
          <>
            <Control_Button
              icon={isVideoOn ? <CiVideoOn /> : <CiVideoOff />}
              label={isVideoOn ? "Turn off video" : "Turn on video"}
              active={isVideoOn}
              onClick={toggleVideo}
            />

            <Control_Button
              className="hidden md:block"
              icon={!isSharingScreen ? <LuScreenShare /> : <LuScreenShareOff />}
              label={!isSharingScreen ? "Stop sharing" : "Share screen"}
              active={!isSharingScreen}
              onClick={screenPlay}
            />
          </>
        )}
        <Control_Button
          icon={<SlCallEnd />}
          label="End call"
          danger
          onClick={handleEndCall}
        />
      </div>
    </div>
  );
};

interface Control_Button_Props {
  icon: React.ReactNode;
  label: string;
  danger?: boolean;
  active?: boolean;
  className?: string;
  onClick: () => void;
}

const Control_Button = ({
  icon,
  label,
  danger = false,
  active = true,
  onClick,
  className,
}: Control_Button_Props) => (
  <button
    title={label}
    onClick={onClick}
    className={`
      p-3 rounded-full transition-all
      ${
        danger
          ? "bg-red-600 hover:bg-red-500 active:bg-red-700"
          : active
          ? "bg-gray-600 hover:bg-gray-500 active:bg-gray-700"
          : "bg-gray-700 hover:bg-gray-600 active:bg-gray-800"
      }
      focus:outline-none focus:ring-2 focus:ring-white focus:ring-offset-2 focus:ring-offset-gray-900 ${className}
    `}
  >
    <div className="w-6 h-6 flex items-center justify-center">{icon}</div>
  </button>
);

export default VideoChat_ControlPanel;
