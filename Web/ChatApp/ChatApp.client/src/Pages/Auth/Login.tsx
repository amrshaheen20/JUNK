import { useEffect, useState } from "react";
import { Api } from "../../Common/axios";
import { GetAuthInfo, SetAuthInfo } from "../../Auth/Profile";
import { useNavigate, useLocation, Link } from "react-router-dom";
import { logError } from "../../Common/Logger";

function Login() {
  const [email, setEmail] = useState<string>("");
  const [password, setPassword] = useState<string>("");
  const [alertMessage, setAlertMessage] = useState<string>("");
  const navigate = useNavigate();
  const location = useLocation();
  
  useEffect(() => {
    if (GetAuthInfo()) {
      navigate("/", { replace: true });
    }
  }, [navigate]);

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    Api.post("/Auth/Login", { email, password })
      .then((res) => {
        SetAuthInfo(res.data);
        setAlertMessage("");
        navigate((location.state as { path?: string })?.path || "/", {
          replace: true,
        });
      })
      .catch((err) => {
        logError(err);
        setAlertMessage("Invalid credentials. Please try again.");
      });
  };

  return (
    <div className="h-screen flex justify-center items-center bg-gray-800">
      <form
        onSubmit={handleSubmit}
        className="bg-gray-900 text-gray-200 p-8 rounded-lg shadow-lg w-full max-w-sm"
      >
        <h2 className="text-2xl font-bold mb-6 text-center">Sign In</h2>

        {alertMessage && (
          <div className="bg-red-500 text-white text-sm p-2 mb-4 rounded-lg">
            {alertMessage}
          </div>
        )}

        <div className="mb-4">
          <label htmlFor="inputEmai" className="block mb-2 text-sm">
          Email or User name
          </label>
          <input
            type="text"
            id="inputEmail"
            value={email}
            autoComplete="email"
            onChange={(e) => setEmail(e.target.value)}
            className="w-full p-2 bg-gray-700 text-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Email or User name"
            required
          />
        </div>

        <div className="mb-6">
          <label htmlFor="inputPassword" className="block mb-2 text-sm">
            Password
          </label>
          <input
            type="password"
            id="inputPassword"
            value={password}
            autoComplete="current-password"
            onChange={(e) => setPassword(e.target.value)}
            className="w-full p-2 bg-gray-700 text-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="Password"
            required
          />
        </div>

        <button
          type="submit"
          className="w-full bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-400"
        >
          Sign In
        </button>

        <div className="mt-4 text-center">
          <span className="text-sm text-gray-400">Don't have an account? </span>
          <Link to="/register" className="text-blue-400 hover:underline">
            Register here
          </Link>
        </div>
      </form>
    </div>
  );
}

export default Login;
