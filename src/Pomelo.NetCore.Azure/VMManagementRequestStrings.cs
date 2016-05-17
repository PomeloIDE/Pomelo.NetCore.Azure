using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pomelo.NetCore.Azure
{
    internal class VMManagementRequestStrings
    {
        // vm-name, admin-name, admin-passwd
        public const string CREATE_VM =
            @"{
    ""id"": ""/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a/resourceGroups/Pomelo/providers/Microsoft.Compute/virtualMachines/<vmname>"",
    ""name"": ""<vmname>"",
    ""type"": ""Microsoft.Compute/virtualMachines"",
    ""location"": ""japanwest"",
    ""properties"": {
        ""hardwareProfile"": {
            ""vmSize"": ""Basic_A1""
        },
        ""storageProfile"": {
            ""imageReference"": {
                ""publisher"": ""Canonical"",
                ""offer"": ""UbuntuServer"",
                ""sku"": ""14.04.4-LTS"",
                ""version"": ""latest""
            },
            ""osDisk"": {
                ""name"": ""<vmname>"",
                ""vhd"": {
                    ""uri"": ""http://pomeloide.blob.core.windows.net/vhds/<vmname>.vhd""
                },
                ""caching"": ""ReadWrite"",
                ""createOption"": ""FromImage""
            }
        },
        ""osProfile"": {
            ""computerName"": ""<vmname>"",
            ""adminUsername"": ""<adminname>"",
            ""adminPassword"": ""<adminpasswd>"",
            ""linuxConfiguration"": {
                ""disablePasswordAuthentication"": false
            },
            ""secrets"": []
        },
        ""networkProfile"": {
            ""networkInterfaces"": [
                {
                    ""id"": ""/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a/resourceGroups/pomelo/providers/Microsoft.Network/networkInterfaces/<vmname>""
                }
            ]
        }
    }
}";

        public const string CREATE_PUBLIC_IP = 
            @"{
   ""location"": ""japanwest"",
   ""properties"": {
      ""publicIPAllocationMethod"": ""Static"",
      ""idleTimeoutInMinutes"": 30
   }
}";


        public const string CREATE_NIC = 
            @"{
   ""location"":""japanwest"",
   ""properties"":{  
      ""networkSecurityGroup"":{  
         ""id"":""/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a/resourceGroups/pomelo/providers/Microsoft.Network/networkSecurityGroups/Pomelo""
      },
      ""ipConfigurations"":[
         {
            ""name"":""<vmname>"",
            ""properties"":{  
               ""subnet"":{
                  ""id"":""/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a/resourceGroups/pomelo/providers/Microsoft.Network/virtualNetworks/Pomelo/subnets/default""
               },
               ""privateIPAllocationMethod"":""Dynamic"",
               ""publicIPAddress"":{
                  ""id"":""/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a/resourceGroups/pomelo/providers/Microsoft.Network/publicIPAddresses/<vmname>""
               }
            }
         }
      ]
   }
}";
    }
}
