using System;
using System.Reflection;
using System.Runtime.Serialization;
using chess_air_Server;

class DuplicateLoginTest
{
    static int Main()
    {
        // An existing session token means the password was accepted and a
        // confirmation challenge has already been started for this connection.
        var client = (ManageClient)FormatterServices.GetUninitializedObject(typeof(ManageClient));
        var type = typeof(ManageClient);
        type.GetField("session_token", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(client, "existing-session");
        var handle = type.GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        try
        {
            // Deliberately malformed credentials must never reach the database.
            handle.Invoke(client, new object[] { "###login###duplicate" });
            Console.WriteLine("Duplicate login ignored while challenge is active");
            return 0;
        }
        catch (TargetInvocationException e)
        {
            Console.Error.WriteLine("Duplicate login was processed: " + e.InnerException.GetType().Name);
            return 1;
        }
    }
}
