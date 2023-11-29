using System.Net.Sockets;

public abstract class Match
{
    public string MatchCode;
    private Client _hostClient;
    public Match (Client hostclient, string matchCode)
    {
        MatchCode = matchCode;
        _hostClient = hostclient;
    }
    protected int MaxClients;

    //List of client in the match and their refrences
    private Dictionary<string, Client> _clients = new Dictionary<string, Client>();
    public string[] GetClientIDs { get{ return _clients.Keys.ToArray(); } }
    // Method for adding client
    public abstract bool TryClient(Client client);
    

    // Checks that there is an available space for the client and add client to match, returns false if no space is available
    protected bool AddClient(Client client, string matchRefrenceForClient = "")
    {
        if (_clients.Count < MaxClients - 1)
        {
            while (_clients.ContainsKey(matchRefrenceForClient))
            {
                matchRefrenceForClient += "1";
            }
            using (Packet packet = new Packet(254))
            {
                packet.Write(matchRefrenceForClient);
                SendToAll(packet, ProtocolType.Tcp);
            }
            _clients.Add(matchRefrenceForClient, client);
        }
        return false;
    }
    public void RemoveClient(Client client)
    {
        
    }
    #region  Distributers
    // Sends data to the Host
    public void SendToHost(Packet packet, ProtocolType protocolType)
    {
        _hostClient.SendData(packet, protocolType);
    }
    // Distributes data to all clients
    public void SendToAll(Packet packet, ProtocolType protocolType)
    {
        _hostClient.SendData(packet, protocolType);
        foreach (Client client in _clients.Values)
        {
            client.SendData(packet, protocolType);
        }
    }
    // Distributes data to all clients except the host
    public void SendToAllClients(Packet packet, ProtocolType protocolType)
    {
        foreach (Client client in _clients.Values)
        {
            client.SendData(packet, protocolType);
        }
    }
    // Distributes data one client
    public void SentToClient(string reference, Packet packet, ProtocolType protocolType)
    {
        _clients[reference].SendData(packet, protocolType);
    }
    // Distributes data to all but one client and the host
    public void SendToAllButOne(string Ignored, Packet packet, ProtocolType protocolType)
    {
        foreach (string client in _clients.Keys)
        {
            if (client != Ignored)
            {
                _clients[client].SendData(packet, protocolType);
            }
        }
    }
    #endregion
}