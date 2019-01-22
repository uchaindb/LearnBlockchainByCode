# 广播信息

## 主动广播信息

```cs
private void Broadcast(CommandBase command)  
{  
    ConnectionNode[] internalnodes;  
    lock (this.nodes)  
    {  
        internalnodes = this.nodes  
            .Where(_ => _.Status == ConnectionStatus.Connected)  
            .ToArray();  
    }  
    foreach (var node in internalnodes)  
    {  
        node.Peer.Send(command);  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/ConnectionPool.cs -->

## 广播区块

```cs
public class ConnectionPool : IDisposable  
{  
    public ConnectionPool(...)  
    {  
        ...  
        this.selfNode.Engine.OnNewBlockCreated += Engine_OnNewBlockCreated;  
    }  
  
    private void Engine_OnNewBlockCreated(object sender, BlockHead e)  
    {  
        var blk = this.selfNode.Engine.BlockChain.GetBlock(e.Hash);  
        this.Broadcast(new BlockCommand { Block = blk });  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/ConnectionPool.cs -->

```cs
public class BlockCommand : CommandBase  
{  
    public Block Block { get; set; }  
  
    public override void OnReceived(Node node, ConnectionNode connectionNode)  
    {  
        var engine = node.Engine;  
        var bc = engine.BlockChain;  
        if (bc.BlockHeadDictionary.ContainsKey(this.Block.Hash)) return;  
        bc.AddSyncBlock(this.Block);  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/BlockCommand.cs -->

```cs
internal void AddSyncBlock(Block block)  
{  
    CacheBlock(block);  
    // just cache block if prevous block not exist  
    if (!this.BlockHeadDictionary.ContainsKey(block.Head.PreviousBlockHash)) return;  
    if (this.TryMoveSyncTail(block.Head))  
    {  
        this.cancelSearchNonce = true;  
    }  
}  
  
private void CacheBlock(Block block)  
{  
    this.BlockDictionary[block.Hash] = block;  
    this.InitBlocks(block.Head);  
}  
  
private bool TryMoveSyncTail(BlockHead newTail)  
{  
    var listnow = this.ReverseIterateBlockHeaders(GenesisBlockHead.Hash, this.Tail.Hash).ToArray();  
    var listnew = this.ReverseIterateBlockHeaders(GenesisBlockHead.Hash, newTail.Hash).ToArray();  
    // broken chain should not count  
    if (listnew.LastOrDefault()?.Hash != GenesisBlockHead.Hash) return false;  
    var cnow = listnow.Length;  
    var cnew = listnew.Length;  
    if (cnew > cnow)  
    {  
        MaintainBlockChain(newTail);  
        return true;  
    }  
  
    return false;  
}  
```
<!-- code:ClassicBlockChain/Core/BlockChain.cs -->

## 广播交易

```cs
public class ConnectionPool : IDisposable  
{  
    public ConnectionPool(...)  
    {  
        ...  
        this.selfNode.Engine.OnNewTxCreated += Engine_OnNewTxCreated;  
    }  
  
    private void Engine_OnNewTxCreated(object sender, Transaction e)  
    {  
        this.Broadcast(new TransactionCommand { Transaction = e });  
    }  13.	}  
```
<!-- code:ClassicBlockChain/Network/ConnectionPool.cs -->

```cs
public class TransactionCommand : CommandBase  
{  
    public Transaction Transaction { get; set; }  
    public override void OnReceived(Node node, ConnectionNode connectionNode)  
    {  
        var bc = node.Engine.BlockChain;  
        bc.SyncTx(this.Transaction);  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/TransactionCommand.cs -->

```cs
internal void SyncTx(Transaction tx)  
{  
    this.TxQueue.Enqueue(tx);  
}  
```
<!-- code:ClassicBlockChain/Core/BlockChain.cs -->

##	广播清单

```cs
public enum InventoryType  
{  
    Transaction,  
    Block,  
}  
代码：ClassicBlockChain\Network\RpcCommands\InventoryEntity.cs
```
<!-- code:ClassicBlockChain/Network/RpcCommands/InventoryEntity.cs -->

```cs
public class InventoryEntity  
{  
    public InventoryType Type { get; set; }  
    public UInt256 Hash { get; set; }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/InventoryEntity.cs -->

```cs
public class InventoryCommand : CommandBase  
{  
    public InventoryEntity[] Items { get; set; }  
    public override void OnReceived(Node node, ConnectionNode connectionNode)  
    {  
        var responseCmd = new GetDataCommand { Items = this.Items };  
        connectionNode.Peer.Send(responseCmd);  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/InventoryCommand.cs -->

