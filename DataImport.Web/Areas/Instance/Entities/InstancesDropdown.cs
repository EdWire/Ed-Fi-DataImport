using System.Collections.Generic;

namespace DataImport.Web.Areas.Instance.Entities;

public class InstancesDropdown
{
    public List<string> Instances { get; set; }
    public string SelectedInstanceName { get; set; }
}
