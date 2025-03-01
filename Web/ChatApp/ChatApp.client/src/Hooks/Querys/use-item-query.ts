import { OmitKeyof, useQuery, UseQueryOptions } from "@tanstack/react-query";
import { Api } from "../../Common/axios";
import { AxiosRequestConfig } from "axios";

interface FetchDataParams {
  queryUrl: string;
  Config?: AxiosRequestConfig;
}

const fetchData = async <T>({
  queryUrl,
  Config,
}: FetchDataParams): Promise<T> => {
  const { data } = await Api.get<T>(queryUrl, Config);
  return data;
};

interface UseItemQueryProps<T>
  extends OmitKeyof<UseQueryOptions<T>, "queryKey" | "queryFn"> {
  queryKey: string;
  queryUrl: string;
  Config?: AxiosRequestConfig;
}

export function useItemQuery<T>({
  queryKey,
  queryUrl,
  Config,
  ...queryOptions
}: UseItemQueryProps<T>) {
  const result = useQuery<T>({
    queryKey: [queryKey],
    queryFn: () => fetchData<T>({ queryUrl, Config }),
    refetchOnMount: false,
    refetchOnReconnect: false,
    refetchOnWindowFocus: false,
    ...queryOptions,
  });

  return {
    ...result,
  };
}
