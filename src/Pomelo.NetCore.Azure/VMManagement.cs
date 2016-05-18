using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pomelo.NetCore.Azure
{
    public class VMManagement
    {
        Authenticator _authenticator = new Authenticator();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="clientId"></param>
        /// <param name="appPassword"></param>
        public VMManagement(string tenantId, string clientId, string appPassword)
        {
            _authenticator.TenantId = tenantId;
            _authenticator.ClientId = clientId;
            _authenticator.AppPassword = appPassword;
        }

        private async Task<bool> CreatePublicIPAddress(string vmname)
        {
            var requestByteAry = System.Text.Encoding.UTF8.GetBytes(VMManagementRequestStrings.CREATE_PUBLIC_IP);
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Network/publicIPAddresses/" + vmname + "?api-version=2016-03-30");

            var result = await _authenticator.Request("PUT", requestUri, "application/json", requestByteAry);
            return result.StatusCode == HttpStatusCode.Created || result.StatusCode == HttpStatusCode.OK;
        }

        private async Task<bool> DeletePublicIPAddress(string vmname)
        {
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Network/publicIPAddresses/" + vmname + "?api-version=2016-03-30");

            var result = await _authenticator.Request("DELETE", requestUri, string.Empty, new byte[0]);
            return result.StatusCode == HttpStatusCode.Accepted || result.StatusCode == HttpStatusCode.NoContent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmname"></param>
        /// <returns></returns>
        public async Task<Tuple<bool, IPAddress>> GetPublicIPAddressAsync(string vmname)
        {
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Network/publicIPAddresses/" + vmname + "?api-version=2016-03-30");

            var result = await _authenticator.Request("GET", requestUri, string.Empty, new byte[0]);

            if (result.StatusCode != HttpStatusCode.OK)
                return new Tuple<bool, IPAddress>(false, new IPAddress(0));

            try
            {
                JToken token = JObject.Parse(result.Content);
                var properties = token.SelectToken("properties");
                if (properties == null)
                    return new Tuple<bool, IPAddress>(false, new IPAddress(0));
                var ipStr = (string)properties.SelectToken("ipAddress");
                if (ipStr == null)
                    return new Tuple<bool, IPAddress>(false, new IPAddress(0));
                var ipAddr = IPAddress.Parse(ipStr);
                return new Tuple<bool, IPAddress>(true, ipAddr);
            }
            catch (JsonException)
            {
                return new Tuple<bool, IPAddress>(false, new IPAddress(0));
            }
        }

        private async Task<bool> CreateNIC(string vmname)
        {
            var request = VMManagementRequestStrings.CREATE_NIC.Replace("<vmname>", vmname);
            var requestByteAry = System.Text.Encoding.UTF8.GetBytes(request);
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Network/networkInterfaces/" + vmname + "?api-version=2016-03-30");

            var result = await _authenticator.Request("PUT", requestUri, "application/json", requestByteAry);
            return result.StatusCode == HttpStatusCode.Created || result.StatusCode == HttpStatusCode.OK;
        }

        private async Task<bool> DeleteNIC(string vmname)
        {
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Network/networkInterfaces/" + vmname + "?api-version=2016-03-30");

            var result = await _authenticator.Request("DELETE", requestUri, string.Empty, new byte[0]);
            return result.StatusCode == HttpStatusCode.Accepted || result.StatusCode == HttpStatusCode.NoContent;
        }


        private async Task<bool> CreateVM(string vmname, string adminname, string adminpasswd)
        {
            var request = VMManagementRequestStrings.CREATE_VM.Replace("<vmname>", vmname).Replace("<adminname>", adminname).Replace("<adminpasswd>", adminpasswd);
            var requestByteAry = System.Text.Encoding.UTF8.GetBytes(request);
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Compute/virtualMachines/" + vmname + "?api-version=2015-05-01-preview");

            var result = await _authenticator.Request("PUT", requestUri, "application/json", requestByteAry);

            return result.StatusCode == HttpStatusCode.Created || result.StatusCode == HttpStatusCode.OK;
        }

        private async Task<bool> DeleteVM(string vmname)
        {
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Compute/virtualMachines/" + vmname + "?api-version=2015-05-01-preview");
            var result = await _authenticator.Request("DELETE", requestUri, string.Empty, new byte[0]);

            return result.StatusCode == HttpStatusCode.Accepted || result.StatusCode == HttpStatusCode.OK || result.StatusCode == HttpStatusCode.NotFound;
        }

        /// <summary>
        /// Retry 3 times if fails
        /// </summary>
        /// <param name="vmname"></param>
        /// <param name="adminname"></param>
        /// <param name="adminpasswd"></param>
        /// <returns></returns>
        public async Task<bool> CreateVirtualMachineAsync(string vmname, string adminname, string adminpasswd)
        {
            var ipaddrsucc = await CreatePublicIPAddress(vmname);
            if (!ipaddrsucc)
                return false;

            // wait for provision
            await Task.Delay(5000);

            var nicsucc = await CreateNIC(vmname);
            if (!nicsucc)
                return false;

            await Task.Delay(5000);

            var vmsucc = await CreateVM(vmname, adminname, adminpasswd);
            if (!vmsucc)
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmname"></param>
        /// <returns></returns>
        public async Task<bool> DeleteVirtualMachineAsync(string vmname)
        {
            var vmsucc = await DeleteVM(vmname);
            if (!vmsucc)
                return false;

            var nicsucc = await DeleteNIC(vmname);
            if (!nicsucc)
                return false;

            var ipaddrsucc = await DeletePublicIPAddress(vmname);
            if (!ipaddrsucc)
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmname"></param>
        /// <returns></returns>
        public async Task<bool> StartVirtualMachineAsync(string vmname)
        {
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Compute/virtualMachines/" + vmname + "/start?api-version=2015-05-01-preview");

            var result = await _authenticator.Request("POST", requestUri, string.Empty, new byte[0]);
            return result.StatusCode == HttpStatusCode.OK || result.StatusCode == HttpStatusCode.Accepted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmname"></param>
        /// <returns></returns>
        public async Task<bool> RestartVirtualMachineAsync(string vmname)
        {
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Compute/virtualMachines/" + vmname + "/restart?api-version=2015-05-01-preview");

            var result = await _authenticator.Request("POST", requestUri, string.Empty, new byte[0]);
            return result.StatusCode == HttpStatusCode.OK || result.StatusCode == HttpStatusCode.Accepted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmname"></param>
        /// <returns></returns>
        public async Task<bool> StopVirtualMachineAsync(string vmname)
        {
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Compute/virtualMachines/" + vmname + "/powerOff?api-version=2015-05-01-preview");

            var result = await _authenticator.Request("POST", requestUri, string.Empty, new byte[0]);
            return result.StatusCode == HttpStatusCode.OK || result.StatusCode == HttpStatusCode.Accepted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmname"></param>
        /// <returns></returns>
        public async Task<bool> DeallocateVirtualMachineAsync(string vmname)
        {
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Compute/virtualMachines/" + vmname + "/deallocate?api-version=2015-05-01-preview");

            var result = await _authenticator.Request("POST", requestUri, string.Empty, new byte[0]);
            return result.StatusCode == HttpStatusCode.OK || result.StatusCode == HttpStatusCode.Accepted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmname"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> UpdateUsernamePassword(string vmname, string username, string password)
        {
            var request = VMManagementRequestStrings.UPDATE_PASSWORD_EXTENSION.
                Replace("<vmname>", vmname).Replace("<username>", username).Replace("<password>", password);
            var requestByteAry = System.Text.Encoding.UTF8.GetBytes(request);

            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Compute/virtualMachines/" + vmname + "/extensions/enablevmaccess?api-version=2015-05-01-preview");

            var result = await _authenticator.Request("PUT", requestUri, "application/json", requestByteAry);
            return result.StatusCode == HttpStatusCode.Accepted || result.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// NB: if wait for ever while there's a network outage, this func may hang.
        /// </summary>
        /// <param name="vmname"></param>
        /// <param name="timeoutMillisecond">0 to wait forever</param>
        /// <returns></returns>
        public async Task<bool> WaitForVirtualMachineStartedAsync(string vmname, int timeoutMillisecond)
        {
            var requestUri = new Uri("https://management.azure.com/subscriptions/6fef287b-09fc-4d87-8dc1-bb154aa68b7a"
                + "/resourceGroups/pomelo/providers/Microsoft.Compute/virtualMachines/" + vmname + "/InstanceView?api-version=2015-05-01-preview");

            DateTime end = timeoutMillisecond == 0 ? DateTime.Now.AddMilliseconds(timeoutMillisecond) : DateTime.MaxValue;

            while (DateTime.Now < end)
            {
                var result = await _authenticator.Request("GET", requestUri, string.Empty, new byte[0]);

                if (result.StatusCode != HttpStatusCode.OK)
                    goto nextLoop;

                try
                {
                    JToken token = JToken.Parse(result.Content);
                    var statuses = token.SelectToken("statuses") as JArray;
                    if (statuses == null || statuses.Count < 2)
                        goto nextLoop;

                    var powerState = (string)statuses[1].SelectToken("code");
                    if (powerState == null || powerState != "PowerState/running")
                        goto nextLoop;
                    else
                        return true;
                }
                catch (JsonException)
                {
                    goto nextLoop;
                }

                nextLoop:
                await Task.Delay(1000);
            }
            // Timeout
            return false;
        }

    } // End of class

} // End of namespace