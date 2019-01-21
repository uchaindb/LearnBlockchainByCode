# 广播信息

## 主动广播信息

  ----------------------------------------------------------------------------
  1.  **private** **void** Broadcast(CommandBase command)  
  
  2.  {  
  
  3.      ConnectionNode\[\] internalnodes;  
  
  4.      **lock** (**this**.nodes)  
  
  5.      {  
  
  6.          internalnodes = **this**.nodes  
  
  7.              .Where(\_ =&gt; \_.Status == ConnectionStatus.Connected)  
  
  8.              .ToArray();  
  
  9.      }  
  
  10.     **foreach** (var node **in** internalnodes)  
  
  11.     {  
  
  12.         node.Peer.Send(command);  
  
  13.     }  
  
  14. }  
  
  ----------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionPool.cs

其中，

第3行，

## 广播区块

  ------------------------------------------------------------------------------------------
  1.  **public** **class** ConnectionPool : IDisposable  
  
  2.  {  
  
  3.      **public** ConnectionPool(...)  
  
  4.      {  
  
  5.          ...  
  
  6.          **this**.selfNode.Engine.OnNewBlockCreated += Engine\_OnNewBlockCreated;  
  
  7.      }  
  
  8.    
  
  9.      **private** **void** Engine\_OnNewBlockCreated(**object** sender, BlockHead e)  
  
  10.     {  
  
  11.         var blk = **this**.selfNode.Engine.BlockChain.GetBlock(e.Hash);  
  
  12.         **this**.Broadcast(**new** BlockCommand { Block = blk });  
  
  13.     }  
  
  14. }  
  
  ------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionPool.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------
  1.  **public** **class** BlockCommand : CommandBase  
  
  2.  {  
  
  3.      **public** Block Block { **get**; **set**; }  
  
  4.    
  
  5.      **public** **override** **void** OnReceived(Node node, ConnectionNode connectionNode)  
  
  6.      {  
  
  7.          var engine = node.Engine;  
  
  8.          var bc = engine.BlockChain;  
  
  9.          **if** (bc.BlockHeadDictionary.ContainsKey(**this**.Block.Hash)) **return**;  
  
  10.         bc.AddSyncBlock(**this**.Block);  
  
  11.     }  
  
  12. }  
  
  -------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\BlockCommand.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------------------------
  1.  **internal** **void** AddSyncBlock(Block block)  
  
  2.  {  
  
  3.      CacheBlock(block);  
  
  4.      // just cache block if prevous block not exist  
  
  5.      **if** (!**this**.BlockHeadDictionary.ContainsKey(block.Head.PreviousBlockHash)) **return**;  
  
  6.      **if** (**this**.TryMoveSyncTail(block.Head))  
  
  7.      {  
  
  8.          **this**.cancelSearchNonce = **true**;  
  
  9.      }  
  
  10. }  
  
  11.   
  
  12. **private** **void** CacheBlock(Block block)  
  
  13. {  
  
  14.     **this**.BlockDictionary\[block.Hash\] = block;  
  
  15.     **this**.InitBlocks(block.Head);  
  
  16. }  
  
  17.   
  
  18. **private** **bool** TryMoveSyncTail(BlockHead newTail)  
  
  19. {  
  
  20.     var listnow = **this**.ReverseIterateBlockHeaders(GenesisBlockHead.Hash, **this**.Tail.Hash).ToArray();  
  
  21.     var listnew = **this**.ReverseIterateBlockHeaders(GenesisBlockHead.Hash, newTail.Hash).ToArray();  
  
  22.     // broken chain should not count  
  
  23.     **if** (listnew.LastOrDefault()?.Hash != GenesisBlockHead.Hash) **return** **false**;  
  
  24.     var cnow = listnow.Length;  
  
  25.     var cnew = listnew.Length;  
  
  26.     **if** (cnew &gt; cnow)  
  
  27.     {  
  
  28.         MaintainBlockChain(newTail);  
  
  29.         **return** **true**;  
  
  30.     }  
  
  31.   
  
  32.     **return** **false**;  
  
  33. }  
  
  -------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Core\\BlockChain.cs

其中，

第3行，

## 广播交易

  -----------------------------------------------------------------------------------------
  1.  **public** **class** ConnectionPool : IDisposable  
  
  2.  {  
  
  3.      **public** ConnectionPool(...)  
  
  4.      {  
  
  5.          ...  
  
  6.          **this**.selfNode.Engine.OnNewTxCreated += Engine\_OnNewTxCreated;  
  
  7.      }  
  
  8.    
  
  9.      **private** **void** Engine\_OnNewTxCreated(**object** sender, Transaction e)  
  
  10.     {  
  
  11.         **this**.Broadcast(**new** TransactionCommand { Transaction = e });  
  
  12.     }  
  
  13. }  
  
  -----------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\ConnectionPool.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------
  1.  **public** **class** TransactionCommand : CommandBase  
  
  2.  {  
  
  3.      **public** Transaction Transaction { **get**; **set**; }  
  
  4.      **public** **override** **void** OnReceived(Node node, ConnectionNode connectionNode)  
  
  5.      {  
  
  6.          var bc = node.Engine.BlockChain;  
  
  7.          bc.SyncTx(**this**.Transaction);  
  
  8.      }  
  
  9.  }  
  
  -------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\TransactionCommand.cs

其中，

第3行，

  ----------------------------------------------------
  1.  **internal** **void** SyncTx(Transaction tx)  
  
  2.  {  
  
  3.      **this**.TxQueue.Enqueue(tx);  
  
  4.  }  
  
  ----------------------------------------------------

参考代码：ClassicBlockChain\\Core\\BlockChain.cs

其中，

第3行，

## 广播清单

  -----------------------------------------
  1.  **public** **enum** InventoryType  
  
  2.  {  
  
  3.      Transaction,  
  
  4.      Block,  
  
  5.  }  
  
  -----------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\InventoryEntity.cs

其中，

第3行，

  ---------------------------------------------------------------
  1.  **public** **class** InventoryEntity  
  
  2.  {  
  
  3.      **public** InventoryType Type { **get**; **set**; }  
  
  4.      **public** UInt256 Hash { **get**; **set**; }  
  
  5.  }  
  
  ---------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\InventoryEntity.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------
  1.  **public** **class** InventoryCommand : CommandBase  
  
  2.  {  
  
  3.      **public** InventoryEntity\[\] Items { **get**; **set**; }  
  
  4.      **public** **override** **void** OnReceived(Node node, ConnectionNode connectionNode)  
  
  5.      {  
  
  6.          var responseCmd = **new** GetDataCommand { Items = **this**.Items };  
  
  7.          connectionNode.Peer.Send(responseCmd);  
  
  8.      }  
  
  9.  }  
  
  -------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\InventoryCommand.cs

其中，

第3行，

