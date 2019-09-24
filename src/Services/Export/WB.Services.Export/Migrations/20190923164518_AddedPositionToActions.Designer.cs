﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WB.Services.Export.Infrastructure;

namespace WB.Services.Export.Migrations
{
    [DbContext(typeof(TenantDbContext))]
    [Migration("20190923164518_AddedPositionToActions")]
    partial class AddedPositionToActions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("WB.Services.Export.Assignment.Assignment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<bool>("AudioRecording")
                        .HasColumnName("audio_recording");

                    b.Property<string>("Comment")
                        .HasColumnName("comment");

                    b.Property<Guid>("PublicKey")
                        .HasColumnName("public_key");

                    b.Property<int?>("Quantity")
                        .HasColumnName("quantity");

                    b.Property<Guid>("ResponsibleId")
                        .HasColumnName("responsible_id");

                    b.Property<bool?>("WebMode")
                        .HasColumnName("web_mode");

                    b.HasKey("Id")
                        .HasName("pk_assignments");

                    b.HasAlternateKey("PublicKey");

                    b.ToTable("__assignment");
                });

            modelBuilder.Entity("WB.Services.Export.Assignment.AssignmentAction", b =>
                {
                    b.Property<long>("GlobalSequence")
                        .HasColumnName("global_sequence");

                    b.Property<int>("Position")
                        .HasColumnName("position");

                    b.Property<int>("AssignmentId")
                        .HasColumnName("assignment_id");

                    b.Property<string>("Comment")
                        .HasColumnName("comment");

                    b.Property<string>("NewValue")
                        .HasColumnName("new_value");

                    b.Property<string>("OldValue")
                        .HasColumnName("old_value");

                    b.Property<Guid>("OriginatorId")
                        .HasColumnName("originator_id");

                    b.Property<Guid>("ResponsibleId")
                        .HasColumnName("responsible_id");

                    b.Property<int>("Status")
                        .HasColumnName("status");

                    b.Property<DateTime>("TimestampUtc")
                        .HasColumnName("timestamp_utc");

                    b.HasKey("GlobalSequence", "Position");

                    b.HasIndex("AssignmentId")
                        .HasName("ix_assignment_actions_assignment_id");

                    b.ToTable("__assignment__action");
                });

            modelBuilder.Entity("WB.Services.Export.InterviewDataStorage.GeneratedQuestionnaireReference", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnName("deleted_at");

                    b.HasKey("Id")
                        .HasName("pk_generated_questionnaires");

                    b.ToTable("__generated_questionnaire_reference");
                });

            modelBuilder.Entity("WB.Services.Export.InterviewDataStorage.InterviewReference", b =>
                {
                    b.Property<Guid>("InterviewId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("interview_id");

                    b.Property<int?>("AssignmentId")
                        .HasColumnName("assignment_id");

                    b.Property<DateTime?>("DeletedAtUtc")
                        .HasColumnName("deleted_at_utc");

                    b.Property<string>("Key")
                        .HasColumnName("key");

                    b.Property<string>("QuestionnaireId")
                        .HasColumnName("questionnaire_id");

                    b.Property<int>("Status")
                        .HasColumnName("status");

                    b.Property<DateTime?>("UpdateDateUtc")
                        .HasColumnName("update_date_utc");

                    b.HasKey("InterviewId");

                    b.ToTable("interview__references");
                });

            modelBuilder.Entity("WB.Services.Export.InterviewDataStorage.Metadata", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Value")
                        .HasColumnName("value");

                    b.HasKey("Id")
                        .HasName("pk_metadata");

                    b.ToTable("metadata");
                });

            modelBuilder.Entity("WB.Services.Export.Assignment.AssignmentAction", b =>
                {
                    b.HasOne("WB.Services.Export.Assignment.Assignment", "Assignment")
                        .WithMany("Actions")
                        .HasForeignKey("AssignmentId")
                        .HasConstraintName("fk_assignment_actions_assignments_assignment_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
