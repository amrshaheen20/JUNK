import { HubConnection } from "@microsoft/signalr";
import {
  ReactNode,
  createContext,
  useContext,
  useReducer,
  useState,
} from "react";
import {
  ChannelResponseDto,
  ConversationResponseDto,
  MemberResponseDto,
  ProfileResponseDto,
  ServerResponseDto,
} from "../Common/Dtos";

interface GlobalDataProps {
  Server?: ServerResponseDto | null;
  Channel?: ChannelResponseDto | null;
  Member?: MemberResponseDto | null;
  Conversation?: ConversationResponseDto | null;
  Profile?: ProfileResponseDto | null;
}

export type ActionType =
  | { type: "ADD_SERVER"; payload: ServerResponseDto | undefined | null }
  | { type: "ADD_CHANNEL"; payload: ChannelResponseDto | undefined | null }
  | { type: "ADD_MEMBER"; payload: MemberResponseDto | undefined | null }
  | {
      type: "ADD_CONVERSATION";
      payload: ConversationResponseDto | undefined | null;
    }
  | { type: "ADD_PROFILE"; payload: ProfileResponseDto | undefined | null };

const reducer = (
  state: GlobalDataProps,
  action: ActionType
): GlobalDataProps => {
  switch (action.type) {
    case "ADD_SERVER":
      return { ...state, Server: action.payload };
    case "ADD_CHANNEL":
      return { ...state, Channel: action.payload };
    case "ADD_MEMBER":
      return { ...state, Member: action.payload };
    case "ADD_PROFILE":
      return { ...state, Profile: action.payload };
    case "ADD_CONVERSATION":
      return { ...state, Conversation: action.payload };
    default:
      return state;
  }
};

interface CommonContextProps {
  IsAppServerDown: boolean;
  setIsAppServerDown: React.Dispatch<React.SetStateAction<boolean>>;
  Auth: boolean;
  setAuth: (auth: boolean) => void;
  connection: HubConnection | undefined;
  setConnection: React.Dispatch<
    React.SetStateAction<HubConnection | undefined>
  >;
  globalData: GlobalDataProps;
  dispatch: React.Dispatch<ActionType>;
  isSideBarOpened: boolean;
  setIsSideBarOpened: React.Dispatch<React.SetStateAction<boolean>>;
}

export const CommonContext = createContext<CommonContextProps | undefined>(
  undefined
);

export function CommonContextProvider({ children }: { children: ReactNode }) {
  const [IsAppServerDown, setIsAppServerDown] = useState<boolean>(true);
  const [Auth, setAuth] = useState<boolean>(false);
  const [isSideBarOpened, setIsSideBarOpened] = useState<boolean>(false);
  const [connection, setConnection] = useState<HubConnection>();
  const [globalData, dispatch] = useReducer(reducer, {});

  return (
    <CommonContext.Provider
      value={{
        IsAppServerDown,
        setIsAppServerDown,
        Auth,
        setAuth,
        connection,
        setConnection,
        globalData,
        dispatch,
        isSideBarOpened,
        setIsSideBarOpened,
      }}
    >
      {children}
    </CommonContext.Provider>
  );
}

export function useCommonContext(): CommonContextProps {
  const context = useContext(CommonContext);
  if (!context) {
    throw new Error(
      "useCommonContext must be used within a CommonContextProvider"
    );
  }
  return context;
}
