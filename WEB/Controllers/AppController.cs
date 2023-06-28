using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace WEB.Controllers
{
    [Route("api/[Controller]"), Authorize]
    public class AppController : BaseApiController
    {
        public AppController(ApplicationDbContext db, UserManager<User> um, AppSettings appSettings)
            : base(db, um, appSettings) { }

        [HttpGet, Route("settings")]
        [HttpGet, AuthorizeRoles(Roles.Administrator)]
        public IActionResult Get()
        {
            var dbSettings = AppSettings.GetDbSettings(db);

            // return the settings for all logged in users
            return Ok(
                new
                {
                    dbSettings.UseSubmit,
                    dbSettings.UseVerify,
                    dbSettings.UseApprove,
                    dbSettings.UseReject
                }
            );
        }

        [HttpGet, Route("fielddata")]
        public async Task<IActionResult> GetFieldData()
        {
            var fields = await db.Fields
                .Include(o => o.Options)
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            var groups = await db.Groups
                .OrderBy(o => o.SortOrder)
                .ToListAsync();

            // add a blank option to unselect the selected value
            foreach (var field in fields)
            {
                if (field.FieldType == FieldType.Picklist && !field.Multiple && !field.Required && !field.RadioCheckbox)
                    field.Options = field.Options.Prepend(new Option { FieldId = field.FieldId, OptionId = Guid.Empty, Name = string.Empty, SortOrder = -1 }).ToList();
                field.Options = field.Options.OrderBy(o => o.SortOrder).ToList();
            }

            return Ok(
                new FieldData
                {
                    Fields = fields.Select(o => ModelFactory.Create(o)).ToList(),
                    Groups = groups.Select(o => ModelFactory.Create(o)).ToList()
                }
            );
        }

        public class FieldData
        {
            public IEnumerable<FieldDTO> Fields { get; set; }
            public IEnumerable<GroupDTO> Groups { get; set; }
        }
    }
}
