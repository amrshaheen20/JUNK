import { useEffect, useRef, useState } from "react";
import { ProfileResponseDto } from "../Common/Dtos";
import { useCommonContext } from "../Contexts/Common_Context";
import { PeerProps } from "./use-video-chat";
import { log, logError, logWarn } from "../Common/Logger";

interface RTCProps {
  channelId: string;
  localStream: MediaStream;
  isMuted: boolean;
  isVideoOff: boolean;
  enabled: boolean;
}

export const useRTC = ({
  channelId,
  localStream,
  isMuted,
  isVideoOff,
  enabled = false,
}: RTCProps) => {
  const { connection, globalData } = useCommonContext();
  const [peerConnections, setPeerConnections] = useState<{
    [id: string]: PeerProps;
  }>({});
  const peerConnectionsRef = useRef<{ [id: string]: PeerProps }>({});

  const BroadcastSignal = async (
    type: "candidate" | "offer" | "answer" | "leave" | "env-state",
    data: object
  ) => {
    if (!enabled) return;
    Object.keys(peerConnectionsRef.current).forEach(async (peerId) => {
      await SendSignalToChannel(peerId, type, data);
    });
  };

  const SendSignalToChannel = async (
    peerId: string,
    type: "candidate" | "offer" | "answer" | "leave" | "env-state",
    data: object
  ) => {
    if (!enabled) return;

    await connection?.invoke(
      "SendSignalToChannel",
      channelId,
      JSON.stringify({ type, data }),
      globalData.Profile?.id,
      peerId
    );
  };

  const setupPeerConnection = (Profile: ProfileResponseDto) => {
    if (peerConnectionsRef.current[Profile.id]?.peerConnection) {
      logWarn(`👀 Peer connection already exists for ${Profile.id}`);
      return peerConnectionsRef.current[Profile.id].peerConnection!;
    }

    const pc = new RTCPeerConnection({
      iceServers: [{ urls: "stun:stun.l.google.com:19302" }],
    });

    pc.onicecandidate = (event) => {
      if (event.candidate) {
        SendSignalToChannel(Profile.id, "candidate", event.candidate);
        SendSignalToChannel(Profile.id, "env-state", {
          isMuted: isMuted,
          isVideoOff: isVideoOff,
        });
      }
    };

    pc.ontrack = (event) => {
      setPeerConnections((prev) => {
        const updated = {
          ...prev,
          [Profile.id]: { ...prev[Profile.id], mediaStream: event.streams[0] },
        };
        peerConnectionsRef.current = updated;
        return updated;
      });
    };

    pc.oniceconnectionstatechange = () => {
      const state = pc.iceConnectionState;
      log(`✌ ICE connection state for ${Profile.id}: ${state}`);
      if (["disconnected", "failed", "closed"].includes(state)) {
        cleanupPeerConnection(Profile.id);
      }
      if (state === "connected") {
        SendSignalToChannel(Profile.id, "env-state", {
          isMuted: isMuted,
          isVideoOff: isVideoOff,
        });
      }
    };

    localStream.getTracks().forEach((track) => {
      pc.addTrack(track, localStream);
    });

    const newPeerProps: PeerProps = {
      ...peerConnectionsRef.current[Profile.id],
      peerConnection: pc,
      profile: Profile,
      state: { isMuted: !false, isVideoOff: !false },
    };

    peerConnectionsRef.current[Profile.id] = newPeerProps;
    setPeerConnections((prev) => ({ ...prev, [Profile.id]: newPeerProps }));

    return pc;
  };

  const cleanupPeerConnection = (peerId: string) => {
    const peer = peerConnectionsRef.current[peerId];
    if (peer?.peerConnection) {
      peer.peerConnection.close();
    }
    delete peerConnectionsRef.current[peerId];
    setPeerConnections((prev) => {
      const updated = { ...prev };
      delete updated[peerId];
      return updated;
    });
  };

  const createOffer = async (Profile: ProfileResponseDto) => {
    const pc = setupPeerConnection(Profile);
    const offer = await pc.createOffer();
    await pc.setLocalDescription(offer);
    await SendSignalToChannel(Profile.id, "offer", offer);
  };

  const handleOffer = async (
    Profile: ProfileResponseDto,
    offer: RTCSessionDescriptionInit
  ) => {
    const pc = setupPeerConnection(Profile);
    await pc.setRemoteDescription(offer);
    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);
    await SendSignalToChannel(Profile.id, "answer", answer);
  };

  const handleJoin = (Profile: ProfileResponseDto) => {
    if (peerConnectionsRef.current[Profile.id]) return;
    createOffer(Profile);
  };

  const handleSignal = async (
    data: string,
    fromProfile: ProfileResponseDto
  ) => {
    try {
      const Obj = JSON.parse(data);
      const peerConnection =
        peerConnectionsRef.current[fromProfile.id]?.peerConnection;

      switch (Obj.type) {
        case "offer":
          await handleOffer(fromProfile, Obj.data);
          break;
        case "answer":
          if (peerConnection)
            await peerConnection.setRemoteDescription(Obj.data);
          break;
        case "candidate":
          if (peerConnection) await peerConnection.addIceCandidate(Obj.data);
          break;
        case "leave":
          cleanupPeerConnection(fromProfile.id);
          break;
        case "env-state":
          peerConnectionsRef.current[fromProfile.id].state = Obj.data;
          setPeerConnections((prev) => ({
            ...prev,
            [fromProfile.id]: {
              ...prev[fromProfile.id],
              state: Obj.data,
            },
          }));

          break;
        default:
          logError("Invalid signal type:", Obj.type);
      }
    } catch (error) {
      logError("Error handling signal:", error, JSON.parse(data));
    }
  };

  useEffect(() => {
    if (!connection || !enabled) return;

    connection.on(`${channelId}:call:join`, handleJoin);
    connection.on(`${channelId}:call:signal`, handleSignal);

    return () => {
      connection.off(`${channelId}:call:join`, handleJoin);
      connection.off(`${channelId}:call:signal`, handleSignal);

      Object.keys(peerConnectionsRef.current).forEach(cleanupPeerConnection);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [channelId, connection, enabled]);

  const ClearAllPeers = () => {
    if (!enabled) return;

    Object.keys(peerConnectionsRef.current).forEach((x) =>
      cleanupPeerConnection(x)
    );
  };

  return {
    peerConnections,
    SendSignalToChannel,
    BroadcastSignal,
    ClearAllPeers,
  };
};
