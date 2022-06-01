namespace Chat;

public static class ChatUtils
{
    public static void PrintChatInfo()
    {
        Console.WriteLine("Welcome to Chat!\nYou can use following command:" +
                          "\nWithout command you can write message to your friend" +
                          "\n#switch [queue_name] - to switch to another queue" +
                          "\n#exit - to disconnect from chat\n");
    }
}