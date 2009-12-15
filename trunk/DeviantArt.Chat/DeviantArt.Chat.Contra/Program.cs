using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;
using System.Web;
using System.Collections;

namespace DeviantArt.Chat.Contra
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Bot bot = new Bot(true);
                bot.LoadConfig("bigmanhaywood", "batman", new string[] { "Botdom" });
                bot.Run();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }
}
