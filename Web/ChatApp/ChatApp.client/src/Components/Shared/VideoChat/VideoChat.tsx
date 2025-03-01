import { useCommonContext } from "../../../Contexts/Common_Context";
import { useVideoChat } from "../../../Hooks/use-video-chat";
import VideoChat_ControlPanel from "./VideoChat_ControlPanel";
import VideoChat_VideoCall from "./VideoChat_VideoCall";

interface VideoChatProps {
  channelId: string;
  isAudioCall: boolean;
}

const VideoChat = ({ channelId, isAudioCall = false }: VideoChatProps) => {
  const { globalData } = useCommonContext();

  const {
    localStream,
    peerConnections,
    isScreenSharing,
    toggleScreen,
    toggleVideo,
    isMuted,
    toggleMute,
    isVideoOff,
    handleEndCall,
    isJoined,
    handleCall,
  } = useVideoChat(channelId,isAudioCall);


  if (!isJoined) {
    return (
      <div className="flex flex-col items-center justify-center h-screen bg-gray-900 text-white p-4">
        <h1 className="text-2xl font-semibold mb-4">
          Join {!isAudioCall ? "Video" : "Audio"} Chat
        </h1>
        <div className="w-64 h-48 bg-black rounded-lg overflow-hidden mb-4">
          <VideoChat_VideoCall
            peer={{
              profile: globalData.Profile,
              mediaStream: localStream,
              state: { isMuted, isVideoOff },
            }}
            MuteVideo={true}
          />
        </div>
        <div className="flex gap-4 mb-4">
          <button onClick={toggleMute} className="p-2 bg-gray-700 rounded-md">
            {isMuted ? "Unmute" : "Mute"}
          </button>
          {!isAudioCall && (
            <button
              onClick={toggleVideo}
              className="p-2 bg-gray-700 rounded-md"
            >
              {isVideoOff ? "Turn Camera On" : "Turn Camera Off"}
            </button>
          )}
        </div>
        <button
          onClick={handleCall}
          className="p-2 bg-green-600 rounded-md w-64"
        >
          Join Call
        </button>
      </div>
    );
  }

  return (
    <div className="flex flex-1 bg-gray-900 text-white p-2 overflow-hidden">
      <div className="flex flex-col w-full">
        <div className="relative flex-1 bg-black rounded-xl shadow-lg overflow-hidden">
          {Object.keys(peerConnections).length == 1 ? (
            <>
              <VideoChat_VideoCall
                peer={Object.values(peerConnections)[0] || undefined}
                Tag={1}
              />
              <div className="absolute bottom-2 bg-black right-2 w-24 h-35 rounded-md overflow-hidden shadow-md border-2 border-white/50">
                <VideoChat_VideoCall
                  peer={{
                    profile: globalData.Profile,
                    mediaStream: localStream,
                    state: {
                      isMuted,
                      isVideoOff: !isScreenSharing ? isVideoOff : false,
                    },
                  }}
                  MuteVideo={true}
                />
              </div>
            </>
          ) : (
            <VideoChat_VideoCall
              peer={{
                profile: globalData.Profile,
                mediaStream: localStream,
                state: {
                  isMuted,
                  isVideoOff: !isScreenSharing ? isVideoOff : false,
                },
              }}
              Tag={1}
              MuteVideo={true}
            />
          )}
        </div>
        {Object.keys(peerConnections).length > 1 && (
          <div className="relative">
            <div className="flex gap-4 p-2 overflow-x-auto h-40">
              {Object.entries(peerConnections).map(([id, obj]) => (
                <div
                  key={id}
                  className="flex-shrink-0 relative basis-48 h-full bg-gray-800 rounded-lg overflow-hidden transition-transform hover:scale-105"
                >
                  <VideoChat_VideoCall Tag={2} peer={obj}  />
                </div>
              ))}
            </div>
          </div>
        )}
        <VideoChat_ControlPanel
          screenPlay={toggleScreen}
          isSharingScreen={isScreenSharing}
          handleEndCall={handleEndCall}
          toggleVideo={toggleVideo}
          isAudioOn={!isMuted}
          isVideoOn={!isVideoOff}
          toggleMute={toggleMute}
          isAudioCall={isAudioCall}
        />
      </div>
    </div>
  );
};

export default VideoChat;
