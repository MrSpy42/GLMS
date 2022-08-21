using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Nat;
using Serilog;

namespace GLMS.Networking
{
    public class PortForwarder
    {
        readonly SemaphoreSlim locker = new SemaphoreSlim(1, 1);
        public PortForwarder()
        {
            // Raised whenever a device is discovered.
            NatUtility.DeviceFound += DeviceFound;

            NatUtility.StartDiscovery();
            Log.Debug("NAT Discovery started.");
        }

        private async void DeviceFound(object sender, DeviceEventArgs args)
        {
            await locker.WaitAsync();
            try
            {
                INatDevice device = args.Device;
                Log.Debug("Device found: {0}", device.NatProtocol);
                Log.Debug("Type: {0}", device.GetType().Name);
                Log.Debug("IP: {0}", await device.GetExternalIPAsync());
                var mapping = new Mapping(Protocol.Tcp, 6777, 6777);
                device.CreatePortMap(mapping);
                Log.Debug("Successfully created mapping: protocol={0}, public={1}, private={2}", mapping.Protocol, mapping.PublicPort,
                                  mapping.PrivatePort);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex,"Unable to port forward, try restarting.");
                SettingsManager.GetRuntimeSettings().isExiting = true;
                return;
            }
        }
    }
}
