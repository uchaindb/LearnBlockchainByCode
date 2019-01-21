# 本章练习

至此，本章的主程序已经全部解析完成，在本节中，通过基本练习，完成程序的入口及基本测试用例，通过调试的方式亲自看一下区块链是如何运行的；通过扩展练习，完成一些本章未涉及到的知识点，进一步提升自己的区块链编程能力。

## 基本练习一 运行程序并观察

本章程序可以通过一个标准的控制台程序进行运行查看，此处为入口代码。

```cs
private static void Main(string[] args)    
{    
    // 该程序设计为按任意键结束程序；
    Console.WriteLine($"Press any key to stop....");    
    // 启动Engine类，并传入矿工的名字，以获取挖矿的奖励；
    var engine = new Engine(myName);    
    // 打印创世区块的信息，方便查看和理解；
    Console.WriteLine($"Genesis Block: {BlockChain.GenesisBlock}");    
    // 监听新区块创建的事件；
    engine.OnNewBlockCreated += Engine_OnNewBlockCreated;    
    // 按任意键即结束程序，否则程序将持续的运行下去；
    Console.ReadKey();    
}    
```
<!-- code:ClassicBlockChain/Program.cs -->

上面提到监听新区块创建的事件，这个是这个事件的处理代码，主要是打印新创建区块信息。

```cs
private static void Engine_OnNewBlockCreated(object sender, Block block)    
{    
    // 将新创建区块的信息打印出来；
    var engine = sender as Engine;    
    var height = engine.BlockChain.Height;    
    Console.WriteLine($"New block created at height[{height:0000}]: {engine.BlockChain.Tail}");    
}    
```
<!-- code:ClassicBlockChain/Program.cs -->

下面是交易的打印代码片段：

```cs
protected override string DebuggerDisplay => $"" +  
    // 打印简短版的哈希值；
    $"{this.Hash.ToShort()}: " +  
    // 把每个接收者及其接收值打印出来，类似于：`(Alice: 50)`或`(Alice: 20, Bob:30)`； 
    $"({string.Join(",", this.OutputOwners?.Select(_ => _.ToString()) ?? new string[] { })}) <-- " +  
    // 根据是否有输入的交易打印不同的信息；
    ((this.InputTransactions != null && this.InputTransactions.Length > 0)  
        // 若有输入的交易，则将哈希值的简短形式其打印出来，类似于：`(95280182E2)`；
        ? $"({string.Join(",", this.InputTransactions.Select(_ => _.ToShort()))})"  
        // 若无输入的交易，则为CoinBase交易，故直接打印CoinBase字符串；
        : $"(Coin Base)");     
```
<!-- code:ClassicBlockChain/Entity/Transaction.cs -->

最终整体打印出的结果类似于：`95280182E2: (Alice: 50) <-- (D005F27FE4)`，
即使用哈希值为`D005F27FE4`的交易作为输入，转账50给Alice，整体交易的哈希值为`95280182E2`，
该哈希值即可一次性的被用作下次交易的输入；

下面是区块的打印代码片段

```cs
protected override string DebuggerDisplay => $"{this.Hash.ToShort()}" + // 打印简短版的哈希值；
    $": (" +  
    $"N: {this.Nonce,8}" + // 打印该区块的随机数值，并以最长8位数右对齐；
    $", " +  
    $"T: {this.Transactions.Length}" + // 打印该区块中的交易数量；
    $")\r\n" +  
    // 在下一行后面，每一行打印一个交易的详情（交易详情输出的样例见前一个代码片段）；
    $"  {string.Join<Transaction>(Environment.NewLine + "  ", this.Transactions ?? new Transaction[] { })}";  
```
<!-- code:ClassicBlockChain/Entity/Block.cs -->

最终整体打印出的结果类似于：`0000A98510: (N: 71081, T: 0)`，即该区块的哈希值为`0000A98510`，
使得该区块有效的随机数为71081，该区块的交易数量为0；

最终，综合以上的所有代码，我们尝试执行程序，执行结果如下：

```
Press any key to stop....
Genesis Block: 0000A98510: (N: 71081, T: 0)

New block created at height\[0002\]: 00009782C2: (N: 4207, T: 1)
  1D3745CF49: (Icer(Miner): 50) &lt;-- (Coin Base)
New block created at height\[0003\]: 00000CE915: (N: 16355, T: 1)
  D1075ABCBC: (Icer(Miner): 50) &lt;-- (Coin Base)
New block created at height\[0004\]: 00004B58D1: (N: 16033, T: 1)
  E44FADFD81: (Icer(Miner): 50) &lt;-- (Coin Base)
New block created at height\[0005\]: 000013D191: (N: 7177, T: 1)
  6271D89EF7: (Icer(Miner): 50) &lt;-- (Coin Base)

……
```

