using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WEB.Models;

namespace WEB.Controllers
{
    [Route("api/[Controller]"), Authorize]
    public class ComponentsController : BaseApiController
    {
        public ComponentsController(ApplicationDbContext db, UserManager<User> um, AppSettings appSettings) : base(db, um, appSettings) { }

        [HttpGet, AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Search([FromQuery] ComponentSearchOptions searchOptions)
        {
            IQueryable<Component> results = db.Components;

            if (searchOptions.IncludeChildren)
            {
                results = results.Include(o => o.ComponentIndicators);
                results = results.Include(o => o.RelationshipsAsSource);
                results = results.Include(o => o.LogFrameRowComponents);
                results = results.Include(o => o.RelationshipsAsTarget);
                results = results.Include(o => o.TheoryOfChangeComponents);
            }

            if (!string.IsNullOrWhiteSpace(searchOptions.q))
                results = results.Where(o => o.Name.Contains(searchOptions.q) || o.Code.Contains(searchOptions.q) || o.Description.Contains(searchOptions.q));

            if (searchOptions.ComponentType.HasValue) results = results.Where(o => o.ComponentType == searchOptions.ComponentType);

            results = results.OrderBy(o => o.SortOrder);

            return Ok((await GetPaginatedResponse(results, searchOptions)).Select(o => ModelFactory.Create(o, searchOptions.IncludeParents, searchOptions.IncludeChildren)));
        }

        [HttpGet("{componentId:Guid}"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Get(Guid componentId)
        {
            var component = await db.Components
                .FirstOrDefaultAsync(o => o.ComponentId == componentId);

            if (component == null)
                return NotFound();

            var item = await db.Items
               .Include(o => o.FieldValues)
               .Include(o => o.OptionValues)
               .FirstOrDefaultAsync(o => o.ItemId == componentId);

            return Ok(ModelFactory.Create(component, true, false, item));
        }

        [HttpPost("{componentId:Guid}"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Save(Guid componentId, [FromBody] ComponentDTO componentDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (componentDTO.ComponentId != componentId) return BadRequest("Id mismatch");

            if (await db.Components.AnyAsync(o => o.Code == componentDTO.Code && o.ComponentId != componentDTO.ComponentId))
                return BadRequest("Code already exists.");

            var isNew = componentDTO.ComponentId == Guid.Empty;

            Component component;
            if (isNew)
            {
                component = new Component();

                componentDTO.SortOrder = (await db.Components.MaxAsync(o => (int?)o.SortOrder) ?? 0) + 1;

                db.Entry(new Item { ItemId = component.ComponentId, ItemType = ItemType.Component }).State = EntityState.Added;

                db.Entry(component).State = EntityState.Added;
            }
            else
            {
                component = await db.Components
                    .FirstOrDefaultAsync(o => o.ComponentId == componentDTO.ComponentId);

                if (component == null)
                    return NotFound();

                db.Entry(component).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(component, componentDTO);

            await ItemFunctions.HydrateFieldsAsync(db, component.ComponentId, componentDTO.FieldValues, componentDTO.OptionValues);

            await db.SaveChangesAsync();

            return await Get(component.ComponentId);
        }

        [HttpDelete("{componentId:Guid}"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Delete(Guid componentId)
        {
            var component = await db.Components
                .FirstOrDefaultAsync(o => o.ComponentId == componentId);

            if (component == null)
                return NotFound();

            foreach (var relationship in db.Relationships.Where(o => o.SourceComponentId == component.ComponentId))
                db.Entry(relationship).State = EntityState.Deleted;

            foreach (var relationship in db.Relationships.Where(o => o.TargetComponentId == component.ComponentId))
                db.Entry(relationship).State = EntityState.Deleted;

            if (await db.LogFrameRowComponents.AnyAsync(o => o.ComponentId == component.ComponentId))
                return BadRequest("Unable to delete the component as it has related log frame row components");

            foreach (var theoryOfChangeComponent in db.TheoryOfChangeComponents.Where(o => o.ComponentId == component.ComponentId))
                db.Entry(theoryOfChangeComponent).State = EntityState.Deleted;

            foreach (var componentIndicator in db.ComponentIndicators.Where(o => o.ComponentId == component.ComponentId))
                db.Entry(componentIndicator).State = EntityState.Deleted;

            ItemFunctions.DeleteFields(db, componentId, true);

            db.Entry(component).State = EntityState.Deleted;

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("sort"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Sort([FromBody] Guid[] sortedIds)
        {
            var components = await db.Components
                .ToListAsync();
            if (components.Count != sortedIds.Length) return BadRequest("Some of the components could not be found");

            foreach (var component in components)
            {
                db.Entry(component).State = EntityState.Modified;
                component.SortOrder = Array.IndexOf(sortedIds, component.ComponentId);
            }

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{componentId:Guid}/componentindicators"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> SaveComponentIndicators(Guid componentId, [FromBody] Guid[] indicatorIds)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var componentIndicators = await db.ComponentIndicators
                .Where(o => o.ComponentId == componentId)
                .ToListAsync();

            foreach (var indicatorId in indicatorIds)
            {
                if (!componentIndicators.Any(o => o.IndicatorId == indicatorId))
                {
                    var componentIndicator = new ComponentIndicator { ComponentId = componentId, IndicatorId = indicatorId };
                    db.Entry(componentIndicator).State = EntityState.Added;
                }
            }

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{componentId:Guid}/theoryofchangecomponents"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> SaveTheoryOfChangeComponents(Guid componentId, [FromBody] Guid[] theoryOfChangeIds)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var theoryOfChangeComponents = await db.TheoryOfChangeComponents
                .Where(o => o.ComponentId == componentId)
                .ToListAsync();

            foreach (var theoryOfChangeId in theoryOfChangeIds)
            {
                if (!theoryOfChangeComponents.Any(o => o.TheoryOfChangeId == theoryOfChangeId))
                {
                    var theoryOfChangeComponent = new TheoryOfChangeComponent { ComponentId = componentId, TheoryOfChangeId = theoryOfChangeId };
                    db.Entry(theoryOfChangeComponent).State = EntityState.Added;
                }
            }

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{componentId:Guid}/relationshipsassource"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> DeleteRelationshipsAsSource(Guid componentId)
        {
            foreach (var relationship in db.Relationships.Where(o => o.SourceComponentId == componentId).ToList())
                db.Entry(relationship).State = EntityState.Deleted;

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{componentId:Guid}/relationshipsastarget"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> DeleteRelationshipsAsTarget(Guid componentId)
        {
            foreach (var relationship in db.Relationships.Where(o => o.TargetComponentId == componentId).ToList())
                db.Entry(relationship).State = EntityState.Deleted;

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{componentId:Guid}/logframerowcomponents"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> DeleteLogFrameRowComponents(Guid componentId)
        {
            foreach (var logFrameRowComponent in db.LogFrameRowComponents.Where(o => o.ComponentId == componentId).ToList())
                db.Entry(logFrameRowComponent).State = EntityState.Deleted;

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{componentId:Guid}/theoryofchangecomponents"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> DeleteTheoryOfChangeComponents(Guid componentId)
        {
            foreach (var theoryOfChangeComponent in db.TheoryOfChangeComponents.Where(o => o.ComponentId == componentId).ToList())
                db.Entry(theoryOfChangeComponent).State = EntityState.Deleted;

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{componentId:Guid}/componentindicators"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> DeleteComponentIndicators(Guid componentId)
        {
            foreach (var componentIndicator in db.ComponentIndicators.Where(o => o.ComponentId == componentId).ToList())
                db.Entry(componentIndicator).State = EntityState.Deleted;

            await db.SaveChangesAsync();

            return Ok();
        }

    }
}
