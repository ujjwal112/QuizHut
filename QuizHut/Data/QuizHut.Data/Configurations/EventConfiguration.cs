﻿namespace QuizHut.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using QuizHut.Data.Models;

    using QuizHut.Data.Validations;

    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> eventEntity)
        {
            eventEntity.Property(g => g.Name)
           .HasMaxLength(DataValidation.Event.NameMaxLength)
           .IsRequired();
        }
    }
}