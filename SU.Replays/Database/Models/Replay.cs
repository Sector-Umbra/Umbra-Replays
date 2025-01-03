using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SU.Replays.Database.Models;

public class Replay : IEntityTypeConfiguration<Replay>
{
    /// <summary>
    /// The location on disk for this replay. Used to check if it still exists.
    /// </summary>
    public string FileLocation { get; set; } = default!;

    /// <summary>
    /// The players that where in this round.
    /// </summary>
    public List<Guid> Participants { get; set; } = default!;


    public void Configure(EntityTypeBuilder<Replay> builder)
    {
        builder.HasKey(e => e.FileLocation);
        builder.Property(e => e.FileLocation)
            .IsRequired();
        builder.Property(e => e.Participants)
            .IsRequired();
    }
}