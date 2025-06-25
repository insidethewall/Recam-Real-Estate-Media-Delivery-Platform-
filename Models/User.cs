using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Identity;


namespace RecamSystemApi.Models;

public class User : IdentityUser
{
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public Agent? Agent { get; set; }
    public Photographer? Photographer { get; set; }

    public Collection<ListingCase> ListingCases { get; set; } = new Collection<ListingCase>();
    public Collection<MediaAsset> MediaAssets { get; set; } = new Collection<MediaAsset>();
}
