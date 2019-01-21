# 网络管理

## 节点管理

  -----------------------------------------------------------------------------------------------------------------------------------------------
  1.  **public** **class** Node : IDisposable  
  
  2.  {  
  
  3.      **private** **readonly** IListener listener;  
  
  4.      **private** **readonly** NodeOptions options;  
  
  5.      **private** **readonly** IPeerFactory peerFactory;  
  
  6.    
  
  7.      **public** Node(IWallet miner, IListener listener, IPeerFactory peerFactory, NodeOptions options = **null**)  
  
  8.      {  
  
  9.          **this**.Engine = **new** Engine(miner);  
  
  10.         **this**.options = options ?? **new** NodeOptions();  
  
  11.   
  
  12.         **this**.listener = listener;  
  
  13.         **this**.listener.Start();  
  
  14.         **this**.peerFactory = peerFactory;  
  
  15.         **this**.ConnPool = **new** ConnectionPool(**this**, **this**.options.WellKnownNodes, **this**.peerFactory, **this**.listener);  
  
  16.         **this**.ConnPool.Start();  
  
  17.     }  
  
  18.   
  
  19.     **public** Engine Engine { **get**; }  
  
  20.     **public** ConnectionPool ConnPool { **get**; }  
  
  21.   
  
  22.     **public** **void** Dispose()  
  
  23.     {  
  
  24.         **this**.Engine?.Dispose();  
  
  25.         **this**.listener.Dispose();  
  
  26.         **this**.ConnPool.Dispose();  
  
  27.         **this**.peerFactory.Dispose();  
  
  28.     }  
  
  29. }  
  
  -----------------------------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Core\\Node.cs

其中，

第3行，

  --------------------------------------------------------------------------
  1.  **public** **class** NodeOptions  
  
  2.  {  
  
  3.      **public** **string**\[\] WellKnownNodes { **get**; **set**; }  
  
  4.  }  
  
  --------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Entity\\NodeOptions.cs

其中，

第3行，

## 连接管理

  --------------------------------------------
  1.  **public** **enum** ConnectionStatus  
  
  2.  {  
  
  3.      Initial,  
  
  4.      Connected,  
  
  5.      Disconnected,  
  
  6.      Dead,  
  
  7.  }  
  
  --------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionNode.cs

其中，

第3行，

  --------------------------------------------------------------------
  1.  **public** **class** ConnectionNode  
  
  2.  {  
  
  3.      **public** **string** Address { **get**; **set**; }  
  
  4.      **public** ConnectionStatus Status { **get**; **set**; }  
  
  5.      **public** IPeer Peer { **get**; **set**; }  
  
  6.  }  
  
  --------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionNode.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------------------------------
  1.  **public** **class** ConnectionPool : IDisposable  
  
  2.  {  
  
  3.      **private** **readonly** List&lt;ConnectionNode&gt; nodes;  
  
  4.      **private** **readonly** Node selfNode;  
  
  5.      **private** **readonly** IPeerFactory peerFactory;  
  
  6.      **private** **readonly** IListener listener;  
  
  7.      **public** ConnectionPool(Node node, **string**\[\] wellKnowns, IPeerFactory peerFactory, IListener listener)  
  
  8.      {  
  
  9.          **this**.selfNode = node;  
  
  10.         **this**.nodes = wellKnowns  
  
  11.             .Where(\_ =&gt; \_ != listener.Address)  
  
  12.             .Select(\_ =&gt; **new** ConnectionNode(\_))  
  
  13.             .ToList();  
  
  14.         **this**.peerFactory = peerFactory;  
  
  15.         **this**.listener = listener;  
  
  16.     }  
  
  17.   
  
  18.     **public** **void** Start()  
  
  19.     **public** **void** Dispose()  
  
  20. }  
  
  -------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionPool.cs

其中，

第3行，

  -----------------------------------------------------------------------------------
  1.  **public** **class** ConnectionPool : IDisposable  
  
  2.  {  
  
  3.      **private** **bool** isReceiving = **false**;  
  
  4.      **private** Thread thReceive;  
  
  5.      **public** **event** EventHandler&lt;CommandBase&gt; OnCommandReceived;  
  
  6.    
  
  7.      **public** **void** Start()  
  
  8.      {  
  
  9.          **this**.thReceive = **new** Thread(Receive);  
  
  10.         **this**.thReceive.Start();  
  
  11.         **this**.isReceiving = **true**;  
  
  12.     }  
  
  13.   
  
  14.     **private** **void** Receive()  
  
  15.     {  
  
  16.         **while** (**this**.isReceiving)  
  
  17.         {  
  
  18.             ConnectionNode\[\] internalnodes;  
  
  19.             **lock** (**this**.nodes)  
  
  20.             {  
  
  21.                 internalnodes = **this**.nodes.ToArray();  
  
  22.             }  
  
  23.             **foreach** (var node **in** internalnodes)  
  
  24.             {  
  
  25.                 **if** (node.Peer == **null**) **continue**;  
  
  26.                 var command = node.Peer.Receive();  
  
  27.                 **if** (command == **null**) **continue**;  
  
  28.                 OnCommandReceived?.Invoke(**this**, command);  
  
  29.                 command.OnReceived(**this**.selfNode, node);  
  
  30.                 **if** (!**this**.isReceiving) **break**;  
  
  31.             }  
  
  32.   
  
  33.             Thread.Sleep(500);  
  
  34.         }  
  
  35.     }  
  
  36. }  
  
  -----------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionPool.cs

