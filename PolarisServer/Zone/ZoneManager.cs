using System.Collections.Generic;

namespace PolarisServer.Zone
{
    public class ZoneManager
    {
        public static ZoneManager Instance { get; } = new ZoneManager();

        internal Dictionary<string, int> playerCounter = new Dictionary<string, int>();
        internal Dictionary<string, List<Map>> instances = new Dictionary<string, List<Map>>();

        private ZoneManager()
        {
            // Create lobby instance
            List<Map> lobbyMaps = new List<Map>()
            {
                new Map("lobby", 106, 0, Map.MapType.Lobby, Map.MapFlags.None),
                new Map("casino", 104, 0, Map.MapType.Casino, Map.MapFlags.MultiPartyArea | Map.MapFlags.Unknown1)
            };

            instances.Add("lobby", lobbyMaps);
        }

        public Map MapFromInstance(string mapName, string instanceName)
        {
            if (!instances.ContainsKey(instanceName))
                throw new KeyNotFoundException();

            foreach (Map m in instances[instanceName])
                if (m.Name == mapName)
                    return m;

            return null;
        }

        public void NewInstance(string instanceName, Map initialMap)
        {
            if (instances.ContainsKey(instanceName))
                return;

            initialMap.InstanceName = instanceName;
            instances.Add(instanceName, new List<Map>() { initialMap });
            playerCounter.Add(instanceName, 0);
        }

        public bool InstanceExists(string instanceName)
        {
            return instances.ContainsKey(instanceName);
        }

        public void AddMapToInstance(string instance, Map m)
        {
            List<Map> maps = instances[instance];
            if (!maps.Contains(m))
                maps.Add(m);
        }
    }
}
