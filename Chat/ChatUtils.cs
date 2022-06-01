namespace Chat;

public static class ChatUtils
{
    public static void ClearCurrentConsoleLine()
    {
        Console.SetCursorPosition(0, Console.CursorTop - 1);
        var currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth)); 
        Console.SetCursorPosition(0, currentLineCursor);
    }

    public static void PrintChatInfo()
    {
        Console.WriteLine("Welcome to Chat!\nYou can use following command:" +
                          "\nWithout command you can write message to your friend" +
                          "\n#switch [queue_name] - to switch to another queue" +
                          "\n#exit - to disconnect from chat\n");
    }
}