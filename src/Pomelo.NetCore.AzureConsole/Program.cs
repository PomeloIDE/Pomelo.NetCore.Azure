using Pomelo.NetCore.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pomelo.NetCore.AzureConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(AsyncMain).GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            VMManagement mgr = new VMManagement("6fef287b-09fc-4d87-8dc1-bb154aa68b7a", "821e2823-d712-46fb-885c-46cc60d8ee66", 
                "60b8650e-26f1-4782-b740-955f551d0776", "7H2sjm1GG7qf3+cSZ3sGl7VivwstyhWeTkEkS+ENIOw=");
            //var result = await mgr.CreateVirtualMachineAsync("0031", "pomelo", "Pomelo123!@#");
            //var result = await mgr.DeleteVirtualMachineAsync("2322");
            //var result = await mgr.StartVirtualMachineAsync("0031");
            //var result = await mgr.RestartVirtualMachineAsync("0031");
            //var result = await mgr.StopVirtualMachineAsync("0031");
            //var result = await mgr.DeallocateVirtualMachineAsync("0031");
            //var result = await mgr.UpdateUsernamePassword("0031", "pomelo", "Pomelo123!@#");
            //var result = await mgr.GetPublicIPAddressAsync("0031");
            //var result = await mgr.WaitForVirtualMachineStartedAsync("0031", 30000);

            //var result = await mgr.CreateVirtualMachineAsync("1435", "pomelo", "Pomelo123!@#");
            //var result2 = await mgr.CreateVirtualMachineAsync("1434", "pomelo", "Pomelo123!@#");
            //var result2 = await mgr.GetPublicIPAddressAsync("1416");
            //var result3 = await mgr.WaitForVirtualMachineStartedAsync("1435", 300000);

            var result = await mgr.DeleteVirtualMachineAsync("1435");
            var result2 = await mgr.DeleteVirtualMachineAsync("1434");

            Console.ReadKey();
        }
    }
}
