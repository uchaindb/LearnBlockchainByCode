# 执行环境

## 虚拟机基础

## 操作指令

```cs
public enum OpCode : byte  
{  
    Object,  
    CheckSignature,  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/OpCode.cs -->

## 栈指令类

```cs
public class ScriptToken   
{  
    public const string TOKEN_PREFIX = "OC_";  
    public bool IsOpCode { get => this.OpCode != OpCode.Object; }  
    public OpCode OpCode { get; set; } = OpCode.Object;  
    public string Object { get; set; }  
  
    public ScriptToken(object obj)  
    {  
        if (obj is OpCode opcode)  
        {  
            this.OpCode = opcode;  
        }  
        else if (obj is string str)  
        {  
            if (str.StartsWith(TOKEN_PREFIX))  
            {  
                this.OpCode = (OpCode)Enum.Parse(typeof(OpCode), str.Remove(0, TOKEN_PREFIX.Length));  
            }  
            else  
            {  
                this.Object = str;  
            }  
        }  
        else  
        {  
            this.Object = obj.ToString();  
        }  
    }  
  
    public string GetValue()  
    public static ScriptToken CreateToken(object obj)  
    public override bool Equals(object obj)  
    public override int GetHashCode()  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/ScriptToken.cs -->

## 指令辅助类

```cs
public class UnlockScripts : ReadOnlyCollection<ScriptToken>  
{  
    public UnlockScripts(IList<ScriptToken> list)  
        : base(list)  
    {  
    }  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/UnlockScripts.cs -->

```cs
public class LockScripts : ReadOnlyCollection<ScriptToken>  
{  
    public LockScripts(IList<ScriptToken> list)  
        : base(list)  
    {  
    }  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/LockScripts.cs -->

```cs
public class WholeScripts : ReadOnlyCollection<ScriptToken>  
{  
    public WholeScripts(IList<ScriptToken> list)  
        : base(list)  
    {  
    }  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/WholeScripts.cs -->

```cs
public static WholeScripts operator +(UnlockScripts us, LockScripts ls)  
    => new WholeScripts(us.Concat(ls).ToList());  
```
<!-- code:ClassicBlockChain/SmartContracts/UnlockScripts.cs -->

## 执行

  ```cs
public static T Evaluate<T>(ScriptToken[] tokens, Transaction transaction, ISignAlgorithm algorithm)  
{  
    var ret = Evaluate(tokens, transaction, algorithm);  
    if (typeof(T) == typeof(Boolean))  
    {  
        var s = ret.ToString().ToLower();  
        return (T)(object)(s == "true");  
    }  
  
    return (T)Convert.ChangeType(ret, typeof(T));  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/Evaluator.cs -->

```cs
public static object Evaluate(ScriptToken[] tokens, Transaction transaction, ISignAlgorithm algorithm)  
{  
    var stack = new Stack<ScriptToken>();  
  
    foreach (var token in tokens)  
    {  
        if (!token.IsOpCode)  
        {  
            stack.Push(token);  
            continue;  
        }  
  
        switch (token.OpCode)  
        {  
            case OpCode.CheckSignature:  
                {  
                    if (!stack.CanPop()) return false;  
                    var pubKey = PublicKey.ParseBase58(stack.Pop().GetValue());  
                    if (!stack.CanPop()) return false;  
                    var sig = Signature.ParseBase58(stack.Pop().GetValue());  
                    var ret = algorithm.Verify(new[] { (byte[])transaction.GetLockHash() }, pubKey, sig);  
                    stack.Push(ScriptToken.CreateToken(ret));  
                    break;  
                }  
            default:  
                break;  
        }  
    }  
  
    if (!stack.CanPop()) return false;  
    return stack.Pop().GetValue();  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/Evaluator.cs -->

```cs
public static bool TryExecuteAsync(this WholeScripts scripts, Transaction trans, ISignAlgorithm signAlgorithm = null)  
{  
    signAlgorithm = signAlgorithm ?? new ECDsaSignAlgorithm();  
    var eval = Evaluator.Evaluate<bool>(scripts, trans, signAlgorithm);  
    return eval;  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/TokenScriptExtension.cs -->

```cs
public static bool CanPop(this Stack<ScriptToken> stack)  
{  
    return stack.Count > 0;  
}  
```
<!-- code:ClassicBlockChain/SmartContracts/TokenScriptExtension.cs -->

