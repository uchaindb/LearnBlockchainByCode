# 最简合约：锁定时间

## 交易结构

```cs
public class Transaction : HashBase
{
    private byte version;
    private TxInput[] inputTxs = new TxInput[] { };
    private TxOutput[] outputs = new TxOutput[] { };
    private uint lockTime;
}
代码：ClassicBlockChain\Entity\Transaction.cs
```
<!-- code:ClassicBlockChain/Entity/Transaction.cs -->

## 验证交易

```cs
public class Engine : IDisposable  
{  
    private const uint LockTimeBreakPoint = 1_500_000_000;  
    private BlockHead GenerateBlock()  
    {  
        var finalTxs = this.BlockChain.DequeueTxs()  
            .Where(this.ValidateTx)  
            .Where(this.ValidateLockTime)  
            .ToList();  
        ...  
    }  
  
    private bool ValidateLockTime(Transaction tx)  
}  
```
<!-- code:ClassicBlockChain/Core/Engine.cs -->

```cs
private bool ValidateLockTime(Transaction tx)  
{  
    return tx.InputTxs  
        .Select(_ => this.BlockChain.GetTx(_.PrevTxHash))  
        .All(_ => this.ValidateLockTime(_.LockTime, DateTime.Now));  
}  
  
private bool ValidateLockTime(uint lockTime, DateTime time)  
{  
    if (lockTime == 0) return true;  
    if (lockTime > LockTimeBreakPoint)  
    {  
        var lockdt = DateTimeOffset.FromUnixTimeSeconds(lockTime);  
        return lockdt > time;  
    }  
    else  
    {  
        return this.BlockChain.Height > lockTime;  
    }  
}  
```
<!-- code:ClassicBlockChain/Core/Engine.cs -->

