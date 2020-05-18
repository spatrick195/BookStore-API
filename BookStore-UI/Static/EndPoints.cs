namespace BookStore_UI.Static
{
    public class EndPoints
    {
        public static string BaseUrl = "https://localhost:44382";
        public static string AuthorsEndPoint = $"{BaseUrl}/api/authors";
        public static string BooksEndPoint = $"{BaseUrl}/api/books";
        public static string LoginEndPoint = $"{BaseUrl}/api/users/login";
        public static string RegisterEndPoint = $"{BaseUrl}/api/users/register";
    }
}