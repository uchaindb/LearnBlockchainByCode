namespace UChainDB.Example.Chain.Network
{
    public interface INodeOperator
    {
        byte[] ExecuteRpcRaw(byte[] request);
    }
}