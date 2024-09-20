/*
 * Generic Singleton
*/

public class Singleton<T> where T : class, new()
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private static T instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }

            return instance;
        }
    }
}
