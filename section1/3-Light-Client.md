# 轻量客户端

在第一章已经学习过轻量客户端的概念，并知道了并非所有的节点都有能力储存完整的区块链，
因此对于这些被设计成运行在空间和功率受限的设备的轻量客户端，
通过简化的支付验证(SPV)的方式可以使它们在不必存储完整区块链的情况下进行工作。
轻量客户端仅仅需要下载区块头信息，而不用下载包含在每个区块中的交易信息。
由此产生的不含交易信息的区块链，大小会远远小于完整的区块链，这样便适合在受限环境上执行。
鉴于轻量客户端并不知道网络上所有交易的完整信息，其验证交易时所使用的方法略有不同，
这个方法需依赖对等的全节点“按需"提供区块链中相关部分的局部视图。

在本节中。。。。。。

## 客户端基础

```cs
public interface IWallet  
{  
    string Name { get; }  
    PrivateKey PrivateKey { get; }  
    PublicKey PublicKey { get; }  
  
    Transaction SendMoney(Engine engine, Transaction utxo, int index, IWallet receiver, int value, int fee = 0);  
    Transaction SendMoney(Engine engine, Utxo[] utxos, params TxOutput[] outputs);  
    void GenerateKeyPair();  
    Utxo[] GetUtxos(Engine engine);  
    void SyncBlockHead(Engine engine);  
    bool VerifyTx(Engine engine, Transaction tx);  
}  
```
<!-- code:ClassicBlockChain/Wallet/IWallet.cs -->
代码：ClassicBlockChain\Wallet\IWallet.cs

```cs
public abstract class BaseWallet : IWallet  
{  
    protected ISignAlgorithm signAlgo = new ECDsaSignAlgorithm();  
    protected Dictionary<UInt256, BlockHead> blockHeads  
  
    protected BaseWallet(string name)  
    {  
        this.Name = name;  
        this.GenerateKeyPair();  
    }  
  
    ...  
  
    protected virtual void AfterKeyPairGenerated()  
    protected abstract PrivateKey FindPrivateKey(PublicKey publicKey);  
    protected abstract bool ContainPubKey(PublicKey publicKey); 
}	
```
<!-- code:ClassicBlockChain/Wallet/BaseWallet.cs -->

```cs
public Transaction SendMoney(Engine engine, Utxo[] utxos, params TxOutput[] outputs)  
{  
    var inputTxs = utxos  
        .Select(_ => new TxInput { PrevTxHash = _.Tx.Hash, PrevTxIndex = _.Index })  
        .ToArray();  
    var tx = new Transaction  
    {  
        InputTxs = inputTxs,  
        Outputs = outputs,  
    };  
    var sigList = new Signature[tx.InputTxs.Length];  
    for (int i = 0; i < tx.InputTxs.Length; i++)  
    {  
        var utxoEnt = utxos[i];  
        sigList[i] = this.signAlgo.Sign(  
            new[] { Encoding.UTF8.GetBytes(tx.HashContent) },  
            this.FindPrivateKey(utxoEnt.Tx.Outputs[utxoEnt.Index].PublicKey));  
    }  
  
    for (int i = 0; i < tx.InputTxs.Length; i++)  
    {  
        tx.InputTxs[i].Signature = sigList[i];  
    }  
    engine.AttachTx(tx);  
  
    return tx;  
}	
```
<!-- code:ClassicBlockChain/Wallet/BaseWallet.cs -->

```cs
public Transaction SendMoney(Engine engine, Transaction utxo, int index, IWallet receiver, int value, int fee = 0)  
{  
    var total = utxo.Outputs[index].Value;  
    var change = total - value - fee;  
    var mainOutput = new TxOutput { PublicKey = receiver.PublicKey, Value = value };  
    var changeOutput = new TxOutput { PublicKey = this.PublicKey, Value = change };  
    return this.SendMoney(engine, new[] { new Utxo(utxo, index) }, mainOutput, changeOutput);  
}  
```
<!-- code:ClassicBlockChain/Wallet/BaseWallet.cs -->

```cs
public Utxo[] GetUtxos(Engine engine)  
{  
    var txlist = engine.BlockChain.TxToBlockDictionary  
        .Select(_ => engine.BlockChain.GetTx(_.Key))  
        .SelectMany(_ => _.Outputs.Select((txo, i) => new { tx = _, txo, i }))  
        .Where(_ => this.ContainPubKey(_.txo.PublicKey))  
        .Where(_ => !engine.BlockChain.UsedTxDictionary.ContainsKey((_.tx.Hash, _.i)))  
        .Select(_ => new Utxo(_.tx, _.i))  
        .ToArray();  
    return txlist;	
}  
```
<!-- code:ClassicBlockChain/Wallet/BaseWallet.cs -->

```cs
public void SyncBlockHead(Engine engine)  
{  
    var blocks = engine.BlockChain.BlockHeadDictionary.ToDictionary(_ => _.Key, _ => _.Value);  
    var newBlockHash = blocks.Select(_ => _.Key).ToArray();  
    var oldBlockHash = this.blockHeads.Select(_ => _.Key).ToArray();  
    var excepts = oldBlockHash.Except(newBlockHash).ToArray();  
    if (excepts.Length > 0)  
    {  
        Console.WriteLine($"found [{excepts.Length}] diffs in sync block");  
        return;  
    }  
    this.blockHeads = blocks;
}  
```
<!-- code:ClassicBlockChain/Wallet/DeterministicWallet.cs -->

```cs
public bool VerifyTx(Engine engine, Transaction tx)  
{  
    var (hs, flags, txnum, block) = engine.GetMerkleBlock(tx.Hash);  
    var merkleRoot = MerkleTree.GetPartialTreeRootHash(txnum, hs, flags);  
    if (!this.blockHeads.ContainsKey(block.Hash)) return false;  
    var localBlock = this.blockHeads[block.Hash];  
    return merkleRoot == localBlock.MerkleRoot;  
}  
```
<!-- code:ClassicBlockChain/Wallet/BaseWallet.cs -->

## 确定性钱包

```cs
public class SimpleWallet : BaseWallet  
{  
    public SimpleWallet(string name) : base(name) { }  
}  
```
<!-- code:ClassicBlockChain/Wallet/SimpleWallet.cs -->

```cs
public class DeterministicWallet : BaseWallet    
{    
    private List<PublicKey> usedPublicKeys = new List<PublicKey>();    
    private List<PrivateKey> usedPrivateKeys = new List<PrivateKey>();    
    
    public DeterministicWallet(string name) : base(name) { }    
}    
```
<!-- code:ClassicBlockChain/Wallet/DeterministicWallet.cs -->

```cs
protected override void AfterKeyPairGenerated()  
{  
    this.usedPrivateKeys.Add(this.PrivateKey);  
    this.usedPublicKeys.Add(this.PublicKey);  
}  
```
<!-- code:ClassicBlockChain/Wallet/DeterministicWallet.cs -->

```cs
protected override PrivateKey FindPrivateKey(PublicKey publicKey)  
{  
    var idx = this.usedPublicKeys.FindIndex(_ => _ == publicKey);  
    if (idx == -1)  
        throw new KeyNotFoundException("cannot find corresponding public key");  
    return this.usedPrivateKeys[idx];  
}  
```
<!-- code:ClassicBlockChain/Wallet/DeterministicWallet.cs -->

```cs
protected override bool ContainPubKey(PublicKey publicKey)  
{  
    return this.usedPublicKeys.Contains(publicKey);  
}  
```
<!-- code:ClassicBlockChain/Wallet/DeterministicWallet.cs -->

## 对交易签名


## 存储的信息

## 找零与交易费

