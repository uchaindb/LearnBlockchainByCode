读书提示：本书[发布在此](https://book.uchaindb.com/)，具有更好的阅读体验。

# 基础结构

鉴于是从零开始创建一个基本的区块链程序，我们本节会涉及到几个极其基础的结构和类。

## 哈希计算

我们设计了以下Hash类进行计算，将一些常用操作进行封装，方便后续代码操作。

```cs
public class Hash  
{  
    // 作为byte数组存在的哈希值；
    private readonly byte[] data;  

    // 接收几种不同的常见参数的构造器；
    public Hash(byte[] origin)  
    public Hash(IEnumerable<byte[]> origins)  
    public Hash(string origin)  

    // 可隐式转换为UInt256或者byte数组；
    public static implicit operator UInt256(Hash value)  
    public static implicit operator byte[] (Hash value)  

    // 做哈希计算的核心代码，主要是调用.Net框架中的SHA256类完成；
    private static byte[] SHA256Hash(byte[] bytes)  
    private static byte[] SHA256Hash(IEnumerable<byte[]> bytesArray)  
    {  
        if (bytesArray == null)  
        {  
            throw new ArgumentNullException(nameof(bytesArray));  
        }  
  
        using (var hash = SHA256.Create())  
        {  
            var e = bytesArray.GetEnumerator();  
            while (e.MoveNext())  
            {  
                var arr = e.Current;  
                hash.TransformBlock(arr, 0, arr.Length, null, 0);  
            }  
            hash.TransformFinalBlock(new byte[] { }, 0, 0);  
            return hash.Hash;  
        }  
    }
}  
```
<!-- code:ClassicBlockChain/Entity/Hash.cs;branch:1_2_basic_blockchain -->

## 256字节数字类型

在我们编写的程序里面会使用SHA256哈希计算方法，这也是比特币中主要的哈希计算方法。
SHA256是安全散列算法SHA2系列算法之一，其摘要长度为256比特，即32个字节。

编程语言中通常情况会内置8位、16位、32位和64位的类，但对于256位的类，系统并未提供，
虽然在较新版本的.NET框架中提供了BigInteger类也可以存储这样的大数，
但其主要目的是存储用于计算的大数，对于我们的需求来看，会缺少转换、比较和限定的功能，
因此为了后续在程序中方便的使用由SHA256输出的结果，我们设计了以下的UInt256类。

```cs
public class UInt256 : IComparable<UInt256>    
{    
    public static readonly UInt256 Zero; // 定义了默认值0；
    private readonly byte[] data; // 内部数据存储为byte型数组；

    // 简单的初始化示意代码，数据设计为只读，使得该类为线程安全，并适合多线程程序的环境；
    public UInt256(byte[] d)    
    {    
        this.data = d;    
    }    

    // 重载相等、不等操作符，方便比较两个UInt256类型是否一致；
    public static bool operator ==(UInt256 left, UInt256 right)    
    public static bool operator !=(UInt256 left, UInt256 right)    
    public override bool Equals(object obj)    
    public override int GetHashCode()    

    // 鉴于该类型本质为byte型数组，故支持隐式转换；
    public static implicit operator byte[] (UInt256 value)    

    // 将该类型与字符串之间进行转换，字符串的表现形式现在为16进制字符串形式，
    // 但在未来的Base58章节后，字符串的表现形式会发生变化；
    public static UInt256 Parse(string str)    
    public override string ToString()    
    public string ToHex()    

    // 实现`IComparable<UInt256>`接口，使得其支持排序；
    public int CompareTo(UInt256 other)    
}    
```
<!-- code:ClassicBlockChain/Entity/UInt256.cs;branch:1_2_basic_blockchain -->

## 带哈希值的基类

在系统中有很多不同的类都需要含有哈希值这个属性，比如本章的区块类和交易类，所以这一节中先定义带有哈希值的基类。

首先定义一个拥有哈希值的类的接口：

```cs
public interface IHashObject  
{  
    UInt256 Hash { get; }  
}  
```
<!-- code:ClassicBlockChain/Entity/IHashObject.cs;branch:1_2_basic_blockchain -->

该接口比较简单，确立了拥有Hash这个属性的类的接口。

以此接口，实现一个通用型的抽象基类，未来该类会主要被作为区块和交易等基于哈希值的类的基础。

```cs
public abstract class HashBase : IHashObject  
{  
    private UInt256 hash; // 哈希值的内部存储；
    private bool dirtyHash = true; // 标记哈希值是否需要重新生成，默认为需要；
    // 哈希值的对外的属性包装；
    public UInt256 Hash  
    {  
        // 如果哈希值需要重新生成，则重新生成并返回，否则直接返回先前的值；
        get  
        {  
            if (this.dirtyHash)  
            {  
                this.hash = new Hash(this.HashContent);  
                this.dirtyHash = false;  
            }  
  
            return this.hash;  
        }  
    }  

    // 用于生成哈希值的信息全文，此处使用字符串主要是为了方便学习，未来会变成字节数组；
    protected internal abstract string HashContent { get; }  
    // 可由子类执行该方法，以告知该抽象基类已有属性发生变化，
    // 故需要重新生成哈希值的变量被置为需要，下次访问哈希值的时候，哈希值会被重新生成，以反映最新的属性情况；
    protected virtual void OnPropertyChanged(string propertyName = null)  
    {  
        this.dirtyHash = true;  
    }  

    // 方便子类编写属性的方法，内部逻辑为判断是否值发生变化，
    // 只有变化时才置新值，并将需要重新生成哈希值变量置为需要；
    protected void SetPropertyField<T>(ref T field, T newValue, [CallerMemberName]string propertyName = "")  
    {  
        if (EqualityComparer<T>.Default.Equals(field, newValue)) return;  
        field = newValue;  
        this.OnPropertyChanged(propertyName);  
    }  
  
    // 为方便比较两个基于该抽象基类的重载，具体比较即为比较哈希值这个属性；
    public static bool operator ==(HashBase a, HashBase b)  
    public static bool operator !=(HashBase a, HashBase b)  
    public override bool Equals(object obj)  
    public override int GetHashCode()  
}  
```
<!-- code:ClassicBlockChain/Entity/HashBase.cs;branch:1_2_basic_blockchain -->

基于该抽象基类的子类中的需要被考虑进哈希生成的属性，需要像以下方式编写：

```cs
public byte Version  
{  
    // 指向本类的内部字段；
    get => this.version;  
    // 调用基类的设置字段方法，可以简便的完成功能及避免重复代码；
    set => this.SetPropertyField(ref this.version, value);  
}  
```
<!-- code:ClassicBlockChain/Entity/Tx.cs;branch:1_2_basic_blockchain;line:14-18 -->

