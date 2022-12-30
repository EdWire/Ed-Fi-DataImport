using System.Threading.Tasks;
using DataImport.Web.Areas.Instance.Entities;
using DataImport.Web.Areas.Instance.Modules;
using Microsoft.AspNetCore.Mvc;

namespace DataImport.Web.Areas.Instance.Pages
{
    [ViewComponent(Name = "Instance")]
    public partial class InstanceViewComponent : ViewComponent
    {
        private readonly IInstanceDropdownValueProvider _instanceDropdownValueProvider;

        public InstanceViewComponent(IInstanceDropdownValueProvider instanceDropdownValueProvider)
        {
            _instanceDropdownValueProvider = instanceDropdownValueProvider;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            InstancesDropdownValues = await _instanceDropdownValueProvider.GetDropdown(HttpContext);
            return View(InstancesDropdownValues);
        }
        public InstancesDropdown InstancesDropdownValues { get; set; }
    }
}
