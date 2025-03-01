import axios, { AxiosInstance } from "axios";
import { GetAuthInfo } from "../Auth/Profile";

export const Api: AxiosInstance = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
    headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${GetAuthInfo()?.token ?? ""}`,
    },
});

Api.interceptors.request.use((config) => {
    const authInfo = GetAuthInfo();
    if (authInfo?.token) {
        config.headers.Authorization = `Bearer ${authInfo.token}`;
        //config.headers["Content-Type"] =  "application/json";
    }
    return config;
});
