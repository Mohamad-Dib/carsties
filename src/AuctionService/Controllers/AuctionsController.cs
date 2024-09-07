using System.Runtime.CompilerServices;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController(AuctionDbContext context, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if(!string.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }   

            return await query.ProjectTo<AuctionDto>(mapper.ConfigurationProvider).ToListAsync();
            
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
            if (auction == null)
            {
                return NotFound();
            }
            return Ok(mapper.Map<AuctionDto>(auction));
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = mapper.Map<Auction>(auctionDto);
            // TODO: add current user as seller
            auction.Seller = "test";
            context.Auctions.Add(auction);
            var result = await context.SaveChangesAsync() > 0;
            if (!result)
            {
                return BadRequest("Could no save changes to the db");
            }
            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, mapper.Map<AuctionDto>(auction));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await context.Auctions.Include(x=>x.Item).FirstOrDefaultAsync(x => x.Id == id);   
            if (auction == null)
            {
                return NotFound();
            }
            // TODO: Check seller == username

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;    
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;    
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;    
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;    
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;    
            
            var result = await context.SaveChangesAsync() > 0;
            if (result)
            {
                return Ok();
            }
            return BadRequest("problem saving changes");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await context.Auctions.FindAsync( id);
            if (auction == null)
            {
                return NotFound();
            }
            
            // TODO: Check seller == username   

            context.Auctions.Remove(auction);
            var result = await context.SaveChangesAsync() > 0;
            if (result)
            {
                return Ok();
            }
            return BadRequest   ("Could not update the db");
        }


    }
}
