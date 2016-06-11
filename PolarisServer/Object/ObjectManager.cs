using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using PolarisServer.Database;
using PolarisServer.Models;

namespace PolarisServer.Object
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; set; } = new ObjectManager();

        private Dictionary<ulong, PSOObject> allObjects = new Dictionary<ulong, PSOObject>();
        private Dictionary<String, Dictionary<ulong, PSOObject>> zoneObjects = new Dictionary<string, Dictionary<ulong, PSOObject>>();

        public ObjectManager()
        {
        }

        public PSOObject[] GetObjectsForZone(string zone)
        {
            if (zone == "tpmap") // Return empty object array for an tp'd map for now (We spawn in a teleporter manually)
            {
                return new PSOObject[0];
            }

            if (!zoneObjects.ContainsKey(zone))
            {
                Dictionary<ulong, PSOObject> objects = new Dictionary<ulong, PSOObject>();

                if (Directory.Exists("Resources/objects/" + zone))
                {
                    var objectPaths = Directory.GetFiles("Resources/objects/" + zone);

                    Array.Sort(objectPaths);

                    foreach (var path in objectPaths)
                    {
                        if (Path.GetExtension(path) == ".bin")
                        {
                            var newObject = PSOObject.FromPacketBin(File.ReadAllBytes(path));

                            objects.Add(newObject.Header.ID, newObject);
                            allObjects.Add(newObject.Header.ID, newObject);

                            Logger.WriteInternal("[OBJ] Loaded object ID {0} with name {1} pos: ({2}, {3}, {4})", newObject.Header.ID, newObject.Name, newObject.Position.PosX,
                                                 newObject.Position.PosY, newObject.Position.PosZ);
                        }
                        else if (Path.GetExtension(path) == ".json")
                        {
                            var newObject = JsonConvert.DeserializeObject<PSOObject>(File.ReadAllText(path));

                            objects.Add(newObject.Header.ID, newObject);
                            allObjects.Add(newObject.Header.ID, newObject);

                            Logger.WriteInternal("[OBJ] Loaded object ID {0} with name {1} pos: ({2}, {3}, {4})", newObject.Header.ID, newObject.Name, newObject.Position.PosX,
                                                  newObject.Position.PosY, newObject.Position.PosZ);
                        }
                    }
                }

                zoneObjects.Add(zone, objects);
            }

            return zoneObjects[zone].Values.ToArray();
        }

        internal PSONPC[] getNPCSForZone(string zone)
        {
            List<PSONPC> npcs = new List<PSONPC>();

            /*
            using (var db = new PolarisEf())
            {
                var dbNpcs = from n in db.NPCs
                             where n.ZoneName == zone
                             select n;

                foreach (NPC npc in dbNpcs)
                {
                    PSONPC dNpc = new PSONPC();
                    dNpc.Header = new ObjectHeader((uint)npc.EntityID, EntityType.Object);
                    dNpc.Position = new PSOLocation(npc.RotX, npc.RotY, npc.RotZ, npc.RotW, npc.PosX, npc.PosY, npc.PosZ);
                    dNpc.Name = npc.NPCName;

                    npcs.Add(dNpc);
                    if (!zoneObjects[zone].ContainsKey(dNpc.Header.ID))
                    {
                        zoneObjects[zone].Add(dNpc.Header.ID, dNpc);
                    }
                    if (!allObjects.ContainsKey(dNpc.Header.ID))
                        allObjects.Add(dNpc.Header.ID, dNpc);
                }
            }
            */

            return npcs.ToArray();
        }

        internal PSOObject GetObjectByID(string zone, uint ID)
        {
            /* FIXME: This has been commented out because we were getting object errors with possible shared objects? That or it was just object 1 which is an edge case.
            if (!zoneObjects.ContainsKey(zone) || !zoneObjects[zone].ContainsKey(ID))
            {
                throw new Exception(String.Format("Object ID {0} does not exist in {1}!", ID, zone));
            } */

            // return zoneObjects[zone][ID];
            return GetObjectByID(ID);
        }

        internal PSOObject GetObjectByID(uint ID)
        {
            if (!allObjects.ContainsKey(ID))
            {
                Logger.WriteWarning("[OBJ] Client requested object {0} which we don't know about. Investigate.", ID);
                return new PSOObject() { Header = new ObjectHeader(ID, EntityType.Object), Name = "Unknown" };
            }

            return allObjects[ID];
        }
    }
}
