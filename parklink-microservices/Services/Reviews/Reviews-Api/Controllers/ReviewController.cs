using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reviews_Domain.Data;
using Reviews_Domain.Entities;
using Reviews_Infrastructure.Persistence.Repositories;

namespace Reviews_Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewRepository _repository;

    public ReviewController(IReviewRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(Review), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<Review>>> GetReview()
    {
        /*
         * Parameters: { None }
         *
         * Return: List<Review> Review - array instances of parking
         *
         * Description: Gets all the parking in the table
         *
         * Authorization: None
         */
        
        return Ok(await _repository.GetReview());
    }

    [HttpGet("{id}", Name = "GetReview")]
    [ProducesResponseType(typeof(Review), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Review>> GetReview(Guid id)
    {
        /*
         * Parameters: { None }
         *
         * Return: Review
         *
         * Description: Gets the review with the corresponding id
         *
         * Authorization: None
         */
        
        var review = await _repository.GetReview(id);

        if (review == null) return NotFound();
        
        return Ok(review);
    }
    
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Review), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Review>> CreateReview([FromBody] ReviewCreateDto reviewCreateDto)
    {
        /*
         * Parameters: { Review }
         *
         * Return: Review
         *
         * Description: Creates a new review
         *
         * Authorization: You need to be a user to create a review
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var email = identity.FindFirst(ClaimTypes.Email);

        if (nameId == null || email == null) return Forbid();
        
        // like in other areas, the accountId is automatically added by the controller when creating a review
        var accountId = new Guid(nameId.Value);
        
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var currentTime = DateTime.UtcNow;
        var isDayLightSaving = info.IsDaylightSavingTime(currentTime);
        var currentDateTime = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;

        var review = new Review
        {
            ReviewRating = reviewCreateDto.ReviewRating,
            ReviewText = reviewCreateDto.ReviewText,
            ReviewTitle = email.Value,
            ParkingId = reviewCreateDto.ParkingId,
            AccountId = accountId,
            CreatedAt = currentDateTime,
            EditDate = currentDateTime
        };

        var reviewId = await _repository.CreateReview(review);

        review.Id = reviewId;
        
        return CreatedAtRoute("GetReview", new { id = reviewId }, review);
    }
    
    [HttpGet("parking/{id}", Name = "GetParkingReview")]
    [ProducesResponseType(typeof(Review), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Review>> GetParkingReviewAnalytics(Guid id)
    {
        /*
         * Parameters: { None }
         *
         * Return: ReviewAnalyticsDto
         *
         * Description: Gets the analytics for the parking spot that is passed
         *
         * Authorization: None
         */

        var review = await _repository.CheckRating(id);

        return Ok(review);
    }
    
    [Authorize]
    [HttpPut]
    [ProducesResponseType(typeof(Review), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Review>> UpdateReview([FromBody] Review review)
    {
        /*
         * Parameters: { Review }
         *
         * Return: Ok
         *
         * Description: Updates the review
         *
         * Authorization: Only an owner of the review can edit it
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var email = identity.FindFirst(ClaimTypes.Email);
        
        if (nameId == null || email == null) return Forbid();
        
        var accountId = new Guid(nameId.Value);

        var existingReview = await _repository.GetReview(review.Id);
        if (existingReview == null) return NotFound();

        if (existingReview.AccountId != accountId) return Forbid();
        
        return Ok(await _repository.UpdateReview(review));
    }
    
    [Authorize]
    [HttpDelete("{id}", Name = "DeleteReview")]
    [ProducesResponseType(typeof(Review), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<Review>> DeleteReview(Guid id)
    {
        /*
         * Parameters: { Guid id }
         *
         * Return: Ok
         *
         * Description: Deletes the review
         *
         * Authorization: Only the admin and the owner of the review can delete
         */
        
        if (User.Identity is not ClaimsIdentity identity)
        {
            return Forbid();
        }
        
        var nameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        var role = identity.FindFirst(ClaimTypes.Role);
        
        if (nameId == null || role == null) return Forbid();

        var exitingReview = await _repository.GetReview(id);
        if (exitingReview == null) return NotFound();
        
        var accountId = new Guid(nameId.Value);

        if (exitingReview.AccountId != accountId && role.Value != "admin") return Forbid();
        
        return Ok(await _repository.DeleteReview(id));
    }
}