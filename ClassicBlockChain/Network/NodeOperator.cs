using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UChainDB.Example.Chain.Core;
using UChainDB.Example.Chain.Network.RpcCommands;

namespace UChainDB.Example.Chain.Network
{
    public class NodeOperator : INodeOperator
    {
        //private const int MaxBlockRetrivalNumber = 100;
        //private const int MaxQueryDataPageSize = 100;
        //private Engine engine;
        //private Node node;

        //private class QuerySuspend
        //{
        //    public IEnumerator<(string value, int histLen)> Data { get; set; }
        //    public string[] Headers { get; set; }
        //    public string PrimaryKeyName { get; set; }
        //}

        //public void Init(Node node)
        //{
        //    this.node = node;
        //    this.engine = node.Engine;
        //}

        //public byte[] ExecuteRpcRaw(byte[] raw)
        //{
        //    var data = Encoding.UTF8.GetString(raw);
        //    var token = JToken.Parse(data);

        //    var requests = new List<JsonRpcRequest>();
        //    var responses = new List<JsonRpcResponse>();
        //    if (token is JArray)
        //    {
        //        requests.AddRange(token.ToObject<List<JsonRpcRequest>>());
        //    }
        //    else if (token is JObject)
        //    {
        //        requests.Add(token.ToObject<JsonRpcRequest>());
        //    }

        //    foreach (var request in requests)
        //    {
        //        responses.Add(this.ProcessRequest(request));
        //    }

        //    string ret = null;
        //    if (responses.Count == 1)
        //    {
        //        ret = JsonConvert.SerializeObject(responses[0]);
        //    }
        //    else
        //    {
        //        ret = JsonConvert.SerializeObject(responses);
        //    }

        //    return Encoding.UTF8.GetBytes(ret);
        //}

        //private JsonRpcResponse ProcessRequest(JsonRpcRequest request)
        //{
        //    try
        //    {
        //        switch (request.Method)
        //        {
        //            case Commands.Connect:
        //                return new JsonRpcResponse(request) { Result = this.Connect() };

        //            case Commands.Status:
        //                return new JsonRpcResponse(request) { Result = this.Status() };

        //            case Commands.Blocks:
        //                return new JsonRpcResponse(request) { Result = this.Blocks(new BlocksRpcRequest(request.Parameters)) };

        //            case Commands.CreateSchemaTransaction:
        //                return new JsonRpcResponse(request) { Result = this.CreateSchemaTransaction(new CreateSchemaTransactionRpcRequest(request.Parameters)) };

        //            case Commands.CreateDataTransaction:
        //                return new JsonRpcResponse(request) { Result = this.CreateDataTransaction(new CreateDataTransactionRpcRequest(request.Parameters)) };

        //            case Commands.CreateLockTransaction:
        //                return new JsonRpcResponse(request) { Result = this.CreateLockTransaction(new CreateLockTransactionRpcRequest(request.Parameters)) };

        //            case Commands.ListTables:
        //                return new JsonRpcResponse(request) { Result = this.ListTables() };

        //            case Commands.QueryData:
        //                return new JsonRpcResponse(request) { Result = this.QueryData(new QueryDataRpcRequest(request.Parameters)) };

        //            case Commands.QueryChain:
        //                return new JsonRpcResponse(request) { Result = this.QueryChain(new QueryChainRpcRequest(request.Parameters)) };

        //            case Commands.QueryCell:
        //                return new JsonRpcResponse(request) { Result = this.QueryCell(new QueryCellRpcRequest(request.Parameters)) };

        //            default:
        //                return new JsonRpcResponse(request) { Error = JsonRpcError.MethodNotFound };
        //        }
        //    }
        //    catch (ArgumentException aex)
        //    {
        //        var error = JsonRpcError.InvalidParameters;
        //        error.Data = aex.Message;
        //        return new JsonRpcResponse(request) { Error = error };
        //    }
        //}
        public byte[] ExecuteRpcRaw(byte[] request)
        {
            throw new NotImplementedException();
        }
    }

}
