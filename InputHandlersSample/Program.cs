using System;

namespace InputHandlersSample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (InputHandlersSample game = new InputHandlersSample())
            {
                game.Run();
            }
        }
    }
}

