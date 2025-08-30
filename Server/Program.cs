using static LuciferCore.Core.Simulation;
using LuciferCore.Server;

var hostServer = GetModel<HostServer>();
hostServer.Init();
// chạy server nền
hostServer.Start();

var running = true;

while (running)
{
    Console.WriteLine("Type /quit or /stop to stop or /restart to restart.");
    var input = Console.ReadLine();

    switch (input)
    {
        case "/quit":
            running = false;
            hostServer.Stop();
            break;
        case "/start":
            hostServer.RequestStart();
            break;
        case "/stop":
            hostServer.RequestStop();
            break;
        case "/restart":
            hostServer.RequestRestart();
            break;

    }
}
