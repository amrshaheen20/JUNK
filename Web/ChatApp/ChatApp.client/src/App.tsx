import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import {
  createBrowserRouter,
  createRoutesFromElements,
  Navigate,
  Route,
  RouterProvider,
} from "react-router-dom";
import "./App.css";
import Axios_Error_Handler from "./Components/Providers/Axios_Error_Handler";
import Require_Auth from "./Components/Providers/Require_Auth";
import SignalR_Provider from "./Components/Providers/SignalR_Provider";
import { CommonContextProvider } from "./Contexts/Common_Context";
import { DialogContextProvider } from "./Contexts/Dialog_Context";
import MainAppLayout from "./Layout/MainAppLayout";
import Confirm_Email from "./Pages/Auth/Confirm_Email";
import Login from "./Pages/Auth/Login";
import Register from "./Pages/Auth/Register";
import Conversation_Page from "./Pages/Conversation/Conversation_Page";
import Home_Page from "./Pages/Home_Page";
import Join_Server_Page from "./Pages/Server/JoinServer/Join_Server_Page";
import Server_Page from "./Pages/Server_Page";

function App() {

  const router = createBrowserRouter(
    createRoutesFromElements(
      <>
        <Route
          path="/"
          element={
            <Require_Auth>
              <SignalR_Provider />
              <Axios_Error_Handler />
              <MainAppLayout />
            </Require_Auth>
          }
        >
          <Route path="/" element={<Home_Page />}>
            <Route
              path="conversation/:conversationId/:channelId/:messageId?"
              element={<Conversation_Page />}
            />
          </Route>
          <Route
            path="/server/:serverId/:channelId?/:messageId?"
            element={<Server_Page />}
          />
          <Route
            path="server/:invitecode/join"
            element={<Join_Server_Page />}
          />
        </Route>

        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/confirm-email" element={<Confirm_Email />} />

        <Route path="*" element={<Navigate to="/" />} />
      </>
    )
  );

  const queryClient = new QueryClient();

  return (
    <QueryClientProvider client={queryClient}>
      <CommonContextProvider>
        <DialogContextProvider>
          <Axios_Error_Handler>
            <RouterProvider router={router} />
          </Axios_Error_Handler>
        </DialogContextProvider>
      </CommonContextProvider>
    </QueryClientProvider>
  );
}

export default App;
