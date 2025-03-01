import { createContext, ReactNode, useContext, useState } from "react";
import { ChannelResponseDto, MemberResponseDto, ServerResponseDto } from "../Common/Dtos";

interface DataStructure {
  server?: ServerResponseDto;
  member?: MemberResponseDto;
  channel?: ChannelResponseDto;
  profileId?:string;
  queryUrl?:string
}

type DialogType =
  | "CreateChannel"
  | "EditChannel"
  | "CreateServer"
  | "DeleteServer"
  | "EditServer"
  | "InviteLinkServer"
  | "LeaveServer"
  | "ProfileDialog"
  | "MembersServer"
  | "DeleteMessage"
  | "DeleteChannel"
  | "DeleteMember"
  | "EditProfile";

interface DialogContextProps {
  isOpened: boolean;
  activeDialog: DialogType | null;
  data: DataStructure;
  openDialog: (type: DialogType, data?: DataStructure) => void;
  closeDialog: () => void;
}

const DialogContext = createContext<DialogContextProps | undefined>(undefined);

interface DialogContextProviderProps {
  children: ReactNode;
}

export function DialogContextProvider({ children }: DialogContextProviderProps) {
  const [isOpened, setIsOpened] = useState(false);
  const [activeDialog , setActiveDialog ] = useState<DialogType | null>(null);
  const [data, setData] = useState<DataStructure>({});

  const openDialog = (type: DialogType, data: DataStructure = {}) => {
    setActiveDialog (type);
    setData(data);
    setIsOpened(true);
  };

  const closeDialog = () => {
    setIsOpened(false);
    setActiveDialog (null);
    setData({});
  };

  return (
    <DialogContext.Provider
      value={{
        isOpened,
        activeDialog ,
        data,
        openDialog,
        closeDialog
      }}
    >
      {children}
    </DialogContext.Provider>
  );
}

export function useDialogContext() {
  const context = useContext(DialogContext);
  if (!context) {
    throw new Error(
      "useDialogContext must be used within a DialogContextProvider"
    );
  }
  return context;
}