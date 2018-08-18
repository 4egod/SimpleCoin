using System.ServiceModel;

namespace XSC.P2P
{
    using Core;
    
    [ServiceContract]
    public interface IPeerService
    {
        [OperationContract(Name = "ProcessTransaction")]
        void Process(Transaction transaction);

        [OperationContract(Name = "ProcessBlock")]
        void Process(Block block);

        [OperationContract]
        ulong GetHeight();

        [OperationContract]
        Block GetBlock(ulong height);
    }
}
