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
                dAmnNET da = new dAmnNET();
                da.Login("bigmanhaywood", "batman");
                da.Join("Botdom");
                da.Say("Botdom", "Wassup!");
                da.Disconnect();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }
}
