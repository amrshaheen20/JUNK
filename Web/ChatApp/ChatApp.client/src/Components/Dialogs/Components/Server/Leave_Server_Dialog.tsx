import { useNavigate } from "react-router-dom";

import { useDialogContext } from "../../../../Contexts/Dialog_Context";
import { useSendQuery } from "../../../../Hooks/Querys/use-send-query";

function Leave_Server_Dialog() {
  const { isOpened, closeDialog, activeDialog, data } = useDialogContext();
  const navigate = useNavigate();

  const isActive = isOpened && activeDialog === "LeaveServer";
  const { queryUrl } = data;

  const { mutate, isLoading: isSubmitting } = useSendQuery({
    Type: "PATCH",
    queryKey: "LeaveServer",
    queryUrl: queryUrl || "",
  });

  if (!isActive) return null;

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    mutate(
      {},
      {
        onSuccess: () => {
          closeDialog();
          navigate(`/`);
        },
      }
    );
  };

  return (
        <form onSubmit={handleSubmit}>
          <h2 className="text-3xl font-semibold mb-6 text-center">
            Leave Server
          </h2>

          <button
            type="submit"
            className="w-full cursor-pointer bg-red-500 hover:bg-red-600 text-white py-3 rounded-lg mb-4"
            disabled={isSubmitting}
          >
            {isSubmitting ? "Leaving..." : " Leave Server"}
          </button>

          <button
            type="button"
            onClick={closeDialog}
            className="w-full bg-green-500 cursor-pointer hover:bg-green-600 text-white py-3 rounded-lg"
            disabled={isSubmitting}
          >
            Close
          </button>
        </form>
  );
}

export default Leave_Server_Dialog;
