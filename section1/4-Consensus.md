# 达成共识

## 快速区块补足定位算法

  -----------------------------------------------------------------------------------
  1.  **private** **static** **long**\[\] GetBlockLocatorIndexes(**long** height)  
  
  2.  {  
  
  3.      var indexes = **new** List&lt;**long**&gt;();  
  
  4.      var step = 1;  
  
  5.      var i = height;  
  
  6.      **do**  
  
  7.      {  
  
  8.          **if** (indexes.Count &gt;= 10) step \*= 2;  
  
  9.          indexes.Add(i);  
  
  10.         i -= step;  
  
  11.     } **while** (i &gt; 0);  
  
  12.   
  
  13.     indexes.Add(0);  
  
  14.     **return** indexes.ToArray();  
  
  15. }  
  
  -----------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Core\\BlockChain.cs

其中，

第3行，

  -------------------------------------------------------------------
  1.  **public** UInt256\[\] GetBlockLocatorHashes()  
  
  2.  {  
  
  3.      var indexes = GetBlockLocatorIndexes(**this**.Height);  
  
  4.      **return** **this**.GetBlockHeaders(**this**.Tail.Hash)  
  
  5.          .Where((hd, i) =&gt; indexes.Contains(i))  
  
  6.          .Select(\_ =&gt; \_.Hash)  
  
  7.          .ToArray();  
  
  8.  }  
  
  -------------------------------------------------------------------

参考代码：ClassicBlockChain\\Core\\BlockChain.cs

其中，

第3行，

## 区块补足机制

## 旧节点数据提供

  ------------------------------------------------------------------------------------------------------------
  1.  **public** IEnumerable&lt;Block&gt; GetBlocks(UInt256 startingHash)  
  
  2.  {  
  
  3.      **return** **this**.GetBlockHeaders(startingHash)  
  
  4.          .Select(\_ =&gt; **this**.GetBlock(\_.Hash));  
  
  5.  }  
  
  6.    
  
  7.  **public** IEnumerable&lt;BlockHead&gt; GetBlockHeaders(UInt256 startingHash)  
  
  8.  {  
  
  9.      **return** **this**.ReverseIterateBlockHeaders(startingHash, **this**.Tail.Hash)  
  
  10.         .Reverse();  
  
  11. }  
  
  12.   
  
  13. **internal** IEnumerable&lt;BlockHead&gt; ReverseIterateBlockHeaders(UInt256 from, UInt256 to)  
  
  14. {  
  
  15.     var cursor = **this**.BlockHeadDictionary\[to\];  
  
  16.     **while** (cursor.Hash != from)  
  
  17.     {  
  
  18.         **if** (cursor.PreviousBlockHash == **null**) yield **break**;  
  
  19.         yield **return** cursor;  
  
  20.         **if** (!**this**.BlockHeadDictionary.TryGetValue(cursor.PreviousBlockHash, **out** cursor))  
  
  21.             yield **break**;  
  
  22.     }  
  
  23.   
  
  24.     yield **return** cursor;  
  
  25. }  
  
  ------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Core\\BlockChain.cs

其中，

第3行，

  --------------------------------------------------------------------------------------------------
  1.  **public** **class** GetDataCommand : CommandBase  
  
  2.  {  
  
  3.      **public** InventoryEntity\[\] Items { **get**; **set**; }  
  
  4.      **public** **override** **void** OnReceived(Node node, ConnectionNode connectionNode)  
  
  5.      {  
  
  6.          var bc = node.Engine.BlockChain;  
  
  7.          **foreach** (var item **in** **this**.Items)  
  
  8.          {  
  
  9.              **switch** (item.Type)  
  
  10.             {  
  
  11.                 **case** InventoryType.Transaction:  
  
  12.                     var tx = bc.GetTx(item.Hash);  
  
  13.                     **if** (tx != **null**)  
  
  14.                     {  
  
  15.                         var responseCmd = **new** TransactionCommand { Transaction = tx };  
  
  16.                         connectionNode.Peer.Send(responseCmd);  
  
  17.                     }  
  
  18.                     **break**;  
  
  19.                 **case** InventoryType.Block:  
  
  20.                     var blk = bc.GetBlock(item.Hash);  
  
  21.                     **if** (blk != **null**)  
  
  22.                     {  
  
  23.                         var responseCmd = **new** BlockCommand { Block = blk };  
  
  24.                         connectionNode.Peer.Send(responseCmd);  
  
  25.                     }  
  
  26.                     **break**;  
  
  27.                 **default**:  
  
  28.                     **break**;  
  
  29.             }  
  
  30.         }  
  
  31.     }  
  
  32. }  
  
  --------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\GetDataCommand.cs

其中，

第3行，

## 新节点数据获取

  --------------------------------------------------------------------------------------------------
  1.  **public** **class** BlockCommand : CommandBase  
  
  2.  {  
  
  3.      **public** **override** **void** OnReceived(Node node, ConnectionNode connectionNode)  
  
  4.      {  
  
  5.          ...  
  
  6.          **if** (bc.BlockHeadDictionary.ContainsKey(**this**.Block.Head.PreviousBlockHash))  
  
  7.          {  
  
  8.              var getblkcmd = **new** GetBlocksCommand  
  
  9.              {  
  
  10.                 BlockLocators = engine.BlockChain.GetBlockLocatorHashes(),  
  
  11.                 LastBlockHash = bc.Tail.Hash,  
  
  12.             };  
  
  13.             connectionNode.Peer.Send(getblkcmd);  
  
  14.         }  
  
  15.     }  
  
  16. }  
  
  --------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\BlockCommand.cs

其中，

第3行，

  -------------------------------------------------------------------------------
  1.  **public** **abstract** **class** BlockLocatorCommandBase : CommandBase  
  
  2.  {  
  
  3.      **public** UInt256 LastBlockHash { **get**; **set**; }  
  
  4.      **public** UInt256\[\] BlockLocators { **get**; **set**; }  
  
  5.  }  
  
  -------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\BlockLocatorCommandBase.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------------
  1.  **public** **class** GetBlocksCommand : BlockLocatorCommandBase  
  
  2.  {  
  
  3.      **private** **const** **int** MaxBlockRetrivalNumber = 100;  
  
  4.      **public** **override** **void** OnReceived(Node node, ConnectionNode connectionNode)  
  
  5.      {  
  
  6.          var engine = node.Engine;  
  
  7.          var recentValidHash = UInt256.Zero;  
  
  8.    
  
  9.          **for** (**int** i = 0; i &lt; **this**.BlockLocators.Length; i++)  
  
  10.         {  
  
  11.             var hash = **this**.BlockLocators\[i\];  
  
  12.   
  
  13.             **if** (engine.BlockChain.BlockHeadDictionary.TryGetValue(hash, **out** var block))  
  
  14.             {  
  
  15.                 recentValidHash = block.Hash;  
  
  16.                 **break**;  
  
  17.             }  
  
  18.         }  
  
  19.   
  
  20.         var items = engine.BlockChain.GetBlockHeaders(recentValidHash)  
  
  21.             .Take(MaxBlockRetrivalNumber)  
  
  22.             .Select(\_ =&gt; **new** InventoryEntity(InventoryType.Block, \_.Hash))  
  
  23.             .ToArray();  
  
  24.   
  
  25.         var responseCmd = **new** InventoryCommand { Items = items };  
  
  26.         connectionNode.Peer.Send(responseCmd);  
  
  27.     }  
  
  28. }  
  
  -------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Network\\RpcCommands\\GetBlocksCommand.cs

其中，

第3行，

## 工作量证明（POW）

