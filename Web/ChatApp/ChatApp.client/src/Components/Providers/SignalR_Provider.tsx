import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { memo, ReactNode, useEffect, useRef } from "react";
import { GetAuthInfo, RemoveAuth } from "../../Auth/Profile";
import { IS_LOGGING, log, logError, logWarn } from "../../Common/Logger";
import { useCommonContext } from "../../Contexts/Common_Context";

type SignalR_Provider_Props = {
  children?: ReactNode;
};

type SignalR_Error = {
  statusCode: number;
  message: string;
};

const SignalR_Provider = memo(({ children }: SignalR_Provider_Props) => {
  const {
    setConnection,
    setIsAppServerDown,
    connection: previousConnection,
  } = useCommonContext();
  const isReconnecting = useRef(false);
  const isInitialized = useRef(false);

  useEffect(() => {
    if (isInitialized.current) return;

    if (
      previousConnection &&
      previousConnection.state === HubConnectionState.Connected
    ) {
      log(
        "✅ Already connected! Connection ID:",
        previousConnection.connectionId
      );
      return;
    }

    const token = GetAuthInfo()?.token;

    async function startConnection(connection: HubConnection) {
      try {
        if (connection.state === HubConnectionState.Connected) {
          setIsAppServerDown(false);
          isReconnecting.current = false;
          return;
        }
        log("🤔 Trying to start connection!");
        await connection.start();
        log("✅ Connection started successfully!", connection);
        setIsAppServerDown(false);
        isReconnecting.current = false;
        setConnection(connection);
      } catch (err: unknown) {
        logError("❌ Failed to start connection:", err);
        setIsAppServerDown(true);
        isReconnecting.current = false;

        if (
          (err as SignalR_Error)?.statusCode === 401 ||
          (err as SignalR_Error)?.message?.includes("401")
        ) {
          logWarn("🔒 Unauthorized! Redirecting to login...");
          //this should be refresh token and if fail remove auth
          RemoveAuth();
          return;
        }

        if (!isReconnecting.current) {
          log("⏳ Retrying connection in 30s...");
          isReconnecting.current = true;

          const retryTimeout = setTimeout(() => {
            startConnection(connection);
            isReconnecting.current = false;
          }, 30000);

          return () => clearTimeout(retryTimeout);
        }
      }
    }

    async function restartConnection(connection: HubConnection) {
      if (connection.state === HubConnectionState.Disconnected) {
        log("♻️ Restarting connection...");
        isReconnecting.current = true;
        await startConnection(connection);
      }
    }

    if (!token) {
      logWarn(
        "⚠️ No authentication token found. SignalR will not connect."
      );
      return;
    }
    const connection: HubConnection = new HubConnectionBuilder()
      .withUrl(import.meta.env.VITE_API_URL_IO, {
        accessTokenFactory: () => token || "",
        transport: HttpTransportType.LongPolling,
      })
      .configureLogging(IS_LOGGING ? LogLevel.Debug : LogLevel.None)
      .build();

    // connection.on("ReceiveNotification", (message: MessageResponseDto) => {
    //   log("🔔 Notification received:", message.id, message);

    //   // Send notification data to service worker
    //   navigator.serviceWorker.ready.then(registration => {
    //     registration.active?.postMessage({ message });
    //   });
    // });

    connection.onreconnecting((error) => {
      logWarn("🔄 Attempting to reconnect...", error);
    });

    connection.onreconnected((connectionId) => {
      log("✅ Reconnected successfully! Connection ID:", connectionId);
    });

    connection.onclose(async (error) => {
      logError("❌ Connection lost permanently. Restarting...", error);
      await restartConnection(connection);
    });

    startConnection(connection);
    isInitialized.current=true;

    window.onbeforeunload = () => {
      if (connection.connectionId) connection.stop();
      return null;
    };

    // return () => {
    //   log("🔌 Cleaning up SignalR connection...");
    //   if (connection.connectionId) connection.stop();
    // };

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return <>{children}</>;
});

export default SignalR_Provider;
