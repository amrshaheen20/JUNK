namespace ChatApi.server.Models.DbSet
{
    public class ProfileRoles
    {
        public const string Admin = "Admin";
        public const string Moderator = "Moderator";
        public const string Member = "Member";

        public static bool IsAdmin(string s) => s == Admin;
        public static bool IsModerator(string s) => s == Moderator;
        public static bool IsSuperAdmin(string s) => s == Admin || s == Moderator;
        public static bool IsMember(string s) => s == Member;
        public static bool IsValid(string s) => s == Admin || s == Moderator || s == Member;
        public static string[] List() => new[] { Admin, Moderator, Member };
        public static bool IsAdmin(IList<string> strings) => strings.Contains(Admin);
        public static bool IsModerator(IList<string> strings) => strings.Contains(Moderator);
        public static bool IsMember(IList<string> strings) => strings.Contains(Member);
        public static bool IsSuperAdmin(IList<string> strings) => strings.Contains(Admin) || strings.Contains(Moderator);
    }
}
