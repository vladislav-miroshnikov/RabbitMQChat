namespace Chat;

static class Program
{
    public static void Main(string[] args)
    {
        var chat = new Chat(args[0], args[1], args[2]);
    }
}