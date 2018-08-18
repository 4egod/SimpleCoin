
namespace XSC.Core
{
    public class Balance
    {
        public decimal Available { get; set; }

        public decimal Locked { get; set; }

        public decimal Total => Available + Locked;
    }
}
