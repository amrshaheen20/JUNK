using ChatApi.server.Models.DbSet;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.server.Context
{
    public class DataBaseContext : IdentityDbContext<Profile>
    {

#pragma warning disable CS8618
        public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options) { }
#pragma warning restore CS8618 

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ProfileSession> ProfileSessions { get; set; }
        public DbSet<Server> Servers { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Friendship> Friendships { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<IdentityUserToken<string>>();
            modelBuilder.Ignore<IdentityUserLogin<string>>();
            modelBuilder.Ignore<IdentityUserClaim<string>>();

            modelBuilder.Entity<Profile>().ToTable("Profiles");

            modelBuilder.Entity<IdentityRole>().ToTable("ProfilesRoles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("ProfileUserRoles");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("ProfileRoleClaims");
            //modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("ProfileUserClaims");
            //modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("ProfileUserLogins");
            //modelBuilder.Entity<IdentityUserToken<string>>().ToTable("ProfileUserTokens");



            //Profile
            //////////
            modelBuilder.Entity<Profile>()
                .HasMany(x => x.Messages)
                .WithOne(x => x.Profile)
                .HasForeignKey(x => x.ProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Profile>()
                .HasMany(x => x.ChannelsCreatedByUser)
                .WithOne(x => x.Profile)
                .HasForeignKey(x => x.ProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Profile>()
                 .HasMany(x => x.Servers)
                 .WithOne(x => x.Profile)
                 .HasForeignKey(x => x.ProfileId)
                 .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Profile>()
                 .HasMany(x => x.ConversationsInitiated)
                 .WithOne(x => x.ProfileOne)
                 .HasForeignKey(x => x.ProfileOneId)
                 .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Profile>()
                 .HasMany(x => x.ConversationsReceived)
                 .WithOne(x => x.ProfileTwo)
                 .HasForeignKey(x => x.ProfileTwoId)
                 .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Profile>()
                .HasMany(x => x.Attachments)
                .WithOne(x => x.Profile)
                .HasForeignKey(x => x.ProfileId)
                .OnDelete(DeleteBehavior.SetNull);


            ////
            modelBuilder.Entity<Profile>()
                 .HasMany(x => x.Members)
                 .WithOne(x => x.Profile)
                 .HasForeignKey(x => x.ProfileId)
                 .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
                 .HasMany(x => x.ProfileSessions)
                 .WithOne(x => x.Profile)
                 .HasForeignKey(x => x.ProfileId)
                 .OnDelete(DeleteBehavior.Cascade);

            ///////////////////////////
            //////////////////////////
            // Conversation
            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Channel)
                .WithOne(ch => ch.Conversation)
                .HasForeignKey<Conversation>(c => c.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Conversation>()
                    .HasOne(c => c.ProfileOne)
                    .WithMany(m => m.ConversationsInitiated)
                    .HasForeignKey(c => c.ProfileOneId)
                    .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Conversation>()
                    .HasOne(c => c.ProfileTwo)
                    .WithMany(m => m.ConversationsReceived)
                    .HasForeignKey(c => c.ProfileTwoId)
                    .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Conversation>()
               .HasIndex(c => new { c.ProfileOneId, c.ProfileTwoId })
               .IsUnique();

            ///////////////////////////
            //////////////////////////
            // Server

            modelBuilder.Entity<Server>()
               .HasMany(x => x.Channels)
               .WithOne(x => x.Server)
               .HasForeignKey(x => x.ServerId)
               .OnDelete(DeleteBehavior.Cascade);



            ///////////////////////////
            //////////////////////////
            // Member
            modelBuilder.Entity<Member>()
                .HasIndex(m => new { m.ProfileId, m.ServerId })
                .IsUnique();

            ///////////////////////////
            //////////////////////////
            // Channel

            modelBuilder.Entity<Channel>()
                .HasMany(m => m.Messages)
                .WithOne(s => s.Channel)
                .HasForeignKey(m => m.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);


            ///////////////////////////
            //////////////////////////
            // Attachment 


            ///////////////////////////
            //////////////////////////
            // Message
            modelBuilder.Entity<Message>()
                    .HasMany(c => c.Attachments)
                    .WithOne(m => m.Message)
                    .HasForeignKey(c => c.MessageId)
                    .OnDelete(DeleteBehavior.SetNull);



            //////////////////////
            //Friendship
            //////////////////////
            modelBuilder.Entity<Friendship>()
                .HasIndex(c => new { c.UserId, c.FriendId })
                .IsUnique();

            modelBuilder.Entity<Friendship>()
            .HasOne(f => f.User)
            .WithMany(u => u.Friendships)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Friendship>()
                .HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.NoAction);

        }





    }
}
