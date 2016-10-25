﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace NoraGrace.Sql
{
    public class ChessDb: System.Data.Entity.DbContext
    {
        public ChessDb() : base()
        {
            //Database.SetInitializer<ChessDb>(new DropCreateDatabaseAlways<ChessDb>());
        }

        public virtual DbSet<DbGame> Games { get; set; }
        public virtual DbSet<DbMove> Moves { get; set; }
        //public virtual DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DbGame>()
                .HasMany(e => e.Moves)
                .WithRequired(e => e.Game)
                .HasForeignKey(e => e.GameId)
                .WillCascadeOnDelete(true);

            //modelBuilder.Entity<Player>()
            //    .HasMany(e => e.GamesAsBlack)
            //    .WithRequired(e => e.BlackPlayer)
            //    .HasForeignKey(e => e.BlackPlayerId)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Player>()
            //    .HasMany(e => e.GamesAsWhite)
            //    .WithRequired(e => e.WhitePlayer)
            //    .HasForeignKey(e => e.WhitePlayerId)
            //    .WillCascadeOnDelete(false);
        }

        public override int SaveChanges()
        {

            return base.SaveChanges();
        }
    }
}