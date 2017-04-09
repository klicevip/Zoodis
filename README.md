# Zoodis
c# client for [Codis](https://github.com/CodisLabs/codis) based on [Stackexchange.Redis](https://github.com/StackExchange/StackExchange.Redis) and [Zookeeper](https://github.com/shayhatsor/zookeeper)
## Features
Use a round robin policy to balance load to multiple codis proxies.

Detect proxy online and offline automatically.

## Demo
```
            ConnectionPool connectionPool = new ConnectionPool("127.0.0.1:2181", "/jodis/codis-demo");
            string input = null;
            do
            {
                var con = await connectionPool.GetConnectionAsync();

                Console.WriteLine("hello, {0}", con.GetDatabase().StringGet("hello"));

                Console.WriteLine("enter to get connection, input quit to close console");
                input = Console.ReadLine();

            } while (input != "quit");
```

## Reference
[Jodis](https://github.com/CodisLabs/jodis) is a java client
[Codis](https://github.com/CodisLabs/codis) is a nice solution for redis cluster