其中，

第3行，

## 节点连接

  -------------------------------------------------------------------------------------------------------------------------------------------------------------------
  1.  **public** **class** ConnectionPool : IDisposable  
  
  2.  {  
  
  3.      **private** Timer reconnectTimer;  
  
  4.    
  
  5.      **public** **void** Start()  
  
  6.      {  
  
  7.          ...  
  
  8.          **this**.reconnectTimer = **new** Timer((\_) =&gt; **this**.ConnectAll(), **null**, **new** TimeSpan(0, 0, 0, 0, 100), **new** TimeSpan(0, 0, 20));  
  
  9.      }  
  
  10.   
  
  11.     **private** **void** ConnectAll()  
  
  12.     {  
  
  13.         ConnectionNode\[\] internalnodes;  
  
  14.         **lock** (**this**.nodes)  
  
  15.         {  
  
  16.             internalnodes = **this**.nodes  
  
  17.                 .Where(\_ =&gt; \_.Status == ConnectionStatus.Initial || \_.Status == ConnectionStatus.Dead)  
  
  18.                 .Where(\_ =&gt; \_.Address != **null**)  
  
  19.                 .ToArray();  
  
  20.         }  
  
  21.         **foreach** (var node **in** internalnodes)  
  
  22.         {  
  
  23.             **this**.TryConnect(node);  
  
  24.         }  
  
  25.     }  
  
  26. }  
  
  -------------------------------------------------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionPool.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------------------------------------------------------------------------
  1.  **public** **class** ConnectionPool : IDisposable  
  
  2.  {  
  
  3.      **private** Timer reconnectTimer;  
  
  4.    
  
  5.      **public** **void** Start()  
  
  6.      {  
  
  7.          ...  
  
  8.          **this**.reconnectTimer = **new** Timer((\_) =&gt; **this**.ConnectAll(), **null**, **new** TimeSpan(0, 0, 0, 0, 100), **new** TimeSpan(0, 0, 20));  
  
  9.      }  
  
  10.   
  
  11.     **private** **void** ConnectAll()  
  
  12.     {  
  
  13.         ConnectionNode\[\] internalnodes;  
  
  14.         **lock** (**this**.nodes)  
  
  15.         {  
  
  16.             internalnodes = **this**.nodes  
  
  17.                 .Where(\_ =&gt; \_.Status == ConnectionStatus.Initial || \_.Status == ConnectionStatus.Dead)  
  
  18.                 .Where(\_ =&gt; \_.Address != **null**)  
  
  19.                 .ToArray();  
  
  20.         }  
  
  21.         **foreach** (var node **in** internalnodes)  
  
  22.         {  
  
  23.             **this**.TryConnect(node);  
  
  24.         }  
  
  25.     }  
  
  26. }  
  
  -------------------------------------------------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionPool.cs

其中，

第3行，

  ------------------------------------------------------------------
  1.  **private** **void** TryConnect(ConnectionNode node)  
  
  2.  {  
  
  3.      **if** (node.Peer != **null**)  
  
  4.      {  
  
  5.          node.Peer.Dispose();  
  
  6.          node.Peer = **null**;  
  
  7.      }  
  
  8.    
  
  9.      var peer = **this**.peerFactory.Produce();  
  
  10.     **try**  
  
  11.     {  
  
  12.         peer.Connect(node.Address);  
  
  13.         node.Peer = peer;  
  
  14.     }  
  
  15.     **catch** (Exception)  
  
  16.     {  
  
  17.         node.Status = ConnectionStatus.Dead;  
  
  18.     }  
  
  19.   
  
  20.     **if** (!peer.IsConnected)  
  
  21.     {  
  
  22.         Debug.WriteLine("open peer channel failed");  
  
  23.         node.Status = ConnectionStatus.Dead;  
  
  24.         **return**;  
  
  25.     }  
  
  26.   
  
  27.     **try**  
  
  28.     {  
  
  29.         peer.Send(**new** VersionCommand());  
  
  30.     }  
  
  31.     **catch** (Exception)  
  
  32.     {  
  
  33.         node.Status = ConnectionStatus.Dead;  
  
  34.     }  
  
  35.     **finally**  
  
  36.     {  
  
  37.         **if** (node.Status != ConnectionStatus.Connected)  
  
  38.         {  
  
  39.             peer.Close();  
  
  40.             peer.Dispose();  
  
  41.         }  
  
  42.     }  
  
  43. }  
  
  ------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionPool.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------
  1.  **public** **class** VersionCommand : CommandBase  
  
  2.  {  
  
  3.      **public** **override** **void** OnReceived(Node node, ConnectionNode connectionNode)  
  
  4.      {  
  
  5.          connectionNode.Status = ConnectionStatus.Connected;  
  
  6.          connectionNode.Peer.Send(**new** VersionAcknowledgeCommand());  
  
  7.      }  
  
  8.  }  
  
  -------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\VersionCommand.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------
  1.  **public** **class** VersionAcknowledgeCommand : CommandBase  
  
  2.  {  
  
  3.      **public** **override** **void** OnReceived(Node node, ConnectionNode connectionNode)  
  
  4.      {  
  
  5.          connectionNode.Status = ConnectionStatus.Connected;  
  
  6.      }  
  
  7.  }  
  
  -------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\VersionAcknowledgeCommand.cs

其中，

第3行，