从结果中我们看出：

- 区块在持续性的产生；
- 区块的哈希值符合了我们的难度定义；
- 每一个区块为了使得哈希值符合难度定义，均找到了一个使区块有效的不同的随机数；
- 因为我们没有定义任何额外的交易，所以除了创世区块之外，每个区块都仅有一笔CoinBase交易；
- 区块的高度是递增的；
- CoinBase交易的受益者（接收者）为我们定义的用户；
- 不管是交易还是区块，都不会有重复的哈希值；

## 基本练习二 发送交易

基于前面基本练习一，试着发送转账交易。

这里，先添加一个简化程序的方法（及其重载）。

```cs
// 发送一笔转账交易的重载版本，仅支持一位接收者；
private static void SendMoney(Engine engine, Transaction utxo, string receiver, int value)    
{    
    SendMoney(engine, utxo, new TransactionOutput { Owner = receiver, Value = value });    
}    
// 发送一笔转账交易的完整版本，支持任意数量的接收者；
private static void SendMoney(Engine engine, Transaction utxo, params TransactionOutput[] outputs)    
{    
    engine.AttachTransaction(new Transaction    
    {    
        InputTransactions = new[] { utxo.Hash },    
        OutputOwners = outputs,    
    });    
}    
```
<!-- code:ClassicBlockChain/Program.cs -->

继而在新区块创建成功的事件里面添加以下代码，分别在高度为2或者3的时候执行转账操作。

```cs
private static void Engine_OnNewBlockCreated(object sender, Block block)    
{    
    ...  
  
    // 当区块的高度为2时，即创世区块后一个正常的区块被挖出来后，这时矿工刚好收到了第一笔奖励，
    // 于是在该区块中找到这笔奖励的交易，并使用该交易执行向Alice发起价值50的转账交易；
    if (height == 2)    
    {    
        var utxo = engine.BlockChain.Tail.Transactions.First();    
        SendMoney(engine, utxo, AliceName, 50);    
    }    
    // 当区块的高度为3时，即上一步骤中发送给Alice的转账交易已经顺利完成的时候，
    // 我们在前一区块中找到属于Alice的未使用交易，并使用该交易执行向Bob发起价值50的转账交易；
    else if (height == 3)    
    {    
        var utxo = engine.BlockChain.Tail.Transactions    
            .First(txs => txs.OutputOwners.Any(_ => _.Owner == AliceName));    
        SendMoney(engine, utxo, BobName, 50);    
    }    
}    
```
<!-- code:ClassicBlockChain/Program.cs -->

综合以上代码执行之后，结果如下：

```
Press any key to stop....
Genesis Block: 0000A98510: (N: 71081, T: 0)

New block created at height\[0002\]: 00006B5C84: (N: 3352, T: 1)
  D005F27FE4: (Icer(Miner): 50) &lt;-- (Coin Base)
New block created at height\[0003\]: 0000181DE3: (N: 6573, T: 2)
  3E95C5F823: (Icer(Miner): 50) &lt;-- (Coin Base)
  95280182E2: (Alice: 50) &lt;-- (D005F27FE4)
New block created at height\[0004\]: 00002F3A57: (N: 16850, T: 2)
  C09C1ECB6D: (Icer(Miner): 50) &lt;-- (Coin Base)
  B1A4A1FE64: (Bob: 50) &lt;-- (95280182E2)
New block created at height\[0005\]: 00001861AF: (N: 45519, T: 1)
  3B19760348: (Icer(Miner): 50) &lt;-- (Coin Base)

……
```

从结果中我们看出：

- 前面一个基本练习中的大部分特点依旧在此成立；
- 高度为3的区块中包含了2笔交易，除了CoinBase交易之外，还有我们定义的交易，
  其使用高度为2的区块中哈希值为`D005F27FE4`的CoinBase交易作为输入，转账50给Alice；
- 高度为4的区块中包含了2笔交易，除了CoinBase交易之外，还有我们定义的交易，
  其使用高度为3的区块中哈希值为`95280182E2`的转账交易作为输入，从Alice转账50给Bob；

## 基本练习三 添加无效交易

在这个练习中，需要添加一个无效的交易，并观察其被忽略的过程。

