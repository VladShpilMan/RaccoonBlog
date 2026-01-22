namespace RaccoonBlog.Web.Models
{
    public class BlackList
    {
        public static string GetId(string ipAddress)
        {
            return "BlackList/" + ipAddress;
        }

        public static BlackList New(string ipAddress)
        {
            var id = GetId(ipAddress);

            return new BlackList
            {
                Id = id
            };
        }

        public string Id { get; set; }
    }
}