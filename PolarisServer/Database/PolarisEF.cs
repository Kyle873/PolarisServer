﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;

using MySql.Data.Entity;

using PolarisServer.Models;

namespace PolarisServer.Database
{
    public class ServerInfo
    {
        [Key, MaxLength(255)]
        public string Info { get; set; }

        public string Setting { get; set; }
    }

    public class Player
    {
        [Key]
        public int PlayerID { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public string SettingsINI { get; set; }
    }

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class PolarisEf : DbContext
    {
        public DbSet<ServerInfo> ServerInfo { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Character> Characters { get; set; }

        public PolarisEf()
            : base(string.Format("server={0};database={1};username={2};password={3}", PolarisApp.Config.DatabaseAddress,
                                 PolarisApp.Config.DatabaseName, PolarisApp.Config.DatabaseUsername,
                                 PolarisApp.Config.DatabasePassword))
        {
        }

        public void SetupDB()
        {
            try
            {
                foreach (var f in Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/Resources/sql/scripts/", "*.sql"))
                {
                    Logger.WriteInternal("[DB ] Executing database script {0}", f);
                    Database.ExecuteSqlCommand(File.ReadAllText(f));
                }

                var revision = ServerInfo.Find("Revision");
                if (revision == null)
                {
                    revision = new ServerInfo { Info = "Revision", Setting = "0" };
                    ServerInfo.Add(revision);

                    // TODO: Possibly move this somewhere else?
                    Database.ExecuteSqlCommand("ALTER TABLE Players AUTO_INCREMENT=10000000");
                }

                SaveChanges();

                Logger.WriteInternal("[DB ] Loaded database with dataset revision {0}", revision.Setting);
            }
            catch (Exception ex)
            {
                Logger.WriteException("A database exception has occured", ex);
            }
        }
    }
}
