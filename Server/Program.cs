
using static LuciferCore.Core.Simulation;
using LuciferCore.Server;

var hostServer = GetModel<HostServer>();
hostServer.Init();
hostServer.Start();
Console.WriteLine("Running...");
while (true) ;