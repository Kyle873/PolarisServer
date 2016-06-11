using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Party
{
    class PartyManager
    {
        private static readonly PartyManager instance = new PartyManager();

        private Dictionary<Party, string> parties = new Dictionary<Party, string>(); // Key: Party, Value: Name? (for now)

        public static PartyManager Instance
        {
            get
            {
                return instance;
            }
        }

        private PartyManager()
        {
            Logger.WriteInternal("[PTY] PartyManager initialized.");
        }

        public Party GetCurrentPartyForClient(Client c)
        {
            foreach (Party p in parties.Keys) //TODO: Filter this on a per-block basis?
            {
                if (p.HasClientInParty(c))
                    return p;
            }


            return null;
        }

        public void CreateNewParty(Client c)
        {
            if (GetCurrentPartyForClient(c) != null)
                return; // For now

            parties.Add(new Party(c.User.Username, c), c.User.Username);
        }

        public void AddPlayerToParty(Client c, Party p)
        {
            if (!parties.ContainsKey(p))
                return;

            if (p.GetSize() >= 4) // For now
                return;

            p.AddClientToParty(c);
        }

        public void RemovePlayerToParty(Client c, Party p)
        {
            if (!parties.ContainsKey(p))
                return;

            //TODO: Later just transfer owner like the real servers.
            if (c == p.Host)
            {
                foreach (Client cl in p.Members)
                {
                    p.RemoveClientFromParty(cl);
                }
                parties.Remove(p);
            }
            else
            {
                p.RemoveClientFromParty(c);
            }
        }

    }
}
