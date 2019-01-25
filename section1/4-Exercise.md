读书提示：本书[发布在此](https://book.uchaindb.com/)，具有更好的阅读体验。

# 本章练习

## 搭建练习环境


```cs
public class ControlService  
{  
    private readonly IHubContext<ControlHub> hubcontext;  
    public ControlService(IHubContext<ControlHub> hubcontext)  
    {  
        this.hubcontext = hubcontext;  
    }  
  
    public Task Start()  
    public Task Stop()  
    public Task AddNode()  
}  
```
<!-- code:DebugConsole/Services/ControlService.cs -->

## 基本练习一 运行程序

## 扩展练习一 同步时验证区块内容

