import { FormEvent, useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { GetAuthInfo } from "../../Auth/Profile";
import { Api } from "../../Common/axios";
import Image_Upload, {
  UploadedFileProps,
} from "../../Components/Shared/Image_Upload";
import Loading_Spinner from "../../Components/Shared/Loading_Spin";
import { useSendQuery } from "../../Hooks/Querys/use-send-query";
import { logError } from "../../Common/Logger";

export const checkAvailability = async (
  field: "userName" | "email",
  value: string
) => {
  try {
    const res = await Api.get(`/Auth/CheckAvailability?${field}=${value}`);
    return res.data;
  } catch (err) {
    logError(`Error checking ${field}:`, err);
    return false;
  }
};

function Register() {
  const [displayName, setDisplayName] = useState("");
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [photo, setPhoto] = useState<UploadedFileProps>({
    File: null,
    FileId: null,
  });
  const [alertMessage, setAlertMessage] = useState("");
  const navigate = useNavigate();

  const { mutate, isLoading: isSubmitting } = useSendQuery<FormData, unknown>({
    Type: "POST",
    queryKey: "Register",
    queryUrl: "/Auth/Register",
    Config: {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    },
  });

  useEffect(() => {
    if (GetAuthInfo()) {
      navigate("/", { replace: true });
    }
  }, [navigate]);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setAlertMessage("");

    const isUserNameAvailable = await checkAvailability("userName", userName);
    const isEmailAvailable = await checkAvailability("email", email);

    if (!isUserNameAvailable) {
      setAlertMessage("Username is already taken.");
      return;
    }

    if (!isEmailAvailable) {
      setAlertMessage("Email is already registered.");
      return;
    }

    const formData = new FormData();
    formData.append("displayName", displayName);
    formData.append("userName", userName);
    formData.append("email", email);
    formData.append("password", password);
    if (photo.File) {
      formData.append("Image", photo.File);
    }

    mutate(formData, {
      onSuccess: () => {
        navigate("/confirm-email", { state: { email } });
      },
      onError: () => {
        setAlertMessage("Registration failed. Please try again.");
      },
    });
  };

  return (
    <div className="h-screen flex justify-center items-center bg-gray-800">
      <form
        onSubmit={handleSubmit}
        className="bg-gray-900 text-gray-200 p-8 rounded-lg shadow-lg w-full max-w-sm"
        encType="multipart/form-data"
      >
        <h2 className="text-2xl font-bold mb-6 text-center">Register</h2>

        {alertMessage && (
          <div className="bg-red-500 text-white text-sm p-2 mb-4 rounded-lg">
            {alertMessage}
          </div>
        )}

        <Image_Upload File={photo} onFileChange={setPhoto} />

        <div className="mb-4">
          <label htmlFor="displayName" className="block mb-2 text-sm">
            Display Name
          </label>
          <input
            type="text"
            id="displayName"
            value={displayName}
            autoComplete="name"
            onChange={(e) => setDisplayName(e.target.value)}
            className="w-full p-2 bg-gray-700 text-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500"
            placeholder="Display Name"
            required
            disabled={isSubmitting}
          />
        </div>

        <div className="mb-4">
          <label htmlFor="userName" className="block mb-2 text-sm">
            Username
          </label>
          <input
            type="text"
            id="userName"
            value={userName}
            autoComplete="username"
            onChange={(e) => setUserName(e.target.value)}
            className="w-full p-2 bg-gray-700 text-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500"
            placeholder="Username"
            required
            disabled={isSubmitting}
          />
        </div>

        <div className="mb-4">
          <label htmlFor="email" className="block mb-2 text-sm">
            Email Address
          </label>
          <input
            type="email"
            id="email"
            value={email}
            autoComplete="email"
            onChange={(e) => setEmail(e.target.value)}
            className="w-full p-2 bg-gray-700 text-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500"
            placeholder="Email address"
            required
            disabled={isSubmitting}
          />
        </div>

        <div className="mb-6">
          <label htmlFor="password" className="block mb-2 text-sm">
            Password
          </label>
          <input
            type="password"
            id="password"
            value={password}
            autoComplete="new-password"
            onChange={(e) => setPassword(e.target.value)}
            className="w-full p-2 bg-gray-700 text-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500"
            placeholder="Password"
            required
            disabled={isSubmitting}
          />
        </div>

        <button
          type="submit"
          className="w-full bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 rounded-lg focus:ring-2 focus:ring-blue-400"
          disabled={isSubmitting}
        >
          {!isSubmitting ? "Register" : <Loading_Spinner className="w-full" />}
        </button>

        <p className="mt-4 text-center text-sm text-gray-400">
          Already have an account?{" "}
          <Link to="/login" className="text-blue-400 hover:underline">
            Log in
          </Link>
        </p>
      </form>
    </div>
  );
}

export default Register;
