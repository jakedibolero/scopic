using System;
using System.Collections.Generic;
using AutoMapper;
using scopic_test_server.DTO;
using scopic_test_server.Helper;
using scopic_test_server.Interface;
using System.Linq;
using static scopic_test_server.Helper.Codes;
using Microsoft.EntityFrameworkCore;
using scopic_test_server.Services;
using Microsoft.AspNetCore.SignalR;
using scopic_test_server.Hubs;
using System.Threading.Tasks;

namespace scopic_test_server.Data
{
    public class BidRepository : IBidRepository
    {
        private readonly ScopicContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IHubContext<BidHub> _bidHubContext;

        public BidRepository(ScopicContext context, IMapper mapper, IEmailService emailService, IHubContext<BidHub> bidHubContext)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
            _bidHubContext = bidHubContext;
        }
        public BidCode AddBid(BidCreateDto Bid)
        {
            var bid = _mapper.Map<Bid>(Bid);
            bid.BidDate = DateTime.UtcNow;
            bid.BidId = Guid.NewGuid();
            var latestBid = _context.Bid.OrderByDescending(x => x.BidAmount).FirstOrDefault(x => x.ProductId == bid.ProductId);
            if (latestBid != null)
            {
                var product = _context.Product.Include("Bids.User").FirstOrDefault(x => x.ProductId == bid.ProductId);
                if (product == null)
                    return BidCode.Null;
                if (bid.BidAmount <= latestBid.BidAmount)
                    return BidCode.PriceTooLow;
                if (bid.UserId == latestBid.UserId)
                    return BidCode.HighestBid;
                var bidders = product.Bids.Select(x => x.User).Distinct();
                foreach (var bidder in bidders)
                {
                    if (bidder.UserId != Bid.UserId)
                    {
                        var message = $"<h3>A new bid has been made for {product.ProductName}!</h3><p>Current highest bid amount:${bid.BidAmount}</p><p>Submit a higher bid in order to win the auction!</p>";
                        var mail = _emailService.NewMail(bidder.Username, $"New Bid for {product.ProductName}", message);
                        Task.Factory.StartNew(() => _emailService.SendEmail(mail));
                    }
                    else
                    {
                        var message = $"<h3>Your bid for {product.ProductName} is a success!</h3><p>Your bid amount:${bid.BidAmount}</p><p>Submit a higher bid in order to win the auction!</p>";
                        var mail = _emailService.NewMail(bidder.Username, $"Bid success for {product.ProductName}", message);
                        Task.Factory.StartNew(() => _emailService.SendEmail(mail));
                    }

                }
            }
            _context.Bid.Add(bid);
            _context.SaveChanges();
            _bidHubContext.Clients.All.SendAsync("ReceiveBid", _mapper.Map<BidReadDto>(bid));
            return Codes.BidCode.Success;
        }

        public IEnumerable<Bid> GetBidsByProduct(Guid ProductId)
        {
            var bidList = _context.Bid.Where(x => x.ProductId == ProductId).Include(y => y.User);
            return bidList;
        }
    }
}