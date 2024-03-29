﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using XiaoTianQuanServer.DataModels;

namespace XiaoTianQuanServer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<VendingMachine>().HasIndex(v => v.MachineId).IsUnique(true);

            builder.Entity<Inventory>().HasIndex(i => new { i.VendingMachineId, i.Slot }).IsUnique(true);

            builder.Entity<Transaction>().HasIndex(t => t.Settled).IsUnique(false);
            builder.Entity<Transaction>().HasIndex(t => t.TransactionExpiry).IsUnique(false);

            builder.Entity<LightningNetworkTransaction>().HasOne(t => t.Transaction).WithOne();
            builder.Entity<LightningNetworkTransaction>().HasIndex(t => t.Settled).IsUnique(false);
            builder.Entity<LightningNetworkTransaction>().HasIndex(t => t.PaymentHash).IsUnique(true);

            base.OnModelCreating(builder);
        }

        public DbSet<VendingMachine> VendingMachines { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<LightningNetworkTransaction> LightningNetworkTransactions { get; set; }
    }
}
