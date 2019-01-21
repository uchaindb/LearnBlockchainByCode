# 执行环境

## 虚拟机基础

## 操作指令

  ---------------------------------------------
  1.  **public** **enum** OpCode : **byte**  
  
  2.  {  
  
  3.      Object,  
  
  4.      CheckSignature,  
  
  5.  }  
  
  ---------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\OpCode.cs

其中，

第3行，

## 栈指令类

  ----------------------------------------------------------------------------------------------------------------------
  1.  **public** **class** ScriptToken   
  
  2.  {  
  
  3.      **public** **const** **string** TOKEN\_PREFIX = "OC\_";  
  
  4.      **public** **bool** IsOpCode { **get** =&gt; **this**.OpCode != OpCode.Object; }  
  
  5.      **public** OpCode OpCode { **get**; **set**; } = OpCode.Object;  
  
  6.      **public** **string** Object { **get**; **set**; }  
  
  7.    
  
  8.      **public** ScriptToken(**object** obj)  
  
  9.      {  
  
  10.         **if** (obj **is** OpCode opcode)  
  
  11.         {  
  
  12.             **this**.OpCode = opcode;  
  
  13.         }  
  
  14.         **else** **if** (obj **is** **string** str)  
  
  15.         {  
  
  16.             **if** (str.StartsWith(TOKEN\_PREFIX))  
  
  17.             {  
  
  18.                 **this**.OpCode = (OpCode)Enum.Parse(**typeof**(OpCode), str.Remove(0, TOKEN\_PREFIX.Length));  
  
  19.             }  
  
  20.             **else**  
  
  21.             {  
  
  22.                 **this**.Object = str;  
  
  23.             }  
  
  24.         }  
  
  25.         **else**  
  
  26.         {  
  
  27.             **this**.Object = obj.ToString();  
  
  28.         }  
  
  29.     }  
  
  30.   
  
  31.     **public** **string** GetValue()  
  
  32.     **public** **static** ScriptToken CreateToken(**object** obj)  
  
  33.     **public** **override** **bool** Equals(**object** obj)  
  
  34.     **public** **override** **int** GetHashCode()  
  
  35. }  
  
  ----------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\ScriptToken.cs

其中，

第3行，

## 指令辅助类

  ----------------------------------------------------------------------------------
  1.  **public** **class** UnlockScripts : ReadOnlyCollection&lt;ScriptToken&gt;  
  
  2.  {  
  
  3.      **public** UnlockScripts(IList&lt;ScriptToken&gt; list)  
  
  4.          : **base**(list)  
  
  5.      {  
  
  6.      }  
  
  7.  }  
  
  ----------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\UnlockScripts.cs

其中，

第3行，

  --------------------------------------------------------------------------------
  1.  **public** **class** LockScripts : ReadOnlyCollection&lt;ScriptToken&gt;  
  
  2.  {  
  
  3.      **public** LockScripts(IList&lt;ScriptToken&gt; list)  
  
  4.          : **base**(list)  
  
  5.      {  
  
  6.      }  
  
  7.  }  
  
  --------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\LockScripts.cs

其中，

第3行，

  ---------------------------------------------------------------------------------
  1.  **public** **class** WholeScripts : ReadOnlyCollection&lt;ScriptToken&gt;  
  
  2.  {  
  
  3.      **public** WholeScripts(IList&lt;ScriptToken&gt; list)  
  
  4.          : **base**(list)  
  
  5.      {  
  
  6.      }  
  
  7.  }  
  
  ---------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\WholeScripts.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------
  1.  **public** **static** WholeScripts **operator** +(UnlockScripts us, LockScripts ls)  
  
  2.      =&gt; **new** WholeScripts(us.Concat(ls).ToList());  
  
  -------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\UnlockScripts.cs

其中，

第3行，

## 执行

  ----------------------------------------------------------------------------------------------------------------------------
  1.  **public** **static** T Evaluate&lt;T&gt;(ScriptToken\[\] tokens, Transaction transaction, ISignAlgorithm algorithm)  
  
  2.  {  
  
  3.      var ret = Evaluate(tokens, transaction, algorithm);  
  
  4.      **if** (**typeof**(T) == **typeof**(Boolean))  
  
  5.      {  
  
  6.          var s = ret.ToString().ToLower();  
  
  7.          **return** (T)(**object**)(s == "true");  
  
  8.      }  
  
  9.    
  
  10.     **return** (T)Convert.ChangeType(ret, **typeof**(T));  
  
  11. }  
  
  ----------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\Evaluator.cs

其中，

第3行，

  -----------------------------------------------------------------------------------------------------------------------------
  1.  **public** **static** **object** Evaluate(ScriptToken\[\] tokens, Transaction transaction, ISignAlgorithm algorithm)  
  
  2.  {  
  
  3.      var stack = **new** Stack&lt;ScriptToken&gt;();  
  
  4.    
  
  5.      **foreach** (var token **in** tokens)  
  
  6.      {  
  
  7.          **if** (!token.IsOpCode)  
  
  8.          {  
  
  9.              stack.Push(token);  
  
  10.             **continue**;  
  
  11.         }  
  
  12.   
  
  13.         **switch** (token.OpCode)  
  
  14.         {  
  
  15.             **case** OpCode.CheckSignature:  
  
  16.                 {  
  
  17.                     **if** (!stack.CanPop()) **return** **false**;  
  
  18.                     var pubKey = PublicKey.ParseBase58(stack.Pop().GetValue());  
  
  19.                     **if** (!stack.CanPop()) **return** **false**;  
  
  20.                     var sig = Signature.ParseBase58(stack.Pop().GetValue());  
  
  21.                     var ret = algorithm.Verify(**new**\[\] { (**byte**\[\])transaction.GetLockHash() }, pubKey, sig);  
  
  22.                     stack.Push(ScriptToken.CreateToken(ret));  
  
  23.                     **break**;  
  
  24.                 }  
  
  25.             **default**:  
  
  26.                 **break**;  
  
  27.         }  
  
  28.     }  
  
  29.   
  
  30.     **if** (!stack.CanPop()) **return** **false**;  
  
  31.     **return** stack.Pop().GetValue();  
  
  32. }  
  
  -----------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\Evaluator.cs

其中，

第3行，

  -------------------------------------------------------------------------------------------------------------------------------------------------
  1.  **public** **static** **bool** TryExecuteAsync(**this** WholeScripts scripts, Transaction trans, ISignAlgorithm signAlgorithm = **null**)  
  
  2.  {  
  
  3.      signAlgorithm = signAlgorithm ?? **new** ECDsaSignAlgorithm();  
  
  4.      var eval = Evaluator.Evaluate&lt;**bool**&gt;(scripts, trans, signAlgorithm);  
  
  5.      **return** eval;  
  
  6.  }  
  
  -------------------------------------------------------------------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\TokenScriptExtension.cs

其中，

第3行，

  --------------------------------------------------------------------------------------
  1.  **public** **static** **bool** CanPop(**this** Stack&lt;ScriptToken&gt; stack)  
  
  2.  {  
  
  3.      **return** stack.Count &gt; 0;  
  
  4.  }  
  
  --------------------------------------------------------------------------------------

参考代码：ClassicBlockChain\\SmartContracts\\TokenScriptExtension.cs

其中，

第3行，


