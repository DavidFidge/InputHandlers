using System;

namespace InputHandlers.Sample
{
    static class Program
    {
        static void Main(string[] args)
        {
            using (InputHandlersSample game = new InputHandlersSample())
            {
                game.Run();
            }
        }
    }
}

