import { FormEvent } from "react";

import { useNavigate } from "react-router-dom";
import { useDialogContext } from "../../../../Contexts/Dialog_Context";
import { useSendQuery } from "../../../../Hooks/Querys/use-send-query";

const Delete_Member_Dialog = () => {
  const { isOpened, closeDialog, activeDialog, data } = useDialogContext();
  const navigate = useNavigate();

  const isActive = isOpened && activeDialog === "DeleteMember";
  const { queryUrl,server } = data;

  const { mutate, isLoading: isSubmitting } = useSendQuery({
    Type: "DELETE",
    queryKey: "DeleteMember",
    queryUrl: queryUrl || "",
  });

  if (!isActive) return null;

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    mutate(
      {},
      {
        onSuccess: () => {
          navigate(`/server/${server?.id}`);
          closeDialog();
        },
      }
    );
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2 className="text-3xl font-semibold mb-6 text-center">
        Delete Member
      </h2>

      <p className="text-center text-gray-600 mb-6">
        Are you sure you want to delete this member? This action cannot be undone.
      </p>

      <div className="flex flex-col gap-3">
        <button
          type="submit"
          className="w-full bg-red-500 hover:bg-red-600 text-white py-3 rounded-lg disabled:opacity-50"
          disabled={isSubmitting}
        >
          {isSubmitting ? "Deleting..." : "Delete Member"}
        </button>
        <button
          type="button"
          onClick={closeDialog}
          className="w-full bg-gray-500 hover:bg-gray-600 text-white py-3 rounded-lg"
          disabled={isSubmitting}
        >
          Cancel
        </button>
      </div>
    </form>
  );
};

export default Delete_Member_Dialog;
