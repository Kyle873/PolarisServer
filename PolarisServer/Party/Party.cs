using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PolarisServer.Models;
using PolarisServer.Packets.PSOPackets;

namespace PolarisServer.Party
{
    public class Party
    {
        public string Name { get; }
        public Quest CurrentQuest { get; set; }
        public Client Host { get; }
        public List<Client> Members { get; }

        public Party(string name, Client host)
        {
            this.Name = name;
            this.Host = host;
            this.Members = new List<Client>();

            AddClientToParty(host);
        }

        public void AddClientToParty(Client c)
        {
            if (Members.Count < 1)
            {
                c.SendPacket(new PartyInitPacket(new Character[1] { c.Character }));
            }
            else
            {
                // ???
            }

            Members.Add(c);

            c.currentParty = this;
        }

        public void RemoveClientFromParty(Client c)
        {
            if (!Members.Contains(c))
            {
                Logger.WriteWarning("[PTY] Client {0} was trying to be removed from {1}, but he was never in {1}!", c.User.Username, Name);
                return;
            }

            Members.Remove(c);

            // TODO: do stuff like send the "remove from party" packet.
        }

        public bool HasClientInParty(Client c)
        {
            return Members.Contains(c);
        }

        public int GetSize()
        {
            return Members.Count;
        }
    }
}
