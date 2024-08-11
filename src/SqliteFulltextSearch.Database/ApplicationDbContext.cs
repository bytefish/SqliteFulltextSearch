// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using SqliteFulltextSearch.Database.Model;

namespace SqliteFulltextSearch.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<FtsDocument> FtsDocuments { get; set; }

        public DbSet<Suggestion> Suggestions { get; set; }

        public DbSet<Keyword> Keywords { get; set; }

        public DbSet<DocumentKeyword> DocumentKeywords { get; set; }

        public DbSet<DocumentSuggestion> DocumentSuggestions { get; set; }

        public int MyProperty { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tables
            modelBuilder.Entity<FtsDocument>(entity =>
            {
                entity.ToTable("fts_document");

                entity.HasKey("rowid");

                entity
                    .Property(x => x.Title)
                    .HasColumnName("title");

                entity
                    .Property(x => x.Content)
                    .HasColumnName("content");

                entity
                    .HasOne(fts => fts.Document)
                    .WithOne()
                    .HasForeignKey<FtsDocument>(fts => fts.RowId);
            });

            modelBuilder.Entity<FtsSuggestion>(entity =>
            {
                entity.ToTable("fts_suggestion");

                entity.HasKey("rowid");

                entity
                    .Property(x => x.Name)
                    .HasColumnName("name");

                entity
                    .HasOne(fts => fts.Suggestion)
                    .WithOne()
                    .HasForeignKey<FtsDocument>(fts => fts.RowId);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("user_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Email)
                    .HasColumnType("text")
                    .HasColumnName("email")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.PreferredName)
                    .HasColumnType("text")
                    .HasColumnName("preferred_name")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("integer")
                    .HasColumnName("row_version")
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.ValidFrom)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ValidTo)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("document");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("document_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasColumnType("text")
                    .HasColumnName("title")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.Filename)
                    .HasColumnType("text")
                    .HasColumnName("filename")
                    .HasMaxLength(2000)
                    .IsRequired(true);

                entity.Property(e => e.Data)
                    .HasColumnType("blob")
                    .HasColumnName("data")
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("integer")
                    .HasColumnName("row_version")
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.ValidFrom)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ValidTo)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasMany<DocumentSuggestion>()
                    .WithOne();

                entity
                    .HasMany<DocumentKeyword>()
                    .WithOne();
            });

            modelBuilder.Entity<Keyword>(entity =>
            {
                entity.ToTable("keyword");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("INT")
                    .HasColumnName("keyword_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnType("text")
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("integer")
                    .HasColumnName("row_version")
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.ValidFrom)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ValidTo)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasMany<DocumentKeyword>()
                    .WithOne();
            });

            modelBuilder.Entity<Suggestion>(entity =>
            {
                entity.ToTable("suggestion");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("INT")
                    .HasColumnName("suggestion_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasColumnType("text")
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("integer")
                    .HasColumnName("row_version")
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.ValidFrom)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ValidTo)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasMany<DocumentSuggestion>()
                    .WithOne();
            });

            modelBuilder.Entity<DocumentKeyword>(entity =>
            {
                entity.ToTable("document_keyword");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("document_keyword_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DocumentId)
                    .HasColumnType("integer")
                    .HasColumnName("document_id")
                    .IsRequired(true);

                entity.Property(e => e.KeywordId)
                    .HasColumnType("integer")
                    .HasColumnName("keyword_id")
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("integer")
                    .HasColumnName("row_version")
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.ValidFrom)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ValidTo)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasOne<Keyword>()
                    .WithMany()
                    .HasForeignKey(x => x.KeywordId);

                entity
                    .HasOne<Document>()
                    .WithMany()
                    .HasForeignKey(x => x.DocumentId);
            });

            modelBuilder.Entity<DocumentSuggestion>(entity =>
            {
                entity.ToTable("document_suggestion");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("integer")
                    .HasColumnName("document_suggestion_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.DocumentId)
                    .HasColumnType("int")
                    .HasColumnName("document_id")
                    .IsRequired(true);

                entity.Property(e => e.SuggestionId)
                    .HasColumnType("int")
                    .HasColumnName("suggestion_id")
                    .IsRequired(true);

                entity.Property(e => e.RowVersion)
                    .HasColumnType("xid")
                    .HasColumnName("xmin")
                    .IsRowVersion()
                    .IsConcurrencyToken()
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LastEditedBy)
                    .HasColumnType("integer")
                    .HasColumnName("last_edited_by")
                    .IsRequired(true);

                entity.Property(e => e.ValidFrom)
                    .HasColumnType("text")
                    .HasColumnName("valid_from")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.ValidTo)
                    .HasColumnType("text")
                    .HasColumnName("valid_to")
                    .IsRequired(false)
                    .ValueGeneratedOnAddOrUpdate();

                entity
                    .HasOne<Suggestion>()
                    .WithMany()
                    .HasForeignKey(x => x.SuggestionId);


                entity
                    .HasOne<Document>()
                    .WithMany()
                    .HasForeignKey(x => x.DocumentId);

            });

            base.OnModelCreating(modelBuilder);
        }
    }
}