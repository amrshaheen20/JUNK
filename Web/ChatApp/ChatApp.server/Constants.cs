namespace ChatApi.server
{
    public static class Constants
    {
        public const string FRONTEND_URL = "http://localhost:5173";
        public const string FRONTEND_URL_PASSWORD_RESET = "http://localhost:5173/ResetPassword";
        public const string UPLOAD_FOLDER = "uploads";
        public const string CURRENT_USER_TAG = "@me";
        public const int MAX_QUERY_LIMIT = 50;
        public const int LAST_ACTIVE_NUMBER = 2;

        public static TimeSpan OPTIMIZE_FILES_AFTER => TimeSpan.FromDays(30);
        public static string EMAIL_CONFIRMATION_TEMPLATE => Path.Combine(Directory.GetCurrentDirectory(), "Templates", "email_confirmation.html");
        public static string PASSWORD_RESET_TEMPLATE => Path.Combine(Directory.GetCurrentDirectory(), "Templates", "password_reset.html");

        public static string GetUploadsFolder()
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), Constants.UPLOAD_FOLDER);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);
            return uploadsFolder;
        }


    }
}
