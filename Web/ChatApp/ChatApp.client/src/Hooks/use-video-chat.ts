import { useEffect, useState } from "react";
import { ProfileResponseDto } from "../Common/Dtos";
import { useCommonContext } from "../Contexts/Common_Context";
import { useRTC } from "./use-webrtc";
import { log, logError } from "../Common/Logger";

export interface PeerProps {
  profile?: ProfileResponseDto;
  mediaStream?: MediaStream | null;
  peerConnection?: RTCPeerConnection;
  state?: {
    isMuted: boolean;
    isVideoOff: boolean;
  };
}

export const useVideoChat = (channelId: string, isAudioCall: boolean) => {
  const { connection } = useCommonContext();
  const [localStream] = useState<MediaStream>(new MediaStream());
  const [isJoined, setIsJoined] = useState(false);
  const [isMuted, setIsMuted] = useState(true);
  const [isVideoOff, setIsVideoOff] = useState(true);
  const [isScreenSharing, setIsScreenSharing] = useState(false);

  const { peerConnections, BroadcastSignal, ClearAllPeers } = useRTC({
    channelId,
    localStream,
    isMuted,
    isVideoOff,
    enabled: isJoined,
  });

  useEffect(() => {
    SendStateToChannel();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isMuted, isVideoOff, isScreenSharing]);

  const SendStateToChannel = async () => {
    await BroadcastSignal("env-state", {
      isMuted: isMuted,
      isVideoOff: !isScreenSharing ? isVideoOff : false,
    });
  };

  const replaceTrackInPeers = (
    kind: "video" | "audio",
    newTrack: MediaStreamTrack
  ) => {
    Object.values(peerConnections).forEach(async ({ peerConnection }) => {

      if (peerConnection) {
        const senders = peerConnection.getSenders();
        const Sender = senders.find((s) => s.track?.kind === kind);

        if (Sender) {
          // if (!isScreenSharing && isVideoOff) newTrack.enabled = !isVideoOff;
          await Sender.replaceTrack(newTrack);
        } 
      }
    });
  };

  const getUserMedia = async () => {
    try {
      const stream = !isAudioCall
        ? await navigator.mediaDevices.getUserMedia({
            video: true,
            audio: true,
          })
        : await navigator.mediaDevices.getUserMedia({
            audio: true,
          });

      localStream.getTracks().forEach((track) => {
        track.stop();
        localStream.removeTrack(track);
      });

      stream.getTracks().forEach((track) => {
        localStream.addTrack(track);

        if (!isAudioCall && track.kind === "video") track.enabled = !isVideoOff;
        if (track.kind === "audio") track.enabled = !isMuted;

        replaceTrackInPeers(track.kind as "video" | "audio", track);
      });

      setIsScreenSharing(false);
    } catch (error) {
      logError("Error accessing camera:", error);
    }
  };

  const getDisplayMedia = async () => {
    try {
      const stream = await navigator.mediaDevices.getDisplayMedia({
        video: true,
      });

      const newTrack = stream.getVideoTracks()[0];

      const oldTrack = localStream.getVideoTracks()[0];
      if (oldTrack) {
        oldTrack.stop();
        localStream.removeTrack(oldTrack);
      }

      localStream.addTrack(newTrack);
      replaceTrackInPeers("video", newTrack);
      setIsScreenSharing(true);
    } catch (error) {
      logError("Error accessing screen sharing:", error);
    }
  };

  const toggleScreen = async () => {
    try {
      if (isScreenSharing) await getUserMedia();
      else await getDisplayMedia();
    } catch (error) {
      logError("Error toggling screen sharing:", error);
    }
  };

  const toggleMute = () => {
    localStream.getAudioTracks().forEach((track) => {
      track.enabled = !track.enabled;
    });

    setIsMuted(!localStream.getAudioTracks()[0].enabled);
  };

  const toggleVideo = () => {
    if (isAudioCall) return;

    if (isScreenSharing) {
      setIsVideoOff((prev) => !prev);
      return;
    }

    localStream.getVideoTracks().forEach((track) => {
      track.enabled = !track.enabled;
    });

    setIsVideoOff(!localStream.getVideoTracks()[0].enabled);
  };

  useEffect(() => {
    if (!connection) return;

    getUserMedia();

    return () => {
      try {
        ClearAllPeers();
        setIsJoined(false);  
        BroadcastSignal("leave", {});
        log("Leaving channel...");
      } finally {
        localStream.getTracks().forEach((track) => {
          track.stop();
          localStream.removeTrack(track);
        });
      }
    };

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [connection, channelId]);

  const handleEndCall = () => {
    BroadcastSignal("leave", {});
    ClearAllPeers();
    setIsJoined(false);
  };

  const handleCall = () => {
    if (connection) {
      connection.invoke("JoinChannel", channelId);
      setIsJoined(true);
    }
  };

  return {
    localStream,
    peerConnections,
    isScreenSharing,
    toggleScreen,
    isMuted,
    toggleMute,
    isVideoOff,
    toggleVideo,
    handleEndCall,
    isJoined,
    handleCall,
  };
};
