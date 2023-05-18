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
    public class SubcategoriesController : BaseApiController
    {
        public SubcategoriesController(ApplicationDbContext db, UserManager<User> um, AppSettings appSettings) : base(db, um, appSettings) { }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SubcategorySearchOptions searchOptions)
        {
            IQueryable<Subcategory> results = db.Subcategories;

            if (searchOptions.IncludeParents)
            {
                results = results.Include(o => o.Category);
            }

            if (searchOptions.IncludeChildren)
            {
                results = results.Include(o => o.Indicators);
            }

            if (!string.IsNullOrWhiteSpace(searchOptions.q))
                results = results.Where(o => o.Name.Contains(searchOptions.q) || o.Code.Contains(searchOptions.q));

            if (searchOptions.CategoryId.HasValue) results = results.Where(o => o.CategoryId == searchOptions.CategoryId);

            results = results.OrderBy(o => o.Category.SortOrder).ThenBy(o => o.SortOrder);

            return Ok((await GetPaginatedResponse(results, searchOptions)).Select(o => ModelFactory.Create(o, searchOptions.IncludeParents, searchOptions.IncludeChildren)));
        }

        [HttpGet("{subcategoryId:Guid}")]
        public async Task<IActionResult> Get(Guid subcategoryId)
        {
            var subcategory = await db.Subcategories
                .Include(o => o.Category)
                .FirstOrDefaultAsync(o => o.SubcategoryId == subcategoryId);

            if (subcategory == null)
                return NotFound();

            return Ok(ModelFactory.Create(subcategory));
        }

        [HttpPost("{subcategoryId:Guid}"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Save(Guid subcategoryId, [FromBody] SubcategoryDTO subcategoryDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (subcategoryDTO.SubcategoryId != subcategoryId) return BadRequest("Id mismatch");

            if (await db.Subcategories.AnyAsync(o => o.CategoryId == subcategoryDTO.CategoryId && o.Name == subcategoryDTO.Name && o.SubcategoryId != subcategoryDTO.SubcategoryId))
                return BadRequest("Subcategory already exists on this Category.");

            if (await db.Subcategories.AnyAsync(o => o.Code == subcategoryDTO.Code && o.SubcategoryId != subcategoryDTO.SubcategoryId))
                return BadRequest("Code already exists.");

            var isNew = subcategoryDTO.SubcategoryId == Guid.Empty;

            Subcategory subcategory;
            if (isNew)
            {
                subcategory = new Subcategory();

                subcategoryDTO.SortOrder = (await db.Subcategories.Where(o => o.CategoryId == subcategoryDTO.CategoryId).MaxAsync(o => (int?)o.SortOrder) ?? 0) + 1;

                db.Entry(subcategory).State = EntityState.Added;
            }
            else
            {
                subcategory = await db.Subcategories
                    .FirstOrDefaultAsync(o => o.SubcategoryId == subcategoryDTO.SubcategoryId);

                if (subcategory == null)
                    return NotFound();

                db.Entry(subcategory).State = EntityState.Modified;
            }

            ModelFactory.Hydrate(subcategory, subcategoryDTO);

            await db.SaveChangesAsync();

            return await Get(subcategory.SubcategoryId);
        }

        [HttpDelete("{subcategoryId:Guid}"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Delete(Guid subcategoryId)
        {
            var subcategory = await db.Subcategories
                .FirstOrDefaultAsync(o => o.SubcategoryId == subcategoryId);

            if (subcategory == null)
                return NotFound();

            if (await db.Indicators.AnyAsync(o => o.SubcategoryId == subcategory.SubcategoryId))
                return BadRequest("Unable to delete the subcategory as it has related indicators");

            db.Entry(subcategory).State = EntityState.Deleted;

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("sort"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> Sort([FromQuery] Guid categoryId, [FromBody] Guid[] sortedIds)
        {
            var subcategories = await db.Subcategories
                .Where(o => o.CategoryId == categoryId)
                .ToListAsync();
            if (subcategories.Count != sortedIds.Length) return BadRequest("Some of the subcategories could not be found");

            foreach (var subcategory in subcategories)
            {
                db.Entry(subcategory).State = EntityState.Modified;
                subcategory.SortOrder = Array.IndexOf(sortedIds, subcategory.SubcategoryId);
            }

            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{subcategoryId:Guid}/indicators"), AuthorizeRoles(Roles.Administrator)]
        public async Task<IActionResult> DeleteIndicators(Guid subcategoryId)
        {
            foreach (var indicator in db.Indicators.Where(o => o.SubcategoryId == subcategoryId).ToList())
            {
                ItemFunctions.DeleteFields(db, indicator.IndicatorId, true);
                db.Entry(indicator).State = EntityState.Deleted;
            }

            await db.SaveChangesAsync();

            return Ok();
        }

    }
}