基于前一个基本练习，代码的改动如下：

```cs
// 声明一个静态变量，用来临时存放高度为2时用过的交易；
private static Transaction h2utxo = null;  
private static void Engine_OnNewBlockCreated(object sender, Block block)  
{  
    ...  
    // 在高度为2的代码片段内，将当时用的交易存储到临时变量中去；
    if (height == 2)  
    {  
        ...  
        h2utxo = utxo;  
    }  
    ...  
    // 当高度为4时，即前面Alice已经完成了转账50给Bob之后，我们尝试使用已经使用过的交易再次发起转账请求；
    else if (height == 4)  
    {  
        SendMoney(engine, h2utxo, BobName, 50);  
    }  
}  
```
<!-- code:ClassicBlockChain/Program.cs -->


运行起来的输出和基本练习二的完全一样，说明最后一笔转账请求被认为是无效交易而被忽略，没有被打包进新的区块。

## 扩展练习一 动态难度

本章代码使用了固定的难度系数来生成有效的区块，但实际使用中，可能会由于算力的不断提升，
使得初期定义的较低难度无法匹配当前网络中的算力。

请在本章代码的基础上增加类似于比特币中的动态难度机制。

---

> #### 扩展知识
> 
> 随着比特币的升值，挖矿显得越来越有价值，因此挖矿的设备也不断的在升级。
> 
> **CPU挖矿**：最早的比特币客户端都是允许用户用他们自己的电脑CPU挖矿的，但随着使用GPU挖矿时代的到来，
> CPU挖矿显得非常不经济，因为在同等的电力消耗情况下，GPU的挖矿速度和能力远远超过CPU。
> 因此，比特币的客户端将CPU挖矿这个选项去掉了。
> 
> **GPU挖矿**：比起CPU挖矿来说，GPU就显得更快、更有效率。GPU是图形处理单元，
> 是计算机系统中用来做视频处理和渲染的组件，而视频的处理本身就是一大堆重复的工作，
> 即需要针对屏幕上大量的像素点进行相同的操作。因此GPU就被设计成适合做这种大量重复工作的模块，
> 比特币的挖矿其实就是一个不断重复的哈希运算，正好符合了GPU的特性。
> 
> **FPGA挖矿**：即便GPU挖矿相比CPU挖矿来说已经非常有效率了，但FPGA将比GPU更高的效率带来了。
> FPGA在通常情况下，只需要消耗很少的一点点电力，便能得到大量的哈希计算能力。
> 
> **ASIC挖矿**：ASIC即应用专用集成芯片，是一种为专门的目的设计和制造出来的微型集成电路。
> 最早的ASIC是在2013年被开发出来用作比特币挖矿的。相比前一代技术，
> 它在速度和效率方面都得到了极大的提升。与此同时，它也使得用GPU挖矿再没有任何经济价值了。

---

## 扩展练习二 让创世区块中包含CoinBase交易

在比特币的创世区块中包含了一笔含有`The Times 03/Jan/2009 Chancellor on brink of second bailout for banks`
信息的CoinBase交易，这句话是泰晤士报当天的头版文章标题，引用这句话，既是对该区块产生时间的说明，
也可视为半开玩笑地提醒人们一个独立的货币制度的重要性，同时告诉人们随着比特币的发展，
一场前所未有的世界性货币革命将要发生。该消息是由比特币的创立者中本聪写入创世区块中。

本章代码中的创世区块设计为无交易，请在本章代码的基础上修改为：让创世区块中包含一笔CoinBase交易，
并在该交易中包含一句你想要在区块链中永流传的话语。

## 扩展练习三 让每笔交易拥有交易费

比特币的经济生态设定中非常重要的一点就是交易费用，对于矿工来说，最重要的收入来源是每个区块挖得时的奖励，但该奖励设计为每过四年减半，直到后来非常少，到最终完全没有，当奖励无法激励矿工持续的工作时，尤其是当所有有奖励的比特币区块都挖掘完成时，整个网络的运营就是靠交易费进行维持的。

请在本章程序的基础上创建一些有交易费的交易，并让矿工获得这些交易费。

---

> #### 扩展知识
> 
> 在第124724号区块，一个名为midnightmagic的用户，在其成功挖得的区块中故意少给了自己1聪
> （等于一亿分之一比特币，“聪”为比特币的最小单位）的奖励，
> 并将该区块中的所有交易费用摧毁，故比特币出产总量也因此略微减少了这1聪。

---

