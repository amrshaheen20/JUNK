import {
  DefinedInitialDataInfiniteOptions,
  OmitKeyof,
  useInfiniteQuery,
} from "@tanstack/react-query";
import { Api } from "../../Common/axios";
import { DataWithId } from "../../Common/Dtos";
import { useEffect } from "react";
import { useSocketQuery } from "./use-socket-query";


export type directionType = "forward" | "backward" | "around";

interface FetchDataParams {
  queryUrl: string;
  cursor?: string | null;
  limit?: number;
  direction?: directionType;
}

const fetchData = async <T>({
  queryUrl,
  cursor,
  limit,
  direction,
}: FetchDataParams): Promise<T[]> => {
  const { data } = await Api.get<T[]>(queryUrl, {
    params:
      direction === "forward"
        ? { before: cursor, limit }
        : direction === "backward"
        ? { after: cursor, limit }
        : { around: cursor, limit },
  });

  return data;
};

interface useListQueryProps<T>
  extends OmitKeyof<
    DefinedInitialDataInfiniteOptions<T[]>,
    | "queryKey"
    | "queryFn"
    | "initialPageParam"
    | "getNextPageParam"
    | "initialData"
  > {
  queryKey: string;
  queryUrl: string;
  limit?: number;
  around?: string;
  SocketKey?: string;
}

export function useListQuery<T extends DataWithId>({
  queryKey,
  queryUrl,
  limit = 50,
  around,
  SocketKey,
  ...queryOptions
}: useListQueryProps<T>) {
  const result = useInfiniteQuery<T[]>({
    queryKey: [queryKey],
    queryFn: ({ pageParam, direction }) => {
      return fetchData<T>({
        queryUrl,
        cursor: around && !pageParam ? around : (pageParam as string),
        limit,
        direction: around && !pageParam ? "around" : direction,
      });
    },
    initialPageParam: null,
    getNextPageParam: (lastPage) =>
      lastPage.length > 0 ? lastPage[lastPage.length - 1]?.id : null,
    getPreviousPageParam: (firstPage, allPages) => {
      if (allPages.length >= 0 && !around) {
        return null;
      }

      return firstPage.length > 0 ? firstPage[0]?.id : null;
    },

    refetchOnMount: false,
    refetchOnReconnect: false,
    refetchOnWindowFocus: false,
    ...queryOptions,
  });


  useEffect(() => {
    if (!around) return;

    const isAroundMissing = !result.data?.pages.some((page) =>
      page.some((item) => item.id === around)
    );

    if (isAroundMissing) {
      result.refetch();
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [around, result.data]);

  useSocketQuery({
    ...result,
    queryKey: queryKey,
    SocketKey,
    enabled: (queryOptions.enabled as boolean) || !!SocketKey,
  });

  return {
    ...result
  };
}
