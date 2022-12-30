namespace DataImport.Web.Areas.EdGraph.Entities;

public class UserProfile
{
    public Tenant[] Tenants { get; set; }
    public UserPreference[] Preferences { get; set; }
}
