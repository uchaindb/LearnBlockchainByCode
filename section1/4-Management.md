# 网络管理

## 节点管理

```cs
public class Node : IDisposable  
{  
    private readonly IListener listener;  
    private readonly NodeOptions options;  
    private readonly IPeerFactory peerFactory;  
  
    public Node(IWallet miner, IListener listener, IPeerFactory peerFactory, NodeOptions options = null)  
    {  
        this.Engine = new Engine(miner);  
        this.options = options ?? new NodeOptions();  
  
        this.listener = listener;  
        this.listener.Start();  
        this.peerFactory = peerFactory;  
        this.ConnPool = new ConnectionPool(this, this.options.WellKnownNodes, this.peerFactory, this.listener);  
        this.ConnPool.Start();  
    }  
  
    public Engine Engine { get; }  
    public ConnectionPool ConnPool { get; }  
  
    public void Dispose()  
    {  
        this.Engine?.Dispose();  
        this.listener.Dispose();  
        this.ConnPool.Dispose();  
        this.peerFactory.Dispose();  
    }
}  
```
<!-- code:ClassicBlockChain/Core/Node.cs -->

```cs
public class NodeOptions  
{  
    public string[] WellKnownNodes { get; set; }  
}  
```
<!-- code:ClassicBlockChain/Entity/NodeOptions.cs -->

## 连接管理

```cs
public enum ConnectionStatus  
{  
    Initial,  
    Connected,  
    Disconnected,  
    Dead,  
}  
```
<!-- code:ClassicBlockChain/Network/ConnectionNode.cs -->

```cs
public class ConnectionNode  
{  
    public string Address { get; set; }  
    public ConnectionStatus Status { get; set; }  
    public IPeer Peer { get; set; }  
}  
```
<!-- code:ClassicBlockChain/Network/ConnectionNode.cs -->

```cs
public class ConnectionPool : IDisposable  
{  
    private readonly List<ConnectionNode> nodes;  
    private readonly Node selfNode;  
    private readonly IPeerFactory peerFactory;  
    private readonly IListener listener;  
    public ConnectionPool(Node node, string[] wellKnowns, IPeerFactory peerFactory, IListener listener)  
    {  
        this.selfNode = node;  
        this.nodes = wellKnowns  
            .Where(_ => _ != listener.Address)  
            .Select(_ => new ConnectionNode(_))  
            .ToList();  
        this.peerFactory = peerFactory;  
        this.listener = listener;  
    }  
  
    public void Start()  
    public void Dispose()  
}  
```
<!-- code:ClassicBlockChain/Network/ConnectionPool.cs -->

```cs
public class ConnectionPool : IDisposable  
{  
    private bool isReceiving = false;  
    private Thread thReceive;  
    public event EventHandler<CommandBase> OnCommandReceived;  
  
    public void Start()  
    {  
        this.thReceive = new Thread(Receive);  
        this.thReceive.Start();  
        this.isReceiving = true;  
    }  
  
    private void Receive()  
    {  
        while (this.isReceiving)  
        {  
            ConnectionNode[] internalnodes;  
            lock (this.nodes)  
            {  
                internalnodes = this.nodes.ToArray();  
            }  
            foreach (var node in internalnodes)  
            {  
                if (node.Peer == null) continue;  
                var command = node.Peer.Receive();  
                if (command == null) continue;  
                OnCommandReceived?.Invoke(this, command);  
                command.OnReceived(this.selfNode, node);  
                if (!this.isReceiving) break;  
            }  
  
            Thread.Sleep(500);  
        }  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/ConnectionPool.cs -->

## 节点连接
```cs
public class ConnectionPool : IDisposable  
{  
    private Timer reconnectTimer;  
  
    public void Start()  
    {  
        ...  
        this.reconnectTimer = new Timer((_) => this.ConnectAll(), null, new TimeSpan(0, 0, 0, 0, 100), new TimeSpan(0, 0, 20));  
    }  
  
    private void ConnectAll()  
    {  
        ConnectionNode[] internalnodes;  
        lock (this.nodes)  
        {  
            internalnodes = this.nodes  
                .Where(_ => _.Status == ConnectionStatus.Initial || _.Status == ConnectionStatus.Dead)  
                .Where(_ => _.Address != null)  
                .ToArray();  
        }  
        foreach (var node in internalnodes)  
        {  
            this.TryConnect(node);  
        }  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/ConnectionPool.cs -->

```cs
public class ConnectionPool : IDisposable  
{  
    private Timer reconnectTimer;  
  
    public void Start()  
    {  
        ...  
        this.reconnectTimer = new Timer((_) => this.ConnectAll(), null, new TimeSpan(0, 0, 0, 0, 100), new TimeSpan(0, 0, 20));  
    }  
  
    private void ConnectAll()  
    {  
        ConnectionNode[] internalnodes;  
        lock (this.nodes)  
        {  
            internalnodes = this.nodes  
                .Where(_ => _.Status == ConnectionStatus.Initial || _.Status == ConnectionStatus.Dead)  
                .Where(_ => _.Address != null)  
                .ToArray();  
        }  
        foreach (var node in internalnodes)  
        {  
            this.TryConnect(node);  
        }  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/ConnectionPool.cs -->

```cs
private void TryConnect(ConnectionNode node)  
{  
    if (node.Peer != null)  
    {  
        node.Peer.Dispose();  
        node.Peer = null;  
    }  
  
    var peer = this.peerFactory.Produce();  
    try  
    {  
        peer.Connect(node.Address);  
        node.Peer = peer;  
    }  
    catch (Exception)  
    {  
        node.Status = ConnectionStatus.Dead;  
    }  
  
    if (!peer.IsConnected)  
    {  
        Debug.WriteLine("open peer channel failed");  
        node.Status = ConnectionStatus.Dead;  
        return;  
    }  
  
    try  
    {  
        peer.Send(new VersionCommand());  
    }  
    catch (Exception)  
    {  
        node.Status = ConnectionStatus.Dead;  
    }  
    finally  
    {  
        if (node.Status != ConnectionStatus.Connected)  
        {  
            peer.Close();  
            peer.Dispose();  
        }  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/ConnectionPool.cs -->

```cs
public class VersionCommand : CommandBase  
{  
    public override void OnReceived(Node node, ConnectionNode connectionNode)  
    {  
        connectionNode.Status = ConnectionStatus.Connected;  
        connectionNode.Peer.Send(new VersionAcknowledgeCommand());  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/VersionCommand.cs -->

```cs
public class VersionAcknowledgeCommand : CommandBase  
{  
    public override void OnReceived(Node node, ConnectionNode connectionNode)  
    {  
        connectionNode.Status = ConnectionStatus.Connected;  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/VersionAcknowledgeCommand.cs -->

