using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviantArt.Chat.Library;
using System.Web;
using System.Collections;

namespace DeviantArt.Chat.Oberon
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Bot bot = Bot.Instance;
                bot.Run();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }
}
