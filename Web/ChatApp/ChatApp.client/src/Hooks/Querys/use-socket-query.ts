import { InfiniteData, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import type { DataWithId, MessageResponseDto } from "../../Common/Dtos";
import { useCommonContext } from "../../Contexts/Common_Context";
import { log } from "../../Common/Logger";

interface UseMessageSocketQueryProps {
  queryKey: string;
  SocketKey?: string;
  topDown?: boolean;
  enabled?: boolean;
  autoupdate?: boolean;
  hasPreviousPage?: boolean;
}

export function useSocketQuery<T extends DataWithId>({
  queryKey,
  SocketKey,
  topDown = true,
  enabled = true,
  autoupdate = true,
  hasPreviousPage,
}: UseMessageSocketQueryProps) {
  const { connection } = useCommonContext();
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!connection || !enabled || !SocketKey) return;

    log(`✅ Listening on: `, SocketKey);

    const queryHandler = {
      add: (newData: T) => {
        if (!autoupdate || hasPreviousPage) return;

        queryClient.setQueryData<InfiniteData<T[]>>([queryKey], (old) => {
          if (!old || !old.pages || old.pages.length === 0) {
            return {
              ...old,
              pages: [[newData]],
              pageParams: old?.pageParams ?? [newData.id],
            };
          }

          const messageExists = old.pages.some((page) =>
            page.some((m) => m.id === newData.id)
          );
          if (messageExists) return old;

          // const updatedPages = topDown
          //   ? [[newData], ...old.pages]
          //   : [...old.pages, [newData]];

          const updatedPages = [...old.pages];

          if (topDown) {
            updatedPages[0] = [newData, ...updatedPages[0]];
          } else {
            updatedPages[updatedPages.length - 1] = [
              ...updatedPages[updatedPages.length - 1],
              newData,
            ];
          }

          return { ...old, pages: updatedPages, pageParams: old.pageParams };
        });
      },

      update: (updatedData: MessageResponseDto) => {
        if (!autoupdate) return;
        queryClient.setQueryData<InfiniteData<MessageResponseDto[]>>(
          [queryKey],
          (old) => {
            if (!old || !old.pages) return old;

            const updatedPages = old.pages.map((page) =>
              page.map((msg) => (msg.id === updatedData.id ? updatedData : msg))
            );

            return { ...old, pages: updatedPages };
          }
        );
      },

      delete: (deletedData: MessageResponseDto) => {
        if (!autoupdate) return;
        queryClient.setQueryData<InfiniteData<MessageResponseDto[]>>(
          [queryKey],
          (old) => {
            if (!old || !old.pages) return old;

            const updatedPages = old.pages.map((page) =>
              page.filter((msg) => msg.id !== deletedData.id)
            );

            return { ...old, pages: updatedPages };
          }
        );
      },
    };

    connection.on(`${SocketKey}:add`, queryHandler.add);
    connection.on(`${SocketKey}:update`, queryHandler.update);
    connection.on(`${SocketKey}:delete`, queryHandler.delete);

    return () => {
      log(`❌ Stopped listening on: `, SocketKey);
      connection.off(`${SocketKey}:add`, queryHandler.add);
      connection.off(`${SocketKey}:update`, queryHandler.update);
      connection.off(`${SocketKey}:delete`, queryHandler.delete);
    };
  }, [
    connection,
    queryClient,
    queryKey,
    SocketKey,
    topDown,
    enabled,
    autoupdate,
    hasPreviousPage,
  ]);

  return {
    connection,
  };
}
