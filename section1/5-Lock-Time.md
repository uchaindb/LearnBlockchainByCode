# 最简合约：锁定时间

## 交易结构

  ------------------------------------------------------
  public class Transaction : HashBase
  
  {
  
  private byte version;
  
  private TxInput\[\] inputTxs = new TxInput\[\] { };
  
  private TxOutput\[\] outputs = new TxOutput\[\] { };
  
  private uint lockTime;
  
  }
  ------------------------------------------------------

参考代码：ClassicBlockChain\\Entity\\Transaction.cs

其中，

第3行，

## 验证交易

  ---------------------------------------------------------------------------------
  1.  **public** **class** Engine : IDisposable  
  
  2.  {  
  
  3.      **private** **const** **uint** LockTimeBreakPoint = 1\_500\_000\_000;  
  
  4.      **private** BlockHead GenerateBlock()  
  
  5.      {  
  
  6.          var finalTxs = **this**.BlockChain.DequeueTxs()  
  
  7.              .Where(**this**.ValidateTx)  
  
  8.              .Where(**this**.ValidateLockTime)  
  
  9.              .ToList();  
  
  10.         ...  
  
  11.     }  
  
  12.   
  
  13.     **private** **bool** ValidateLockTime(Transaction tx)  
  
  14. }  
  
  ---------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Core\\Engine.cs

其中，

第3行，

  ------------------------------------------------------------------------------------
  1.  **private** **bool** ValidateLockTime(Transaction tx)  
  
  2.  {  
  
  3.      **return** tx.InputTxs  
  
  4.          .Select(\_ =&gt; **this**.BlockChain.GetTx(\_.PrevTxHash))  
  
  5.          .All(\_ =&gt; **this**.ValidateLockTime(\_.LockTime, DateTime.Now));  
  
  6.  }  
  
  7.    
  
  8.  **private** **bool** ValidateLockTime(**uint** lockTime, DateTime time)  
  
  9.  {  
  
  10.     **if** (lockTime == 0) **return** **true**;  
  
  11.     **if** (lockTime &gt; LockTimeBreakPoint)  
  
  12.     {  
  
  13.         var lockdt = DateTimeOffset.FromUnixTimeSeconds(lockTime);  
  
  14.         **return** lockdt &gt; time;  
  
  15.     }  
  
  16.     **else**  
  
  17.     {  
  
  18.         **return** **this**.BlockChain.Height &gt; lockTime;  
  
  19.     }  
  
  20. }  
  
  ------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Core\\Engine.cs

其中，

第3行，


