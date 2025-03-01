import { useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ServerResponseDto } from "../../../Common/Dtos";
import Loading_Spinner from "../../../Components/Shared/Loading_Spin";
import { useSendQuery } from "../../../Hooks/Querys/use-send-query";

function Join_Server_Page() {
  const { invitecode } = useParams<{
    invitecode?: string;
  }>();

  const navigate = useNavigate();

  const {
    mutate: JoinServer,
    isLoading,
    isError,
  } = useSendQuery<unknown, ServerResponseDto>({
    Type: "PATCH",
    queryKey: `/Servers/${invitecode}/Join`,
    queryUrl: `/Servers/${invitecode}/Join`,
  });

  useEffect(() => {
    JoinServer(
      {},
      {
        onSuccess: (response) => {
          navigate(`/server/${response.id}`, { replace: true });
        },
      }
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [invitecode]);

  if (isLoading)
    return (
      <div className="w-full flex flex-1 justify-center items-center">
        <Loading_Spinner />
      </div>
    );

  if (isError)
    return (
      <div className="flex flex-1 justify-center items-center h-full bg-gray-800 w-full ">
        <p>Something went wrong!</p>
      </div>
    );
}

export default Join_Server_Page;
