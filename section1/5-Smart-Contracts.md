# 智能合约

## 交易结构

## 智能合约

  --------------------------------------------------------------------------------------------------------------------------------
  1.  **public** **static** UnlockScripts ProduceSingleUnlockScript(**this** Signature sig) =&gt; (UnlockScripts)**new**\[\] {  
  
  2.      ScriptToken.CreateToken(sig.ToBase58()),  
  
  3.  };  
  
  --------------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\TokenScriptExtension.cs

其中，

第3行，

  -----------------------------------------------------------------------------------------------------------------------------
  1.  **public** **static** LockScripts ProduceSingleLockScript(**this** PublicKey pubKey) =&gt; (LockScripts)**new**\[\] {  
  
  2.      ScriptToken.CreateToken(pubKey.ToBase58()),  
  
  3.      ScriptToken.CreateToken(OpCode.CheckSignature),  
  
  4.  };  
  
  -----------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\TokenScriptExtension.cs

其中，

第3行，

## 验证交易

  -----------------------------------------------------------------------------------------------------------
  1.  **public** **static** **bool** CanUnlock(**this** Transaction tran, TxInput input, TxOutput output)  
  
  2.  {  
  
  3.      **try**  
  
  4.      {  
  
  5.          var scripts = input.UnlockScripts + output.LockScripts;  
  
  6.          var result = scripts.TryExecuteAsync(tran);  
  
  7.          **if** (!result) **return** **false**;  
  
  8.      }  
  
  9.      **catch** (Exception)  
  
  10.     {  
  
  11.         **return** **false**;  
  
  12.     }  
  
  13.   
  
  14.     **return** **true**;  
  
  15. }  
  
  -----------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\TokenScriptExtension.cs

其中，

第3行，

  ----------------------------------------------------------------------------------------
  1.  **private** BlockHead GenerateBlock()  
  
  2.  {  
  
  3.      ...  
  
  4.      var minerTxOut = **new** TxOutput  
  
  5.      {  
  
  6.          LockScripts = **this**.MinerWallet.PublicKey.ProduceSingleLockScript(),  
  
  7.          Value = **this**.BlockChain.RewardOfBlock + fee  
  
  8.      };  
  
  9.      var minerTx = **new** Transaction { Outputs = **new**\[\] { minerTxOut }, };  
  
  10.     ...  
  
  11. }  
  
  ----------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Core\\Engine.cs

其中，

第3行，

  ----------------------------------------------------------
  1.  **private** **bool** ValidateTx(Transaction tx)  
  
  2.  {  
  
  3.      ...  
  
  4.      **foreach** (var intx **in** tx.InputTxs)  
  
  5.      {  
  
  6.          ...  
  
  7.          **if** (!verifyTx.CanUnlock(intx, output))  
  
  8.              **return** **false**;  
  
  9.      }  
  
  10.     ...  
  
  11. }  
  
  ----------------------------------------------------------

参考代码：ClassicBlockChain\\Core\\Engine.cs

其中，

第3行，

## 客户端适应

  -----------------------------------------------------------------------------------------------------------------------------------------------------------------
  1.  **public** Transaction SendMoney(Engine engine, Transaction utxo, **int** index, IWallet receiver, **int** value, **int** fee = 0, **uint** lockTime = 0)  
  
  2.  {  
  
  3.      var total = utxo.Outputs\[index\].Value;  
  
  4.      var change = total - value - fee;  
  
  5.      var mainOutput = **new** TxOutput { LockScripts = receiver.PublicKey.ProduceSingleLockScript(), Value = value };  
  
  6.      var changeOutput = **new** TxOutput { LockScripts = **this**.PublicKey.ProduceSingleLockScript(), Value = change };  
  
  7.      **return** **this**.SendMoney(engine, lockTime, **new**\[\] { **new** Utxo(utxo, index) }, mainOutput, changeOutput);  
  
  8.  }  
  
  -----------------------------------------------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Wallet\\BaseWallet.cs

其中，

