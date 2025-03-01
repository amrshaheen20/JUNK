import { useMutation } from "@tanstack/react-query";
import { Api } from "../../Common/axios";
import { AxiosRequestConfig } from "axios";


export type QueryType =
    | "POST"
    | "PATCH"
    | "DELETE";


interface FetchDataParams<R> {
    queryUrl: string;
    Type: QueryType
    params?: R;
    Config?: AxiosRequestConfig
}

const fetchData = async <T, R>({ Type, queryUrl, params, Config }: FetchDataParams<R>): Promise<T> => {
    if (Type === "POST") {
        const { data } = await Api.post<T>(queryUrl, params, Config);
        return data;
    }
    else if (Type === "PATCH") {
        const { data } = await Api.patch<T>(queryUrl, params, Config);
        return data;
    }
    else if (Type === "DELETE") {
        const { data } = await Api.delete<T>(queryUrl,
            {
                headers: {
                    "Content-Type": "application/json",  //Axios i don't know why not set "Content-Type" in header and asp.net give "415 Unsupported Media Type"
                    accept: 'application/json',
   
                },
                data :{
                    temp:"Delete"
                },
                ...Config,
            }
        );
        return data;
    } else {
        throw new Error(`Invalid queryType: ${Type}`);
    }
};



interface UseSendQueryProps {
    queryKey: string;
    queryUrl: string;
    Type: QueryType
    Config?: AxiosRequestConfig
}

export function useSendQuery<R, T>({ Type, queryKey, queryUrl, Config }: UseSendQueryProps) {
    const mutation = useMutation<T, unknown, R>({
        mutationKey: [queryKey],
        mutationFn: (params) => fetchData<T, R>({ Type, queryUrl, params, Config }),
    });

    return {
        mutation: mutation,
        mutate: mutation.mutate,
        queryData: mutation.data,
        isLoading: mutation.isPending,
        isSuccess: mutation.isSuccess,
        isError: mutation.isError,
        error: mutation.error,
    };
}
