﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using Models;
using server.Data;
using server.DTOs;

namespace server.Services
{
    public class OfferService : Service<Offer>, IOfferService
    {
        private readonly ApplicationDbContext _context;


        public OfferService(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<OfferDTO> GetOfferAsync(string offerId, string studentId)
        {
            var offer = await _context.Offers
                .Where(o => o.OfferId == offerId && o.StudentId == studentId)
                .FirstOrDefaultAsync();

            if (offer == null)
            {
                throw new Exception("Offer not found");
            }

            return new OfferDTO
            {
                OfferId = offer.OfferId,
                StudentId = offer.StudentId,
                RecruiterId = offer.RecruiterId,
                BandId = offer.BandId,
                BandName = offer.BandName,
                Amount = offer.Amount,
                Status = offer.Status,
                OfferDate = offer.OfferDate
            };
        }


        public async Task<OfferDTO> CreateOfferAsync(OfferDTO offerDto)
        {
            var offer = new Offer
            {
                OfferId = Guid.NewGuid().ToString(),
                StudentId = offerDto.StudentId,
                RecruiterId = offerDto.RecruiterId,
                BandId = offerDto.BandId,
                BandName = offerDto.BandName,
                Amount = offerDto.Amount,
                Status = offerDto.Status,
                OfferDate = offerDto.OfferDate
            };

            _context.Offers.Add(offer);
            await _context.SaveChangesAsync();

            return offerDto;
        }

        // Get offers made by a specific band
        public async Task<IEnumerable<OfferDTO>> GetOffersByBandAsync(string bandId)
        {
            return await _context.Offers
                .Where(o => o.BandId == bandId)
                .Select(o => new OfferDTO
                {
                    OfferId = o.OfferId,
                    StudentId = o.StudentId,
                    RecruiterId = o.RecruiterId,
                    BandId = o.BandId,
                    BandName = o.BandName,
                    Amount = o.Amount,
                    Status = o.Status,
                    OfferDate = o.OfferDate
                })
                .ToArrayAsync();
        }

        public async Task<IEnumerable<OfferDTO>> GetOffersByRecruiterAsync(string recruiterId)
        {
            return await _context.Offers
                .Where(o => o.RecruiterId == recruiterId)
                .Select(o => new OfferDTO
                {
                    OfferId = o.OfferId,
                    StudentId = o.StudentId,
                    RecruiterId = o.RecruiterId,
                    BandId = o.BandId,
                    BandName = o.BandName,
                    Amount = o.Amount,
                    Status = o.Status,
                    OfferDate = o.OfferDate
                })
                .ToArrayAsync();
        }

        public async Task<IEnumerable<StudentDTO>> GetStudentsByRecruiterAsync(string recruiterId)
        {
            // Fetch all students to whom the recruiter has sent offers
            var students = await _context.Offers
                .Where(o => o.RecruiterId == recruiterId)  // Filter offers by recruiterId
                .Select(o => o.Student)  // Select the related Student entity
                .Distinct()  // Avoid duplicate students in case of multiple offers to the same student
                .ToListAsync();

            // Map the list of students to StudentDTO
            var studentDTOs = students.Select(s => new StudentDTO(s)).ToArray();

            // Here, if you need to set additional properties like OverallRating and OfferCount,
            // you can call other methods or calculate them here. Example:
            foreach (var studentDTO in studentDTOs)
            {
                studentDTO.OverallRating = await GetStudentOverallRatingAsync(studentDTO.Id);
                studentDTO.OfferCount = await GetStudentOfferCountAsync(studentDTO.Id);
            }

            return studentDTOs;
        }

        public async Task<decimal?> GetStudentOverallRatingAsync(string studentId)
        {
            // Example logic to calculate the average rating for a student
            var ratings = await _context.Ratings
                .Where(r => r.StudentId == studentId)
                .ToListAsync();

            if (ratings.Any())
            {
                return ratings.Average(r => (decimal)r.Score); // Assuming `Value` is a decimal rating
            }

            return null; // No ratings available
        }

        public async Task<int> GetStudentOfferCountAsync(string studentId)
        {
            // Example logic to count the number of offers for a student
            return await _context.Offers
                .Where(o => o.StudentId == studentId)
                .CountAsync();
        }

        // Get offers received by a specific student
        public async Task<IEnumerable<OfferDTO>> GetOffersByStudentAsync(string studentId)
        {
            var offers = await _context.Offers
                .Where(o => o.StudentId == studentId)
                .Select(o => new OfferDTO
                {
                    OfferId = o.OfferId,
                    StudentId = o.StudentId,
                    RecruiterId = o.RecruiterId,
                    BandId = o.BandId,
                    BandName = o.BandName,
                    Amount = o.Amount,
                    Status = o.Status,
                    OfferDate = o.OfferDate
                })
                .ToArrayAsync();

            return offers;
        }

 
        // Update the status of an offer (e.g., Accepted, Rejected)
        public async Task<OfferDTO> UpdateOfferAsync(string offerId, OfferDTO offerDto)
        {
            var offer = await _context.Offers.FindAsync(offerId);
            if (offer == null)
            {
                throw new Exception("Offer not found");
            }

            offer.Status = offerDto.Status;
            offer.Amount = offerDto.Amount;
            offer.BandName = offerDto.BandName;

            _context.Offers.Update(offer);
            await _context.SaveChangesAsync();

            return offerDto;
        }

        public async Task DeleteOfferAsync(string offerId)
        {
            var offer = await _context.Offers.FindAsync(offerId);
            if (offer == null)
            {
                throw new Exception("Offer not found");
            }

            _context.Offers.Remove(offer);
            await _context.SaveChangesAsync();
        }
    }
}
