using System.Reflection;

namespace LightPhoenixBA.StrideExtentions.MultiplayerBase;

public static class ConnectionPacket
{
    public static List<MP_PacketBase> registry => MP_PacketBase.registry;

    public static void SyncConnectionPacket(NetIncomingMessage incFromServer)
    {
        if (StrideClientBase.IsSingleplayer)
        {
            Console.WriteLine("Singleplayer game detected skipping packet sync");
            return;
        }
    packetLoop:
        string packetName = incFromServer.ReadString();
        while (packetName.Length > 0)
        {
            var packetType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a =>
                        {
                            try
                            {
                                return a.GetTypes();
                            }
                            catch (ReflectionTypeLoadException e)
                            {
                                return e.Types.Where(t => t != null)!;
                            }
                        })
                        .FirstOrDefault(t => t.FullName == packetName);

            if (packetType == null)
            {
                Console.WriteLine($"[WARN] Packet type '{packetName}' not found.");
                continue;
            }
            if (packetType.BaseType.BaseType == typeof(MP_PacketBase))
            {
                MP_PacketBase.Register(Activator.CreateInstance(packetType) as MP_PacketBase);
            }
            goto packetLoop;
        }
    }
    public static NetOutgoingMessage SyncConnectionPacket(NetServer netServer)
    {
        NetOutgoingMessage outFromServer = netServer.CreateMessage();
        foreach (MP_PacketBase packet in registry)
        {
            outFromServer.Write(packet.ToString());
        }
        return outFromServer;
    }
}
