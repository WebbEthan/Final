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
    public int ClientCount { get{ return _clients.Count + 1; }}
    // Method for adding client
    public abstract bool TryClient(Client client);
    

    // Checks that there is an available space for the client and add client to match, returns false if no space is available
    protected bool AddClient(Client client)
    {
        if (_clients.Count < MaxClients - 1)
        {
            while (_clients.ContainsKey(client.matchRefrenceForClient))
            {
                client.matchRefrenceForClient += "1";
            }
            using (Packet packet = new Packet(254))
            {
                packet.Write(client.matchRefrenceForClient);
                SendToAll(packet, ProtocolType.Tcp);
            }
            _clients.Add(client.matchRefrenceForClient, client);
        }
        return false;
    }
    // Removes client from the match and sends that info to the other clients in match
    public void RemoveClient(string clientID)
    {
        _clients.Remove(clientID);
        using (Packet packet = new Packet(252))
        {
            packet.Write(clientID);
            SendToAll(packet, ProtocolType.Tcp);
        }
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