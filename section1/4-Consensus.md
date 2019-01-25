读书提示：本书[发布在此](https://book.uchaindb.com/)，具有更好的阅读体验。

# 达成共识

## 快速区块补足定位算法

```cs
private static long[] GetBlockLocatorIndexes(long height)  
{  
    var indexes = new List<long>();  
    var step = 1;  
    var i = height;  
    do  
    {  
        if (indexes.Count >= 10) step *= 2;  
        indexes.Add(i);  
        i -= step;  
    } while (i > 0);  
  
    indexes.Add(0);  
    return indexes.ToArray();  
}  
```
<!-- code:ClassicBlockChain/Core/BlockChain.cs -->

```cs
public UInt256[] GetBlockLocatorHashes()  
{  
    var indexes = GetBlockLocatorIndexes(this.Height);  
    return this.GetBlockHeaders(this.Tail.Hash)  
        .Where((hd, i) => indexes.Contains(i))  
        .Select(_ => _.Hash)  
        .ToArray();  
}  
```
<!-- code:ClassicBlockChain/Core/BlockChain.cs -->

## 区块补足机制

## 旧节点数据提供

```cs
public IEnumerable<Block> GetBlocks(UInt256 startingHash)  
{  
    return this.GetBlockHeaders(startingHash)  
        .Select(_ => this.GetBlock(_.Hash));  
}  
  
public IEnumerable<BlockHead> GetBlockHeaders(UInt256 startingHash)  
{  
    return this.ReverseIterateBlockHeaders(startingHash, this.Tail.Hash)  
        .Reverse();  
}  
  
internal IEnumerable<BlockHead> ReverseIterateBlockHeaders(UInt256 from, UInt256 to)  
{  
    var cursor = this.BlockHeadDictionary[to];  
    while (cursor.Hash != from)  
    {  
        if (cursor.PreviousBlockHash == null) yield break;  
        yield return cursor;  
        if (!this.BlockHeadDictionary.TryGetValue(cursor.PreviousBlockHash, out cursor))  
            yield break;  
    }  
  
    yield return cursor;  
}  
```
<!-- code:ClassicBlockChain/Core/BlockChain.cs -->

```cs
public class GetDataCommand : CommandBase  
{  
    public InventoryEntity[] Items { get; set; }  
    public override void OnReceived(Node node, ConnectionNode connectionNode)  
    {  
        var bc = node.Engine.BlockChain;  
        foreach (var item in this.Items)  
        {  
            switch (item.Type)  
            {  
                case InventoryType.Transaction:  
                    var tx = bc.GetTx(item.Hash);  
                    if (tx != null)  
                    {  
                        var responseCmd = new TransactionCommand { Transaction = tx };  
                        connectionNode.Peer.Send(responseCmd);  
                    }  
                    break;  
                case InventoryType.Block:  
                    var blk = bc.GetBlock(item.Hash);  
                    if (blk != null)  
                    {  
                        var responseCmd = new BlockCommand { Block = blk };  
                        connectionNode.Peer.Send(responseCmd);  
                    }  
                    break;  
                default:  
                    break;  
            }  
        }  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/GetDataCommand.cs -->

## 新节点数据获取

```cs
public class BlockCommand : CommandBase  
{  
    public override void OnReceived(Node node, ConnectionNode connectionNode)  
    {  
        ...  
        if (bc.BlockHeadDictionary.ContainsKey(this.Block.Head.PreviousBlockHash))  
        {  
            var getblkcmd = new GetBlocksCommand  
            {  
                BlockLocators = engine.BlockChain.GetBlockLocatorHashes(),  
                LastBlockHash = bc.Tail.Hash,  
            };  
            connectionNode.Peer.Send(getblkcmd);  
        }  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/BlockCommand.cs -->

```cs
public abstract class BlockLocatorCommandBase : CommandBase  
{  
    public UInt256 LastBlockHash { get; set; }  
    public UInt256[] BlockLocators { get; set; }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/BlockLocatorCommandBase.cs -->

```cs
public class GetBlocksCommand : BlockLocatorCommandBase  
{  
    private const int MaxBlockRetrivalNumber = 100;  
    public override void OnReceived(Node node, ConnectionNode connectionNode)  
    {  
        var engine = node.Engine;  
        var recentValidHash = UInt256.Zero;  
  
        for (int i = 0; i < this.BlockLocators.Length; i++)  
        {  
            var hash = this.BlockLocators[i];  
  
            if (engine.BlockChain.BlockHeadDictionary.TryGetValue(hash, out var block))  
            {  
                recentValidHash = block.Hash;  
                break;  
            }  
        }  
  
        var items = engine.BlockChain.GetBlockHeaders(recentValidHash)  
            .Take(MaxBlockRetrivalNumber)  
            .Select(_ => new InventoryEntity(InventoryType.Block, _.Hash))  
            .ToArray();  
  
        var responseCmd = new InventoryCommand { Items = items };  
        connectionNode.Peer.Send(responseCmd);  
    }  
}  
```
<!-- code:ClassicBlockChain/Network/RpcCommands/GetBlocksCommand.cs -->

## 工作量证明（POW）

