using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zoodis;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = Test();
            Task.WaitAll(t);
            Console.ReadLine();
        }

        static async Task Test()
        {
            ConnectionPool connectionPool = new ConnectionPool("127.0.0.1:2181", "/jodis/codis-demo");
            string input = null;
            do
            {
                var con = await connectionPool.GetConnectionAsync();

                Console.WriteLine("hello, {0}", con.GetDatabase().StringGet("hello"));

                Console.WriteLine("enter to get connection, input quit to close console");
                input = Console.ReadLine();

            } while (input != "quit");
        }
    }
}
