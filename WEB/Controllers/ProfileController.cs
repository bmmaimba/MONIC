using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB.Controllers;
using WEB.Models;

namespace AuthorizationServer.Controllers
{
    [Route("api/[Controller]"), Authorize]
    public class ProfileController : BaseApiController
    {
        public ProfileController(
            IDbContextFactory<ApplicationDbContext> dbFactory,
            UserManager<User> _um,
            AppSettings _appSettings
            )
            : base(dbFactory, _um, _appSettings)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {

            var entityIds = CurrentUser.GetPermittedEntityIds();

            var indicatorPermissions = await db.IndicatorPermissions.Where(o => o.UserId == CurrentUser.Id).ToListAsync();

            var profile = new ProfileModel
            {
                Email = CurrentUser.Email,
                FirstName = CurrentUser.FirstName,
                LastName = CurrentUser.LastName,
                FullName = CurrentUser.FullName,
                UserId = CurrentUser.Id,
                Roles = roleNames,
                UserName = CurrentUser.UserName,
                OrganisationId = CurrentUser.OrganisationId,
                Organisation = ModelFactory.Create(CurrentUser.Organisation),
                EntityIds = entityIds,
                IndicatorPermissions = indicatorPermissions.Select(o => ModelFactory.Create(o)).ToList(),
                DashboardSettings = CurrentUser.DashboardSettings
            };

            return Ok(profile);
        }

        [HttpPost("dashboardsettings"), Authorize]
        public async Task<IActionResult> Save(DashboardSettingsDTO dashboardSettingsDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await db.Users.FirstAsync(o => o.UserName == User.Identity.Name);

            user.DashboardSettings = dashboardSettingsDTO.DashboardSettings;
            db.Entry(user).State = EntityState.Modified;

            await db.SaveChangesAsync();

            return Ok();
        }

        public class DashboardSettingsDTO
        {
            public string DashboardSettings { get; set; }
        }
    }
}