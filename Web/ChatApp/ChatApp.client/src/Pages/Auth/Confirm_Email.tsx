import { FormEvent, MouseEvent, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { Api } from "../../Common/axios";
import { logError } from "../../Common/Logger";

function Confirm_Email() {
  const [token, setToken] = useState("");
  const [alertMessage, setAlertMessage] = useState("");
  const navigate = useNavigate();
  const location = useLocation();

  const email = location.state?.email || "";

  if (!email) {
    navigate("/");
  }

  const handleResendCode = async (e: MouseEvent<HTMLElement>) => {
    e.preventDefault();
    try {
      await Api.post("/Auth/ResendEmailConfirmation", { email });
      setAlertMessage("Check your email!");
    } catch (err) {
      logError("Error resend code:", err);
      setAlertMessage(
        "An error happen while resend verification code. Please try again."
      );
    }
  };
  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setAlertMessage("");

    if (!token) {
      setAlertMessage("Please enter the verification code.");
      return;
    }

    try {
      const res = await Api.post("/Auth/ConfirmEmail", {
        email,
        token,
      });
      if (res.status === 200) {
        setAlertMessage("Email confirmed");
        navigate("/", { replace: true });
      }
    } catch (err) {
      logError("Error confirming email:", err);
      setAlertMessage("Email confirmation failed. Please try again.");
    }
  };

  return (
    <div className="h-screen flex justify-center items-center bg-gray-800">
      <form
        onSubmit={handleSubmit}
        className="bg-gray-900 text-gray-200 p-8 rounded-lg shadow-lg w-full max-w-sm"
      >
        <h2 className="text-2xl font-bold mb-6 text-center">
          Confirm Your Email
        </h2>

        {alertMessage && (
          <div className="bg-red-500 text-white text-sm p-2 mb-4 rounded-lg">
            {alertMessage}
          </div>
        )}

        <div className="mb-4">
          <label htmlFor="verificationCode" className="block mb-2 text-sm">
            Verification Code
          </label>
          <input
            type="text"
            id="verificationCode"
            value={token}
            onChange={(e) => setToken(e.target.value)}
            className="w-full p-2 bg-gray-700 text-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500"
            placeholder="Enter verification code"
          />
        </div>

        <button
          type="submit"
          className="w-full bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 rounded-lg focus:ring-2 focus:ring-blue-400"
        >
          Confirm Email
        </button>

        <p className="mt-4 text-center text-sm text-gray-400">
          Didn’t receive a code?{" "}
          <button
            onClick={handleResendCode}
            className="text-blue-400 hover:underline cursor-pointer"
          >
            Resend Code
          </button>
        </p>
      </form>
    </div>
  );
}

export default Confirm_Email;
