# 智能合约

## 交易结构

## 智能合约

```cs
public static UnlockScripts ProduceSingleUnlockScript(this Signature sig) => (UnlockScripts)new[] {  
    ScriptToken.CreateToken(sig.ToBase58()),  
};  
```
<!-- code:ClassicBlockChain/SmartContracts/TokenScriptExtension.cs -->

```cs
public static LockScripts ProduceSingleLockScript(this PublicKey pubKey) => (LockScripts)new[] {  
    ScriptToken.CreateToken(pubKey.ToBase58()),  
    ScriptToken.CreateToken(OpCode.CheckSignature),  
};  
```
<!-- code:ClassicBlockChain/SmartContracts/TokenScriptExtension.cs -->

## 验证交易

```cs
public static bool CanUnlock(this Transaction tran, TxInput input, TxOutput output)  
{  
    try  
    {  
        var scripts = input.UnlockScripts + output.LockScripts;  
        var result = scripts.TryExecuteAsync(tran);  
        if (!result) return false;  
    }  
    catch (Exception)  
    {  
        return false;  
    }  
  
    return true;  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/TokenScriptExtension.cs -->

```cs
private BlockHead GenerateBlock()  
{  
    ...  
    var minerTxOut = new TxOutput  
    {  
        LockScripts = this.MinerWallet.PublicKey.ProduceSingleLockScript(),  
        Value = this.BlockChain.RewardOfBlock + fee  
    };  
    var minerTx = new Transaction { Outputs = new[] { minerTxOut }, };  
    ...  
}  
```
<!-- code:ClassicBlockChain/Core/Engine.cs -->

```cs
private bool ValidateTx(Transaction tx)  
{  
    ...  
    foreach (var intx in tx.InputTxs)  
    {  
        ...  
        if (!verifyTx.CanUnlock(intx, output))  
            return false;  
    }  
    ...  
}  
```
<!-- code:ClassicBlockChain/Core/Engine.cs -->

## 客户端适应

```cs
public Transaction SendMoney(Engine engine, Transaction utxo, int index, IWallet receiver, int value, int fee = 0, uint lockTime = 0)  
{  
    var total = utxo.Outputs[index].Value;  
    var change = total - value - fee;  
    var mainOutput = new TxOutput { LockScripts = receiver.PublicKey.ProduceSingleLockScript(), Value = value };  
    var changeOutput = new TxOutput { LockScripts = this.PublicKey.ProduceSingleLockScript(), Value = change };  
    return this.SendMoney(engine, lockTime, new[] { new Utxo(utxo, index) }, mainOutput, changeOutput);  
}  
```
<!-- code:ClassicBlockChain/Wallet/BaseWallet.cs -->

```cs
public Transaction SendMoney(Engine engine, uint lockTime, Utxo[] utxos, params TxOutput[] outputs)  
{  
    var inputTxs = utxos  
        .Select(_ => new TxInput { PrevTxHash = _.Tx.Hash, PrevTxIndex = _.Index })  
        .ToArray();  
    var tx = new Transaction  
    {  
        InputTxs = inputTxs,  
        Outputs = outputs,  
        LockTime = lockTime,  
    };  
    var sigList = new Signature[tx.InputTxs.Length];  
    for (int i = 0; i < tx.InputTxs.Length; i++)  
    {  
        var utxoEnt = utxos[i];  
        sigList[i] = this.signAlgo.Sign(  
            new[] { (byte[])tx.GetLockHash() },  
            this.FindPrivateKey(utxoEnt.Tx.Outputs[utxoEnt.Index].LockScripts));  
    }  
  
    for (int i = 0; i < tx.InputTxs.Length; i++)  
    {  
        tx.InputTxs[i].UnlockScripts = sigList[i].ProduceSingleUnlockScript();  
    }  
    engine.AttachTx(tx);  
  
    return tx;  
}  
```
<!-- code:ClassicBlockChain/Wallet/BaseWallet.cs -->

```cs
protected abstract PrivateKey FindPrivateKey(LockScripts lockScripts);  
protected abstract bool ContainPubKey(LockScripts lockScripts);  
```
<!-- code:ClassicBlockChain/Wallet/BaseWallet.cs -->

```cs
public class DeterministicWallet : BaseWallet  
{  
    protected override PrivateKey FindPrivateKey(LockScripts lockScripts)  
    {  
        var idx = this.usedPublicKeys.FindIndex(_ => lockScripts.Contains(new ScriptToken(_.ToBase58())));  
        if (idx == -1)  
            throw new KeyNotFoundException("cannot find corresponding public key");  
        return this.usedPrivateKeys[idx];  
    }  
  
    protected override bool ContainPubKey(LockScripts lockScripts)  
    {  
        return this.usedPublicKeys.Any(_ => lockScripts.Contains(new ScriptToken(_.ToBase58())));  
    }  
}  
```
<!-- code:ClassicBlockChain/Wallet/DeterministicWallet.cs -->

```cs
public class SimpleWallet : BaseWallet  
{  
    protected override PrivateKey FindPrivateKey(LockScripts lockScripts)  
    {  
        if (!lockScripts.Contains(new ScriptToken(this.PublicKey.ToBase58())))  
            throw new KeyNotFoundException("cannot find corresponding public key");  
        return this.PrivateKey;  
    }  
  
    protected override bool ContainPubKey(LockScripts lockScripts)  
    {  
        return lockScripts.Contains(new ScriptToken(this.PublicKey.ToBase58()));  
    }  
}  
```
<!-- code:ClassicBlockChain/Wallet/SimpleWallet.cs -->
