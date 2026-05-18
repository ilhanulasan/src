using Dental.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dental.Web.Data;

internal static class Icd10SeedData
{
    public static readonly Icd10Code[] Codes =
    [
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000001"), Code = "K02", DescriptionTr = "Diş çürüğü", DescriptionEn = "Dental caries", Category = "Diş" },
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000002"), Code = "K04.0", DescriptionTr = "Pulpitis", DescriptionEn = "Pulpitis", Category = "Diş" },
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000003"), Code = "K04.7", DescriptionTr = "Periapikal apse", DescriptionEn = "Periapical abscess", Category = "Diş" },
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000004"), Code = "K05.0", DescriptionTr = "Akut gingivit", DescriptionEn = "Acute gingivitis", Category = "Dişeti" },
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000005"), Code = "K05.3", DescriptionTr = "Kronik periodontitis", DescriptionEn = "Chronic periodontitis", Category = "Dişeti" },
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000006"), Code = "K08.1", DescriptionTr = "Diş kaybı", DescriptionEn = "Loss of teeth", Category = "Diş" },
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000007"), Code = "K08.3", DescriptionTr = "Kalan kök", DescriptionEn = "Retained dental root", Category = "Diş" },
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000008"), Code = "K10.2", DescriptionTr = "Çene inflamasyonu", DescriptionEn = "Inflammatory conditions of jaws", Category = "Çene" },
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000009"), Code = "K12.0", DescriptionTr = "Rekürren aftöz stomatit", DescriptionEn = "Recurrent oral aphthae", Category = "Ağız" },
        new() { Id = Guid.Parse("a1000001-0000-4000-8000-000000000010"), Code = "K13.0", DescriptionTr = "Dudak çatlakları", DescriptionEn = "Diseases of lips", Category = "Ağız" },
    ];

    public static void Seed(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Icd10Code>().HasData(Codes);
}
