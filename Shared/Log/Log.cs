using LiteNetLib;

public class Log : INetLogger
{
    public static void Write(string str)
    {
        Console.WriteLine(str);
    }

    void INetLogger.WriteNet(NetLogLevel level, string str, params object[] args)
	{
		switch (level)
		{
			case NetLogLevel.Warning:
			case NetLogLevel.Error:
			case NetLogLevel.Trace:
			case NetLogLevel.Info:
				{
					Console.WriteLine(str, args);
				}
				break;
		}
	}
}