第3行，

  ---------------------------------------------------------------------------------------------------------------------------
  1.  **public** Transaction SendMoney(Engine engine, **uint** lockTime, Utxo\[\] utxos, **params** TxOutput\[\] outputs)  
  
  2.  {  
  
  3.      var inputTxs = utxos  
  
  4.          .Select(\_ =&gt; **new** TxInput { PrevTxHash = \_.Tx.Hash, PrevTxIndex = \_.Index })  
  
  5.          .ToArray();  
  
  6.      var tx = **new** Transaction  
  
  7.      {  
  
  8.          InputTxs = inputTxs,  
  
  9.          Outputs = outputs,  
  
  10.         LockTime = lockTime,  
  
  11.     };  
  
  12.     var sigList = **new** Signature\[tx.InputTxs.Length\];  
  
  13.     **for** (**int** i = 0; i &lt; tx.InputTxs.Length; i++)  
  
  14.     {  
  
  15.         var utxoEnt = utxos\[i\];  
  
  16.         sigList\[i\] = **this**.signAlgo.Sign(  
  
  17.             **new**\[\] { (**byte**\[\])tx.GetLockHash() },  
  
  18.             **this**.FindPrivateKey(utxoEnt.Tx.Outputs\[utxoEnt.Index\].LockScripts));  
  
  19.     }  
  
  20.   
  
  21.     **for** (**int** i = 0; i &lt; tx.InputTxs.Length; i++)  
  
  22.     {  
  
  23.         tx.InputTxs\[i\].UnlockScripts = sigList\[i\].ProduceSingleUnlockScript();  
  
  24.     }  
  
  25.     engine.AttachTx(tx);  
  
  26.   
  
  27.     **return** tx;  
  
  28. }  
  
  ---------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Wallet\\BaseWallet.cs

其中，

第3行，

  --------------------------------------------------------------------------------------
  1.  **protected** **abstract** PrivateKey FindPrivateKey(LockScripts lockScripts);  
  
  2.  **protected** **abstract** **bool** ContainPubKey(LockScripts lockScripts);  
  
  --------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Wallet\\BaseWallet.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------------------------------------
  1.  **public** **class** DeterministicWallet : BaseWallet  
  
  2.  {  
  
  3.      **protected** **override** PrivateKey FindPrivateKey(LockScripts lockScripts)  
  
  4.      {  
  
  5.          var idx = **this**.usedPublicKeys.FindIndex(\_ =&gt; lockScripts.Contains(**new** ScriptToken(\_.ToBase58())));  
  
  6.          **if** (idx == -1)  
  
  7.              **throw** **new** KeyNotFoundException("cannot find corresponding public key");  
  
  8.          **return** **this**.usedPrivateKeys\[idx\];  
  
  9.      }  
  
  10.   
  
  11.     **protected** **override** **bool** ContainPubKey(LockScripts lockScripts)  
  
  12.     {  
  
  13.         **return** **this**.usedPublicKeys.Any(\_ =&gt; lockScripts.Contains(**new** ScriptToken(\_.ToBase58())));  
  
  14.     }  
  
  15. }  
  
  -------------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Wallet\\DeterministicWallet.cs

其中，

第3行，

  ----------------------------------------------------------------------------------------------------
  1.  **public** **class** SimpleWallet : BaseWallet  
  
  2.  {  
  
  3.      **protected** **override** PrivateKey FindPrivateKey(LockScripts lockScripts)  
  
  4.      {  
  
  5.          **if** (!lockScripts.Contains(**new** ScriptToken(**this**.PublicKey.ToBase58())))  
  
  6.              **throw** **new** KeyNotFoundException("cannot find corresponding public key");  
  
  7.          **return** **this**.PrivateKey;  
  
  8.      }  
  
  9.    
  
  10.     **protected** **override** **bool** ContainPubKey(LockScripts lockScripts)  
  
  11.     {  
  
  12.         **return** lockScripts.Contains(**new** ScriptToken(**this**.PublicKey.ToBase58()));  
  
  13.     }  
  
  14. }  
  
  ----------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\Wallet\\SimpleWallet.cs

其中，

第3行，

